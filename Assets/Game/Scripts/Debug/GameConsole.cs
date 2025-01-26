using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class GameConsole : MonoBehaviour
{
    public static GameConsole Main;
    
    private ConsoleInputActions _inputAction;

    public bool visible;
    public int maxLines = 20;
    private bool _opened;

    [Space]
    
    [SerializeField]
    private GameObject console;
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private TextMeshProUGUI outPutText;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if (Main)
        {
            DestroyImmediate(this);
        }
        else
        {
            Main = this;
        }
        
        _inputAction = new ConsoleInputActions();

        _inputAction.Console.Toggle.performed += ctx =>
        {
            _opened = !_opened;
            ToggleConsole(_opened);
        };

        _inputAction.Console.Send.performed += ctx => OnInputSubmitted();

        // Enregistre les commandes pour cette instance
        CommandManager.RegisterCommands(this);
    }

    private void Start()
    {
        _opened = visible;
        ToggleConsole(_opened);
    }

    private void Update()
    {
        // Réactive le focus si l'InputField n'a pas le focus
        if (_opened && !inputField.isFocused)
        {
            inputField.ActivateInputField();
        }
    }

    private void ToggleConsole(bool value)
    {
        console.GetComponent<CanvasGroup>().alpha = value ? 1 : 0;

        if (value)
        {
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }
        else
        {
            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }
    }

    private void OnEnable()
    {
        _inputAction.Enable();
    }

    private void OnDestroy()
    {
        _inputAction.Dispose();
    }

    private void OnDisable()
    {
        _inputAction.Disable();
    }

    // Méthode appelée lorsque l'utilisateur appuie sur Entrée
    public void OnInputSubmitted()
    {
        string commandLine = inputField.text.Trim().Replace("\n", "");
        if (string.IsNullOrEmpty(commandLine)) return;

        var output = CommandManager.ExecuteCommand(this, commandLine);
        AppendToOutput(output);
        
        Debug.Log($"[GameConsole] {output}");

        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    private void AppendToOutput(string message)
    {
        outPutText.text += message + "\n";

        // Découpe le texte en lignes
        var lines = outPutText.text.Split('\n');

        // Si le nombre de lignes dépasse la limite, enlève les plus anciennes
        if (lines.Length > maxLines)
        {
            outPutText.text = string.Join("\n", lines[^maxLines..]); // Garde seulement les dernières `maxLines` lignes
        }
    }

    public static void Print(string message)
    {
        GameConsole.Main.AppendToOutput(GameConsole.Main.Echo(message));
    }

    // Commande : Afficher la liste des commandes
    [ConsoleCommand("help", "Displays all available commands.")]
    private string Help()
    {
        return CommandManager.GetCommandList();
    }

    // Commande : Nettoyer la console
    [ConsoleCommand("clear", "Clears the console.")]
    private void Clear()
    {
        outPutText.text = string.Empty;
    }

    // Commande : Echo
    [ConsoleCommand("echo", "Repeats the given message.")]
    private string Echo(string message)
    {
        return message;
    }
    
    [ConsoleCommand("choose", "(ServerSide) Choose a player to cut next")]
    public string RequestChoosePlayerCommand(string playerId, string chosenPlayer)
    {
        MatchManager.Main.RequestChoosePlayer(int.Parse(playerId), int.Parse(chosenPlayer));

        return $"[Player {playerId}] Player {chosenPlayer} has been chosen.";
    }
    
    [ConsoleCommand("cut", "(ServerSide) Choose a wire to cut")]
    public string RequestCutWireCommand(string playerId, string wireIndex)
    {
        MatchManager.Main.RequestCutWire(int.Parse(playerId), int.Parse(wireIndex));
        
        return $"[Player {playerId}] Wire {wireIndex} has been chosen.";
    }
}
