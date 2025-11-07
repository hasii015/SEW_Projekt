using FHTW.Swen1.Forum.Server;



namespace FHTW.Swen1.Forum.Handlers;

public interface IHandler
{
    public void Handle(HttpRestEventArgs e);
}
