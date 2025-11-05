using System.Net;

namespace FHTW.Swen1.Forum.Server;

public sealed class HttpRestServer: IDisposable
{
    private readonly HttpListener _Listener;


    public HttpRestServer(int port = 12000)
    {
        _Listener = new();
        _Listener.Prefixes.Add($"http://+:{port}/");
    }


    public event EventHandler<HttpRestEventArgs> RequestReceived;


    public bool Running
    {
        get; private set;
    }


    public void Stop()
    {
        _Listener.Close();
        Running = false;
    }


    public void Run()
    {
        if(Running) return;

        _Listener.Start();
        Running = true;

        while(Running)
        {
            HttpListenerContext context = _Listener.GetContext();

            _ = Task.Run(() =>
            {
                HttpRestEventArgs args = new(context);
                RequestReceived?.Invoke(this, args);

                if(!args.Responded)
                {
                    args.Respond((int) HttpStatusCode.NotFound, new() { ["success"] = false, ["reason"] = "Not found." });
                }
            });
        }
    }


    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
