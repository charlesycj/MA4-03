using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Score Display")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    
    [Header("Turn Display")]
    public TextMeshProUGUI turnText;
    public Image player1TurnIndicator;
    public Image player2TurnIndicator;
    public TextMeshProUGUI infoText;
    [Header("Game End")]
    public GameObject gameEndPanel;
    public TextMeshProUGUI winnerText;
    public Button restartButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);
    }

    public void UpdateUI(int score1, int score2, bool isPlayer1Turn, bool isGameOver)
    {
        if (player1ScoreText != null) 
            player1ScoreText.text = $"Player 1: {score1}";
        if (player2ScoreText != null) 
            player2ScoreText.text = $"Player 2: {score2}";
        
        if (turnText != null) 
            turnText.text = isPlayer1Turn ? "Player 1's Turn" : "Player 2's Turn";
        if (player1TurnIndicator != null) 
            player1TurnIndicator.color = isPlayer1Turn ? Color.green : Color.gray;
        if (player2TurnIndicator != null) 
            player2TurnIndicator.color = !isPlayer1Turn ? Color.green : Color.gray;

        if (isGameOver && gameEndPanel != null)
        {
            gameEndPanel.SetActive(true);
            if (winnerText != null)
            {
                if (score1 > score2)
                    winnerText.text = "Player 1 Wins!";
                else if (score1 < score2)
                    winnerText.text = "Player 2 Wins!";
                else
                    winnerText.text = "Draw!";
            }
        }
    }

    public void UpdatePlayerScore(int playerId, int score)
    {
        if (playerId == 1 && player1ScoreText != null)
            player1ScoreText.text = $"Player 1: {score}";
        else if (playerId == 2 && player2ScoreText != null)
            player2ScoreText.text = $"Player 2: {score}";
    }
    
}