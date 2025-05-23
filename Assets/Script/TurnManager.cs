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

        if (occupiedPositions.Count >= 25)
        {
            Debug.Log("게임 종료! 모든 셀에 돌이 놓였습니다.");
            isGameEnd = true;

            int scoreP1 = CalculateScore(Player.Player1, occupiedPositions);
            int scoreP2 = CalculateScore(Player.Player2, occupiedPositions);

            Debug.Log($"Player1 점수: {scoreP1}");
            Debug.Log($"Player2 점수: {scoreP2}");
        }
    }

public int CalculateScore(Player player, Dictionary<Vector3Int, Player> board)
{
    int totalScore = 0;

    int stoneCount = board.Count(kvp => kvp.Value == player);
    totalScore += stoneCount;

    Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),  // →
        new Vector2Int(0, 1),  // ↓
        new Vector2Int(1, 1),  // ↘
        new Vector2Int(1, -1)  // ↗
    };

    // 이미 점수로 인정된 줄 목록 (각 줄은 돌 위치 리스트)
    List<List<Vector3Int>> scoredLines = new List<List<Vector3Int>>();

    foreach (var length in new int[] { 5, 4, 3 })
    {
        foreach (var kvp in board)
        {
            if (kvp.Value != player)
                continue;

            Vector3Int start = kvp.Key;

            foreach (var dir in directions)
            {
                List<Vector3Int> sequence = new List<Vector3Int>();
                Vector3Int current = start;

                while (board.TryGetValue(current, out Player p) && p == player)
                {
                    sequence.Add(current);
                    current += new Vector3Int(dir.x, dir.y, 0);
                }

                if (sequence.Count >= length)
                {
                    for (int i = 0; i <= sequence.Count - length; i++)
                    {
                        var subSeq = sequence.GetRange(i, length);

                        // 기존에 점수로 인정된 줄들과 비교해서 겹치는 칸 개수 세기
                        bool canScore = true;
                        foreach (var scoredLine in scoredLines)
                        {
                            int overlapCount = subSeq.Intersect(scoredLine).Count();
                            if (overlapCount > 1) // 1칸 초과 겹침이면 중복 점수 불가
                            {
                                canScore = false;
                                break;
                            }
                        }

                        if (!canScore) continue;

                        switch (length)
                        {
                            case 3: totalScore += 1; break;
                            case 4: totalScore += 3; break;
                            case 5: totalScore += 5; break;
                        }

                        scoredLines.Add(subSeq);
                    }
                }
            }
        }
    }

    return totalScore;
}


// // 줄을 고유하게 식별하기 위한 문자열 생성 함수
//     private string GetLineKey(List<Vector3Int> positions)
//     {
//         positions.Sort((a, b) =>
//         {
//             int cmpY = a.y.CompareTo(b.y);
//             return cmpY != 0 ? cmpY : a.x.CompareTo(b.x);
//         });
//
//         return string.Join("-", positions.Select(p => $"{p.x},{p.y}"));
//     }

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
