using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class WireAmount : INetworkSerializable
{
    public int NeutralWires;
    public int GreenWires;
    public int RedWires;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Sérialisation des champs de WireAmount
        serializer.SerializeValue(ref NeutralWires);
        serializer.SerializeValue(ref GreenWires);
        serializer.SerializeValue(ref RedWires);
    }
}

public class MatchManager : MonoBehaviour
{
    [Serializable]
    private class PlayerData {
        [FormerlySerializedAs("Id")] public int id;
        [FormerlySerializedAs("Name")] public string name;
        [FormerlySerializedAs("Role")] public Roles role;
        [FormerlySerializedAs("VisibleWires")] public List<Wires> visibleWires;
        [FormerlySerializedAs("Wires")] public List<Wires> wires;
        public PlayerManager.Mode mode;
    }
    
    private struct RoleAmount
    {
        public int Survivors;
        public int Saboteurs;
    }

    private class ChooseRequest
    {
        public int playerId;
        public int choosenPlayerId;

        public ChooseRequest(int playerId, int choosenPlayerId)
        {
            this.playerId = playerId;
            this.choosenPlayerId = choosenPlayerId;
        }
    }

    private class WireRequest
    {
        public int playerId;
        public int choosenWireIndex;

        public WireRequest(int playerId, int choosenWireIndex)
        {
            this.playerId = playerId;
            this.choosenWireIndex = choosenWireIndex;
        }
    }
    
    private enum EndCondition
    {
        None,
        TimesUp,
        BombExploded,
        BombDefused
    }

    private enum State
    {
        Idle,
        WaitingForPlayer,
        WaitingForWire,
        Ended
    }
    
    public static MatchManager Main;

    [SerializeField]
    public List<PlayerManager> playerManagerList;

    private int numberPlayers;
    private List<Wires> AllWires;
    private List<Roles> AllRoles;
    private int turnCount;
    private int wirePerPlayer = 5;
    private int maxTurn = 4;
    public readonly int maxPlayers = 8;

    private int playerSelected;

    private int RemainingNeutralWires;
    private int RemainingGreenWires;
    private int RemainingRedWires;
    private int RemainingRoundWires;

    [SerializeField]
    private List<PlayerData> players;

    private EndCondition endCondition = EndCondition.None;
    private Roles winners = Roles.Saboteur;
    [SerializeField]
    private State state = State.WaitingForPlayer;

    private ChooseRequest chooseRequest;
    private WireRequest wireRequest;

    private void Awake()
    {
        CommandManager.RegisterCommands(this);

        Main = this;
    }

    private void Start()
    {
        // StartGame();
    }

    public void ResetGame()
    {
        endCondition = EndCondition.None;
        chooseRequest = null;
        wireRequest = null;
        wirePerPlayer = 5;
        turnCount = 0;
    }

    public void StartGame()
    {
        ResetGame();
            
        // Met à jour les joueurs selon la liste de joueurs du serveur
        playerManagerList = new List<PlayerManager>(GameManager.Main.networkServer.playerObjects.Values);
        
        numberPlayers = playerManagerList.Count;
        
        /*
        if (numberPlayers < 4)
        {
            throw new Exception("Not enough players.");
        }
        */
        
        if (numberPlayers > 8)
        {
            throw new Exception("Too many players.");
        }
        
        // Calculer le nombre de fils par rapport au nombre de joueurs
        var wiresPerType = GetWiresFromNumberPlayers(numberPlayers);
        AllWires = GetStartWires(wiresPerType);

        // Distribuer les roles aux joueurs
        var rolesPerType = GetRolesFromNumberPlayers(numberPlayers);
        AllRoles = GetStartRoles(rolesPerType);
        
        // Mélanger les fils
        ShuffleWires();
        // Mélanger les roles
        ShuffleRoles();
        
        // Setup le premier tour
        turnCount = 0;
        RemainingGreenWires = wiresPerType.GreenWires;
        RemainingNeutralWires = wiresPerType.NeutralWires;
        RemainingRedWires = wiresPerType.RedWires;
        
        playerSelected = SelectRandomPlayerId();
        
        StartNextTurn(turnCount, false);
        // Force the first player to choose another player
        players[playerSelected].mode = PlayerManager.Mode.ChoosePlayer;
        SendAllPlayerData();
        
        StartCoroutine(GameLoop());
    }
    
