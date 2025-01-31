using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline))]
public class OutlineHover : MonoBehaviour
{
    private Outline outline;

    [SerializeField]
    private UnityEvent onHover = new UnityEvent();
    [SerializeField]
    private UnityEvent onLeave = new UnityEvent();
    [SerializeField]
    private UnityEvent onClick = new UnityEvent();

    public PlayerManager.Mode modeFilter = PlayerManager.Mode.ChooseWire;

    void Start()
    {
        // Récupère le composant Outline attaché à l'objet
        outline = GetComponent<Outline>();

        // Assure que l'OutlineWidth est initialisé à 0
        if (outline != null)
        {
            outline.OutlineWidth = 0;
        }
        else
        {
            Debug.LogError("Outline component missing on " + gameObject.name);
        }
    }

    public void Hover()
    {
        onHover.Invoke();
        
        if (GameManager.Main.playerMode != modeFilter) return;
        
        SetOutlineActive(true);
    }

    public void Leave()
    {
        onLeave.Invoke();
        SetOutlineActive(false);
    }

    private void Update()
    {
        if (GameManager.Main.playerMode != modeFilter)
        {
            SetOutlineActive(false);
        }
    }

    public void SetOutlineActive(bool active)
    {
        if (outline)
        {
            outline.OutlineWidth = active ? 10 : 0;
        }
    }

    public void TriggerClick()
    {
        onClick.Invoke();
    }

    /*
    void OnMouseEnter()
    {
        // Lorsque le curseur de la souris entre dans le collider, augmente l'épaisseur de l'outline
        if (outline != null)
        {
            outline.OutlineWidth = 10;
        }
    }

    void OnMouseExit()
    {
        // Lorsque le curseur de la souris quitte le collider, réinitialise l'épaisseur de l'outline
        if (outline != null)
        {
            outline.OutlineWidth = 0;
        }
    }
    
    void OnMouseDown()
    {
        // Déclenche l'événement onClick lorsqu'on clique sur l'objet
        onClick.Invoke();
    }
    */
}