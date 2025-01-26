using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerHover()
    {
        transform.Find("Head").GetComponent<Outline>().OutlineWidth = 10;
        transform.Find("Body").GetComponent<Outline>().OutlineWidth = 10;
    }

    public void PlayerLeave()
    {
        transform.Find("Head").GetComponent<Outline>().OutlineWidth = 0;
        transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
    }
}
