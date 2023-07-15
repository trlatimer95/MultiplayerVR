using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get ; private set; }  
    public NetworkRunner SessionRunner { get; private set; }
    public INetworkSceneManager sceneManager { get; private set; }

    [SerializeField] private GameObject _runnerPrefab;
    [SerializeField] private TMP_InputField roomCodeInputField;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void CreateRunner()
    {
        SessionRunner = Instantiate(_runnerPrefab, transform).GetComponent<NetworkRunner>();
        SessionRunner.AddCallbacks(this);
    }

    public async void StartSharedSession()
    {
        // Create Runner
        CreateRunner();

        // Load Scene
        //await LoadScene();

        // ConnectSession
        string roomCode = roomCodeInputField.text;
        //await Connect(roomCode);

        await ConnectFusion(roomCode);
    }

    public async Task LoadScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

    private async Task Connect(string roomCode)
    {
        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomCode,
            SceneManager = GetComponent<NetworkSceneManagerDefault>(),      
            Scene = 1,
        };

        var result = await SessionRunner.StartGame(args);

        if (result.Ok)
        {
            Debug.Log("StartGame success");
        }
        else
        {
            Debug.LogError(result.ErrorMessage);
        }
    }

    public async Task ConnectFusion(string roomCode)
    {
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomCode,
            SceneManager = sceneManager,
            Scene = SceneManager.GetActiveScene().buildIndex + 1,
        };

        await SessionRunner.StartGame(args);
    }

    public void Disconnect()
    {
        //SessionRunner.Despawn(SessionRunner.GetPlayerObject(SessionRunner.LocalPlayer.PlayerId));
        SceneManager.LoadScene(0);
        SessionRunner.Shutdown();
        Destroy(gameObject);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("A new player joined the session");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Session Shutdown");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player left");  
    }

    #region Unused Network Callbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
       
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }   

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
    #endregion
}
