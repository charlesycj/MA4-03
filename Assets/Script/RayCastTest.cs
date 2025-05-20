using UnityEngine;
using System.Collections.Generic;

public class PiecePlacer : MonoBehaviour
{
    public GameObject ghostPrefab;
    public GameObject piecePrefab;
    public Grid grid;

    private GameObject ghostInstance;

    private HashSet<Vector3Int> placedPositions = new HashSet<Vector3Int>();

    //보드의 크기 5*5
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

        Vector3Int snappedCellPos = grid.WorldToCell(mouseWorldPos);

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
            if (!placedPositions.Contains(snappedCellPos))
            {
                Instantiate(piecePrefab, cellCenterPos, Quaternion.identity);
                placedPositions.Add(snappedCellPos);
            }
            else
            {
                Debug.Log("이미 말이 놓여 있는 위치입니다.");
            }
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
