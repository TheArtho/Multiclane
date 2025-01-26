using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private Roles role;
    [SerializeField]
    private WireAmount wireAmount;
    [SerializeField]
    private int remainingGreen;

    public void ReceivePlayerData(Roles role, WireAmount wireAmount, int remainingGreen)
    {
        this.wireAmount = wireAmount;
        this.role = role;
        this.remainingGreen = remainingGreen;

        string neutral = wireAmount.NeutralWires > 0 ? $"{wireAmount.NeutralWires} Neutre" + (wireAmount.NeutralWires > 1 ? "s" : "") : "";
        string green = wireAmount.GreenWires > 0 ? $"{wireAmount.GreenWires} Vert" + (wireAmount.GreenWires > 1 ? "s" : "") : "";
        string red = wireAmount.RedWires > 0 ? $"{wireAmount.RedWires} Rouge" + (wireAmount.RedWires > 1 ? "s" : "") : "";

        mallet.Text.text = neutral + "\n" + green + "\n" + red;
    }
}
