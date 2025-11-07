using FHTW.Swen1.Forum.Handlers;
using FHTW.Swen1.Forum.Server;

namespace FHTW.Swen1.Forum;

/// <summary>Program class.</summary>
internal static class Program
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // entry point                                                                                                      //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    /// <summary>Main entry point of the application.</summary>
    /// <param name="args">Command line arguments.</param>
    static void Main(string[] args)
    {
        HttpRestServer svr = new();
        svr.RequestReceived += Handler.HandleEvent;
        svr.Run();
    }
}
