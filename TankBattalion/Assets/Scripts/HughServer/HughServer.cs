using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using HughSingleTon;
using System.Threading.Tasks;
using System;
using System.Net.Http;

public class HughServer : LazySingleton<HughServer>
{
    protected string Scheme = "http";
    protected string Host = "localhost";
    protected int Port = 7350;

    protected string ServerKey = "defaultkey";

    protected string SessionPrefName = "nakama.session";
    private const string DeviceIdentifierPrefName = "nakama.deviceUniqueIdentifier";

    public IClient Client;
    public ISession Session;
    public ISocket Socket;

    protected UnityMainThreadDispatcher mainThread;

    private string currentMatchTicket;

    public async Task ConnecToServer()
    {
        //device id login
        Client = new Nakama.Client(Scheme, Host, Port, ServerKey, UnityWebRequestAdapter.Instance);

        var authToken = PlayerPrefs.GetString(SessionPrefName);
        if (!string.IsNullOrEmpty(authToken))
        {
            var session = Nakama.Session.Restore(authToken);
            if (!session.IsExpired)
            {
                Session = session;
            }
        }

        if (Session == null)
        {
            string deviceId;
            if (PlayerPrefs.HasKey(DeviceIdentifierPrefName))
            {
                deviceId = PlayerPrefs.GetString(DeviceIdentifierPrefName);
            }
            else
            {
                deviceId = SystemInfo.deviceUniqueIdentifier;
                if (deviceId == SystemInfo.unsupportedIdentifier)
                {
                    deviceId = System.Guid.NewGuid().ToString();
                }

#if UNITY_EDITOR
                Debug.LogFormat("<color=orange><b>[HughServer]</b> deviceId : {0} </color>", deviceId);
#endif

                PlayerPrefs.SetString(DeviceIdentifierPrefName, deviceId);
            }
            Session = await Client.AuthenticateDeviceAsync(deviceId);

            PlayerPrefs.SetString(SessionPrefName, Session.AuthToken);
        }

        await SocketConnect();
#if UNITY_EDITOR
        Debug.Log("<color=orange><b>[HughServer]</b> Socekt Connect : {0} </color>");
#endif
    }
    protected async Task SocketConnect()
    {
        Socket = Client.NewSocket(false);
        await Socket.ConnectAsync(Session, true);
        BindSocketEvents();
#if UNITY_EDITOR
        Debug.Log("<color=green><b>[HughServer]</b> Socekt Connect : {0} </color>");
#endif
    }

    protected void BindSocketEvents()
    {
        if (mainThread == null)
        {
            mainThread = UnityMainThreadDispatcher.Instance();
        }
    }

    public async Task Disconnect()
    {
        if (Socket != null)
        {
            await Socket.CloseAsync();
            Socket = null;
        }

        if (Session != null)
        {
            //await Client.SessionLogoutAsync(Session);
            Session = null;
        }

#if UNITY_EDITOR
        Debug.Log("<color=red><b>[HughServer]</b> Socekt DisConnect : {0} </color>");
#endif
    }

    public async Task FindMatch(int minPlayers = 2)
    {
        var matchmakingProperties = new Dictionary<string, string>
        {
            {"engine", "unity" }
        };

        var matchMakerTicket = await Socket.AddMatchmakerAsync("+properties.engine:unity", minPlayers, minPlayers, matchmakingProperties);
        currentMatchTicket = matchMakerTicket.Ticket;
    }

    public async Task CancelMatch()
    {
        await Socket.RemoveMatchmakerAsync(currentMatchTicket);
    }
}
