namespace PassLine.Assets.Scripts
{
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemController : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference pointAction;
    public InputActionReference pressAction;

    [Header("Launch Settings")]
    public float minLaunchForce = 5f;
    public float maxLaunchForce = 20f;
    public float maxDragDistance = 2f;
    public float minDragDistance = 0.3f;
    public float velocityMultiplier = 2f;

    [Header("Force Balance")]
    [Range(0f, 1f)]
    public float distanceWeight = 0.6f; // %60 distance
    [Range(0f, 1f)]
    public float velocityWeight = 0.4f; // %40 velocity

    [Header("Visual")]
    public LineRenderer trajectoryLine;
    public Gradient trajectoryColorGradient;

    private Camera cam;
    private Item selectedItem;
    private Vector2 dragStartPos;
    private float dragStartTime;
    private bool isDragging = false;

    private void OnEnable()
    {
        pressAction.action.started += OnPressStarted;
        pressAction.action.canceled += OnPressCanceled;

        pressAction.action.Enable();
        pointAction.action.Enable();
    }

    private void OnDisable()
    {
        pressAction.action.started -= OnPressStarted;
        pressAction.action.canceled -= OnPressCanceled;

        pressAction.action.Disable();
        pointAction.action.Disable();
    }

    private void Start()
    {
        cam = Camera.main;
        trajectoryLine.enabled = false;

        // Default gradient oluştur
        if (trajectoryColorGradient == null)
        {
            trajectoryColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.yellow, 0f);
            colorKeys[1] = new GradientColorKey(Color.red, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            trajectoryColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    private void Update()
    {
        if (isDragging && selectedItem != null)
        {
            UpdateDrag();
        }
    }

    private Vector2 GetPointerWorldPos()
    {
        Vector2 screen = pointAction.action.ReadValue<Vector2>();
        Vector2 world = cam.ScreenToWorldPoint(screen);
        return world;
    }

    private void OnPressStarted(InputAction.CallbackContext ctx)
    {
        TrySelectItem();
    }

    private void OnPressCanceled(InputAction.CallbackContext ctx)
    {
        if (isDragging && selectedItem != null)
            LaunchItem();
    }

    private void TrySelectItem()
    {
        Vector2 worldPos = GetPointerWorldPos();
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (!hit.collider)
        {
            return;
        }

        Item item = hit.collider.GetComponent<Item>();
        if (item == null)
        {
            return;
        }

        if (item.IsMoving())
        {
            return;
        }

        selectedItem = item;
        selectedItem.SetSelected(true);

        dragStartPos = worldPos;
        dragStartTime = Time.time;
        isDragging = true;

        trajectoryLine.enabled = true;

        if (ItemManager.Instance != null)
            ItemManager.Instance.OnItemSelected(item);
    }

    private void UpdateDrag()
    {
        Vector2 currentPos = GetPointerWorldPos();
        Vector2 dragVector = currentPos - dragStartPos;
        float dragDistance = dragVector.magnitude;

        // Max distance limiti
        if (dragDistance > maxDragDistance)
        {
            dragVector = dragVector.normalized * maxDragDistance;
            dragDistance = maxDragDistance;
        }

        // Kuvvet hesapla
        float calculatedForce = CalculateLaunchForce(dragDistance);

        // Trajectory çiz
        DrawTrajectory(selectedItem.transform.position, dragVector.normalized * calculatedForce, calculatedForce);
    }

    private void LaunchItem()
    {
        Vector2 currentPos = GetPointerWorldPos();
        Vector2 dragVector = currentPos - dragStartPos;
        float dragDistance = dragVector.magnitude;

        // Minimum drag kontrolü
        if (dragDistance < minDragDistance)
        {
            Debug.Log("Drag distance too small, not launching");
            CancelDrag();
            return;
        }

        // Max distance limiti
        if (dragDistance > maxDragDistance)
        {
            dragVector = dragVector.normalized * maxDragDistance;
            dragDistance = maxDragDistance;
        }

        // Kuvvet hesapla
        float finalForce = CalculateLaunchForce(dragDistance);

        Debug.Log($"Launch - Distance: {dragDistance:F2}, Force: {finalForce:F2}");

        selectedItem.Launch(dragVector.normalized, finalForce);

        trajectoryLine.enabled = false;
        isDragging = false;
        selectedItem = null;
    }

    private float CalculateLaunchForce(float dragDistance)
    {
        // Distance bazlı kuvvet (0-1 normalize)
        float normalizedDistance = Mathf.Clamp01(dragDistance / maxDragDistance);
        float distanceForce = Mathf.Lerp(0f, maxLaunchForce, normalizedDistance);

        // Velocity bazlı bonus
        float dragDuration = Time.time - dragStartTime;
        float dragVelocity = dragDistance / Mathf.Max(dragDuration, 0.01f); // Sıfıra bölme koruması

        // Velocity'yi normalize et (örnek: 10 birim/saniye maksimum olarak kabul edelim)
        float normalizedVelocity = Mathf.Clamp01(dragVelocity / 10f);
        float velocityBonus = normalizedVelocity * maxLaunchForce;

        // İki kuvveti weight'lere göre birleştir
        float combinedForce = (distanceForce * distanceWeight) + (velocityBonus * velocityWeight);

        // Final kuvveti min-max arasında clamp et
        float finalForce = Mathf.Clamp(combinedForce, minLaunchForce, maxLaunchForce);

        return finalForce;
    }

    private void CancelDrag()
    {
        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
        }

        trajectoryLine.enabled = false;
        isDragging = false;
        selectedItem = null;
    }

    private void DrawTrajectory(Vector2 start, Vector2 velocity, float force)
    {
        int steps = 20;
        float stepTime = 0.05f;
        float drag = 2f; // Item'daki drag ile aynı

        trajectoryLine.positionCount = steps;

        // Kuvvete göre renk
        float forceRatio = Mathf.InverseLerp(minLaunchForce, maxLaunchForce, force);
        Color trajectoryColor = trajectoryColorGradient.Evaluate(forceRatio);
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor;

        Vector2 currentPos = start;
        Vector2 currentVel = velocity;

        for (int i = 0; i < steps; i++)
        {
            trajectoryLine.SetPosition(i, currentPos);
            currentVel *= (1 - drag * stepTime);
            currentPos += currentVel * stepTime;
        }
    }
}
}

