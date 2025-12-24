using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public TetrominoBlock blockPrefab_S;
    public TetrominoBlock blockPrefab_H;
    public TetrominoBlock blockPrefab_A;
    public TetrominoBlock blockPrefab_R;
    public TetrominoBlock blockPrefab_O;
    public TetrominoBlock blockPrefab_N;

    public float initialSpawnDelay = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Invoke(nameof(SpawnNewBlock), initialSpawnDelay);
    }

    // Spawn at top row (Y = 0) and second column from left (X = 1). Fallback to X=0 if grid is too narrow.
    public void SpawnNewBlock()
    {
        if (GridManager.Instance.IsGameOver()) return;

        int spawnX = (GridManager.Instance.gridWidth > 1) ? 6 : 0; // second cell from left (0-based)
        int spawnY = 0; // top row

        LetterType randomLetter = (LetterType)Random.Range(1, 7);
        TetrominoBlock targetPrefab = GetPrefabByLetter(randomLetter);

        // Check spawn position is empty
        if (!GridManager.Instance.IsCellEmpty(spawnX, spawnY))
        {
            GameManager.Instance.GameOver();
            return;
        }

        TetrominoBlock newBlock = Instantiate(targetPrefab, transform);
        newBlock.Initialize(spawnX, spawnY, randomLetter);
    }

    private TetrominoBlock GetPrefabByLetter(LetterType letter)
    {
        switch (letter)
        {
            case LetterType.S: return blockPrefab_S;
            case LetterType.H: return blockPrefab_H;
            case LetterType.A: return blockPrefab_A;
            case LetterType.R: return blockPrefab_R;
            case LetterType.O: return blockPrefab_O;
            case LetterType.N: return blockPrefab_N;
            default: throw new System.Exception($"未找到字母{letter}对应的预制体！");
        }
    }
}