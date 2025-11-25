using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PassLine.Assets._Slime.Scripts.Physics
{
using UnityEngine;

public class SlimeCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("SlimePhysics component'ini sürükle")]
    public SlimePhysics slimePhysics;

    [Header("Follow Settings")]
    [Tooltip("Kamera ne kadar hızlı takip ediyor (düşük = smooth)")]
    [Range(0.01f, 1f)]
    public float followSpeed = 0.15f;

    [Tooltip("Kamera Z pozisyonu (negatif olmalı)")]
    public float cameraDistance = -10f;

    [Header("Bounds (Opsiyonel)")]
    [Tooltip("Kamera sınırları olsun mu?")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-50, -50);
    public Vector2 maxBounds = new Vector2(50, 50);

    [Header("Deadzone (Opsiyonel)")]
    [Tooltip("Merkez bu alandan çıkınca kamera hareket eder")]
    public bool useDeadzone = false;
    public float deadzoneRadius = 1f;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (slimePhysics == null)
        {
            Debug.LogError("SlimeCameraFollow: SlimePhysics referansı atanmamış!");
            enabled = false;
            return;
        }

        // İlk pozisyonu ayarla
        Vector3 centerPos = slimePhysics.GetCenterPosition();
        transform.position = new Vector3(centerPos.x, centerPos.y, cameraDistance);
        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (slimePhysics == null) return;

        Vector3 centerPos = slimePhysics.GetCenterPosition();
        Vector3 desiredPos = new Vector3(centerPos.x, centerPos.y, cameraDistance);

        // Deadzone check
        if (useDeadzone)
        {
            Vector3 currentPosFlat = new Vector3(transform.position.x, transform.position.y, 0);
            Vector3 centerPosFlat = new Vector3(centerPos.x, centerPos.y, 0);
            float distance = Vector3.Distance(currentPosFlat, centerPosFlat);

            if (distance < deadzoneRadius)
            {
                // Merkez deadzone içinde, kamera hareket etme
                return;
            }

            // Deadzone dışına çıktı, sadece fazla kısmı takip et
            Vector3 direction = (centerPosFlat - currentPosFlat).normalized;
            float excessDistance = distance - deadzoneRadius;
            desiredPos = currentPosFlat + direction * excessDistance;
            desiredPos.z = cameraDistance;
        }

        // Smooth follow
        targetPosition = Vector3.Lerp(targetPosition, desiredPos, followSpeed);

        // Bounds check
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = targetPosition;
    }

    void OnDrawGizmos()
    {
        if (slimePhysics == null) return;

        // Deadzone görselleştir
        if (useDeadzone && Application.isPlaying)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            DrawCircleGizmo(transform.position, deadzoneRadius, 32);
        }

        // Bounds görselleştir
        if (useBounds)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) / 2f,
                (minBounds.y + maxBounds.y) / 2f,
                0
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0
            );
            Gizmos.DrawWireCube(center, size);
        }
    }

    void DrawCircleGizmo(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}

/*
=== KURULUM ===

1. Main Camera'ya bu script'i ekle

2. Inspector'da:
   - Slime Physics → SlimeBody'deki SlimePhysics component'ini sürükle
   - Follow Speed → 0.15 (ne kadar smooth olsun)
   - Camera Distance → -10 (Z pozisyonu)

3. OPSİYONEL AYARLAR:

   A) Bounds (Harita sınırları):
      - Use Bounds → aktif et
      - Min Bounds → (-50, -50)
      - Max Bounds → (50, 50)

   B) Deadzone (Merkez biraz hareket etsin kamera etmesin):
      - Use Deadzone → aktif et
      - Deadzone Radius → 1.0 (1 birimlik alan)

=== NASIL ÇALIŞIR ===

- Slime'ın merkezi hareket ettikçe kamera takip eder
- Follow Speed ile ne kadar smooth olacağını ayarlarsın
- Deadzone varsa: merkez küçük hareketler yaparken kamera hareketsiz kalır
- Bounds varsa: kamera harita dışına çıkmaz

=== İPUCU ===

Eğer kamera çok geç kalıyorsa: Follow Speed'i artır (0.3-0.5)
Eğer kamera çok hızlıysa: Follow Speed'i azalt (0.05-0.1)
*/

}