    private IEnumerator GameLoop()
    {
        GameConsole.Print($"Game Started with {AllWires.Count} wires.");
        
        // For each round
        while (endCondition == EndCondition.None && CheckEndGame())
        {
            GameConsole.Print($"\n####Turn {turnCount + 1}####\n");
            
            // For each player turn
            while (CheckEndRound())
            {
                // Select player to cut
                state = State.WaitingForPlayer;
                GameConsole.Print($"{players[playerSelected].name} has to choose a cutter.");
                // Waiting for response from the player
                // TODO Request will be validated during the RTC callback
                while (chooseRequest == null)
                {
                    yield return null;
                }
                // Process the choice
                ProcessChosenCutter(chooseRequest.playerId, chooseRequest.choosenPlayerId);

                for (int i = 0; i < players.Count; i++)
                {
                    players[i].mode = CalculatePlayerMode(i);
                }
                
                SendAllPlayerData();
                state = State.Idle;
                
                // Player has to cut
                state = State.WaitingForWire;
                GameConsole.Print($"{players[playerSelected].name} has to cut a wire.");
                // Waiting for response from the player
                // TODO Request will be validated during the RTC callback
                while (wireRequest == null)
                {
                    yield return null;
                }
                // Process the choice
                ProcessChosenWire(wireRequest.playerId, wireRequest.choosenWireIndex);
                
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].mode = CalculatePlayerMode(i);
                }
                
                SendAllPlayerData();
                state = State.Idle;
                
                // Reset the requests
                chooseRequest = null;
                wireRequest = null;

                UpdateAllWires();

                yield return new WaitForSeconds(1);
            }

