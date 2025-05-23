using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public enum Player { Player1, Player2 }

    public GameObject piecePrefabP1;
    public GameObject piecePrefabP2;
    public Grid grid;

    private Vector3Int? player1PlannedMove = null;
    private Vector3Int? player2PlannedMove = null;

    private bool isPlayer1Attacker = true;
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

    public void SubmitMove(Player player, Vector3Int cellPos)
    {
        if (!IsWithinBoard(cellPos))
        {
            Debug.Log("보드 범위 밖입니다.");
            return;
        }

        if (occupiedPositions.Contains(cellPos))
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

        if (pos1 == pos2)
        {
            if (isPlayer1Attacker)
                PlacePiece(piecePrefabP1, pos1);
            else
                PlacePiece(piecePrefabP2, pos2);
        }
        else
        {
            PlacePiece(piecePrefabP1, pos1);
            PlacePiece(piecePrefabP2, pos2);
        }

        player1PlannedMove = null;
        player2PlannedMove = null;
        isPlayer1Attacker = !isPlayer1Attacker;
    }

    private void PlacePiece(GameObject prefab, Vector3Int cell)
    {
        if (occupiedPositions.Contains(cell))
        {
            Debug.Log("이미 말이 놓인 위치입니다 (중복 검사 실패): " + cell);
            return;
        }

        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        Instantiate(prefab, worldPos, Quaternion.identity);
        occupiedPositions.Add(cell);
    }

    private bool IsWithinBoard(Vector3Int pos)
    {
        return pos.x >= -2 && pos.x <= 2 && pos.y >= -2 && pos.y <= 2;
    }
}
