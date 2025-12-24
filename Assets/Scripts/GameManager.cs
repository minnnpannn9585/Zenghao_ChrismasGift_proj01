using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Text scoreText;      // 分数文本
    public Text gameOverText;   // 游戏结束文本
    public Button restartButton;// 重启按钮

    private int score = 0;      // 当前分数
    private bool isGameOver = false;

    public GameObject[] stars;
    public GameObject win;
    public GameObject gift;
    public GameObject lose;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化UI
        gameOverText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        UpdateScoreUI();
        // 绑定重启按钮
        restartButton.onClick.AddListener(RestartGame);
    }

    // 加分
    public void AddScore(int points)
    {
        if (isGameOver) return;
        score += points;
        UpdateScoreUI();
        if (score == 9)
        {
            win.SetActive(true);
            gift.SetActive(true);
            return;
        }

        stars[score - 1].SetActive(false);
        stars[score].SetActive(true);
    }

    // 更新分数UI
    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    // 游戏结束
    public void GameOver()
    {
        isGameOver = true;
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        lose.SetActive(true);
        Invoke("RestartGame", 3f); // 3秒后自动重启
    }

    // 重启游戏
    public void RestartGame()
    {
        if(lose != null)
            lose.SetActive(false);
        // 重置状态
        score = 0;
        isGameOver = false;
        UpdateScoreUI();
        gameOverText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);

        // 销毁所有方块
        TetrominoBlock[] allBlocks = FindObjectsOfType<TetrominoBlock>();
        foreach (var block in allBlocks) Destroy(block.gameObject);

        // 重置网格
        GridManager.Instance.ResetGrid();

        // 生成新方块
        SpawnManager.Instance.SpawnNewBlock();
    }
}