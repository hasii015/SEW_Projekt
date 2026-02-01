using System.Net;
using System.Text.Json.Nodes;
using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum.System;

/// <summary>Handler for rating endpoints.</summary>
public sealed class RatingHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/api/media") && !e.Path.StartsWith("/api/ratings"))
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

        // POST /api/media/{id}/rate
        if (s.Length == 4 && s[0] == "api" && s[1] == "media" && int.TryParse(s[2], out int mediaId) && s[3] == "rate")
        {
            if (e.Method != HttpMethod.Post)
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
                e.Responded = true;
                return;
            }

            int stars = e.Content?["stars"]?.GetValue<int>() ?? 0;
            string comment = e.Content?["comment"]?.GetValue<string>() ?? string.Empty;

            if (stars < 1 || stars > 5)
            {
                e.Respond(HttpStatusCode.BadRequest,
                    new JsonObject { ["success"] = false, ["reason"] = "Stars must be between 1 and 5." });
                e.Responded = true;
                return;
            }

            var rating = RatingStore.AddOrUpdate(mediaId, session.UserName, stars, comment);

            e.Respond(HttpStatusCode.OK,
                new JsonObject { ["success"] = true, ["id"] = rating.Id });

            e.Responded = true;
            return;
        }

        // PUT /api/ratings/{id}
        if (s.Length == 3 && s[0] == "api" && s[1] == "ratings" && int.TryParse(s[2], out int ratingId))
        {
            if (e.Method != HttpMethod.Put)
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
                e.Responded = true;
                return;
            }

            var rating = RatingStore.Get(ratingId);
            if (rating is null)
            {
                e.Respond(HttpStatusCode.NotFound,
                    new JsonObject { ["success"] = false, ["reason"] = "Rating not found." });
                e.Responded = true;
                return;
            }

            if (!rating.CreatedBy.Equals(session.UserName, StringComparison.OrdinalIgnoreCase))
            {
                e.Respond(HttpStatusCode.Forbidden,
                    new JsonObject { ["success"] = false, ["reason"] = "Only author may update this rating." });
                e.Responded = true;
                return;
            }

            int stars = e.Content?["stars"]?.GetValue<int>() ?? rating.Stars;
            string comment = e.Content?["comment"]?.GetValue<string>() ?? rating.Comment;

            if (stars < 1 || stars > 5)
            {
                e.Respond(HttpStatusCode.BadRequest,
                    new JsonObject { ["success"] = false, ["reason"] = "Stars must be between 1 and 5." });
                e.Responded = true;
                return;
            }

            rating.Stars = stars;
            rating.Comment = comment;
            rating.IsConfirmed = false;

            e.Respond(HttpStatusCode.OK, new JsonObject { ["success"] = true });
            e.Responded = true;
            return;
        }

        // POST /api/ratings/{id}/confirm
        if (s.Length == 4 && s[0] == "api" && s[1] == "ratings" && int.TryParse(s[2], out int ridConfirm) && s[3] == "confirm")
        {
            if (e.Method != HttpMethod.Post)
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
                e.Responded = true;
                return;
            }

            bool ok = RatingStore.Confirm(ridConfirm, session.UserName);

            if (!ok)
            {
                e.Respond(HttpStatusCode.Forbidden,
                    new JsonObject { ["success"] = false, ["reason"] = "Only the author can confirm this rating." });
                e.Responded = true;
                return;
            }

            e.Respond(HttpStatusCode.OK, new JsonObject { ["success"] = true });
            e.Responded = true;
            return;
        }

        // POST /api/ratings/{id}/like
        if (s.Length == 4 && s[0] == "api" && s[1] == "ratings" && int.TryParse(s[2], out int ridLike) && s[3] == "like")
        {
            if (e.Method != HttpMethod.Post)
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject { ["success"] = false, ["reason"] = "Method not allowed." });
                e.Responded = true;
                return;
            }

            bool ok = RatingStore.Like(ridLike, session.UserName);

            if (!ok)
            {
                e.Respond(HttpStatusCode.BadRequest,
                    new JsonObject { ["success"] = false, ["reason"] = "Cannot like this rating." });
                e.Responded = true;
                return;
            }

            e.Respond(HttpStatusCode.OK, new JsonObject { ["success"] = true });
            e.Responded = true;
            return;
        }
    }
}
