using System;
using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Fusion.Sockets;
using UnityEngine.SceneManagement;


public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Network Settings")]
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [Header("Game Settings")]
    [SerializeField] private int maxPlayers = 2;
    
    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;
    public Dictionary<PlayerRef, NetworkPlayer> players = new Dictionary<PlayerRef, NetworkPlayer>();
    private GameManager gameManager;
    public NetworkLinkedList<PlayerRef> PlayerRefs { get; } = new NetworkLinkedList<PlayerRef>();
    public List<NetworkPlayer> Players = new List<NetworkPlayer>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (runner == null)
        {
            StartGame(GameMode.Shared);
        }
    }

    public async void StartGame(GameMode mode)
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.AddCallbacks(this);
        }
            
        if (sceneManager == null)
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = sceneManager,
            SessionName = "OmokGame",
            PlayerCount = maxPlayers
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (gameManager != null && !gameManager.HasStateAuthority)
            {
                runner.SetPlayerObject(player, gameManager.Object);
            }
        }

        if (player == runner.LocalPlayer)
        {
            Vector3 spawnPosition = new Vector3(0, 0, 0);
            NetworkPlayer networkPlayer = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player).GetComponent<NetworkPlayer>();
            players[player] = networkPlayer;
            Players.Add(networkPlayer);

            // PlayerRef.Raw를 PlayerId로 사용 (1, 2, ...)
            networkPlayer.PlayerId = player.RawEncoded - 1;
            Debug.Log($"Player joined with ID: {networkPlayer.PlayerId}");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (players.TryGetValue(player, out NetworkPlayer networkPlayer))
        {
            Players.Remove(networkPlayer);
            players.Remove(player);
            runner.Despawn(networkPlayer.Object);
        }
    }

    public void RestartGame()
    {
       Application.Quit();
    }

    public void RegisterPlayer(NetworkPlayer player)
    {
        if (!Players.Contains(player))
        {
            Players.Add(player);
            Debug.Log($"[RegisterPlayer] PlayerId: {player.PlayerId}");
        }
    }

    // INetworkRunnerCallbacks implementation
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"OnShutdown: {shutdownReason}");
        players.Clear();
        Players.Clear();
    }
}
