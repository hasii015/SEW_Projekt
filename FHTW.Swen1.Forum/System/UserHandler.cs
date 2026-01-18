using System.Net;
using System.Text.Json.Nodes;

using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;



namespace FHTW.Swen1.Forum.System;

/// <summary>This class implements a Handler for user endpoints.</summary>
public sealed class UserHandler: Handler, IHandler
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // [override] Handler                                                                                               //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Handles a request if possible.</summary>
    /// <param name="e">Event arguments.</param>
    public override void Handle(HttpRestEventArgs e)
    {
        // SPEC: POST /api/users/register
        if (e.Path == "/api/users/register")
        {
            if (e.Method == HttpMethod.Post)
            {
                try
                {
                    string username = e.Content?["username"]?.GetValue<string>() ?? string.Empty;
                    string password = e.Content?["password"]?.GetValue<string>() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        e.Respond(HttpStatusCode.BadRequest,
                            new JsonObject() { ["success"] = false, ["reason"] = "username and password required." });
                    }
                    else
                    {
                        User user = new()
                        {
                            UserName = username,
                            FullName = string.Empty,
                            EMail = string.Empty
                        };

                        user.SetPassword(password);
                        user.Save();

                        // SPEC doesn't require a message, only success
                        e.Respond(HttpStatusCode.OK, new JsonObject() { ["success"] = true });
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"[UserHandler] Handled {e.Method} {e.Path}.");
                }
                catch (Exception ex)
                {
                    e.Respond(HttpStatusCode.InternalServerError,
                        new JsonObject() { ["success"] = false, ["reason"] = ex.Message });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[UserHandler] Exception creating user. {e.Method} {e.Path}: {ex.Message}");
                }
            }
            else
            {
                e.Respond(HttpStatusCode.MethodNotAllowed,
                    new JsonObject() { ["success"] = false, ["reason"] = "Method not allowed." });

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[UserHandler] Method not allowed: {e.Method} {e.Path}");
            }

            e.Responded = true;
            return;
        }
    }

}
