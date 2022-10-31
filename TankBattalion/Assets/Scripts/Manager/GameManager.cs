using Nakama;
using Nakama.TinyJson;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Sockets;

sealed class GameManager : MonoBehaviour
{
    #region SingleTon
    private static GameManager instance;

    public static GameManager GetInstance
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

    public Button singlePlayBt;
    public Button multiPlayBt;

    // Nakama Setting
    private HughServer hughServer;

    private IMatch currentMatch;
    private IUserPresence localUser;
    private string ticket;

    private GameObject localPlayer;
    private IDictionary<string, GameObject> players;

    public GameObject SpawnPoints;
    public GameObject NetworkLocalPlayerPrefab;
    public GameObject NetworkRemotePlayerPrefab;

    private Transform[] spawnPoints;

    private string localDisplayName;

    [SerializeField] private GameObject winDisplayPanel;

    [Tooltip("이긴 유저 이름 Text")]
    [SerializeField] private Text WinningPlayerText;

    private async void Start()
    {
        if (this.gameObject != null)
        {
            // 버튼 연결
            singlePlayBt.onClick.AddListener(SinglePlayMode);
            multiPlayBt.onClick.AddListener(MultiPlayMode);

            winDisplayPanel.SetActive(false);

            PanelActiveControlWhenMoveScene(true);

            // Nakama Match관련 초기 연결
            players = new Dictionary<string, GameObject>();
            var mainThread = UnityMainThreadDispatcher.Instance();

            await HughServer.GetInstace.ConnecToServer();
            HughServer.GetInstace.Socket.ReceivedMatchmakerMatched += m => mainThread.Enqueue(() => OnRecivedMatchMakerMatched(m));
            HughServer.GetInstace.Socket.ReceivedMatchPresence += m => mainThread.Enqueue(() => OnReceivedMatchPresence(m));
            HughServer.GetInstace.Socket.ReceivedMatchState += m => mainThread.Enqueue(async () => await OnReceivedMatchState(m));
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
        await FindMatch();
#if UNITY_EDITOR
        Debug.Log("<color=orange><b> Find Done Match</b></color>");
#endif
        HughSceneManager.GetInstace.LoadMultiPlayScene();
    }

    public async void GoToMainScene()
    {
        if (HughSceneManager.GetInstace.GetActiveSceneName() == "MultiPlay")
        {
            await QuickMatch();
        }

        HughSceneManager.GetInstace.LoadMainScene();

        PanelActiveControlWhenMoveScene(true);
    }

    private void PanelActiveControlWhenMoveScene(bool active)
    {
        titlePanel.SetActive(active);
        selectModePanel.SetActive(active);
    }

    public void SetDisplayName(string displayName)
    {
        localDisplayName = displayName;
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
            SpawnPlayer(match.Id, user);
        }

        currentMatch = match;
    }
    private void OnReceivedMatchPresence(IMatchPresenceEvent matchPresenceEvent)
    {
        // 각 유저 참여시 스폰해주기
        foreach (var user in matchPresenceEvent.Joins)
        {
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

    // 함수 내에서 await 사용 안해서 뜨는 줄
    private async Task OnReceivedMatchState(IMatchState matchState)
    {
        // local 유저의 session id 가져오기
        var userSessionId = matchState.UserPresence.SessionId;

        // match state의 길이가 있다면 dictionary에 decode해주기
        var state = matchState.State.Length > 0 ? System.Text.Encoding.UTF8.GetString(matchState.State).FromJson<Dictionary<string, string>>() : null;

        // OpCode에 따라 Match 상태 변경
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
        // 이미 플레이어 생성 했다면 return, 만약 players 초기화 안해주면 에러 생김
        if (players.ContainsKey(user.SessionId))
        {
            return;
        }

        var spawnPoint = spawnIndex == -1 ?
            SpawnPoints.transform.GetChild(Random.Range(0, SpawnPoints.transform.childCount))
            : SpawnPoints.transform.GetChild(spawnIndex);

        // local player인지 아닌지 체크
        var isLocal = user.SessionId == localUser.SessionId;

        // Choose the appropriate player prefab based on if it's the local player or not.
        var playerPrefab = isLocal ? NetworkLocalPlayerPrefab : NetworkRemotePlayerPrefab;

        // Spawn the new player.
        var player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        // Setup the appropriate network data values if this is a remote player.
        if (!isLocal)
        {
            player.GetComponent<PlayerNetworkRemoteSync>().netWorkData = new RemotePlayerNetworkData
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

    public async void LocalPlayerDied(GameObject player)
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
        WinningPlayerText.text = string.Format("{0} Won This Round!", winningPlayerName);

        // 2초 기다리기
        await Task.Delay(1000);

        // Reset the winner player text label.
        WinningPlayerText.text = "";

        // Remove ourself from the players array and destroy our player.
        players.Remove(localUser.SessionId);
        Destroy(localPlayer);

        // Choose a new spawn position and spawn our local player.
        // var spawnIndex = Random.Range(0, spawnPoint.childCount - 1);

        // 현재 spawn 장소가 2개가 최대이므로 0이상 2미만으로 index 정해준다.
        var spawnIndex = Random.Range(0, 2);
        SpawnPlayer(currentMatch.Id, localUser, spawnIndex);

        // Tell everyone where we respawned.
        SendMatchState(OpCodes.Respawn, MatchDataJson.Respawned(spawnIndex));
    }

    public async Task QuickMatch()
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

    public async Task FindMatch(int minPlayers = 2)
    {
        var matchMakingTicket = await HughServer.GetInstace.Socket.AddMatchmakerAsync("*", minPlayers, 8);
        ticket = matchMakingTicket.Ticket;
    }

    public async Task CancelMatch()
    {
        await HughServer.GetInstace.Socket.RemoveMatchmakerAsync(ticket);
    }
    #endregion
}
