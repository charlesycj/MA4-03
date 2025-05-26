using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    /// 점수 계산: 플레이어의 기본 점수(돌 수) + 연속 보너스
    public int CalculateScore(TurnManager.Player player, Dictionary<Vector3Int, TurnManager.Player> board)
    {
        int totalScore = 0;

        // 기본 점수: 돌의 개수
        int stoneCount = board.Count(kvp => kvp.Value == player);
        totalScore += stoneCount;

        // 보너스 점수 계산
        totalScore += CalculateBonusPoints( player, board);

        return totalScore;
    }


    /// 연속된 돌에 대한 보너스 점수 계산
    private int CalculateBonusPoints(TurnManager.Player player, Dictionary<Vector3Int, TurnManager.Player> board)
    {
        int bonus = 0;
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),  // →
            new Vector2Int(0, 1),  // ↓
            new Vector2Int(1, 1),  // ↘
            new Vector2Int(1, -1)  // ↗
        };

        foreach (var kvp in board)
        {
            if (kvp.Value != player) continue;

            Vector3Int start = kvp.Key;

            foreach (var dir in directions)
            {
                int count = 1;
                Vector3Int next = start + new Vector3Int(dir.x, dir.y, 0);

                while (board.TryGetValue(next, out TurnManager.Player p) && p == player)
                {
                    count++;
                    next += new Vector3Int(dir.x, dir.y, 0);
                }

                if (count >= 3)
                {
                    switch (count)
                    {
                        case 3: bonus += 1; break;
                        case 4: bonus += 3; break;
                        case 5:
                        default: bonus += 5; break;
                    }
                }
            }
        }

        return bonus;
    }
}