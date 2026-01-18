using System.Net;
using System.Text.Json.Nodes;
using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum.System;

/// <summary>Handler for /api/leaderboard endpoint (SPEC).</summary>
public sealed class LeaderboardHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (e.Path != "/api/leaderboard")
            return;

        if (e.Method != HttpMethod.Get)
        {
            e.Respond(HttpStatusCode.MethodNotAllowed,
                new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
            e.Responded = true;
            return;
        }

        var list = RatingStore.GetLeaderboard();

        var arr = new JsonArray();
        foreach (var entry in list)
        {
            arr.Add(new JsonObject
            {
                ["username"] = entry.UserName,
                ["ratings"] = entry.RatingCount
            });
        }

        e.Respond(HttpStatusCode.OK, new JsonObject
        {
            ["success"] = true,
            ["leaderboard"] = arr
        });

        e.Responded = true;
    }

}
