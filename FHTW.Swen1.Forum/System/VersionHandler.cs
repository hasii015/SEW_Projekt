using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;

using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;



namespace FHTW.Swen1.Forum.System;

/// <summary>This class implements a Handler for version endpoints.</summary>
public sealed class VersionHandler: Handler, IHandler
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // [override] Handler                                                                                               //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    /// <summary>Handles a request if possible.</summary>
    /// <param name="e">Event arguments.</param>
    public override void Handle(HttpRestEventArgs e)
    {
        if(e.Path.StartsWith("/version"))
        {
            if((e.Path == "/version") && (e.Method == HttpMethod.Get))
            {
                e.Respond(HttpStatusCode.OK, new JsonObject() 
                          { ["success"] = true, ["version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown" });

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method.ToString()} {e.Path}.");
            }
            else
            {
                e.Respond(HttpStatusCode.BadRequest, new JsonObject(){ ["success"] = false, ["reason"] = "Invalid version endpoint." });

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{nameof(VersionHandler)} Invalid session endpoint.");
            }

            e.Responded = true;
        }
    }
}
