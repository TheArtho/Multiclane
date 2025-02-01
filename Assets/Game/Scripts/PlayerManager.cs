using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : NetworkBehaviour
{
    public enum Mode
    {
        Spectate,
        ChoosePlayer,
        ChooseWire
    }

    [Serializable]
    public class PlayerGameData
    {
        public int playerId;
        public int remainingTurns;
        public Roles role;
        public WireAmount wireAmount;
        public int remainingGreen;
        public int remainingRoundWire;
        public int playerCutter;
        public List<List<Wires>> allVisibleWires;
        public Mode mode;
    }

    [Serializable]
    public class PlayerNetworkData : INetworkSerializable
    {
        public int playerId;
        public int remainingTurns;
        public Roles role;
        public int neutralAmount;
        public int greenAmount;
        public int redAmount;
        public int remainingGreen;
        public int remainingRoundWire;
        public int playerCutter;
        public Mode mode;
        public Wires[] visibleWires;
        public int selectedPlayer;
        
        // Implémentation de la sérialisation des données
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Sérialisation des types simples (int, etc.)
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref remainingTurns);
            serializer.SerializeValue(ref neutralAmount);
            serializer.SerializeValue(ref greenAmount);
            serializer.SerializeValue(ref redAmount);
            serializer.SerializeValue(ref remainingGreen);
            serializer.SerializeValue(ref remainingRoundWire);
            serializer.SerializeValue(ref playerCutter);
            serializer.SerializeValue(ref selectedPlayer);
            
            // Sérialisation de tableaux
            serializer.SerializeValue(ref visibleWires);

            // Sérialisation des énumérations (s'ils sont sérialisables en tant qu'entiers)
            serializer.SerializeValue(ref role);  // Assurez-vous que Roles est sérialisable
            serializer.SerializeValue(ref mode);  // Assurez-vous que Mode est sérialisable
        }
    }


    [SerializeField]
    private Suitcase suitcase;
    [SerializeField] 
    private GameObject playerHeadController;
    [SerializeField]
    private GameObject mesh;
    [SerializeField] 
    private Transform wireCutterHolder;

    [Space] 
    
    [SerializeField] 
    public string playerName;
    [SerializeField] 
    private int playerId;
    [SerializeField]
    private Roles role;
    [SerializeField] 
    private Mode mode;
    [SerializeField]
    private int remainingTurns = -1;
    [SerializeField]
    private WireAmount wireAmount;
    [SerializeField]
    private int remainingGreen;
    [SerializeField] 
    private int remainingRoundWire;
    
    public int PlayerId => playerId;
    public Mode PlayerMode => mode;

    [Space] 
    
    [SerializeField] 
    private GameObject wirePrefab;

    [SerializeField] private int playerCutter;

    [SerializeField] private List<Wire> wireObjects;
    
    #region Network RPC
    
    // Server RPCs
    
    [ServerRpc(RequireOwnership = false)]
    private void ClientChoosePlayerRequestServerRpc(int chosenPlayer, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        MatchManager.Main.RequestChoosePlayer(clientId, chosenPlayer);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ClientChooseWireRequestServerRpc(int chosenPlayer, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        MatchManager.Main.RequestCutWire(clientId, chosenPlayer);
    }
    
    // Client RPCs
    
    [ClientRpc]
    public void PingClientRpc()
    {
        Debug.Log($"Ping.");
    }

    [ClientRpc]
    public void SetNameClientRpc(string name)
    {
        playerName = name;

        if (IsOwner)
        {
            GameManager.Main.playerNameInfo.text = "Username: " + playerName;
        }
    }

    [ClientRpc]
    public void ReceivePrivatePlayerDataClientRpc(PlayerNetworkData data, ClientRpcParams clientRpcParams = default)
    {
        ReceivePlayerData(data);
    }
    
    [ClientRpc]
    public void ReceiveVisibleInfoClientRpc(int remainingTurns, int playerId, Mode mode, Wires[] wiresArray, ClientRpcParams clientRpcParams = default)
    {
        this.playerId = playerId;
        UpdateSuitcaseWires(remainingTurns, new List<Wires>(wiresArray));
        this.remainingTurns = remainingTurns;
    }
    
    [ClientRpc]
    public void ReceiveModeClientRpc(Mode mode, ClientRpcParams clientRpcParams = default)
    {
        this.mode = mode;
    }

    [ClientRpc]
    public void PlacePlayerClientRpc(int playerIndex, int totalPlayers, Vector3 centerPosition)
    {
        PlacePlayerAtPosition(playerIndex, totalPlayers, 5.15f, centerPosition, transform);
    }
    
    #endregion

    private void Start()
    {
        // Si c'est le client
        if (IsOwner)
        {
            // Désactiver le Mesh
            mesh.transform.localScale = Vector3.zero;
        }
        else
        {
            // Désactiver la Fps Camera et la camera
            playerHeadController.SetActive(false);
        }
    }
    
    public void ReceivePlayerData(PlayerNetworkData data)
    {
        // Code Owner only
        if (!IsOwner) return;
        
        this.wireAmount = new WireAmount()
        {
            NeutralWires = data.neutralAmount,
            GreenWires = data.greenAmount,
            RedWires = data.redAmount
        };
        this.role = data.role;
        this.remainingGreen = data.remainingGreen;
        this.remainingRoundWire = data.remainingRoundWire;
        this.playerCutter = data.playerCutter;
        this.mode = data.mode;
        UpdateSuitcase();
        
        GameManager.Main.playerMode = this.mode;
        GameManager.UpdateInfo(data.remainingTurns, remainingGreen, remainingRoundWire, this.role);
        GameManager.Main.selectedPlayer = data.selectedPlayer;
    }

    void UpdateSuitcaseWires(int remainingTurns, List<Wires> visibleWires)
    {
        // Update des fils visibles
        if (this.remainingTurns != remainingTurns)
        {
            wireObjects.Clear();
            
            // Régénère tous les fils si le round a changé
            while (suitcase.AllWires.transform.childCount > 0)
            {
                DestroyImmediate(suitcase.AllWires.transform.GetChild(0).gameObject);
            }

            if (visibleWires != null)
            {
                for (int i = 0; i < visibleWires.Count; i++)
                {
                    var go = GameObject.Instantiate(wirePrefab, suitcase.AllWires.transform);
                    Wire wireComponent = go.GetComponent<Wire>();
                    wireComponent.player = this;
                    wireComponent.id = i;
                    wireObjects.Add(wireComponent);

                    if (!IsOwner)
                    {
                        wireComponent.wireMesh.GetComponent<BoxCollider>().enabled = false;
                    }
                }
                
                PlaceObjectsInLine(wireObjects, suitcase.spacing);
            }
        }
        
        if (visibleWires != null && wireObjects.Count == visibleWires.Count)
        {
            for (int i = 0; i < wireObjects.Count; i++)
            {
                wireObjects[i].WireType = visibleWires[i];
            }
        }
        else
        {
            throw new Exception("Visible wire data doesn't match.");
        }
    }

    void UpdateSuitcase()
    {
        // Update des montants des fils
        string neutral = wireAmount.NeutralWires > 0 ? $"{wireAmount.NeutralWires} Neutre" + (wireAmount.NeutralWires > 1 ? "s" : "") : "";
        string green = wireAmount.GreenWires > 0 ? $"{wireAmount.GreenWires} Vert" + (wireAmount.GreenWires > 1 ? "s" : "") : "";
        string red = wireAmount.RedWires > 0 ? $"{wireAmount.RedWires} Rouge" + (wireAmount.RedWires > 1 ? "s" : "") : "";

        suitcase.Text.text = neutral + "\n" + green + "\n" + red;
    }

    public void PlaceObjectsInLine(List<Wire> objectsToPlace, float spacing)
    {
        if (objectsToPlace == null || objectsToPlace.Count == 0)
        {
            Debug.LogWarning("Aucun objet à placer !");
            return;
        }

        // Position de départ pour centrer les objets
        float startX = -spacing * (objectsToPlace.Count - 1) / 2f;

        // Positionner les objets en ligne
        float currentX = startX;
        foreach (Wire wire in objectsToPlace)
        {
            float objectWidth = 0f;

            // Définir la position
            wire.transform.localPosition = new Vector3(currentX + objectWidth / 2f, 0, 0);

            // Avancer pour le prochain objet
            currentX += objectWidth + spacing;
        }
    }

    public void ChooseWire(int wireIndex)
    {
        if (GameManager.Main.playerMode != Mode.ChooseWire) return;
        
        Debug.Log($"Cut Wire Method: {NetworkManager.Singleton.LocalClientId} choose {wireIndex}");
        // MatchManager.Main.RequestCutWire(GameManager.Main.playerId, wireIndex);
        ClientChooseWireRequestServerRpc(wireIndex);
    }

    public void ChoosePlayer()
    {
        if (GameManager.Main.playerMode != Mode.ChoosePlayer) return;
        
        Debug.Log($"Choose Player Method: {NetworkManager.Singleton.LocalClientId} choose {playerId}");
        // MatchManager.Main.RequestChoosePlayer(GameManager.Main.playerId, playerId);
        ClientChoosePlayerRequestServerRpc(playerId);
    }
    
    public static void PlacePlayerAtPosition(int playerNumber, int totalPlayers, float radius, Vector3 tableCenter, Transform playerTransform)
    {
        // L'angle entre chaque joueur
        float angleStep = 360f / totalPlayers;

        // Calcul de l'angle pour le joueur actuel basé sur son numéro
        float angle = playerNumber * angleStep;

        // Conversion de l'angle en radians
        float angleInRadians = Mathf.Deg2Rad * angle;

        // Calcul de la position du joueur sur le cercle
        float x = tableCenter.x + Mathf.Cos(angleInRadians) * radius;
        float z = tableCenter.z + Mathf.Sin(angleInRadians) * radius;

        // Positionner le joueur à la position calculée
        playerTransform.position = new Vector3(x, playerTransform.position.y, z);

        // Calcul de la rotation du joueur pour qu'il regarde le centre de la table
        Quaternion rotation = Quaternion.LookRotation(tableCenter - playerTransform.position);

        // Appliquer la rotation au joueur
        playerTransform.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
    }

    public void ShowTargetName()
    {
        SetTargetName(this.playerName);
    }
    
    public void HideTargetName()
    {
        SetTargetName();
    }

    private void SetTargetName(string name = "")
    {
        GameManager.Main.targetNameInfo.text = name;
    }
}
