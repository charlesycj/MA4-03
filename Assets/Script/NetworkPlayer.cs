
using Fusion;
using UnityEngine;
using System.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [Networked] public string PlayerName { get; set; } = "";
    [Networked] public int PlayerId { get; set; }
    [Networked] public bool IsReady { get; set; }
    [Networked] public bool IsConnected { get; set; } = true;
    
    [Header("Game Stats")]
    [Networked] public int Score { get; set; }
    [Networked] public int PiecesOnBoard { get; set; } = 0;

    public GameManager gameManager;
    private UIManager uiManager;
    private Camera mainCamera;
    
    

    public override void Spawned()
    {
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        mainCamera = Camera.main;
        StartCoroutine(RegisterToManagerWhenReady());
    }

    private IEnumerator RegisterToManagerWhenReady()
    {
        // NetworkManager가 준비될 때까지 대기
        while (NetworkManager.Instance == null)
            yield return null;

        // NetworkManager에 자신을 등록하고 PlayerId를 부여받음
        NetworkManager.Instance.RegisterPlayer(this);

        // PlayerId에 따라 점수 초기화
        Score = (PlayerId == 2) ? 3 : 0;
    }
    
    
    void Update()
    {
        if (!Object.IsValid)
            return;
        if (!HasStateAuthority)
            return;
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            return;
        }
        if (gameManager.GameEnded)
            return;
        
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            Vector3Int tilePos = gameManager.tilemap.WorldToCell(worldPos);

            if (Mathf.Abs(tilePos.x) > 2 || Mathf.Abs(tilePos.y) > 2)
                return;
            
            gameManager.RPC_SendInput(tilePos, PlayerId);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UpdateScore(int newScore)
    {
        Score = newScore;
        if (uiManager != null)
        {
            uiManager.UpdatePlayerScore(PlayerId, Score);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UpdatePiecesCount(int count)
    {
        PiecesOnBoard = count;
    }

    public int GetTotalScore()
    {
        return Score;
    }

    public bool IsLocalPlayer()
    {
        return Object.HasInputAuthority;
    }

    public PlayerInfo GetPlayerInfo()
    {
        return new PlayerInfo
        {
            playerId = PlayerId,
            playerName = PlayerName,
            score = Score,
            piecesOnBoard = PiecesOnBoard,
            totalScore = GetTotalScore(),
            isReady = IsReady,
            isConnected = IsConnected,
            isLocalPlayer = IsLocalPlayer()
        };
    }
}

[System.Serializable]
public struct PlayerInfo
{
    public int playerId;
    public string playerName;
    public int score;
    public int piecesOnBoard;
    public int totalScore;
    public bool isReady;
    public bool isConnected;
    public bool isLocalPlayer;
}
