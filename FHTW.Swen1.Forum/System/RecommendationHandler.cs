using System.Net;
using System.Text.Json.Nodes;
using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum.System;

/// <summary>Handler for recommendation endpoint (SPEC).</summary>
public sealed class RecommendationHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/api/users/"))
            return;

        var s = e.Path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (!(s.Length == 4 && s[0] == "api" && s[1] == "users" && s[3] == "recommendations"))
            return;

        var session = e.Session;
        if (session is null)
        {
            e.Respond(HttpStatusCode.Unauthorized,
                new JsonObject { ["success"] = false, ["reason"] = "Missing or invalid token." });
            e.Responded = true;
            return;
        }

        if (e.Method != HttpMethod.Get)
        {
            e.Respond(HttpStatusCode.MethodNotAllowed,
                new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
            e.Responded = true;
            return;
        }

        string type = e.Context.Request.QueryString["type"] ?? string.Empty;
        type = type.Trim().ToLowerInvariant();

        if (type != "genre" && type != "content")
        {
            e.Respond(HttpStatusCode.BadRequest,
                new JsonObject { ["success"] = false, ["reason"] = "Query param type must be 'genre' or 'content'." });
            e.Responded = true;
            return;
        }

        var recs = RecommendationService.GetRecommendations(session.UserName, type);

        var arr = new JsonArray();
        foreach (var m in recs)
        {
            var genresArr = new JsonArray();
            foreach (var g in m.Genres.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                genresArr.Add(g);

            arr.Add(new JsonObject
            {
                ["id"] = m.Id,
                ["title"] = m.Title,
                ["description"] = m.Description,
                ["mediaType"] = m.MediaType,
                ["releaseYear"] = m.ReleaseYear,
                ["genres"] = genresArr,
                ["ageRestriction"] = m.AgeRestriction
            });
        }

        e.Respond(HttpStatusCode.OK, new JsonObject
        {
            ["success"] = true,
            ["recommendations"] = arr
        });

        e.Responded = true;
    }
}
