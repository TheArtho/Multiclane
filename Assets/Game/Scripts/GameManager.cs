using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Main;
    
    public TextMeshProUGUI score;

    public Material NeutralWireMaterial;
    public Material GreenWireMaterial;
    public Material RedWireMaterial;
    public Material UnknownWireMaterial;
    
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

    public static void UpdateScore(int remainingTurns, int remainingGreen, int remainingRoundWire)
    {
        Main.score.text = $"Remaining Rounds : {remainingTurns}\n";
        Main.score.text += $"Remaining Wires in round : {remainingRoundWire}\n";
        Main.score.text += $"Remaining Green Wires : {remainingGreen}";
    }
}
