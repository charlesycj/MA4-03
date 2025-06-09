using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI score1;
    [SerializeField] private TMPro.TextMeshProUGUI score2;
    [SerializeField] private TMPro.TextMeshProUGUI wintext;

    [SerializeField] private Toggle attackertoggle1;
    [SerializeField] private Toggle attackertoggle2;

    /// <summary>
    /// 점수 계산: 기본 점수 + 연속 보너스
    /// </summary>
    public int CalculateScore(int playerId, Dictionary<Vector3Int, int> placedPieces)
    {
        int totalScore = 0;
        
        // 기본 점수: 돌 개수
        int stoneCount = placedPieces.Count(kvp => kvp.Value == playerId);
        totalScore += stoneCount;

        // 보너스 점수
        totalScore += CalculateBonusPoints(playerId, placedPieces);
        Debug.Log($"total score :{totalScore}");
        Debug.Log($"stone count :{stoneCount}");
        Debug.Log($"Bonus count :{CalculateBonusPoints(playerId, placedPieces)}");
        return totalScore;
    }

    /// <summary>
    /// 연속된 돌에 대한 보너스 점수 계산
    /// </summary>
    private int CalculateBonusPoints(int playerId, Dictionary<Vector3Int, int> board)
    {
        int bonus = 0;
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // →
            new Vector2Int(0, 1),   // ↓
            new Vector2Int(1, 1),   // ↘
            new Vector2Int(1, -1)   // ↗
        };

        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        foreach (var kvp in board)
        {
            if (kvp.Value != playerId || visited.Contains(kvp.Key)) continue;

            Vector3Int start = kvp.Key;

            foreach (var dir in directions)
            {
                int count = 1;
                Vector3Int current = start + new Vector3Int(dir.x, dir.y, 0);

                // 연속된 돌 세기
                while (board.TryGetValue(current, out int val) && val == playerId)
                {
                    count++;
                    visited.Add(current);
                    current += new Vector3Int(dir.x, dir.y, 0);
                }

                // 보너스 점수 계산
                if (count >= 3)
                {
                    switch (count)
                    {
                        case 3: bonus += 1; break;  // 3개 연속: 1점
                        case 4: bonus += 3; break;  // 4개 연속: 3점
                        case 5: bonus += 5; break;  // 5개 연속: 5점
                        case 6: bonus += 12; break; // 6개 연속: 12점
                        default: bonus += 12; break; // 7개 이상: 12점
                    }
                }
            }

            visited.Add(start);
        }

        return bonus;
    }
}
