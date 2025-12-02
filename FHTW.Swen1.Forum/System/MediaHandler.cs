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
        if (!e.Path.StartsWith("/media"))
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
        var segments = e.Path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

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
        var items = MediaStore.GetAll();
        var arr = JsonSerializer.SerializeToNode(items) as JsonArray ?? new JsonArray();

        var result = new JsonObject
        {
            ["success"] = true,
            ["items"] = arr
        };

        e.Respond(HttpStatusCode.OK, result);
    }


    private static void HandleCreate(HttpRestEventArgs e, string userName)
    {
        var media = new MediaEntry
        {
            Title = e.Content?["title"]?.GetValue<string>() ?? string.Empty,
            Description = e.Content?["description"]?.GetValue<string>() ?? string.Empty,
            MediaType = e.Content?["mediatype"]?.GetValue<string>() ?? string.Empty,
            ReleaseYear = e.Content?["releaseYear"]?.GetValue<int>() ?? 0,
            Genres = e.Content?["genres"]?.GetValue<string>() ?? string.Empty,
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
