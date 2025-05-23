using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    public Grid grid;
    public GameObject ghostPrefab;
    public TurnManager turnManager;

    private GameObject ghostInstance;

    void Start()
    {
        if (grid == null)
            grid = FindObjectOfType<Grid>();

        ghostInstance = Instantiate(ghostPrefab);
        SetGhostTransparency(0.5f);
    }

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10f;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);

        if (IsWithinBoard(cellPos))
        {
            ghostInstance.SetActive(true);
            ghostInstance.transform.position = grid.GetCellCenterWorld(cellPos);

            if (Input.GetMouseButtonDown(0)) // Player1
            {
                turnManager.SubmitMove(TurnManager.Player.Player1, cellPos);
            }
            else if (Input.GetMouseButtonDown(1)) // Player2
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
