using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    public Grid grid;
    public GameObject ghostPrefabp1;
    public GameObject ghostPrefabp2;
    public TurnManager turnManager;

    private GameObject ghostInstance;

    void Start()
    {
        if (grid == null)
            grid = FindObjectOfType<Grid>();

        ghostInstance = Instantiate(ghostPrefabp1);
        SetGhostTransparency(0.5f);
    }

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;

        // 카메라의 Z값을 기준으로 보정
        mouseScreenPos.z = -Camera.main.transform.position.z;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f; // 2D이므로 Z 고정

        Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);

        if (IsWithinBoard(cellPos)&&turnManager.isGameEnd==false)
        {
            ghostInstance.SetActive(true);
            ghostInstance.transform.position = grid.GetCellCenterWorld(cellPos);

            if (Input.GetMouseButtonDown(0)&&turnManager.isGameEnd==false) // Player1
            {
                turnManager.SubmitMove(TurnManager.Player.Player1, cellPos);
            }
            else if (Input.GetMouseButtonDown(1)&&turnManager.isGameEnd==false) // Player2
            {
                turnManager.SubmitMove(TurnManager.Player.Player2, cellPos);
            }
        }
        else
        {
            ghostInstance.SetActive(false);
        }
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