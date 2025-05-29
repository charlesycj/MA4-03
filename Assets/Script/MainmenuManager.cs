using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class MainMenuManager : MonoBehaviour
{
    public Button startServerButton;
    public Button joinServerButton;

    private NetworkRunner runner;

    private void Awake()
    {
        startServerButton.onClick.AddListener(StartServer);
        joinServerButton.onClick.AddListener(JoinServer);
    }

    private async void StartServer()
    {
        Debug.Log("서버 시작 (Shared 모드)...");

        runner = gameObject.AddComponent<NetworkRunner>();

        // 씬 매니저 설정
        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "TestRoom",
            SceneManager = sceneManager,      // Shared 모드에서는 씬 매니저만 설정해주면 돼
            // Scene = SceneRef.FromIndex(1), // 필요하다면 빌드 인덱스 직접 지정
        });

        Debug.Log("서버 시작 결과: " + result.Ok);
    }

    private async void JoinServer()
    {
        Debug.Log("서버에 접속...");

        runner = gameObject.AddComponent<NetworkRunner>();

        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "TestRoom",
            SceneManager = sceneManager,
        });

        Debug.Log("서버 접속 결과: " + result.Ok);
    }
}