using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;
using System.Net.Sockets;

[Serializable]
[CreateAssetMenu]
public class HughServer : ScriptableObject
{
    public string Scheme = "http";
    public string Host = "localhost";
    public int Port = 7350;
    public string ServerKey = "defaultkey";

    private const string SessionPrefName = "nakama.session";
    private const string DeviceIdentifierPrefName = "nakama.deviceUniqueIdentifier";

    public IClient Client;
    public ISession Session;
    public ISocket Socket;

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

                PlayerPrefs.SetString(DeviceIdentifierPrefName, deviceId);
            }
            Session = await Client.AuthenticateDeviceAsync(deviceId);

            PlayerPrefs.SetString(SessionPrefName, Session.AuthToken);
        }

        // realtime communication을 위해 새로운 Socket을 열기
        Socket = Client.NewSocket();

#if UNITY_EDITOR
        Debug.Log("나카마 서버 연결 완료");
#endif
        await Socket.ConnectAsync(Session, true);
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
    }

    private string ticket;

    public async Task FindMatch(int minPlayers = 2)
    {
        var matchMakingTicket = await Socket.AddMatchmakerAsync("*", minPlayers, minPlayers);
        ticket = matchMakingTicket.Ticket;
    }

    public async Task CancelMatch()
    {
        await Socket.RemoveMatchmakerAsync(ticket);
    }
}
