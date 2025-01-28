using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : NetworkBehaviour
{
    // Dictionnaire pour associer un clientId à l'objet joueur correspondant
    public Dictionary<ulong, PlayerManager> playerObjects = new Dictionary<ulong, PlayerManager>();
    public Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    
    // ServerRpc

    [ServerRpc(RequireOwnership = false)]
    public void SendPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        if (NetworkManager.ConnectedClients.ContainsKey((ulong) clientId))
        {
            GameConsole.Print($"{playerName} joined the game.");
                
            playerNames[clientId] = playerName;
                
            foreach (var p in playerObjects)
            {
                p.Value.SetNameClientRpc(playerNames[p.Key]);
            }
        }
    }
    
    // Client Rpcs

    [ClientRpc]
    public void ConsoleMessageClientRpc(string message)
    {
        if (!IsServer)
        {
            GameConsole.Print(message);
        }
    }
    
    // Start is called before the first frame update
    public void Subscribe()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
    }

    public void Unsubscribe()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
    }

    // Network Callbacks
    
    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Vérifiez si le nombre de joueurs dépasse la limite
        if (NetworkManager.Singleton.ConnectedClients.Count >= MatchManager.Main.maxPlayers)
        {
            Debug.Log("Connexion refusée : limite de joueurs atteinte.");
            response.Approved = false; // Refuser la connexion
            response.CreatePlayerObject = false; // Ne pas créer de joueur pour ce client
        }
        else
        {
            Debug.Log("Connexion approuvée.");
            response.Approved = true; // Approuver la connexion
            response.CreatePlayerObject = true; // Créer un joueur pour ce client
        }
    }
    
    private void OnClientConnect(ulong clientId)
    {
        if (NetworkManager.ConnectedClients.ContainsKey((ulong) clientId))
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerManager>();
            
            // Stocker la référence dans le dictionnaire
            if (playerObject != null)
            {
                playerObjects[clientId] = playerObject;
                Debug.Log($"Player {clientId} connecté et enregistré.");

                List<ulong> tmp = new List<ulong>(playerObjects.Keys);

                int index = 0;
                foreach (var keyValuePair in playerObjects)
                {
                    keyValuePair.Value.PlacePlayerClientRpc(index, playerObjects.Count, GameManager.Main.tableCenter.position);
                    index++;
                }

                if (IsHost)
                {
                    playerNames[clientId] = GameManager.Main.playerName;
                
                    foreach (var p in playerObjects)
                    {
                        p.Value.SetNameClientRpc(playerNames[p.Key]);
                    }
                }
            }
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (NetworkManager.ConnectedClients.ContainsKey((ulong) clientId))
        {
            Debug.Log($"Player {clientId} déconnecté.");

            playerObjects.Remove(clientId);
            GameConsole.Print($"{playerNames[clientId]} left the game.");
        }
    }
}
