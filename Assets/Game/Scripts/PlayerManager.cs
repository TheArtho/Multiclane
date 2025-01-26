using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public enum Mode
    {
        Spectate,
        ChoosePlayer,
        ChooseWire
    }

    [SerializeField]
    private Mallet mallet;

    [Space] [SerializeField] 
    private int playerId;
    [SerializeField]
    private Roles role;
    [SerializeField]
    private int remainingTurns = -1;
    [SerializeField]
    private WireAmount wireAmount;
    [SerializeField]
    private int remainingGreen;
    [SerializeField] 
    private int remainingRoundWire;

    [Space] 
    
    [SerializeField] 
    private GameObject wirePrefab;

    [SerializeField] private int playerCutter;

    [SerializeField] private List<Wire> wireObjects;

    public void ReceivePlayerData(int playerId, int remainingTurns, Roles role, WireAmount wireAmount, int remainingGreen, int remainingRoundWire, int playerCutter, List<List<Wires>> allVisibleWires)
    {
        this.playerId = playerId;
        this.wireAmount = wireAmount;
        this.role = role;
        this.remainingGreen = remainingGreen;
        this.remainingRoundWire = remainingRoundWire;
        this.playerCutter = playerCutter;
        
        UpdateMallet(remainingTurns, allVisibleWires[playerId]);
        this.remainingTurns = remainingTurns;
        
        GameManager.UpdateScore(remainingTurns, remainingGreen, remainingRoundWire);
    }

    void UpdateMallet(int remainingTurns, List<Wires> visibleWires)
    {
        // Update des montants des fils
        string neutral = wireAmount.NeutralWires > 0 ? $"{wireAmount.NeutralWires} Neutre" + (wireAmount.NeutralWires > 1 ? "s" : "") : "";
        string green = wireAmount.GreenWires > 0 ? $"{wireAmount.GreenWires} Vert" + (wireAmount.GreenWires > 1 ? "s" : "") : "";
        string red = wireAmount.RedWires > 0 ? $"{wireAmount.RedWires} Rouge" + (wireAmount.RedWires > 1 ? "s" : "") : "";

        mallet.Text.text = neutral + "\n" + green + "\n" + red;
        
        // Update des fils visibles
        if (this.remainingTurns != remainingTurns)
        {
            wireObjects.Clear();
            
            // Régénère tous les fils si le round a changé
            while (mallet.AllWires.transform.childCount > 0)
            {
                DestroyImmediate(mallet.AllWires.transform.GetChild(0).gameObject);
            }

            for (int i = 0; i < visibleWires.Count; i++)
            {
                var go = GameObject.Instantiate(wirePrefab, mallet.AllWires.transform);

                wireObjects.Add(go.GetComponent<Wire>());
            }

            PlaceObjectsInLine(wireObjects, mallet.spacing);
        }
        
        if (wireObjects.Count == visibleWires.Count)
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
}
