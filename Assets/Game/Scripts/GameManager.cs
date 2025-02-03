using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Main;

    public string playerName;
    public int playerId;
    public int selectedPlayer;
    public PlayerManager.Mode playerMode = PlayerManager.Mode.Spectate;
    
    [FormerlySerializedAs("score")] public TextMeshProUGUI info;
    public TextMeshProUGUI roleInfo;
    public TextMeshProUGUI playerNameInfo;
    public TextMeshProUGUI targetNameInfo;
    
    public NetworkServer networkServer;
    public ConnectionPage connectionPage;

    public Material NeutralWireMaterial;
    public Material GreenWireMaterial;
    public Material RedWireMaterial;
    public Material UnknownWireMaterial;
    
    [Space]
    
    public Color SurvivorColor;
    public Color SaboteurColor;

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
        
        #if UNITY_EDITOR
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        #endif
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        networkServer = GetComponent<NetworkServer>();
    }

    public void StartServer(ushort port = 7777)
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

    public void StartClient(string address, ushort port = 7777)
    {
        if (!string.IsNullOrEmpty(address))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(address, port);
        }
        
        NetworkManager.Singleton.StartClient();
    }
    
    public void StopClient()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public static void UpdateInfo(int remainingTurns, int remainingGreen, int remainingRoundWire, Roles role)
    {
        Main.info.text = $"Remaining Wires in round : {remainingRoundWire}\n";
        Main.info.text += $"Remaining Green Wires : {remainingGreen}";

        if (role == Roles.Survivor)
        {
            Main.roleInfo.text = "You are Survivor";
            Main.roleInfo.color = Main.SurvivorColor;
        }
        else
        {
            Main.roleInfo.text = "You are Saboteur";
            Main.roleInfo.color = Main.SaboteurColor;
        }
    }
}
