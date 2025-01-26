using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct WireAmount
{
    public int NeutralWires;
    public int GreenWires;
    public int RedWires;
}

public class GameManager : MonoBehaviour
{
    [Serializable]
    private struct PlayerData {
        public int Id;
        public string Name;
        public Roles Role;
        public List<Wires> PlayerWires;
    }
    
    private struct RoleAmount
    {
        public int Survivors;
        public int Saboteurs;
    }

    [SerializeField]
    private List<PlayerManager> playerManagerList;

    private int numberPlayers;
    private List<Wires> AllWires;
    private List<Roles> AllRoles;
    private int turnCount;
    private int wirePerPlayer = 5;

    private int playerCutter;

    private int RemainingGreenWires;
    private int RemainingTurnWires;

    [SerializeField]
    private List<PlayerData> players;

    private void Start()
    {
        StartGame(playerManagerList.Count);
    }

    public void StartGame(int numberPlayers)
    {
        if (numberPlayers < 4)
        {
            throw new Exception("Not enough players.");
        }
        
        if (numberPlayers > 8)
        {
            throw new Exception("Too many players.");
        }
        
        this.numberPlayers = numberPlayers;
        
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
        RemainingGreenWires = wiresPerType.GreenWires;
        StartNextTurn();
        
        Debug.Log($"Game Started with {AllWires.Count} wires");
    }

    private WireAmount GetWiresFromNumberPlayers(int numberPlayers)
    {
        return numberPlayers switch
        {
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
            NeutralWires = players[playerId].PlayerWires.Count(wires => wires == Wires.Neutral),
            GreenWires = players[playerId].PlayerWires.Count(wires => wires == Wires.Green),
            RedWires = players[playerId].PlayerWires.Count(wires => wires == Wires.Red),
        };
    }

    private void ProcessChosenCutter(int chosenPlayerId)
    {
        // Vérifier que le joueur ne s'est pas choisi lui-même
        if (playerCutter == chosenPlayerId)
        {
            throw new Exception("Player can't choose himself.");
        }
        
        if (chosenPlayerId < 0 || chosenPlayerId >= numberPlayers)
        {
            throw new Exception("Wire index is incorrect.");
        }
        
        playerCutter = chosenPlayerId;
        
        // Envoyer l'information aux autres joueurs
    }

    private void ProcessChosenWire(int playerId, int wireIndex)
    {
        // Vérifier que le fil coupé est correct
        if (wireIndex < 0 || wireIndex >= wirePerPlayer)
        {
            throw new Exception("Wire index is incorrect.");
        }

        // Change le file pour sa version coupée
        Wires wire = players[playerId].PlayerWires[wireIndex];
        players[playerId].PlayerWires[wireIndex] = wire;
        
        int index = AllWires.FindIndex(x => x == wire);

        // Si le fil est un fil vert, alors on décrémente le compteur de fils verts
        if (wire == Wires.Green)
        {
            RemainingGreenWires--;
        }

        // On décrémente le nombre de fils à couper pendant le round
        RemainingTurnWires--;
        
        // Envoyer l'information aux autres joueurs
    }

    private void UpdateAllWires()
    {
        AllWires.Clear();

        foreach (var p in players)
        {
            AllWires.AddRange(p.PlayerWires);
        }

        // Supprime tous les fils qui sont coupés
        AllWires.RemoveAll((wire => wire != Wires.Green && wire != Wires.Neutral && wire != Wires.Red));
    }

    private void StartNextTurn()
    {
        List<PlayerData> playersTemp = new List<PlayerData>();
        
        RemainingTurnWires = numberPlayers;
        
        // Set roles  and wires for each player
        for (int i = 0; i < numberPlayers; i++)
        {
            PlayerData newPlayer = new PlayerData
            {
                Id = i,
                Name = "Player " + (i+1),
                Role = AllRoles[i],
                PlayerWires = SetPlayerWires(i, wirePerPlayer)
            };
            playersTemp.Add(newPlayer);
        }

        players = playersTemp;
        
        for (int i = 0; i < playerManagerList.Count; i++)
        {
            SendPlayerData(i, players[i].Role, CalculateWireAmountForPlayer(i), RemainingGreenWires);
        }
    }

    private void SendPlayerData(int playerId, Roles role, WireAmount wireAmount, int remainingGreen)
    {
        playerManagerList[playerId].ReceivePlayerData(role, wireAmount, remainingGreen);
    }
}
