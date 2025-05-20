using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    public GameObject ghostPrefab;
    public GameObject piecePrefab;
    public Grid grid;

   

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

        Vector3Int rawCellPos = grid.WorldToCell(mouseWorldPos);

        int boardCellX = rawCellPos.x / 2;
        int boardCellY = rawCellPos.y / 2;

        Vector3Int snappedCellPos = new Vector3Int(boardCellX * 2, boardCellY * 2, 0);
        Vector3 cellCenterPos = grid.GetCellCenterWorld(snappedCellPos) + new Vector3(1f, 1f, 0f);

        // 보드 영역 밖이면 고스트 숨기기
        if (boardCellX < -5 || boardCellX > 4||
            boardCellY < -4 || boardCellY > 5)
        {
            ghostInstance.SetActive(false);
            return;
        }
        else
        {
            ghostInstance.SetActive(true);
        }

        ghostInstance.transform.position = cellCenterPos;

        //if (ghostInstance != null)
        //    ghostInstance.transform.position = cellCenterPos;

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
