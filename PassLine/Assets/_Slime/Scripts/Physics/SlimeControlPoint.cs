using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PassLine.Assets._Slime.Scripts.Physics
{
using UnityEngine;
using UnityEngine.InputSystem;

public class SlimeControlPoint : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference pointerPosition;
    public InputActionReference pointerPress;

    [Header("Settings")]
    public float touchRadius = 0.5f; // Touch algılama yarıçapı

    private SlimePhysics slimePhysics;
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private int touchId = -1; // Hangi touch bu control point'i tutuyor

    void Start()
    {
        slimePhysics = GetComponentInParent<SlimePhysics>();
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        if (pointerPosition != null)
            pointerPosition.action.Enable();
        if (pointerPress != null)
            pointerPress.action.Enable();
    }

    void OnDisable()
    {
        if (pointerPosition != null)
            pointerPosition.action.Disable();
        if (pointerPress != null)
            pointerPress.action.Disable();
    }

    void Update()
    {
        if (pointerPosition == null || pointerPress == null) return;

        Vector2 screenPos = pointerPosition.action.ReadValue<Vector2>();
        bool isPressed = pointerPress.action.IsPressed();

        Vector3 worldPos = ScreenToWorldPosition(screenPos);

        if (!isDragging)
        {
            // Touch başladı mı kontrol et
            if (isPressed && IsTouchingThisPoint(worldPos))
            {
                isDragging = true;
                dragOffset = transform.position - worldPos;
            }
        }
        else
        {
            // Sürükleme devam ediyor
            if (isPressed)
            {
                Vector3 targetPos = worldPos + dragOffset;

                // Max distance check
                if (slimePhysics != null)
                {
                    Vector3 toTarget = targetPos - slimePhysics.GetCenterPosition();
                    float distance = toTarget.magnitude;

                    if (distance > slimePhysics.maxDistanceFromCenter)
                    {
                        targetPos = slimePhysics.GetCenterPosition() +
                                   toTarget.normalized * slimePhysics.maxDistanceFromCenter;
                    }
                }

                transform.position = targetPos;

                // Merkezi güncelle
                if (slimePhysics != null)
                {
                    slimePhysics.RecalculateCenter();
                }
            }
            else
            {
                // Touch bırakıldı
                isDragging = false;
            }
        }
    }

    bool IsTouchingThisPoint(Vector3 worldPos)
    {
        float distance = Vector3.Distance(worldPos, transform.position);
        return distance <= touchRadius;
    }

    Vector3 ScreenToWorldPosition(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z));
        return mainCamera.ScreenToWorldPoint(pos);
    }

    void OnDrawGizmos()
    {
        // Control point gizmo
        Gizmos.color = isDragging ? Color.yellow : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}


}
