using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHead : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField] 
    private Transform Head;

    // Update is called once per frame
    void Update()
    {
        Head.transform.eulerAngles = new Vector3(cameraTransform.eulerAngles.x, transform.eulerAngles.y,0);
    }
}
