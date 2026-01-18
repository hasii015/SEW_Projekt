using System.Net;
using System.Text.Json.Nodes;
using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum.System;

/// <summary>Handler for favorites endpoints (SPEC).</summary>
public sealed class FavoritesHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        // login/register must NOT require token
        if (e.Path == "/api/users/login" || e.Path == "/api/users/register")
            return;


        if (!e.Path.StartsWith("/api/media") && !e.Path.StartsWith("/api/users"))
            return;

        var session = e.Session;
        if (session is null)
        {
            e.Respond(HttpStatusCode.Unauthorized,
                new JsonObject { ["success"] = false, ["reason"] = "Missing or invalid token." });
            e.Responded = true;
            return;
        }

        var s = e.Path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        // POST /api/media/{id}/favorite
        // DELETE /api/media/{id}/favorite
        if (s.Length == 4 && s[0] == "api" && s[1] == "media" && int.TryParse(s[2], out int mediaId) && s[3] == "favorite")
        {
            // must exist
            if (MediaStore.GetById(mediaId) is null)
            {
                e.Respond(HttpStatusCode.NotFound,
                    new JsonObject { ["success"] = false, ["reason"] = "Media not found." });
                e.Responded = true;
                return;
            }

            if (e.Method == HttpMethod.Post)
            {
                FavoriteStore.Add(session.UserName, mediaId);
                e.Respond(HttpStatusCode.OK, new JsonObject { ["success"] = true });
                e.Responded = true;
                return;
            }

            if (e.Method == HttpMethod.Delete)
            {
                FavoriteStore.Remove(session.UserName, mediaId);
                e.Respond(HttpStatusCode.OK, new JsonObject { ["success"] = true });
                e.Responded = true;
                return;
            }

            e.Respond(HttpStatusCode.MethodNotAllowed,
                new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
            e.Responded = true;
            return;
        }

        // GET /api/users/{id}/favorites
        if (s.Length == 4 && s[0] == "api" && s[1] == "users" && int.TryParse(s[2], out _) && s[3] == "favorites")
        {
            // spec uses user id, but we only have session user -> return session favorites
            var favIds = FavoriteStore.Get(session.UserName);

            var arr = new JsonArray();
            foreach (var id in favIds)
                arr.Add(id);

            e.Respond(HttpStatusCode.OK,
                new JsonObject { ["success"] = true, ["favorites"] = arr });

            e.Responded = true;
            return;
        }
    }
}
