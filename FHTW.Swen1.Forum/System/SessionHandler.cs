using System.Net;
using System.Text.Json.Nodes;

using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;



namespace FHTW.Swen1.Forum.System;

/// <summary>This class implements a Handler for session endpoints.</summary>
public sealed class SessionHandler: Handler, IHandler
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // [override] Handler                                                                                               //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Handles a request if possible.</summary>
    /// <param name="e">Event arguments.</param>
    public override void Handle(HttpRestEventArgs e)
    {
        // SPEC: POST /api/users/login
        if (e.Path == "/api/users/login")
        {
            if (e.Method == HttpMethod.Post)
            {
                try
                {
                    string username = e.Content?["username"]?.GetValue<string>() ?? string.Empty;
                    string password = e.Content?["password"]?.GetValue<string>() ?? string.Empty;

                    Session? session = Session.Create(username, password);

                    if (session is null)
                    {
                        e.Respond(HttpStatusCode.Unauthorized,
                            new JsonObject() { ["success"] = false, ["reason"] = "Invalid username or password." });

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[SessionHandler] Invalid login attempt. {e.Method} {e.Path}.");
                    }
                    else
                    {
                        e.Respond(HttpStatusCode.OK,
                            new JsonObject() { ["success"] = true, ["token"] = session.Token });

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"[SessionHandler] Handled {e.Method} {e.Path}.");
                    }
                }
                catch (Exception ex)
                {
                    e.Respond(HttpStatusCode.InternalServerError,
                        new JsonObject() { ["success"] = false, ["reason"] = ex.Message });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[SessionHandler] Exception creating session. {e.Method} {e.Path}: {ex.Message}");
                }
            }
            else
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject() { ["success"] = false, ["reason"] = "Method not allowed." });

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[SessionHandler] Method not allowed: {e.Method} {e.Path}");
            }

            e.Responded = true;
            return;
        }
    }
}
