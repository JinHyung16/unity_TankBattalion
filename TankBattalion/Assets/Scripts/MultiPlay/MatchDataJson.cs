using UnityEngine;
using Nakama.TinyJson;
using System.Collections.Generic;

public static class MatchDataJson
{
    // move speed and position data
    public static string VelocityAndPosition(Vector2 velocity, Vector3 position)
    {
        var values = new Dictionary<string, string>
        {
            {"velocity_x", velocity.x.ToString() },
            {"velocity_y", velocity.y.ToString() },
            {"position_x", position.x.ToString() },
            {"position_y", position.y.ToString() },
        };

        return values.ToJson();
    }

    // input data
    public static string Input(float horizontalInput, float verticalInput, bool fireInput)
    {
        var values = new Dictionary<string, string>
        {
            {"hor_input", horizontalInput.ToString() },
            {"ver_input", verticalInput.ToString() },
            {"fire", fireInput.ToString() },
        };

        return values.ToJson();
    }

    // player state data
    public static string Died(Vector2 position)
    {
        var values = new Dictionary<string, string>
        {
            {"position.x", position.x.ToString() },
            {"position.y", position.y.ToString() },
        };

        return values.ToJson();
    }

    public static string RoundDoneAndAnounceWin(string winnerPlayerName)
    {
        var values = new Dictionary<string, string>
        {
            { "winningPlayerName", winnerPlayerName }
        };

        return values.ToJson();
    }
}
