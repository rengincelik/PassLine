using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PassLine.Assets._Slime.Scripts.Physics
{
using UnityEngine;

public class SlimePhysics : MonoBehaviour
{
    [Header("Control Points")]
    [Tooltip("Inspector'dan child control point'leri buraya sürükle")]
    public Transform[] controlPoints;

    [Header("Settings")]
    [Tooltip("Control point'lerin merkeze max uzaklığı")]
    public float maxDistanceFromCenter = 3f;

    [Tooltip("Merkez ne kadar hızlı yeni pozisyona kayıyor")]
    [Range(0.01f, 1f)]
    public float centerSmoothSpeed = 0.1f;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Color centerColor = Color.red;
    public Color lineColor = Color.cyan;

    // Private data
    private Vector3 currentCenter;
    private Vector3 targetCenter;

    void Start()
    {
        ValidateControlPoints();
        InitializeCenter();
    }

    void ValidateControlPoints()
    {
        if (controlPoints == null || controlPoints.Length < 3)
        {
            Debug.LogError("SlimePhysics: En az 3 control point gerekli!");
        }

        // Null check
        for (int i = 0; i < controlPoints.Length; i++)
        {
            if (controlPoints[i] == null)
            {
                Debug.LogError($"SlimePhysics: Control point {i} null!");
            }
        }
    }

    void InitializeCenter()
    {
        // İlk merkez pozisyonunu hesapla
        currentCenter = CalculateCenterFromPoints();
        targetCenter = currentCenter;
    }

    void Update()
    {
        // Merkezi smooth kaydir
        currentCenter = Vector3.Lerp(currentCenter, targetCenter, centerSmoothSpeed);
    }

    public void RecalculateCenter()
    {
        targetCenter = CalculateCenterFromPoints();
    }

    Vector3 CalculateCenterFromPoints()
    {
        if (controlPoints == null || controlPoints.Length == 0)
            return transform.position;

        Vector3 sum = Vector3.zero;
        int validPointCount = 0;

        foreach (Transform point in controlPoints)
        {
            if (point != null)
            {
                sum += point.position;
                validPointCount++;
            }
        }

        return validPointCount > 0 ? sum / validPointCount : transform.position;
    }

    public Vector3 GetCenterPosition()
    {
        return currentCenter;
    }

    public Vector3 GetTargetCenterPosition()
    {
        return targetCenter;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Runtime'da current center kullan, editor'da hesapla
        Vector3 centerPos = Application.isPlaying ? currentCenter : CalculateCenterFromPoints();

        // Merkez noktası
        Gizmos.color = centerColor;
        Gizmos.DrawWireSphere(centerPos, 0.2f);
        Gizmos.DrawSphere(centerPos, 0.1f);

        // Control point'lerden merkeze çizgiler
        if (controlPoints != null)
        {
            Gizmos.color = lineColor;
            foreach (Transform point in controlPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawLine(point.position, centerPos);
                }
            }
        }

        // Max distance circle
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        DrawCircle(centerPos, maxDistanceFromCenter, 32);
    }

    void DrawCircle(Vector3 center, float radius, int segments)
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
=== KURULUM ADIMLARI ===

1. BOŞ GAMEOBJECT OLUŞTUR:
   - İsim: "SlimeBody"
   - Bu script'i ekle (SlimePhysics)

2. CONTROL POINT'LERİ EKLE:
   - SlimeBody'ye 3-6 adet child GameObject ekle
   - İsimleri: "ControlPoint_1", "ControlPoint_2" vs
   - Her birine CircleCollider2D ekle (radius: 0.15)
   - Her birine SlimeControlPoint script'i ekle
   - Üçgen/daire formunda yerleştir (el ile veya script ile)

3. SCRIPTLERI BAĞLA:
   - SlimePhysics component'inde "Control Points" array'ini genişlet
   - Child control point'leri sürükle

4. AYARLARI DÜZENLE:
   - Max Distance From Center: 3 (istediğin uzaklık)
   - Center Smooth Speed: 0.1 (ne kadar smooth kayacak)

5. PLAY!
   - Control point'lere tıklayıp sürükle
   - Merkez otomatik yeniden hesaplanıyor
   - Gizmos ile sistem görünüyor

=== SONRAKI ADIMLAR ===
- Görsel katman (mesh generation) eklenebilir
- Mobile touch input desteği
- Control point'leri runtime'da ekleme/çıkarma
- Spring/damping efektleri
*/

}
