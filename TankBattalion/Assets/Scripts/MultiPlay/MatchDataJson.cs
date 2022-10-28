using UnityEngine;
using Nakama.TinyJson;
using System.Collections.Generic;

public static class MatchDataJson
{
    // move speed and position data
    public static string VelocityAndPosition(Vector2 velocity, Vector2 position)
    {
        var values = new Dictionary<string, string>
        {
            {"velocity.x", velocity.x.ToString() },
            {"velocity.y", velocity.y.ToString() },
            {"position.x", position.x.ToString() },
            {"position.y", position.y.ToString() },
        };

        return values.ToJson();
    }

    // input data
    public static string Input(float horizontalInput, float verticalInput, bool fireInput)
    {
        var values = new Dictionary<string, string>
        {
            {"horizontalInput", horizontalInput.ToString() },
            {"verticalInput", verticalInput.ToString() },
            {"fire", fireInput.ToString() },
        };

        return values.ToJson();
    }

    // player respawn data
    public static string Respawned(int spawnIndex)
    {
        var values = new Dictionary<string, string>
        {
            { "spawnIndex", spawnIndex.ToString() },
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
}
