using Nakama;
using Nakama.TinyJson;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Sockets;

public class MatchManager : MonoBehaviour
{
    public HughServer hughServer;

    private IMatch currentMatch;
    private IUserPresence localUser;

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
        winDisplayPanel.SetActive(false);

        // Nakama Match관련 초기 연결
        players = new Dictionary<string, GameObject>();

        UnityMainThreadDispatcher mainThread = UnityMainThreadDispatcher.Instance();

        await hughServer.ConnecToServer();

        hughServer.Socket.ReceivedMatchmakerMatched += m => mainThread.Enqueue(() => OnReceivedMatchmakerMatched(m));
        hughServer.Socket.ReceivedMatchPresence += m => mainThread.Enqueue(() => OnReceivedMatchPresence(m));
        hughServer.Socket.ReceivedMatchState += m => mainThread.Enqueue(async () => await OnReceivedMatchState(m));
    }

    private async void OnReceivedMatchmakerMatched(IMatchmakerMatched matched)
    {
        // localuser 캐싱
        localUser = matched.Self.Presence;
        var match = await hughServer.Socket.JoinMatchAsync(matched);

        foreach (var user in match.Presences)
        {
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
                    AnnounceWinnerAndRouondDone();
                }
                break;
            case OpCodes.RoundDone:
                await AnnounceWinnerAndDone(state["winningPlayerName"]);
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
            SpawnPoints.transform.GetChild(Random.Range(0, SpawnPoints.transform.childCount)): 
            SpawnPoints.transform.GetChild(spawnIndex);

        // local player인지 아닌지 체크
        var isLocal = user.SessionId == localUser.SessionId;

        // Choose the appropriate player prefab based on if it's the local player or not.
        var playerPrefab = isLocal ? NetworkLocalPlayerPrefab : NetworkRemotePlayerPrefab;

        // Spawn the new player.
        var player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        // Setup the appropriate network data values if this is a remote player.
        if (!isLocal)
        {
            player.GetComponent<PlayerNetworkRemoteSync>().NetWorkData = new RemotePlayerNetworkData
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

        player.GetComponentInChildren<PlayerColorController>().SetColor(System.Array.IndexOf(players.Keys.ToArray(), user.SessionId));
    }

    public async void LocalPlayerDied(GameObject player)
    {
        await SendMatchStateAsync(OpCodes.Died, MatchDataJson.Died(player.transform.position));

        players.Remove(localUser.SessionId);
        Destroy(player, 0.5f);
    }

    public async void AnnounceWinnerAndRouondDone()
    {
        var winningPlayerName = localDisplayName;

        await SendMatchStateAsync(OpCodes.RoundDone, MatchDataJson.RoundDoneAndAnounceWin(winningPlayerName));

        await AnnounceWinnerAndDone(winningPlayerName);
    }

    private async Task AnnounceWinnerAndDone(string winningPlayerName)
    {
        // Set the winning player text label.
        WinningPlayerText.text = string.Format("{0} Won This Round!", winningPlayerName);

        // 1초 기다리기
        await Task.Delay(1000);

        // Reset the winner player text label.
        WinningPlayerText.text = "";

        // Remove ourself from the players array and destroy our player.
        players.Remove(localUser.SessionId);
        Destroy(localPlayer);
    }

    public async Task SendMatchStateAsync(long opCode, string state)
    {
        await hughServer.Socket.SendMatchStateAsync(currentMatch.Id, opCode, state);
    }

    public void SendMatchState(long opCode, string state)
    {
        hughServer.Socket.SendMatchStateAsync(currentMatch.Id, opCode, state);
    }

    public async Task QuickMatch()
    {
        await hughServer.Socket.LeaveMatchAsync(currentMatch);

        currentMatch = null;
        localUser = null;

        foreach (var player in players.Values)
        {
            Destroy(player);
        }

        players.Clear();
    }
}
