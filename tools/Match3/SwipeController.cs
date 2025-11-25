using UnityEngine;

public class SwipeController : MonoBehaviour
{
    public Match3Board board; // reference to the board
    public Camera cam; // camera used to convert screen->world

    [Tooltip("Minimum swipe distance in world units to consider a swipe")]
    public float minSwipeDistance = 0.2f;

    private Vector2 startWorld;
    private Vector2Int startCell;
    private bool isDragging = false;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        //if (board == null) board = FindObjectOfType<Match3Board>();
    }

    void Update()
    {
        // Touch (mobile)
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                OnPointerDown(t.position);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                OnPointerUp(t.position);
            }
        }
        else // Mouse (editor/standalone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                OnPointerUp(Input.mousePosition);
            }
        }
    }

    void OnPointerDown(Vector2 screenPos)
    {
        if (board == null || cam == null) return;
        Vector3 wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        startWorld = new Vector2(wp.x, wp.y);
        startCell = WorldToGrid(startWorld);

        // Ensure start cell is inside board
        if (startCell.x < 0 || startCell.y < 0 || startCell.x >= board.width || startCell.y >= board.height)
        {
            isDragging = false;
            return;
        }

        isDragging = true;
    }

    void OnPointerUp(Vector2 screenPos)
    {
        if (!isDragging || board == null || cam == null) return;
        Vector3 wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        Vector2 endWorld = new Vector2(wp.x, wp.y);
        Vector2 delta = endWorld - startWorld;

        if (delta.magnitude < minSwipeDistance)
        {
            // Too short - treat as tap (no action)
            isDragging = false;
            return;
        }

        Vector2Int target = startCell;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // horizontal swipe
            target.x += delta.x > 0 ? 1 : -1;
        }
        else
        {
            // vertical swipe
            target.y += delta.y > 0 ? -1 : 1;
        }

        // Clamp target to board range
        if (target.x < 0 || target.y < 0 || target.x >= board.width || target.y >= board.height)
        {
            isDragging = false;
            return;
        }

        // Try swap on board
        board.TrySwap(startCell, target);

        isDragging = false;
    }

    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        // Convert world position to grid indices using board's origin and cell size
        // Match3Board now uses boardOrigin as top-left and GridToWorld: y_world = boardOrigin.y - y*cellSize
        float localX = worldPos.x - board.boardOrigin.x;
        float localY = board.boardOrigin.y - worldPos.y; // invert Y relative to top-left origin
        int gx = Mathf.RoundToInt(localX / board.cellSize);
        int gy = Mathf.RoundToInt(localY / board.cellSize);
        return new Vector2Int(gx, gy);
    }
}
