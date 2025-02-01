using System;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using UnityEngine.Serialization;
using Physics = UnityEngine.Physics;

public class CameraRaycastController : MonoBehaviour
{
    public Camera mainCamera; // Référence à la caméra principale
    public Transform wireCutterHolder; // Référence à l'outil de coupe
    public float maxDistance = 2f; // Distance maximale pour la pince
    public float moveSpeed = 5f; // Vitesse de déplacement de la pince
    
    private OutlineHover lastHoveredObject;
    private Vector3 targetPosition;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!mainCamera)
        {
            Debug.LogError("Main Camera is not assigned to CameraRaycastController");
            return;
        }

        RaycastHit hit;
        bool ray = RotaryHeart.Lib.PhysicsExtension.Physics.Raycast(transform.position, transform.forward, out hit, PreviewCondition.Editor);
        
        if (ray)
        {
            Vector3 hitPosition = hit.point;
            float distance = Vector3.Distance(transform.position, hitPosition);
            
            if (distance > maxDistance)
            {
                hitPosition = transform.position + transform.forward * maxDistance;
            }
            
            targetPosition = hitPosition;
            
            OutlineHover hoveredObject = hit.collider.GetComponent<OutlineHover>();
            if (hoveredObject)
            {
                if (hoveredObject != lastHoveredObject)
                {
                    if (lastHoveredObject)
                    {
                        lastHoveredObject.Leave();
                    }
                    hoveredObject.Hover();
                    lastHoveredObject = hoveredObject;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    try
                    {
                        hoveredObject.TriggerClick();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            else if (lastHoveredObject)
            {
                lastHoveredObject.Leave();
                lastHoveredObject = null;
            }
        }
        else
        {
            if (lastHoveredObject)
            {
                lastHoveredObject.Leave();
                lastHoveredObject = null;
            }
            
            targetPosition = transform.position + transform.forward * maxDistance;
        }
        
        // Appliquer un léger retard au déplacement de la pince
        if (wireCutterHolder && GameManager.Main.playerMode == PlayerManager.Mode.ChooseWire)
        {
            wireCutterHolder.transform.position = Vector3.Lerp(wireCutterHolder.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Faire matcher la rotation Y avec celle de la caméra
            Vector3 wireCutterRotation = wireCutterHolder.transform.eulerAngles;
            wireCutterHolder.transform.rotation = Quaternion.Euler(wireCutterRotation.x, mainCamera.transform.eulerAngles.y, wireCutterRotation.z);
        }
    }
}
