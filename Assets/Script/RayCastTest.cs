using Fusion;
using UnityEngine;

public class PiecePlacer : NetworkBehaviour
{
    public Grid grid;
    public GameObject ghostPrefabp1;
    public TurnManager turnManager;

    private GameObject ghostInstance;

    public override void Spawned() // Fusion 방식의 초기화 메서드
    {
        if (grid == null)
            grid = FindObjectOfType<Grid>();

        ghostInstance = Instantiate(ghostPrefabp1);
        SetGhostTransparency(0.5f);
    }

    void Update()
    {
        if (turnManager == null || turnManager.isGameEnd)
            return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);

        if (IsWithinBoard(cellPos))
        {
            ghostInstance.SetActive(true);
            ghostInstance.transform.position = grid.GetCellCenterWorld(cellPos);

            if (Input.GetMouseButtonDown(0))
            {
                // Fusion RPC 방식으로 서버에 말 놓기 요청
                RPC_SubmitMove(cellPos);
            }
        }
        else
        {
            ghostInstance.SetActive(false);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SubmitMove(Vector3Int cellPos, RpcInfo info = default)
    {
        turnManager.SubmitMove(turnManager.GetCurrentPlayer(), cellPos);
    }

    bool IsWithinBoard(Vector3Int pos)
    {
        return pos.x >= -2 && pos.x <= 2 && pos.y >= -2 && pos.y <= 2;
    }

    void SetGhostTransparency(float alpha)
    {
        var sr = ghostInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}