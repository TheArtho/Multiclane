using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager;
    [SerializeField]
    private Transform cameraTransform;
    
    [Space]
    public float maxDistance = 2.0f;
    public float moveSpeed = 5;
    
    private Vector3 targetPosition;
    private Transform wireCutter;

    private int layerMask;

    void Start()
    {
        wireCutter = GameObject.Find("WireCutter").transform;
        
        // Ignorer le layer 9 (Ignore PlayerRaycast)
        layerMask = ~(1 << 9);
    }

    void Update()
    {
        RaycastHit hit;
        bool ray = RotaryHeart.Lib.PhysicsExtension.Physics.Raycast(
            cameraTransform.position, 
            cameraTransform.forward, 
            out hit, 
            PreviewCondition.Editor, 
            layerMask
        );

        if (ray)
        {
            Vector3 hitPosition = hit.point;
            float distance = Vector3.Distance(cameraTransform.position, hitPosition);
            
            if (distance > maxDistance)
            {
                hitPosition = cameraTransform.position + cameraTransform.forward * maxDistance;
            }
            
            targetPosition = hitPosition;
        }
        else
        {
            targetPosition = cameraTransform.position + cameraTransform.forward * maxDistance;
        }
        
        // Appliquer un léger retard au déplacement de la pince
        if (playerManager.PlayerMode == PlayerManager.Mode.ChooseWire && GameManager.Main.selectedPlayer == playerManager.PlayerId)
        {
            wireCutter.transform.position = Vector3.Lerp(wireCutter.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Faire matcher la rotation Y avec celle de la caméra
            Vector3 wireCutterRotation = wireCutter.transform.eulerAngles;
            wireCutter.transform.rotation = Quaternion.Euler(wireCutterRotation.x, cameraTransform.transform.eulerAngles.y, wireCutterRotation.z);
        }
        /*
        else
        {
            Vector3 centerPosition = new Vector3(0, -1.88f, 0);
            
            wireCutter.transform.position = Vector3.Lerp(wireCutter.transform.position, centerPosition, moveSpeed * Time.deltaTime);
        }
        */
    }
}