            wirePerPlayer--;
            turnCount++;
            // Mélanger les fils
            ShuffleWires();
            StartNextTurn(turnCount);
            players[playerSelected].mode = PlayerManager.Mode.ChoosePlayer;
            SendAllPlayerData();
            yield return null;
        }

        state = State.Ended;
        
        GameConsole.Print($"Game is finished ! Result : {endCondition}");
        GameConsole.Print($"{winners}s won!");
    }

    private WireAmount GetWiresFromNumberPlayers(int numberPlayers)
    {
        return numberPlayers switch
        {
            2 => new WireAmount() {NeutralWires = 7, GreenWires = 2, RedWires = 1},
            4 => new WireAmount() {NeutralWires = 15, GreenWires = 4, RedWires = 1},
            5 => new WireAmount() {NeutralWires = 19, GreenWires = 6, RedWires = 1},
            6 => new WireAmount() {NeutralWires = 23, GreenWires = 6, RedWires = 1},
            7 => new WireAmount() {NeutralWires = 27, GreenWires = 7, RedWires = 1},
            8 => new WireAmount() {NeutralWires = 31, GreenWires = 8, RedWires = 1},
            _ => throw new Exception("Incorrect number of players")
        };
    }

    private List<Wires> GetStartWires(WireAmount amountPerType)
    {
        List<Wires> wiresList = new List<Wires>();
        
        for (int i = 0; i < amountPerType.NeutralWires; i++)
        {
            wiresList.Add(Wires.Neutral);
        }
        for (int i = 0; i < amountPerType.GreenWires; i++)
        {
            wiresList.Add(Wires.Green);
        }
        for (int i = 0; i < amountPerType.RedWires; i++)
        {
            wiresList.Add(Wires.Red);
        }

        return wiresList;
    }
    
    private void ShuffleWires()
    {
        AllWires = AllWires.OrderBy(_ => UnityEngine.Random.value).ToList();
    }
    
    private RoleAmount GetRolesFromNumberPlayers(int numberPlayers)
    {
        int random = Mathf.RoundToInt(UnityEngine.Random.Range(0, 1));
        
        return numberPlayers switch
        {
            2 => new RoleAmount() {Survivors = 1, Saboteurs = 1},
            4 => random == 0 ? new RoleAmount() {Survivors = 2, Saboteurs = 2} : new RoleAmount() {Survivors = 3, Saboteurs = 1},
            5 => new RoleAmount() {Survivors = 3, Saboteurs = 2},
            6 => new RoleAmount() {Survivors = 4, Saboteurs = 2},
            7 => random == 0 ? new RoleAmount() {Survivors = 4, Saboteurs = 3} : new RoleAmount() {Survivors = 5, Saboteurs = 2},
            8 => new RoleAmount() {Survivors = 5, Saboteurs = 3},
            _ => throw new Exception("Incorrect number of players")
        };
    }
    
    private List<Roles> GetStartRoles(RoleAmount amountPerType)
    {
        List<Roles> rolesList = new List<Roles>();
        
        for (int i = 0; i < amountPerType.Survivors; i++)
        {
            rolesList.Add(Roles.Survivor);
        }
        for (int i = 0; i < amountPerType.Saboteurs; i++)
        {
            rolesList.Add(Roles.Saboteur);
        }

        return rolesList;
    }

    private void ShuffleRoles()
    {
        AllRoles = AllRoles.OrderBy(_ => UnityEngine.Random.value).ToList();
    }

    private List<Wires> SetPlayerWires(int id, int cardPerPlayer)
    {
        List<Wires> wireList = new List<Wires>();
        
        for (int i = 0; i < cardPerPlayer; i++)
        {
            wireList.Add(AllWires[i + id * cardPerPlayer]);
        }

        return wireList;
    }

    private int SelectRandomPlayerId()
    {
        return UnityEngine.Random.Range(0, numberPlayers);
    }

    private Wires GetCutWire(Wires wire)
    {
        return wire switch
        {
            Wires.Neutral => Wires.CutNeutral,
            Wires.Green => Wires.CutGreen,
            Wires.Red => Wires.CutRed,
            _ => throw new Exception("Incorrect wire type.")
        };
    }

    private WireAmount CalculateWireAmountForPlayer(int playerId)
    {
        return new WireAmount()
        {
            NeutralWires = players[playerId].wires.Count(wires => wires == Wires.Neutral),
            GreenWires = players[playerId].wires.Count(wires => wires == Wires.Green),
            RedWires = players[playerId].wires.Count(wires => wires == Wires.Red),
        };
    }

    private PlayerManager.Mode CalculatePlayerMode(int playerId)
    {
        if (state == State.WaitingForPlayer && playerSelected == playerId)
        {
            return PlayerManager.Mode.ChooseWire;
        }
        
        if (state == State.WaitingForWire && playerSelected == playerId)
        {
            return PlayerManager.Mode.ChoosePlayer;
        }

        return PlayerManager.Mode.Spectate;
    }

    private void ProcessChosenCutter(int playerId, int chosenPlayerId)
    {
        playerSelected = chosenPlayerId;
    }

    private void ProcessChosenWire(int playerId, int wireIndex)
    {
        GameConsole.Print($"It was a {players[playerId].wires[wireIndex].ToString()}!");
        
        // Change le file pour sa version coupée
        Wires wire = players[playerId].wires[wireIndex];
        Wires wireCut = GetCutWire(wire);
        players[playerId].wires[wireIndex] = wireCut;
        players[playerId].visibleWires[wireIndex] = wireCut;

        // Si le fil est un fil vert, alors on décrémente le compteur de fils verts
        if (wire == Wires.Green)
        {
            RemainingGreenWires--;
        }
        else if (wire == Wires.Neutral)
        {
            RemainingNeutralWires--;
        }
        else if (wire == Wires.Red)
        {
            RemainingRedWires--;
        }

        // On décrémente le nombre de fils à couper pendant le round
        RemainingRoundWires--;
    }

    private void UpdateAllWires()
    {
        AllWires.Clear();

        foreach (var p in players)
        {
            AllWires.AddRange(p.wires);
        }

        // Supprime tous les fils qui sont coupés
        AllWires.RemoveAll((wire => wire != Wires.Green && wire != Wires.Neutral && wire != Wires.Red));
    }

    private bool CheckEndRound()
    {
        if (RemainingRedWires <= 0)
        {
            // Saboteurs win
            endCondition = EndCondition.BombExploded;
            return false;
        }
        else if (RemainingGreenWires <= 0)
        {
            // Survivors win
            endCondition = EndCondition.BombDefused;
            return false;
        }
        else if (RemainingRoundWires <= 0)
        {
            // Next Round
            endCondition = EndCondition.None;
            return false;
        }

        return true;
    }

    private bool CheckEndGame()
    {
        // Game has already ended
        if (endCondition != EndCondition.None)
        {
            return false;
        }
        
        // Time is up, not round left
        if (turnCount >= maxTurn)
        {
            // Saboteurs win
            endCondition = EndCondition.TimesUp;
            return false;
        }
        
        return true;
    }

    private void StartNextTurn(int nextTurn, bool shuffle = false)
    {
        List<PlayerData> playersTemp = new List<PlayerData>();
        
        RemainingRoundWires = numberPlayers;
        
        // Set roles  and wires for each player
        for (int i = 0; i < numberPlayers; i++)
        {
            PlayerData newPlayer = new PlayerData
            {
                id = i,
                name = "Player " + (i),
                role = AllRoles[i],
                wires = SetPlayerWires(i, wirePerPlayer),
                visibleWires = Enumerable.Repeat(Wires.Unknown, wirePerPlayer).ToList(),
                mode = CalculatePlayerMode(i)
            };
            playersTemp.Add(newPlayer);
        }

        players = playersTemp;
    }

    private void SendPlayerData(PlayerManager.PlayerNetworkData data)
    {
        playerManagerList[data.playerId].ReceivePrivatePlayerDataClientRpc(data);
        //playerManagerList[data.playerId].ReceivePlayerData(data);
    }
    
    private void SendAllPlayerData()
    {
        List<List<Wires>> allVisibleWires = new List<List<Wires>>();

        foreach (var p in players)
        {
            allVisibleWires.Add(p.visibleWires);
        }
        
        for (int i = 0; i < playerManagerList.Count; i++)
        {
            WireAmount wireAmount = CalculateWireAmountForPlayer(i);
            
            PlayerManager.PlayerNetworkData data = new PlayerManager.PlayerNetworkData
            {
                playerId = i,
                remainingTurns = maxTurn - turnCount,
                role = players[i].role,
                neutralAmount = wireAmount.NeutralWires,
                greenAmount = wireAmount.GreenWires,
                redAmount = wireAmount.RedWires,
                remainingGreen = RemainingGreenWires,
                remainingRoundWire = RemainingRoundWires,
                playerCutter = playerSelected,
                mode = players[i].mode,
                visibleWires = allVisibleWires[i].ToArray()
            };
            
            SendPlayerData(data);
        }
    }
    
    public void RequestChoosePlayer(int playerId, int chosenPlayerId)
    {
        if (state != State.WaitingForPlayer)
        {
            throw new Exception($"Server is not waiting for this request: cut {playerId} {chosenPlayerId}");
        }
        
        // Vérifier l'identité de la personne qui choisit
        if (playerId != playerSelected)
        {
            throw new Exception($"Player {playerId} doesn't have the right to choose.");
        }
        
        // Vérifier que le joueur ne s'est pas choisi lui-même
        if (playerSelected == chosenPlayerId)
        {
            throw new Exception("Player can't choose himself.");
        }
        
        if (chosenPlayerId < 0 || chosenPlayerId >= numberPlayers)
        {
            throw new Exception("PlayerId is incorrect.");
        }
        
        // TODO verify identity

        chooseRequest = new ChooseRequest(playerId, chosenPlayerId);
    }
    
    public void RequestCutWire(int playerId, int wireIndex)
    {
        if (state != State.WaitingForWire)
        {
            throw new Exception($"Server is not waiting for this request: cut {playerId} {wireIndex}");
        }
        
        if (playerId != playerSelected)
        {
            throw new Exception($"Player {playerId} doesn't have the right to choose.");
        }
        
        if (wireIndex < 0 || wireIndex >= wirePerPlayer)
        {
            throw new Exception("Invalid wire index.");
        }

        if (players[playerId].wires[wireIndex] != Wires.Neutral && players[playerId].wires[wireIndex] != Wires.Green && players[playerId].wires[wireIndex] != Wires.Red)
        {
            throw new Exception($"Wire {wireIndex} has already been cut.");
        }
        
        // TODO verify identity
        
        wireRequest = new WireRequest(playerId, wireIndex);
    }
}
