using UnityEngine;

public class TetrominoBlock : MonoBehaviour
{
    [HideInInspector] public LetterType letterType;
    public float fallSpeed = 1f;
    
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

    public void MoveLeft()
    {
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
        int newX = gridX + 1;
        if (GridManager.Instance.IsCellEmpty(newX, gridY))
        {
            GridManager.Instance.RemoveBlock(gridX, gridY);
            GridManager.Instance.AddBlock(newX, gridY, letterType, this);
            gridX = newX;
            transform.position = GridManager.Instance.GridToWorldPosition(gridX, gridY);
        }
    }

    // 【修改5】下落逻辑：Grid Y值递减（向底部Y=0移动）
    public void MoveDown()
    {
        int newY = gridY - 1; // Y-1 → 向底部移动
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
        GridManager.Instance.CheckForElimination();
        if (GridManager.Instance.IsGameOver())
            GameManager.Instance.GameOver();
        else
            SpawnManager.Instance.SpawnNewBlock();
    }

    public void SetGridPosition(int x, int y)
    {
        GridManager.Instance.UnregisterBlock(gridX, gridY);
        gridX = x;
        gridY = y;
        GridManager.Instance.RegisterBlock(this, x, y);
        transform.position = GridManager.Instance.GridToWorldPosition(x, gridY);
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

        // 可选：显示字母
        // TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        // if (text != null) text.text = letter.ToString();
    }

    private void OnDestroy()
    {
        GridManager.Instance.UnregisterBlock(gridX, gridY);
    }
}