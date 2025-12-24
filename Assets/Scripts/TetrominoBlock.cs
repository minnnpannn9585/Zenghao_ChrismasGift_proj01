using UnityEngine;

public class TetrominoBlock : MonoBehaviour
{
    [HideInInspector] public LetterType letterType;
    public float fallSpeed = 1f;

    // New: assign the layer(s) considered obstacles in the Inspector
    public LayerMask obstacleLayer;
    
    private int gridX, gridY;
    private float fallTimer;
    private bool isLanded;

    private void Update()
    {
        if (isLanded) return;

        fallTimer += Time.deltaTime;
        if (fallTimer >= fallSpeed)
        {
            MoveDown();
            fallTimer = 0;
        }

        HandlePlayerInput();
    }

    private void HandlePlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveLeft();
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveRight();
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveDown();
    }

    // Raycast helper: returns true if a collider on obstacleLayer is between this block and the adjacent cell in dir.
    private bool IsBlocked(Vector2 dir)
    {
        if (GridManager.Instance == null) return false;
        float cell = GridManager.Instance.cellSize;
        // cast almost one cell length to detect obstacles in the neighboring cell
        float distance = cell * 0.9f;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, dir, distance, obstacleLayer);
        // Optional: debug draw
        // Debug.DrawRay(transform.position, dir * distance, hit.collider != null ? Color.red : Color.green, 0.5f);
        return hit.collider != null;
    }

    public void MoveLeft()
    {
        // prevent movement if a physics obstacle is to the left
        if (IsBlocked(Vector2.left)) return;

        int newX = gridX - 1;
        if (GridManager.Instance.IsCellEmpty(newX, gridY))
        {
            GridManager.Instance.RemoveBlock(gridX, gridY);
            GridManager.Instance.AddBlock(newX, gridY, letterType, this);
            gridX = newX;
            transform.position = GridManager.Instance.GridToWorldPosition(gridX, gridY);
        }
    }

    public void MoveRight()
    {
        // prevent movement if a physics obstacle is to the right
        if (IsBlocked(Vector2.right)) return;

        int newX = gridX + 1;
        if (GridManager.Instance.IsCellEmpty(newX, gridY))
        {
            GridManager.Instance.RemoveBlock(gridX, gridY);
            GridManager.Instance.AddBlock(newX, gridY, letterType, this);
            gridX = newX;
            transform.position = GridManager.Instance.GridToWorldPosition(gridX, gridY);
        }
    }

    // Changed: gridY increases to move down (GridManager uses Y=0 at top, larger Y → lower on screen)
    public void MoveDown()
    {
        int newY = gridY + 1; // Y+1 → move down
        if (GridManager.Instance.IsCellEmpty(gridX, newY))
        {
            GridManager.Instance.RemoveBlock(gridX, gridY);
            GridManager.Instance.AddBlock(gridX, newY, letterType, this);
            gridY = newY;
            transform.position = GridManager.Instance.GridToWorldPosition(gridX, gridY);
        }
        else
        {
            Land();
        }
    }

    private void Land()
    {
        isLanded = true;
        GetComponent<AudioSource>().Play();
        GridManager.Instance.CheckForElimination();
        if (GridManager.Instance.IsGameOver())
            GameManager.Instance.GameOver();
        else
            SpawnManager.Instance.SpawnNewBlock();
    }

    public void SetGridPosition(int x, int y)
    {
        // Defensive and atomic update:
        // - Only remove previous mapping if previous coords were in-bounds,
        //   previous != new coords, and the mapping at previous coords points to this object.
        // - Ensure the target cell is registered to this object after the update.
        if (GridManager.Instance != null)
        {
            bool prevInBounds = gridX >= 0 && gridX < GridManager.Instance.gridWidth
                                && gridY >= 0 && gridY < GridManager.Instance.gridHeight;
        }

        if (GridManager.Instance != null)
        {
            bool prevInBounds = gridX >= 0 && gridX < GridManager.Instance.gridWidth
                                && gridY >= 0 && gridY < GridManager.Instance.gridHeight;

            if (prevInBounds && (gridX != x || gridY != y))
            {
                var existing = GridManager.Instance.GetBlockAtGridPosition(gridX, gridY);
                if (existing == this)
                    GridManager.Instance.UnregisterBlock(gridX, gridY);
            }
        }

        gridX = x;
        gridY = y;

        // Ensure the target position is registered to this object.
        if (GridManager.Instance != null)
        {
            var existingAtNew = GridManager.Instance.GetBlockAtGridPosition(x, y);
            if (existingAtNew != this)
                GridManager.Instance.RegisterBlock(this, x, y);
            transform.position = GridManager.Instance.GridToWorldPosition(x, gridY);
        }
    }

    public void Initialize(int x, int y, LetterType letter)
    {
        letterType = letter;
        gridX = x;
        gridY = y;
        GridManager.Instance.AddBlock(x, y, letter, this);
        transform.position = GridManager.Instance.GridToWorldPosition(x, y);
        fallTimer = 0;
        isLanded = false;

        // Optional: show letter
        // TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        // if (text != null) text.text = letter.ToString();
    }

    private void OnDestroy()
    {
        // Only unregister if the mapping at our stored coords points to this instance.
        if (GridManager.Instance == null) return;

        bool inBounds = gridX >= 0 && gridX < GridManager.Instance.gridWidth
                        && gridY >= 0 && gridY < GridManager.Instance.gridHeight;
        if (!inBounds) return;

        var existing = GridManager.Instance.GetBlockAtGridPosition(gridX, gridY);
        if (existing == this)
            GridManager.Instance.UnregisterBlock(gridX, gridY);
    }
}