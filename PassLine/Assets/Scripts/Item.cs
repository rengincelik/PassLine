
namespace PassLine.Assets.Scripts
{
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float growthMultiplier = 1.1f;

    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    private bool isSelected = false;
    private bool isMoving = false;
    private bool hasPassedThrough = false; // Geçiş kontrolü için

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
    }

    private void Start()
    {
        SetSelected(false);
    }

    private void Update()
    {
        if (isMoving && rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            isMoving = false;
            hasPassedThrough = false; // Durduğunda reset
        }
    }

    public void Launch(Vector2 direction, float force)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        isMoving = true;
        hasPassedThrough = false;
        SetSelected(false);
    }

    public void Grow()
    {
        transform.localScale *= growthMultiplier;
        Debug.Log($"{gameObject.name} grew to scale: {transform.localScale.x}");
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = selected ? selectedColor : normalColor;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool HasPassedThrough()
    {
        return hasPassedThrough;
    }

    public void SetPassedThrough(bool value)
    {
        hasPassedThrough = value;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public float GetRadius()
    {
        if (circleCollider != null)
        {
            return circleCollider.radius * transform.localScale.x;
        }
        return 0.5f * transform.localScale.x; // Default radius
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMoving) return;

        // Duvar veya başka item'a çarptı
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Item"))
        {
            Debug.Log($"Collision with {collision.gameObject.name} - Game Over!");
            GameManager.Instance.GameOver();
        }
    }
}

}



