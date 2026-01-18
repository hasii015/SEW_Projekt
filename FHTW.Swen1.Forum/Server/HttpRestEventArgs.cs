using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

using FHTW.Swen1.Forum.System;

namespace FHTW.Swen1.Forum.Server;

/// <summary>This class defines event arguments for the <see cref="HttpRestServer.RequestReceived"/> event.</summary>
public class HttpRestEventArgs : EventArgs
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // constructors                                                                                                     //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Creates a new instance of this class.</summary>
    /// <param name="context">HTTP listener context.</param>
    public HttpRestEventArgs(HttpListenerContext context)
    {
        Context = context;

        Method = HttpMethod.Parse(context.Request.HttpMethod);
        Path = context.Request.Url?.AbsolutePath ?? string.Empty;

        // Query support added here
        Query = context.Request.QueryString.AllKeys
            .Where(k => k != null)
            .ToDictionary(
                k => k!,
                k => context.Request.QueryString[k!] ?? string.Empty
            );

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"Received: {Method} {Path}");

        if (context.Request.HasEntityBody)
        {
            using Stream input = context.Request.InputStream;
            using StreamReader re = new(input, context.Request.ContentEncoding);
            Body = re.ReadToEnd();

            try
            {
                Content = JsonNode.Parse(Body)?.AsObject() ?? new JsonObject();
            }
            catch (Exception ex)
            {
                Console.WriteLine("JSON parse failed: " + ex.Message);
                Console.WriteLine("BODY >>> " + Body);

                Content = new JsonObject();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Body);
        }
        else
        {
            Body = string.Empty;
            Content = new JsonObject();
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public properties                                                                                                //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Gets the underlying HTTP listener context object.</summary>
    public HttpListenerContext Context { get; }

    /// <summary>Gets the HTTP method for the request.</summary>
    public HttpMethod Method { get; }

    /// <summary>Gets the path for the request.</summary>
    public string Path { get; }

    /// <summary> Gets the query parameters for the request.</summary>
    public IReadOnlyDictionary<string, string> Query { get; }

    /// <summary>Gets the request body.</summary>
    public string Body { get; }

    /// <summary>Gets the request JSON content.</summary>
    public JsonObject Content { get; }

    /// <summary>Gets the session from the request.</summary>
    public Session? Session
    {
        get
        {
            if (Path == "/api/users/login" || Path == "/api/users/register")
            {
                return null;
            }

            string raw = Context.Request.Headers["Authorization"] ?? string.Empty;
            Console.WriteLine($"[Auth] Authorization header raw: '{raw}'");

            if (string.IsNullOrWhiteSpace(raw))
            {
                Console.WriteLine("[Auth] Missing Authorization header");
                return null;
            }

            string token = raw;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token[7..].Trim();
                Console.WriteLine($"[Auth] Parsed token: '{token}'");
            }
            else
            {
                Console.WriteLine("[Auth] Header did not start with 'Bearer'");
                return null;
            }

            var session = Session.Get(token);
            Console.WriteLine(session is null
                ? "[Auth] No session found for this token"
                : "[Auth] Session found");

            return session;
        }
    }

    /// <summary>Gets or sets if the request has been responded to.</summary>
    public bool Responded { get; set; } = false;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public methods                                                                                                   //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Sends a response to the request.</summary>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="content">Response message JSON content.</param>
    public void Respond(HttpStatusCode statusCode, JsonObject? content)
    {
        HttpListenerResponse response = Context.Response;
        response.StatusCode = (int)statusCode;
        string rstr = content?.ToString() ?? string.Empty;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Responding: {statusCode}: {rstr}\n\n");

        byte[] buf = Encoding.UTF8.GetBytes(rstr);
        response.ContentLength64 = buf.Length;
        response.ContentType = "application/json; charset=UTF-8";

        using Stream output = response.OutputStream;
        output.Write(buf, 0, buf.Length);
        output.Close();

        Responded = true;
    }
}
