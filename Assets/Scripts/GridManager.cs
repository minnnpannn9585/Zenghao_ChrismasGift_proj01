using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int gridWidth = 8;
    public int gridHeight = 15;
    public float cellSize = 1f;

    private LetterType[,] grid;
    // 网格原点：左上角（X居中，Y为网格顶部）
    private Vector2 gridOrigin;
    private Dictionary<Vector2Int, TetrominoBlock> gridToBlock = new Dictionary<Vector2Int, TetrominoBlock>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        grid = new LetterType[gridWidth, gridHeight];
        ResetGrid();

        // ========== 核心修改1：重新计算网格原点 ==========
        // 让网格水平居中，垂直方向Y=0对应屏幕上方（网格顶部），Y=14对应屏幕下方（网格底部）
        float gridHalfWidth = (gridWidth * cellSize) / 2f;
        gridOrigin = new Vector2(
            transform.position.x - gridHalfWidth + cellSize / 2f, // X居中
            transform.position.y // Y从当前位置开始（顶部）
        );
    }

    public void ResetGrid()
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                grid[x, y] = LetterType.None;
        gridToBlock.Clear();
    }

    // ========== 核心修改2：网格坐标 → 世界坐标 ==========
    // 网格Y=0 → 世界坐标最高（顶部），网格Y越大 → 世界坐标越低（向下）
    public Vector2 GridToWorldPosition(int x, int y)
    {
        return new Vector2(
            gridOrigin.x + x * cellSize, // X轴正常向右
            gridOrigin.y - y * cellSize  // Y轴：网格Y越大，世界Y越小（向下移动）
        );
    }

    // ========== 核心修改3：世界坐标 → 网格坐标 ==========
    public bool WorldToGridPosition(Vector2 worldPos, out int x, out int y)
    {
        // 反向计算，确保世界坐标转网格坐标正确
        x = Mathf.RoundToInt((worldPos.x - gridOrigin.x) / cellSize);
        y = Mathf.RoundToInt((gridOrigin.y - worldPos.y) / cellSize);
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    // 以下方法仅微调消除和下落逻辑，其余完全保留
    public bool IsCellEmpty(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;
        return grid[x, y] == LetterType.None;
    }

    public bool AddBlock(int x, int y, LetterType letter, TetrominoBlock block)
    {
        if (!IsCellEmpty(x, y)) return false;
        
        grid[x, y] = letter;
        RegisterBlock(block, x, y);
        block.SetGridPosition(x, y);
        return true;
    }

    public void RemoveBlock(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;
        grid[x, y] = LetterType.None;
        UnregisterBlock(x, y);
    }

    public void RegisterBlock(TetrominoBlock block, int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (gridToBlock.ContainsKey(pos)) gridToBlock[pos] = block;
        else gridToBlock.Add(pos, block);
    }

    public void UnregisterBlock(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (gridToBlock.ContainsKey(pos)) gridToBlock.Remove(pos);
    }

    public TetrominoBlock GetBlockAtGridPosition(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        gridToBlock.TryGetValue(pos, out TetrominoBlock block);
        return block;
    }

    public void CheckForElimination()
    {
        // 遍历所有行（Y从0到14，0是顶部，14是底部）
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x <= gridWidth - 5; x++)
            {
                if (grid[x, y] == LetterType.A &&
                    grid[x+1, y] == LetterType.L &&
                    grid[x+2, y] == LetterType.I &&
                    grid[x+3, y] == LetterType.C &&
                    grid[x+4, y] == LetterType.E)
                {
                    EliminateBlocks(x, y, 5);
                    GameManager.Instance.AddScore(10);
                    DropBlocksAbove(y);
                }
            }
        }
    }

    private void EliminateBlocks(int startX, int y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int x = startX + i;
            TetrominoBlock block = GetBlockAtGridPosition(x, y);
            if (block != null)
            {
                RemoveBlock(x, y);
                Destroy(block.gameObject);
            }
        }
    }

    // ========== 核心修改4：消除后上方方块下落逻辑 ==========
    private void DropBlocksAbove(int eliminatedY)
    {
        // 遍历消除行上方的所有行（Y更小的行，即更靠近顶部的行）
        for (int y = eliminatedY - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != LetterType.None)
                {
                    // 找到可下落的最低位置（Y更大的方向，即向下）
                    int targetY = y;
                    while (targetY + 1 < gridHeight && IsCellEmpty(x, targetY + 1))
                        targetY++;

                    if (targetY != y)
                    {
                        TetrominoBlock block = GetBlockAtGridPosition(x, y);
                        if (block != null)
                        {
                            RemoveBlock(x, y);
                            AddBlock(x, targetY, grid[x, y], block);
                            block.transform.position = GridToWorldPosition(x, targetY);
                        }
                    }
                }
            }
        }
    }

    // ========== 核心修改5：游戏结束检测 ==========
    // 检测网格顶部行（Y=0）是否有方块
    public bool IsGameOver()
    {
        for (int x = 0; x < gridWidth; x++)
            if (grid[x, 0] != LetterType.None) return true; // Y=0是网格顶部
        return false;
    }

    // Gizmos绘制网格（可视化验证）
    private void OnDrawGizmos()
    {
        if (grid == null) return;
        Gizmos.color = Color.grey;

        // 计算网格实际范围
        float gridTotalWidth = gridWidth * cellSize;
        float gridTotalHeight = gridHeight * cellSize;
        Vector2 bottomRight = new Vector2(gridOrigin.x + gridTotalWidth, gridOrigin.y - gridTotalHeight);

        // 绘制网格外框
        Gizmos.DrawLine(gridOrigin, new Vector2(bottomRight.x, gridOrigin.y)); // 顶部横线
        Gizmos.DrawLine(gridOrigin, new Vector2(gridOrigin.x, bottomRight.y)); // 左侧竖线
        Gizmos.DrawLine(bottomRight, new Vector2(bottomRight.x, gridOrigin.y)); // 右侧竖线
        Gizmos.DrawLine(bottomRight, new Vector2(gridOrigin.x, bottomRight.y)); // 底部横线

        // 绘制内部网格线
        for (int x = 1; x < gridWidth; x++)
        {
            Vector2 start = new Vector2(gridOrigin.x + x * cellSize, gridOrigin.y);
            Vector2 end = new Vector2(gridOrigin.x + x * cellSize, bottomRight.y);
            Gizmos.DrawLine(start, end);
        }
        for (int y = 1; y < gridHeight; y++)
        {
            Vector2 start = new Vector2(gridOrigin.x, gridOrigin.y - y * cellSize);
            Vector2 end = new Vector2(bottomRight.x, gridOrigin.y - y * cellSize);
            Gizmos.DrawLine(start, end);
        }
    }
}