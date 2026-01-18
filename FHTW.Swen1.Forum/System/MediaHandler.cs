using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum.System;

/// <summary>Handler for /media endpoints.</summary>
public sealed class MediaHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/api/media"))
        {
            return;
        }

        // every media action requires a logged-in user
        var session = e.Session;
        if (session is null)
        {
            e.Respond(HttpStatusCode.Unauthorized,
                new JsonObject { ["success"] = false, ["reason"] = "Missing or invalid token." });
            e.Responded = true;
            return;
        }

        // /media or /media/{id}
        string path = e.Path.StartsWith("/api") ? e.Path.Substring(4) : e.Path;
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        // /api/media/{id}/rate belongs to RatingHandler 
        if (segments.Length == 3 && segments[0] == "media" && segments[2] == "rate")
        {
            return; // don't respond -> let RatingHandler handle it
        }


        // /api/media/{id}/favorite belongs to FavoritesHandler 
        if (segments.Length == 3 && segments[0] == "media" && segments[2] == "favorite")
        {
            return;
        }


        if (segments.Length == 1)
        {
            if (e.Method == HttpMethod.Get)
            {
                HandleGetAll(e);
            }
            else if (e.Method == HttpMethod.Post)
            {
                HandleCreate(e, session.UserName);
            }
            else
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject { ["success"] = false, ["reason"] = "Method not allowed on /media." });
            }
        }
        else if (segments.Length == 2 && int.TryParse(segments[1], out var id))
        {
            if (e.Method == HttpMethod.Get)
            {
                HandleGetById(e, id);
            }
            else if (e.Method == HttpMethod.Put)
            {
                HandleUpdate(e, id, session.UserName);
            }
            else if (e.Method == HttpMethod.Delete)
            {
                HandleDelete(e, id, session.UserName);
            }
            else
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject { ["success"] = false, ["reason"] = "Method not allowed on /media/{id}." });
            }
        }
        else
        {
            e.Respond(HttpStatusCode.BadRequest,
                new JsonObject { ["success"] = false, ["reason"] = "Invalid media endpoint." });
        }

        e.Responded = true;
    }

    private static void HandleGetAll(HttpRestEventArgs e)
    {
        var items = MediaStore.GetAll().AsEnumerable();

        // ----- FILTERS from query -----
        if (e.Query.TryGetValue("title", out var title) && !string.IsNullOrWhiteSpace(title))
        {
            items = items.Where(m => (m.Title ?? "")
                .Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (e.Query.TryGetValue("genre", out var genre) && !string.IsNullOrWhiteSpace(genre))
        {
            items = items.Where(m =>
            {
                var genres = (m.Genres ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                return genres.Any(g => g.Equals(genre, StringComparison.OrdinalIgnoreCase));
            });
        }

        if (e.Query.TryGetValue("mediaType", out var mediaType) && !string.IsNullOrWhiteSpace(mediaType))
        {
            items = items.Where(m => (m.MediaType ?? "")
                .Equals(mediaType, StringComparison.OrdinalIgnoreCase));
        }

        if (e.Query.TryGetValue("releaseYear", out var yearStr) && int.TryParse(yearStr, out var year))
        {
            items = items.Where(m => m.ReleaseYear == year);
        }

        if (e.Query.TryGetValue("ageRestriction", out var ageStr) && int.TryParse(ageStr, out var age))
        {
            items = items.Where(m => m.AgeRestriction == age);
        }

        // rating = minimum average stars (confirmed only)
        if (e.Query.TryGetValue("rating", out var ratingStr) && int.TryParse(ratingStr, out var minRating))
        {
            items = items.Where(m => GetAvgStars(m.Id) >= minRating);
        }

        // ----- SORTING -----
        if (e.Query.TryGetValue("sortBy", out var sortBy) && !string.IsNullOrWhiteSpace(sortBy))
        {
            if (sortBy.Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                items = items.OrderBy(m => m.Title);
            }
            else if (sortBy.Equals("score", StringComparison.OrdinalIgnoreCase))
            {
                items = items.OrderByDescending(m => GetAvgStars(m.Id));
            }
        }

        // return as JSON
        var arr = JsonSerializer.SerializeToNode(items.ToList()) as JsonArray ?? new JsonArray();

        e.Respond(HttpStatusCode.OK, new JsonObject
        {
            ["success"] = true,
            ["items"] = arr
        });
    }

    private static double GetAvgStars(int mediaId)
    {
        var ratings = RatingStore.GetForMedia(mediaId, onlyConfirmed: true);
        if (ratings.Count == 0) return 0;
        return ratings.Average(r => r.Stars);
    }



    private static void HandleCreate(HttpRestEventArgs e, string userName)
    {
        var media = new MediaEntry
        {
            Title = e.Content?["title"]?.GetValue<string>() ?? string.Empty,
            Description = e.Content?["description"]?.GetValue<string>() ?? string.Empty,

            MediaType = e.Content?["mediaType"]?.GetValue<string>()
            ?? e.Content?["type"]?.GetValue<string>()
            ?? e.Content?["mediatype"]?.GetValue<string>()
            ?? string.Empty,

            ReleaseYear = e.Content?["releaseYear"]?.GetValue<int>() ?? 0,

            Genres = e.Content?["genres"] is JsonArray arr
            ? string.Join(",", arr.Select(x => x?.GetValue<string>() ?? string.Empty))
            : e.Content?["genres"]?.GetValue<string>() ?? string.Empty,

            AgeRestriction = e.Content?["ageRestriction"]?.GetValue<int>() ?? 0,
            CreatedBy = userName
        };


        var stored = MediaStore.Add(media);

        e.Respond(HttpStatusCode.Created,
            new JsonObject { ["success"] = true, ["id"] = stored.Id });
    }

    private static void HandleGetById(HttpRestEventArgs e, int id)
    {
        var media = MediaStore.GetById(id);
        if (media is null)
        {
            e.Respond(HttpStatusCode.NotFound,
                new JsonObject { ["success"] = false, ["reason"] = "Media not found." });
            return;
        }

        var json = JsonSerializer.SerializeToNode(media);
        e.Respond(HttpStatusCode.OK, json?.AsObject() ?? new JsonObject());
    }

    private static void HandleUpdate(HttpRestEventArgs e, int id, string userName)
    {
        var media = MediaStore.GetById(id);
        if (media is null)
        {
            e.Respond(HttpStatusCode.NotFound,
                new JsonObject { ["success"] = false, ["reason"] = "Media not found." });
            return;
        }

        if (media.CreatedBy != userName)
        {
            e.Respond(HttpStatusCode.Forbidden,
                new JsonObject { ["success"] = false, ["reason"] = "Only creator may update this media." });
            return;
        }

        media.Title = e.Content?["title"]?.GetValue<string>() ?? media.Title;
        media.Description = e.Content?["description"]?.GetValue<string>() ?? media.Description;
        media.MediaType = e.Content?["mediatype"]?.GetValue<string>() ?? media.MediaType;
        media.ReleaseYear = e.Content?["releaseYear"]?.GetValue<int>() ?? media.ReleaseYear;
        media.Genres = e.Content?["genres"]?.GetValue<string>() ?? media.Genres;
        media.AgeRestriction = e.Content?["ageRestriction"]?.GetValue<int>() ?? media.AgeRestriction;

        MediaStore.Update(media);

        e.Respond(HttpStatusCode.OK,
            new JsonObject { ["success"] = true, ["message"] = "Media updated." });
    }

    private static void HandleDelete(HttpRestEventArgs e, int id, string userName)
    {
        var media = MediaStore.GetById(id);
        if (media is null)
        {
            e.Respond(HttpStatusCode.NotFound,
                new JsonObject { ["success"] = false, ["reason"] = "Media not found." });
            return;
        }

        if (media.CreatedBy != userName)
        {
            e.Respond(HttpStatusCode.Forbidden,
                new JsonObject { ["success"] = false, ["reason"] = "Only creator may delete this media." });
            return;
        }

        MediaStore.Delete(id);

        e.Respond(HttpStatusCode.OK,
            new JsonObject { ["success"] = true, ["message"] = "Media deleted." });
    }
}
