using System.Net;
using System.Text.Json.Nodes;
using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum.System;

/// <summary>User profile + history endpoints (SPEC).</summary>
public sealed class UserProfileHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        // IMPORTANT: do not block login/register
        if (e.Path == "/api/users/login" || e.Path == "/api/users/register")
            return;

        if (!e.Path.StartsWith("/api/users"))
            return;

        // must be logged in
        var session = e.Session;
        if (session is null)
        {
            e.Respond(HttpStatusCode.Unauthorized,
                new JsonObject { ["success"] = false, ["reason"] = "Missing or invalid token." });
            e.Responded = true;
            return;
        }

        var s = e.Path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        // SPEC:
        // GET /api/users/{id}/profile
        // PUT /api/users/{id}/profile
        // GET /api/users/{id}/ratings
        if (s.Length != 4 || s[0] != "api" || s[1] != "users")
            return;

        // we accept any numeric id in the URL, but we always return the logged-in user's data
        if (!int.TryParse(s[2], out _))
            return;

        // ----------------------------
        // PROFILE
        // ----------------------------
        if (s[3] == "profile")
        {
            if (e.Method == HttpMethod.Get)
            {
                var user = User.Get(session.UserName, session);

                e.Respond(HttpStatusCode.OK, new JsonObject
                {
                    ["success"] = true,
                    ["username"] = user.UserName,
                    ["email"] = user.EMail,
                    ["favoriteGenre"] = user.FavoriteGenre
                });

                e.Responded = true;
                return;
            }

            if (e.Method == HttpMethod.Put)
            {
                var user = User.Get(session.UserName, session);

                user.EMail = e.Content?["email"]?.GetValue<string>() ?? user.EMail;
                user.FavoriteGenre = e.Content?["favoriteGenre"]?.GetValue<string>() ?? user.FavoriteGenre;

                user.Save();

                e.Respond(HttpStatusCode.OK, new JsonObject { ["success"] = true });
                e.Responded = true;
                return;
            }

            e.Respond(HttpStatusCode.MethodNotAllowed,
                new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
            e.Responded = true;
            return;
        }

        // ----------------------------
        // RATING HISTORY
        // ----------------------------
        if (s[3] == "ratings" && e.Method == HttpMethod.Get)
        {
            var list = RatingStore.GetForUser(session.UserName);

            var arr = new JsonArray();
            foreach (var r in list)
            {
                arr.Add(new JsonObject
                {
                    ["id"] = r.Id,
                    ["mediaId"] = r.MediaId,
                    ["stars"] = r.Stars,
                    ["comment"] = r.Comment,
                    ["confirmed"] = r.IsConfirmed,
                    ["likes"] = r.LikedBy.Count
                });
            }

            e.Respond(HttpStatusCode.OK, new JsonObject
            {
                ["success"] = true,
                ["ratings"] = arr
            });

            e.Responded = true;
            return;
        }
    }
}
