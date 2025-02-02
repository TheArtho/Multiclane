using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public Wires WireType
    {
        get => _wireType;
        set
        {
            _wireType = value;
            OnWireUpdate(value);
        }
    }

    public PlayerManager player;
    public int id = -1;

    [SerializeField]
    private Wires _wireType = Wires.Unknown;

    [SerializeField] 
    public MeshRenderer wireMesh;
    [SerializeField] 
    private MeshRenderer wireCutmesh;
    [SerializeField] 
    private MeshRenderer wireCutmesh2;

    private void OnWireUpdate(Wires value)
    {
        switch (value)
        {
            case Wires.Neutral:
                wireMesh.gameObject.SetActive(true);
                wireCutmesh.gameObject.SetActive(false);
                wireCutmesh2.gameObject.SetActive(false);
                wireMesh.material = GameManager.Main.NeutralWireMaterial;
                break;
            case Wires.CutNeutral:
                wireMesh.gameObject.SetActive(false);
                wireCutmesh.gameObject.SetActive(true);
                wireCutmesh2.gameObject.SetActive(true);
                wireCutmesh.material = GameManager.Main.NeutralWireMaterial;
                wireCutmesh2.material = GameManager.Main.NeutralWireMaterial;
                break;
            case Wires.Green:
                wireMesh.gameObject.SetActive(true);
                wireCutmesh.gameObject.SetActive(false);
                wireCutmesh2.gameObject.SetActive(false);
                wireMesh.material = GameManager.Main.GreenWireMaterial;
                break;
            case Wires.CutGreen:
                wireMesh.gameObject.SetActive(false);
                wireCutmesh.gameObject.SetActive(true);
                wireCutmesh2.gameObject.SetActive(true);
                wireCutmesh.material = GameManager.Main.GreenWireMaterial;
                wireCutmesh2.material = GameManager.Main.GreenWireMaterial;
                break;
            case Wires.Red:
                wireMesh.gameObject.SetActive(true);
                wireCutmesh.gameObject.SetActive(false);
                wireCutmesh2.gameObject.SetActive(false);
                wireMesh.material = GameManager.Main.RedWireMaterial;
                break;
            case Wires.CutRed:
                wireMesh.gameObject.SetActive(false);
                wireCutmesh.gameObject.SetActive(true);
                wireCutmesh2.gameObject.SetActive(true);
                wireCutmesh.material = GameManager.Main.RedWireMaterial;
                wireCutmesh2.material = GameManager.Main.RedWireMaterial;
                break;
            case Wires.Unknown:
            default:
                wireMesh.gameObject.SetActive(true);
                wireCutmesh.gameObject.SetActive(false);
                wireCutmesh2.gameObject.SetActive(false);
                wireMesh.material = GameManager.Main.UnknownWireMaterial;
                break;
        }
    }

    public void Select()
    {
        player.ChooseWire(id);
    }
}
