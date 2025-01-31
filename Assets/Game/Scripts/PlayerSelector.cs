using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    public PlayerManager playerManager;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Main.playerMode == PlayerManager.Mode.ChoosePlayer) return;
        
        transform.Find("Head").GetComponent<Outline>().OutlineWidth = 0;
        transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
    }

    public void PlayerHover()
    {
        playerManager.ShowTargetName();
        
        if (GameManager.Main.playerMode != PlayerManager.Mode.ChoosePlayer) return;
        
        transform.Find("Head").GetComponent<Outline>().OutlineWidth = 10;
        transform.Find("Body").GetComponent<Outline>().OutlineWidth = 10;
    }

    public void PlayerLeave()
    {
        playerManager.HideTargetName();
        
        if (GameManager.Main.playerMode != PlayerManager.Mode.ChoosePlayer) return;
        
        transform.Find("Head").GetComponent<Outline>().OutlineWidth = 0;
        transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
    }
}
