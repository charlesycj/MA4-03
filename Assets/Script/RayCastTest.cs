using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    public GameObject ghostPrefab;
    public GameObject piecePrefab;
    public Grid grid;

    private GameObject ghostInstance;

    // 보드 크기 (예: -2 ~ 2 범위 총 5칸)
    public int boardMinX = -2;
    public int boardMaxX = 2;
    public int boardMinY = -2;
    public int boardMaxY = 2;

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
        Vector3 cellCenterPos = grid.GetCellCenterWorld(cellPos);

        // 보드 범위 밖이면 숨김
        if (cellPos.x < boardMinX || cellPos.x > boardMaxX ||
            cellPos.y < boardMinY || cellPos.y > boardMaxY)
        {
            ghostInstance.SetActive(false);
            return;
        }
        else
        {
            ghostInstance.SetActive(true);
        }

        ghostInstance.transform.position = cellCenterPos;

        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(piecePrefab, cellCenterPos, Quaternion.identity);
        }
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
