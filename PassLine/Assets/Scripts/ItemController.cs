namespace PassLine.Assets.Scripts
{
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemController : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference pointAction;
    public InputActionReference pressAction;

    [Header("Settings")]
    public float launchForce = 10f;
    public float maxDragDistance = 2f;
    public LineRenderer trajectoryLine;

    private Camera cam;
    private Item selectedItem;
    private Vector2 dragStartPos;
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
        isDragging = true;

        trajectoryLine.enabled = true;


        if (ItemManager.Instance != null)
            ItemManager.Instance.OnItemSelected(item);

    }

    private void UpdateDrag()
    {
        Vector2 pos = GetPointerWorldPos();
        Vector2 drag = dragStartPos - pos;

        if (drag.magnitude > maxDragDistance)
            drag = drag.normalized * maxDragDistance;

        DrawTrajectory(selectedItem.transform.position, drag.normalized * launchForce);
    }

    private void LaunchItem()
    {
        Vector2 pos = GetPointerWorldPos();
        Vector2 drag = dragStartPos - pos;

        if (drag.magnitude > maxDragDistance)
            drag = drag.normalized * maxDragDistance;

        selectedItem.Launch(drag, launchForce);

        trajectoryLine.enabled = false;
        isDragging = false;
        selectedItem = null;
    }

    private void DrawTrajectory(Vector2 start, Vector2 force)
    {
        int steps = 20;
        float stepTime = 0.05f;

        trajectoryLine.positionCount = steps;

        Vector2 velocity = force;
        Vector2 prev = start;

        for (int i = 0; i < steps; i++)
        {
            float t = i * stepTime;
            Vector2 pos = start + velocity * t + 0.5f * Physics2D.gravity * (t * t);

            trajectoryLine.SetPosition(i, pos);
            prev = pos;
        }
    }

}

}
