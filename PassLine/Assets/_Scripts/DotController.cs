using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference touchAction;
    [SerializeField] private InputActionReference touchPositionAction;
    
    [Header("Drag Settings")]
    [SerializeField] private float forceMultiplier = 15f;
    [SerializeField] private float maxDragDistance = 4f;
    [SerializeField] private float minDragDistance = 0.5f;
    
    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;
    
    [Header("Visual")]
    [SerializeField] private LineRenderer lineRenderer;
    
    private Vector2 dragStartWorldPos;
    private bool isDragging = false;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        
        // LineRenderer setup
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        touchAction.action.Enable();
        touchPositionAction.action.Enable();
        
        touchAction.action.started += OnTouchStarted;
        touchAction.action.canceled += OnTouchEnded;
    }

    private void OnDisable()
    {
        touchAction.action.started -= OnTouchStarted;
        touchAction.action.canceled -= OnTouchEnded;
        
        touchAction.action.Disable();
        touchPositionAction.action.Disable();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.isPlaying) return;
        
        Vector2 touchPos = touchPositionAction.action.ReadValue<Vector2>();
        Vector2 worldPos = mainCam.ScreenToWorldPoint(touchPos);
        
        // Objeye dokundu mu kontrol et
        float distance = Vector2.Distance(worldPos, transform.position);
        if (distance < 1f) // 1 unit içinde dokunmalı
        {
            isDragging = true;
            dragStartWorldPos = transform.position;
            
            lineRenderer.enabled = true;
            rb.linearVelocity = Vector2.zero; // Durduralım
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector2 currentTouchPos = touchPositionAction.action.ReadValue<Vector2>();
            Vector2 currentWorldPos = mainCam.ScreenToWorldPoint(currentTouchPos);
            
            // Çizgiyi güncelle
            UpdateLineRenderer(currentWorldPos);
        }
    }

    // private void UpdateLineRenderer(Vector2 currentWorldPos)
    // {
    //     // Obje pozisyonundan parmak pozisyonuna çizgi
    //     lineRenderer.SetPosition(0, dragStartWorldPos);
        
    //     Vector2 dragVector = currentWorldPos - dragStartWorldPos;
    //     float distance = dragVector.magnitude;
        
    //     // Max mesafe sınırla
    //     if (distance > maxDragDistance)
    //     {
    //         Vector2 direction = dragVector.normalized;
    //         lineRenderer.SetPosition(1, dragStartWorldPos + direction * maxDragDistance);
    //     }
    //     else
    //     {
    //         lineRenderer.SetPosition(1, currentWorldPos);
    //     }
    // }

private void UpdateLineRenderer(Vector2 currentWorldPos)
{
    Vector2 dragVector = currentWorldPos - dragStartWorldPos;
    float distance = dragVector.magnitude;
    
    // TERS YÖN - Obje bu yöne gidecek
    Vector2 launchDirection = -dragVector;
    
    // Başlangıç noktası
    lineRenderer.SetPosition(0, dragStartWorldPos);
    
    // Bitiş noktası (gideceği yön)
    if (distance > maxDragDistance)
    {
        // Max mesafe sınırlı
        Vector2 clampedDirection = launchDirection.normalized * maxDragDistance;
        lineRenderer.SetPosition(1, dragStartWorldPos + clampedDirection);
    }
    else
    {
        lineRenderer.SetPosition(1, dragStartWorldPos + launchDirection);
    }
}


    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (!isDragging) return;
        
        isDragging = false;
        lineRenderer.enabled = false;
        
        // Fırlatma hesabı
        Vector2 currentTouchPos = touchPositionAction.action.ReadValue<Vector2>();
        Vector2 currentWorldPos = mainCam.ScreenToWorldPoint(currentTouchPos);
        
        Vector2 dragVector = currentWorldPos - dragStartWorldPos;
        float dragDistance = dragVector.magnitude;
        
        // Minimum mesafe kontrolü
        if (dragDistance < minDragDistance)
        {
            Debug.Log("Drag too short!");
            return;
        }
        
        // Max mesafe sınırla
        dragDistance = Mathf.Min(dragDistance, maxDragDistance);
        
        // Fırlatma yönü (TERS yön)
        Vector2 launchDirection = -dragVector.normalized;
        float launchForce = dragDistance * forceMultiplier;
        
        // Fırlat!
        rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
        
        Debug.Log($"Launched! Force: {launchForce}");
    }
}
