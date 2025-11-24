

namespace PassLine.Assets.Scripts
{
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Items")]
    [SerializeField] private Item[] items = new Item[3];

    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer passLineRenderer;
    [SerializeField] private float lineFeedbackDuration = 0.5f;
    [SerializeField] private Color passLineColor = Color.green;

    private Item selectedItem;
    private Item[] otherItems = new Item[2];

    private float lineDisplayTimer = 0f;
    private bool isLineVisible = false;

    // Geçiş kontrolü için
    private Vector2 previousPosition;
    private bool wasOnLeftSide;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        if (passLineRenderer == null)
        {
            GameObject lineObj = new GameObject("PassLine");
            passLineRenderer = lineObj.AddComponent<LineRenderer>();
        }

        passLineRenderer.startWidth = 0.1f;
        passLineRenderer.endWidth = 0.1f;
        passLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        passLineRenderer.startColor = passLineColor;
        passLineRenderer.endColor = passLineColor;
        passLineRenderer.positionCount = 2;
        passLineRenderer.enabled = false;
    }

    private void Update()
    {
        // Line feedback timer
        if (isLineVisible)
        {
            lineDisplayTimer -= Time.deltaTime;
            if (lineDisplayTimer <= 0f)
            {
                HidePassLine();
            }
        }

        // Dinamik geçiş kontrolü
        if (selectedItem != null && selectedItem.IsMoving() && !selectedItem.HasPassedThrough())
        {
            CheckPassThrough();
        }
    }

    public void OnItemSelected(Item item)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        selectedItem = item;

        // Diğer 2 item'ı bul
        int otherIndex = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != selectedItem)
            {
                otherItems[otherIndex] = items[i];
                otherIndex++;
            }
        }

        // Visual feedback göster
        ShowPassLine();

        // İlk pozisyonu kaydet
        previousPosition = selectedItem.GetPosition();

        // Başlangıç tarafını belirle
        wasOnLeftSide = IsOnLeftSide(previousPosition, otherItems[0].GetPosition(), otherItems[1].GetPosition());

    }

    private void ShowPassLine()
    {
        if (otherItems[0] != null && otherItems[1] != null)
        {
            passLineRenderer.SetPosition(0, otherItems[0].transform.position);
            passLineRenderer.SetPosition(1, otherItems[1].transform.position);
            passLineRenderer.enabled = true;

            isLineVisible = true;
            lineDisplayTimer = lineFeedbackDuration;
        }
    }

    private void HidePassLine()
    {
        passLineRenderer.enabled = false;
        isLineVisible = false;
    }

    private void CheckPassThrough()
    {
        Vector2 currentPosition = selectedItem.GetPosition();
        bool isOnLeftSide = IsOnLeftSide(currentPosition, otherItems[0].GetPosition(), otherItems[1].GetPosition());

        // Taraf değiştirdiyse = geçti
        if (isOnLeftSide != wasOnLeftSide)
        {
            // Çizgiyi geçerken item'lara çarpmadı mı kontrol et
            if (!HasCollidedWithOtherItems(previousPosition, currentPosition))
            {
                OnSuccessfulPass();
            }
        }

        previousPosition = currentPosition;
        wasOnLeftSide = isOnLeftSide;
    }

    private bool IsOnLeftSide(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        // Cross product ile nokta çizginin hangi tarafında kontrol
        return ((lineEnd.x - lineStart.x) * (point.y - lineStart.y) -
                (lineEnd.y - lineStart.y) * (point.x - lineStart.x)) > 0;
    }

    private bool HasCollidedWithOtherItems(Vector2 prevPos, Vector2 currentPos)
    {
        // İki pozisyon arasında line segment oluştur
        // Her bir other item'a olan minimum mesafeyi kontrol et

        foreach (Item otherItem in otherItems)
        {
            if (otherItem == null) continue;

            float distance = DistanceFromPointToLineSegment(
                otherItem.GetPosition(),
                prevPos,
                currentPos
            );

            float minDistance = selectedItem.GetRadius() + otherItem.GetRadius();

            if (distance < minDistance)
            {
                Debug.Log($"Too close to {otherItem.name}! Distance: {distance}, Min: {minDistance}");
                return true;
            }
        }

        return false;
    }

    private float DistanceFromPointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float lineLength = line.magnitude;

        if (lineLength == 0)
            return Vector2.Distance(point, lineStart);

        float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, line) / (lineLength * lineLength));
        Vector2 projection = lineStart + t * line;

        return Vector2.Distance(point, projection);
    }

    private void OnSuccessfulPass()
    {
        Debug.Log($"{selectedItem.name} successfully passed through!");

        selectedItem.SetPassedThrough(true);
        selectedItem.Grow();

        ScoreManager.Instance.AddScore(1);

        // Reset
        selectedItem = null;
    }

    // Debug görselleştirme
    private void OnDrawGizmos()
    {
        if (otherItems[0] != null && otherItems[1] != null && selectedItem != null && selectedItem.IsMoving())
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(otherItems[0].transform.position, otherItems[1].transform.position);

            // Item radius'larını göster
            Gizmos.color = Color.red;
            foreach (Item item in otherItems)
            {
                if (item != null)
                {
                    Gizmos.DrawWireSphere(item.transform.position, item.GetRadius());
                }
            }

            if (selectedItem != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(selectedItem.transform.position, selectedItem.GetRadius());
            }
        }
    }
}


}
