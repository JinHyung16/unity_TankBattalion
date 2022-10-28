using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nakama;
using Nakama.TinyJson;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    #region SingleTon
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    // Panel
    public GameObject titlePanel;
    public GameObject selectModePanel;
    public GameObject WinDisplayPanel;

    public Button singlePlayBt;
    public Button multiPlayBt;

    // Nakama match cashing
    private IMatch currentMatch;
    private IUserPresence localUser;

    private GameObject localPlayer;
    private IDictionary<string, GameObject> players;

    public GameObject NetworkLocalPlayerPrefab;
    public GameObject NetworkRemotePlayerPrefab;

    [SerializeField] private Transform[] SpawnPoints;

    private string localDisplayName;

    [Tooltip("이긴 유저 이름 Text")]
    [SerializeField] private Text WinningPlayerText;

    public bool isLocal = false;

    private async void Start()
    {
        if (this.gameObject != null)
        {
            await HughServer.GetInstace.ConnecToServer();

            singlePlayBt.onClick.AddListener(SinglePlayMode);
            multiPlayBt.onClick.AddListener(MultiPlayMode);

            // nakama match socket bind
            var mainThread = UnityMainThreadDispatcher.Instance();

            HughServer.GetInstace.Socket.ReceivedMatchmakerMatched += m => mainThread.Enqueue(() => OnRecivedMatchMakerMatched(m));
            HughServer.GetInstace.Socket.ReceivedMatchPresence += m => mainThread.Enqueue(() => OnReceivedMatchPresence(m)); // 에러 포인트
            HughServer.GetInstace.Socket.ReceivedMatchState += m => mainThread.Enqueue(async () => await OnReceivedMatchState(m));

            ControlWinDisPlayPanel(false);
            PanelActiveControlWhenMoveScene(true);
        }
    }

    private void SinglePlayMode()
    {
        PanelActiveControlWhenMoveScene(false);
        HughSceneManager.GetInstace.LoadSinglePlayScene();
    }

    private async void MultiPlayMode()
    {
        PanelActiveControlWhenMoveScene(false);
        HughSceneManager.GetInstace.LoadMultiPlayScene();
        await HughServer.GetInstace.FindMatch();
    }

    public async void GoToMainScene()
    {
        if (HughSceneManager.GetInstace.GetActiveSceneName() == "MultiPlay")
        {
            ControlWinDisPlayPanel(false);
            await QuickMatch();
            await HughServer.GetInstace.Disconnect();
        }

        HughSceneManager.GetInstace.LoadMainScene();

        PanelActiveControlWhenMoveScene(true);
    }

    private void PanelActiveControlWhenMoveScene(bool active)
    {
        titlePanel.SetActive(active);
        selectModePanel.SetActive(active);
    }

    public void SetDisplayName(string name)
    {
        localDisplayName = name;
    }

    public void ControlWinDisPlayPanel(bool active)
    {
        WinDisplayPanel.SetActive(active);
    }
    public void GetSpawnPosition(Transform[] transforms)
    {
        this.SpawnPoints = transforms;
    }

    #region Nakama Match Function

    private async void OnRecivedMatchMakerMatched(IMatchmakerMatched matchmakerMatched)
    {
        // localuser 캐싱
        localUser = matchmakerMatched.Self.Presence;
        var match = await HughServer.GetInstace.Socket.JoinMatchAsync(matchmakerMatched);

#if UNITY_EDITOR
        Debug.Log("Our Session Id: " + match.Self.SessionId);
#endif

        foreach (var user in match.Presences)
        {
            Debug.Log("Connected User Session Id: " + user.SessionId);
            SpawnPlayer(match.Id, user); // 에러 포인트
        }

        currentMatch = match;
    }
    private void OnReceivedMatchPresence(IMatchPresenceEvent matchPresenceEvent)
    {
        // 각 유저 참여시 스폰해주기
        foreach (var user in matchPresenceEvent.Joins)
        {
            Debug.Log("Joint User Session Id : " + user.SessionId);
            SpawnPlayer(matchPresenceEvent.MatchId, user);
        }

        // 각 유저가 떠날 때 삭제해주기
        foreach (var user in matchPresenceEvent.Leaves)
        {
            Debug.Log("Leave User Session Id : " + user.SessionId);

            if (players.ContainsKey(user.SessionId))
            {
                Destroy(players[user.SessionId]);
                players.Remove(user.SessionId);
            }
        }
    }

    private async Task OnReceivedMatchState(IMatchState matchState)
    {
        // Get the local user's session ID.
        var userSessionId = matchState.UserPresence.SessionId;

        // If the matchState object has any state length, decode it as a Dictionary.
        var state = matchState.State.Length > 0 ? System.Text.Encoding.UTF8.GetString(matchState.State).FromJson<Dictionary<string, string>>() : null;

        // Decide what to do based on the Operation Code as defined in OpCodes.

        switch (matchState.OpCode)
        {
            case OpCodes.Died:
                var playerToDestroy = players[userSessionId];
                Destroy(playerToDestroy, 0.5f);
                players.Remove(userSessionId);
                if (players.Count == 1 && players.First().Key == localUser.SessionId)
                {
                    AnnounceWinner();
                }
                break;
            case OpCodes.Respawn:
                SpawnPlayer(currentMatch.Id, matchState.UserPresence, int.Parse(state["spawnIndex"]));
                break;
            default:
                break;
        }

    }
    private void SpawnPlayer(string matchId, IUserPresence user, int spawnIndex = -1)
    {
        // 이미 플레이어 생성 했다면 return -> 에러 포인트
        if (players.ContainsKey(user.SessionId))
        {
            return;
        }

        // Set a variable to check if the player is the local player or not based on session ID.
        //var isLocal = user.SessionId == localUser.SessionId;
        isLocal = user.SessionId == localUser.SessionId;

        // Choose the appropriate player prefab based on if it's the local player or not.
        var playerPrefab = isLocal ? NetworkLocalPlayerPrefab : NetworkRemotePlayerPrefab;

        var spawnPoint = isLocal ? SpawnPoints[0] : SpawnPoints[1];

        // Spawn the new player.
        var player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        // Setup the appropriate network data values if this is a remote player.
        if (!isLocal)
        {
            player.GetComponent<PlayerNetworkRemoteSync>().NetworkData = new RemotePlayerNetworkData
            {
                MatchId = matchId,
                User = user
            };
        }

        // Add the player to the players array.
        players.Add(user.SessionId, player);

        // If this is our local player, add a listener for the PlayerDied event.
        if (isLocal)
        {
            localPlayer = player;
        }
    }

    public async void OnLocalPlayerDied(GameObject player)
    {
        await SendMatchStateAsync(OpCodes.Died, MatchDataJson.Died(player.transform.position));

        players.Remove(localUser.SessionId);
        Destroy(player, 0.5f);
    }

    public async void AnnounceWinner()
    {
        var winningPlayerName = localDisplayName;
        await AnnounceWinner(winningPlayerName);
    }

    private async Task AnnounceWinner(string winningPlayerName)
    {
        // Set the winning player text label.
        WinningPlayerText.text = string.Format("{0} won this round!", winningPlayerName);

        // Wait for 2 seconds.
        await Task.Delay(2000);

        // Reset the winner player text label.
        WinningPlayerText.text = "";

        // Remove ourself from the players array and destroy our player.
        players.Remove(localUser.SessionId);
        Destroy(localPlayer);

        var spawnPoint = isLocal == true ? SpawnPoints[0] : SpawnPoints[1];
        // Choose a new spawn position and spawn our local player.
        var spawnIndex = Random.Range(0, spawnPoint.childCount - 1);
        SpawnPlayer(currentMatch.Id, localUser, spawnIndex);

        // Tell everyone where we respawned.
        SendMatchState(OpCodes.Respawn, MatchDataJson.Respawned(spawnIndex));
    }

    private async Task QuickMatch()
    {
        await HughServer.GetInstace.Socket.LeaveMatchAsync(currentMatch);

        currentMatch = null;
        localUser = null;

        foreach (var player in players.Values)
        {
            Destroy(player);
        }

        players.Clear();
    }
    public async Task SendMatchStateAsync(long opCode, string state)
    {
        await HughServer.GetInstace.Socket.SendMatchStateAsync(currentMatch.Id, opCode, state);
    }

    public void SendMatchState(long opCode, string state)
    {
        HughServer.GetInstace.Socket.SendMatchStateAsync(currentMatch.Id, opCode, state);
    }
    #endregion
}
