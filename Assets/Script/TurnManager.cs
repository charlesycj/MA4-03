using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public enum Player { Player1, Player2 }

    public GameObject piecePrefabP1;
    public GameObject piecePrefabP2;
    public Grid grid;
    

    private Vector3Int? player1PlannedMove = null;
    private Vector3Int? player2PlannedMove = null;
    private bool isPlayer1Attacker = true;
    private Dictionary<Vector3Int, Player> occupiedPositions = new Dictionary<Vector3Int, Player>();

    public bool isGameEnd = false;

    public ScoreManager scoreManager;

    private void Awake()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager == null)
            Debug.LogError("ScoreManager를 찾을 수 없습니다!");
    }

    public void SubmitMove(Player player, Vector3Int cellPos)
    {
        if (!IsWithinBoard(cellPos))
        {
            Debug.Log("보드 범위 밖입니다.");
            return;
        }

        if (occupiedPositions.ContainsKey(cellPos))
        {
            Debug.Log("이미 말이 놓인 위치입니다: " + cellPos);
            return;
        }

        if (player == Player.Player1 && player1PlannedMove == null)
        {
            player1PlannedMove = cellPos;
            Debug.Log("Player1 좌표 예약: " + cellPos);
        }
        else if (player == Player.Player2 && player2PlannedMove == null)
        {
            player2PlannedMove = cellPos;
            Debug.Log("Player2 좌표 예약: " + cellPos);
        }

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
            if (isPlayer1Attacker)
                PlacePiece(piecePrefabP1, pos1);
            else
                PlacePiece(piecePrefabP2, pos2);

            Debug.Log("공격 성공! 공격권 유지");
        }
        else
        {
            PlacePiece(piecePrefabP1, pos1);
            PlacePiece(piecePrefabP2, pos2);

            Debug.Log("공격 실패. 공격권 전환");
            isPlayer1Attacker = !isPlayer1Attacker;
        }

        player1PlannedMove = null;
        player2PlannedMove = null;

        // ScoreManager를 통해 점수 계산
        int scoreP1 = scoreManager.CalculateScore(Player.Player1, occupiedPositions);
        int scoreP2 = scoreManager.CalculateScore(Player.Player2, occupiedPositions);
        Debug.Log($"[턴 종료 후 점수] Player1: {scoreP1} / Player2: {scoreP2}");

        if (occupiedPositions.Count >= 25)
        {
            Debug.Log("게임 종료! 모든 셀에 돌이 놓였습니다.");
            isGameEnd = true;

            Debug.Log($"최종 점수 - Player1: {scoreP1}, Player2: {scoreP2}");
        }
    }

    private void PlacePiece(GameObject prefab, Vector3Int cell)
    {
        if (occupiedPositions.ContainsKey(cell))
        {
            Debug.Log("이미 말이 놓인 위치입니다 (중복 검사 실패): " + cell);
            return;
        }

        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        Instantiate(prefab, worldPos, Quaternion.identity);

        Player owner = prefab == piecePrefabP1 ? Player.Player1 : Player.Player2;
        occupiedPositions[cell] = owner;
    }

    private bool IsWithinBoard(Vector3Int pos)
    {
        return pos.x >= -2 && pos.x <= 2 && pos.y >= -2 && pos.y <= 2;
    }
}