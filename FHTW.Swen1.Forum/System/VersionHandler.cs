using System.Net;
using System.Text.Json.Nodes;

using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum.System;

public sealed class VersionHandler: Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if(e.Path.StartsWith("/version"))
        {
            if((e.Path == "/version") && (e.Method == HttpMethod.Get))
            {

            }
            else
            {
                e.Respond(HttpStatusCode.BadRequest, new JsonObject(){ ["success"] = false, ["reason"] = "Invalid version endpoint." });

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{nameof(VersionHandler)} Invalid session endpoint.");
            }
        }

        e.Responded = true;
    }
}
