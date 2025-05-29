using UnityEngine;
using System.Collections.Generic;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class TurnManager : NetworkBehaviour
{
    public enum Player { Player1, Player2 }

    [Header("Prefabs")]
    public GameObject piecePrefabP1;
    public GameObject piecePrefabP2;

    [Header("Board")]
    public Grid grid;

    [Header("UI")]
    public TMP_Text score1;
    public TMP_Text score2;
    public TMP_Text wintext;
    public Toggle attackertoggle1;
    public Toggle attackertoggle2;

    [Header("References")]
    public ScoreManager scoreManager;

    [Header("Game State")]
    private Vector3Int? player1PlannedMove;
    private Vector3Int? player2PlannedMove;
    private bool isPlayer1Attacker = true;
    private readonly Dictionary<Vector3Int, Player> occupiedPositions = new();

    public bool isGameEnd = false;

    private void Awake()
    {
        // ScoreManager 연결
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager == null)
            Debug.LogError("ScoreManager를 찾을 수 없습니다!");
    }

    public TurnManager.Player GetCurrentPlayer()
    {
        return isPlayer1Attacker ? Player.Player1 : Player.Player2;
    }
    public void SubmitMove(Player player, Vector3Int cellPos)
    {
        if (!IsWithinBoard(cellPos))
        {
            Debug.LogWarning("보드 범위 밖입니다.");
            return;
        }

        if (occupiedPositions.ContainsKey(cellPos))
        {
            Debug.LogWarning($"이미 말이 놓인 위치입니다: {cellPos}");
            return;
        }

        // 각 플레이어 입력 처리
        if (player == Player.Player1 && player1PlannedMove == null)
        {
            player1PlannedMove = cellPos;
            Debug.Log($"Player1 예약: {cellPos}");
        }
        else if (player == Player.Player2 && player2PlannedMove == null)
        {
            player2PlannedMove = cellPos;
            Debug.Log($"Player2 예약: {cellPos}");
        }

        // 둘 다 예약되면 처리
        if (player1PlannedMove != null && player2PlannedMove != null)
        {
            ResolveTurn();
        }
    }

    private void ResolveTurn()
    {
        Vector3Int pos1 = player1PlannedMove.Value;
        Vector3Int pos2 = player2PlannedMove.Value;

        bool isAttackSuccess = pos1 == pos2;

        if (isAttackSuccess)
        {
            // 공격 성공 - 공격자만 놓음
            if (isPlayer1Attacker)
                PlacePiece(piecePrefabP1, pos1);
            else
                PlacePiece(piecePrefabP2, pos2);

            Debug.Log("공격 성공! 공격권 유지");
        }
        else
        {
            // 공격 실패 - 둘 다 놓음, 공격권 교체
            PlacePiece(piecePrefabP1, pos1);
            PlacePiece(piecePrefabP2, pos2);

            isPlayer1Attacker = !isPlayer1Attacker;
            Debug.Log("공격 실패. 공격권 전환");
        }

        player1PlannedMove = null;
        player2PlannedMove = null;

        UpdateScoresAndUI();

        // 게임 종료 여부 체크
        if (occupiedPositions.Count >= 25)
        {
            EndGame();
        }
    }

    private void PlacePiece(GameObject prefab, Vector3Int cell)
    {
        if (occupiedPositions.ContainsKey(cell))
        {
            Debug.LogWarning($"중복 배치 시도: {cell}");
            return;
        }

        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        Instantiate(prefab, worldPos, Quaternion.identity);

        Player owner = prefab == piecePrefabP1 ? Player.Player1 : Player.Player2;
        occupiedPositions[cell] = owner;
    }

    private void UpdateScoresAndUI()
    {
        int scoreP1 = scoreManager.CalculateScore(Player.Player1, occupiedPositions);
        int scoreP2 = scoreManager.CalculateScore(Player.Player2, occupiedPositions);

        score1.SetText($"score : {scoreP1}");
        score2.SetText($"score : {scoreP2}");

        Debug.Log($"[턴 종료 후 점수] Player1: {scoreP1} / Player2: {scoreP2}");

        // 공격권 토글 UI 갱신
        attackertoggle1.interactable = isPlayer1Attacker;
        attackertoggle2.interactable = !isPlayer1Attacker;
    }

    private void EndGame()
    {
        isGameEnd = true;

        int scoreP1 = scoreManager.CalculateScore(Player.Player1, occupiedPositions);
        int scoreP2 = scoreManager.CalculateScore(Player.Player2, occupiedPositions);

        string winner = scoreP1 > scoreP2 ? "Winner is Player1" : "Winner is Player2";
        wintext.SetText(winner);

        Debug.Log($"게임 종료! 최종 점수 - Player1: {scoreP1}, Player2: {scoreP2}");
    }

    private bool IsWithinBoard(Vector3Int pos)
    {
        return pos.x >= -2 && pos.x <= 2 && pos.y >= -2 && pos.y <= 2;
    }
}
