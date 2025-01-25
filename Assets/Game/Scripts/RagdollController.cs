using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RagdollController : NetworkBehaviour
{
    private Rigidbody _rb;
    private bool _ragDoll = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    
}
