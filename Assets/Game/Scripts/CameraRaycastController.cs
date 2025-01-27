using System;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using Physics = UnityEngine.Physics;

public class CameraRaycastController : MonoBehaviour
{
    public Camera mainCamera; // Référence à la caméra principale
    private OutlineHover lastHoveredObject;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Vérifie que la caméra principale est définie
        if (!mainCamera)
        {
            Debug.LogError("Main Camera is not assigned to CameraRaycastController");
            return;
        }

        // Lance un raycast depuis le centre de l'écran
        RaycastHit hit;
        bool ray = RotaryHeart.Lib.PhysicsExtension.Physics.Raycast(transform.position, transform.forward, out hit, PreviewCondition.Editor);
        if (ray)
        {
            // Vérifie si l'objet touché possède le composant OutlineHover
            OutlineHover hoveredObject = hit.collider.GetComponent<OutlineHover>();

            if (hoveredObject)
            {
                // Active l'outline de l'objet touché
                if (hoveredObject != lastHoveredObject)
                {
                    if (lastHoveredObject)
                    {
                        lastHoveredObject.Leave();
                    }
                    hoveredObject.Hover();
                    lastHoveredObject = hoveredObject;
                }

                // Gère le clic de souris
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
            else
            {
                // Désactive l'outline de l'objet précédent si nécessaire
                if (lastHoveredObject)
                {
                    lastHoveredObject.Leave();
                    lastHoveredObject = null;
                }
            }
        }
        else
        {
            // Désactive l'outline de l'objet précédent si aucun objet n'est touché
            if (lastHoveredObject)
            {
                lastHoveredObject.Leave();
                lastHoveredObject = null;
            }
        }
    }
}
