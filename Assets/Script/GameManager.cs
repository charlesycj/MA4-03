using Fusion;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class GameManager : NetworkBehaviour
{
    [Header("Board Settings")]
    public Tilemap tilemap;
    [SerializeField] private GameObject redPiecePrefab;
    [SerializeField] private GameObject bluePiecePrefab;
    [SerializeField] private GameObject previewPiecePrefab;
    
    [Header("Game State")]
    [Networked] public int CurrentTurn { get; set; } = 1;
    [Networked] public bool GameEnded { get; set; } = false;
    [Networked] public int AttackPlayerId { get; set; } = 1;
    [Networked, Capacity(25)] public NetworkDictionary<Vector3Int, int> PlacedPieces { get; } = new NetworkDictionary<Vector3Int, int>();
    
    [Header("References")]
    public ScoreManager scoreManager;
    
    // Local state
    private Dictionary<int, GameObject> piecePrefabMap;
    private Dictionary<Vector3Int, int> localPlacedPieces = new Dictionary<Vector3Int, int>();
    private List<NetworkObject> placedPieceObjects = new List<NetworkObject>();
    private GameObject previewPiece;
    private Vector3Int? P1Input { get; set; }
    private Vector3Int? P2Input { get; set; }
    private Camera mainCamera;
    private UIManager uiManager;
    
    void Awake()
    {
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
        
        piecePrefabMap = new Dictionary<int, GameObject>
        {
            { 1, redPiecePrefab },
            { 2, bluePiecePrefab }
        };
        
        mainCamera = Camera.main;
        scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager == null)
            Debug.LogError("ScoreManager를 찾을 수 없습니다!");
            
        // Create preview piece
        previewPiece = Instantiate(previewPiecePrefab);
        previewPiece.SetActive(false);
    }
    
    public override void Spawned()
    {
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
        uiManager = UIManager.Instance;
        if (uiManager == null)
            Debug.LogError("UIManager를 찾을 수 없습니다!");
        
    }
    
    void Update()
    {
        if (!Object || !Object.IsValid) return;
        if (GameEnded) return;
        
        // Update preview piece position
        UpdatePreviewPiece();
        
    }
    
    private void UpdatePreviewPiece()
    {
        if (!Object || !Object.IsValid) return;
        
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        Vector3Int tilePos = tilemap.WorldToCell(worldPos);
        
        if (Mathf.Abs(tilePos.x) <= 2 && Mathf.Abs(tilePos.y) <= 2)
        {
            previewPiece.SetActive(true);
            previewPiece.transform.position = tilemap.GetCellCenterWorld(tilePos);
        }
        else
        {
            previewPiece.SetActive(false);
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendInput(Vector3Int tilePos, int playerId)
    {
        if (GameEnded) return;
        if (Mathf.Abs(tilePos.x) > 2 || Mathf.Abs(tilePos.y) > 2) return;
        if (PlacedPieces.ContainsKey(tilePos)) return;
        if (playerId == 1)
            P1Input = tilePos;
        else if (playerId == 2)
            P2Input = tilePos;
        TryProcessTurn();
    }
    
    private void TryProcessTurn()
    {
        // PlacedPieces를 Dictionary로 변환
        var syncedPieces = PlacedPieces.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (P1Input.HasValue && P2Input.HasValue) {
            var p1 = P1Input.Value;
            var p2 = P2Input.Value;

            bool p1Valid = !PlacedPieces.ContainsKey(p1);
            bool p2Valid = !PlacedPieces.ContainsKey(p2);
            bool attacksuccess = true; 
            if (p1 == p2) {
                attacksuccess = true;
                if ((AttackPlayerId == 1 && p1Valid) || (AttackPlayerId == 2 && p2Valid)) {
                    PlacePiece(p1, AttackPlayerId);
                } else {
                    Debug.Log($"중복 공격 실패: {p1}, 이미 돌이 있음");
                }
            } else {
                attacksuccess = false;
                if (p1Valid) PlacePiece(p1, 1);
                else Debug.Log($"Player1 좌표 {p1} 는 이미 점유됨");

                if (p2Valid) PlacePiece(p2, 2);
                else Debug.Log($"Player2 좌표 {p2} 는 이미 점유됨");
            }

            if (attacksuccess)
            {
                AttackPlayerId = AttackPlayerId;
            }
            else
            {
                AttackPlayerId = (AttackPlayerId == 1) ? 2 : 1;
            }
            CheckGameEnd();
            if(HasStateAuthority)
                UpdateAllPlayerScores();
            P1Input = null;
            P2Input = null;
            
            CurrentTurn++;
            if(HasStateAuthority)
                RPC_UpdateGameUI();
        }
    }
    
    private void PlacePiece(Vector3Int tilePos, int playerId)
    { 
        if (!HasStateAuthority)return;
        
        if (!piecePrefabMap.TryGetValue(playerId, out var piecePrefab))
        {
            Debug.LogError($"No prefab assigned for player {playerId}");
            return;
        }
        Vector3 spawnPos = tilemap.GetCellCenterWorld(tilePos);
       
       NetworkObject piece = Runner.Spawn(piecePrefab, spawnPos, Quaternion.identity);
       placedPieceObjects.Add(piece); // 소환된 돌을 리스트에 추가
        localPlacedPieces[tilePos] = playerId;
        
        // NetworkDictionary 수정을 위한 RPC 호출
        if(HasStateAuthority)
            RPC_UpdatePlacedPieces(tilePos, playerId);
        
       
        var player = FindObjectsOfType<NetworkPlayer>().FirstOrDefault(p => p.PlayerId == playerId);
        if (player != null )
        {
            int count = localPlacedPieces.Count(kvp => kvp.Value == playerId);
            if(player.HasStateAuthority)
                player.RPC_UpdatePiecesCount(count);
        }
        
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_UpdatePlacedPieces(Vector3Int tilePos, int playerId)
    {
        if (!PlacedPieces.ContainsKey(tilePos))
        {
            PlacedPieces.Add(tilePos, playerId);
        }
    }
    
    public void UpdateAllPlayerScores()
    {
        Dictionary<Vector3Int, int> syncedPieces = PlacedPieces.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        // if (!HasStateAuthority) return;
        Debug.Log(FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Length);
        foreach (var player in FindObjectsOfType<NetworkPlayer>())
        {
            //if (!player.Object || !player.Object.HasStateAuthority) continue;
            
            int bonus = scoreManager.CalculateScore(player.PlayerId, syncedPieces);
            int baseScore = (player.PlayerId == 2) ? 3 : 0;
            int total = baseScore + bonus;
            
            if(HasStateAuthority)
                player.RPC_UpdateScore(total);
        }
    }
    
    private void CheckGameEnd()
    {
        Debug.Log("Current Turn :" + CurrentTurn);
        if (CurrentTurn > 25)
        {
            GameEnded = true;
            Debug.Log("Game Over - 25 turns completed");
        }

        if (PlacedPieces.Count >= 25)
        {
            GameEnded = true;
            Debug.Log("Game Over - 모든 돌을 다 놓았습니다");
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateGameUI()
    {
        var players = FindObjectsOfType<NetworkPlayer>();
        int score1 = 0;
        int score2 = 0;
        
        foreach (var player in players)
        {
            if (player.PlayerId == 1)
            {
                score1 = player.GetTotalScore();
                Debug.Log("Player 1 score: " + score1);
            }
            else if (player.PlayerId == 2)
            {
                score2 = player.GetTotalScore();
                Debug.Log("Player 2 score: " + score2);
            }
        }
        
        bool isP1Turn = AttackPlayerId == 1;
        UIManager.Instance.UpdateUI(score1, score2, isP1Turn, GameEnded);
    }
}
