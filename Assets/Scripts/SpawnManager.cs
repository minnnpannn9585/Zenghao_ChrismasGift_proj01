using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public TetrominoBlock blockPrefab_A;
    public TetrominoBlock blockPrefab_L;
    public TetrominoBlock blockPrefab_I;
    public TetrominoBlock blockPrefab_C;
    public TetrominoBlock blockPrefab_E;

    public float initialSpawnDelay = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        ValidatePrefabs();
    }

    private void Start()
    {
        Invoke(nameof(SpawnNewBlock), initialSpawnDelay);
    }

    // 【修改6】生成位置改为网格顶部行（Y=14，即gridHeight-1）
    public void SpawnNewBlock()
    {
        if (GridManager.Instance.IsGameOver()) return;

        int randomX = Random.Range(0, GridManager.Instance.gridWidth);
        int spawnY = GridManager.Instance.gridHeight - 1; // 顶部行（Y=14）

        LetterType randomLetter = (LetterType)Random.Range(1, 6);
        TetrominoBlock targetPrefab = GetPrefabByLetter(randomLetter);

        // 检查生成位置是否为空（顶部行是否被占）
        if (!GridManager.Instance.IsCellEmpty(randomX, spawnY))
        {
            GameManager.Instance.GameOver();
            return;
        }

        TetrominoBlock newBlock = Instantiate(targetPrefab, transform);
        newBlock.Initialize(randomX, spawnY, randomLetter);
    }

    private TetrominoBlock GetPrefabByLetter(LetterType letter)
    {
        switch (letter)
        {
            case LetterType.A: return blockPrefab_A;
            case LetterType.L: return blockPrefab_L;
            case LetterType.I: return blockPrefab_I;
            case LetterType.C: return blockPrefab_C;
            case LetterType.E: return blockPrefab_E;
            default: throw new System.Exception($"未找到字母{letter}对应的预制体！");
        }
    }

    private void ValidatePrefabs()
    {
        if (blockPrefab_A == null) Debug.LogError("SpawnManager: 未赋值A字母预制体！");
        if (blockPrefab_L == null) Debug.LogError("SpawnManager: 未赋值L字母预制体！");
        if (blockPrefab_I == null) Debug.LogError("SpawnManager: 未赋值I字母预制体！");
        if (blockPrefab_C == null) Debug.LogError("SpawnManager: 未赋值C字母预制体！");
        if (blockPrefab_E == null) Debug.LogError("SpawnManager: 未赋值E字母预制体！");
    }
}