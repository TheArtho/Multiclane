using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionPage : MonoBehaviour
{
    public Camera menuCamera;
    
    public TMP_InputField username;
    
    // Create Game
    public TMP_InputField server_port;
    
    // Join Game
    public TMP_InputField join_addres;
    public TMP_InputField join_port;
    
    public GameObject menuPage;
    public GameObject joinPage;
    public GameObject createPage;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                HideMenu();
                GameManager.Main.networkServer.SendPlayerNameServerRpc(GameManager.Main.playerName);
                GameConsole.Main.ToggleConsole(true);
            }
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            Debug.Log($"[OnClientDisconnectCallback] Server shut down.");
            QuitGame();
        };

        username.text = PlayerPrefs.GetString("username");
        join_addres.text = PlayerPrefs.GetString("join_address");
        join_port.text = PlayerPrefs.GetString("join_port");
        server_port.text = PlayerPrefs.GetString("server_port");
    }

    public void QuitGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowMenu();
        GameConsole.Main.ToggleConsole(false);
    }

    public void ShowMenuPage()
    {
        joinPage.SetActive(false);
        createPage.SetActive(false);
        menuPage.SetActive(true);
    }

    public void ShowJoinPage()
    {
        menuPage.SetActive(false);
        joinPage.SetActive(true);
    }
    
    public void ShowCreatePage()
    {
        menuPage.SetActive(false);
        createPage.SetActive(true);
    }
    
    public void Cancel()
    {
        ShowMenu();
    }
    
    public void Create()
    {
        StartCoroutine(CreateServer());
    }

    IEnumerator CreateServer()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        if (!string.IsNullOrEmpty(username.text.Trim()))
        {
            PlayerPrefs.SetString("username", username.text.Trim());
        }
        
        if (!string.IsNullOrEmpty(server_port.text.Trim()))
        {
            PlayerPrefs.SetString("server_port", server_port.text.Trim());
        }
        
        PlayerPrefs.Save();
        
        while (NetworkManager.Singleton.ShutdownInProgress)
        {
            yield return null;
        }
        
        GameManager.Main.playerName = string.IsNullOrEmpty(username.text.Trim())
            ? GetRandomUsername(GenerateFunnyUsernames())
            : username.text.Trim();
        GameManager.Main.StartServer(string.IsNullOrEmpty(server_port.text) ? (ushort) 7777 : ushort.Parse(server_port.text.Trim()));
    }

    public void Join()
    {
        StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        if (!string.IsNullOrEmpty(username.text.Trim()))
        {
            PlayerPrefs.SetString("username", username.text.Trim());
        }
        
        if (!string.IsNullOrEmpty(join_addres.text.Trim()))
        {
            PlayerPrefs.SetString("join_address", join_addres.text.Trim());
        }
        
        if (!string.IsNullOrEmpty(join_port.text.Trim()))
        {
            PlayerPrefs.SetString("join_port", join_port.text.Trim());
        }
        
        PlayerPrefs.Save();

        while (NetworkManager.Singleton.ShutdownInProgress)
        {
            yield return null;
        }
        
        GameManager.Main.playerName = string.IsNullOrEmpty(username.text.Trim())
            ? GetRandomUsername(GenerateFunnyUsernames())
            : username.text.Trim();
        GameManager.Main.StartClient(join_addres.text.Trim(), string.IsNullOrEmpty(join_port.text) ? (ushort) 7777 : ushort.Parse(join_port.text.Trim()));
    }

    public void ShowMenu()
    {
        menuCamera.gameObject.SetActive(true);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        ShowMenuPage();
    }

    public void HideMenu()
    {
        menuCamera.gameObject.SetActive(false);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    
    static List<string> GenerateFunnyUsernames()
    {
        return new List<string>
        {
            "SirLaughsALot", "PunnyBunny", "WaffleWizard", "CaptainObvious", "BananaBandit",
            "CerealKiller", "DonutDisturber", "FunkyMonkey", "PickleMaster", "SnailTrail",
            "UnicornTamer", "TacoDestroyer", "SlothRacer", "DuckyMcQuack", "BurgerNinja",
            "NachoKnight", "PastaPirate", "TofuTitan", "GuacGuardian", "ToastTyrant",
            "BaconBandito", "SpaghettiYeti", "CookieCrusher", "JamJuggler", "PopcornProwler",
            "MilkshakeMarauder", "SalsaSamurai", "SundaeSlayer", "ChocoChampion", "MarshmallowMage",
            "PizzaPaladin", "MuffinManiac", "HotdogHero", "PancakePhantom", "KetchupKing",
            "FriesFiend", "NoodleNinja", "CheeseChampion", "BaguetteBandit", "CroissantCrusher",
            "SoupSleuth", "WaffleWarrior", "CarrotCrusader", "BerryBandit", "PeanutProwler",
            "IceCreamImp", "PuddingPirate", "BrownieBeast", "SyrupSlinger", "LemonLurker",
            "PeachProwler", "CherryChallenger", "AppleAssassin", "MelonMarauder", "DragonfruitDestroyer",
            "KiwiKnight", "CoconutConqueror", "AvocadoAvenger", "GrapeGambler", "StrawberrySneaker"
        };
    }

    static string GetRandomUsername(List<string> usernames)
    {
        int index = UnityEngine.Random.Range(0, usernames.Count);
        return usernames[index];
    }
}
