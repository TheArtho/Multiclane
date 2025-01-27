using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Main;

    public int playerId;
    public PlayerManager.Mode playerMode = PlayerManager.Mode.Spectate;
    
    public TextMeshProUGUI score;

    [SerializeField] 
    public NetworkServer networkServer;

    public Material NeutralWireMaterial;
    public Material GreenWireMaterial;
    public Material RedWireMaterial;
    public Material UnknownWireMaterial;

    [Space]
    
    public Transform tableCenter;
    
    private void Awake()
    {
        if (Main)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Main = this;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        networkServer = GetComponent<NetworkServer>();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);
        
        networkServer.Subscribe();
        NetworkManager.Singleton.StartHost();
    }

    public void StopServer()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Arrêt du serveur...");
            NetworkManager.Singleton.Shutdown();
            networkServer.Unsubscribe();
        }
        else
        {
            Debug.LogWarning("Tentative d'arrêt du serveur, mais ce client n'est pas le serveur.");
        }
    }

    public void StartClient(string address)
    {
        if (!string.IsNullOrEmpty(address))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(address, 7777);
        }
        
        NetworkManager.Singleton.StartClient();
    }
    
    public void StopClient()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public static void UpdateScore(int remainingTurns, int remainingGreen, int remainingRoundWire)
    {
        Main.score.text = $"Remaining Rounds : {remainingTurns}\n";
        Main.score.text += $"Remaining Wires in round : {remainingRoundWire}\n";
        Main.score.text += $"Remaining Green Wires : {remainingGreen}";
    }
}
