using System.Security.Cryptography;
using System.Text;



namespace FHTW.Swen1.Forum.System;

public sealed class User: Atom, IAtom
{
    private string? _UserName = null;

    private bool _New;

    private string? _PasswordHash = null;



    public User(Session? session = null)
    {
        _EditingSession = session;
        _New = true;
    }


    public static User Get(string userName, Session? session = null)
    {
        // TODO: load user and return if admin or owner.
        throw new NotImplementedException();
    }


    public string UserName
    {
        get { return _UserName ?? string.Empty; }
        set 
        {
            if(!_New) { throw new InvalidOperationException("User name cannot be changed."); }
            if(string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("User name must not be empty."); }
            
            _UserName = value; 
        }
    }

    internal static string _HashPassword(string userName, string password)
    {
        StringBuilder rval = new();
        foreach(byte i in SHA256.HashData(Encoding.UTF8.GetBytes(userName + password)))
        {
            rval.Append(i.ToString("x2"));
        }
        return rval.ToString();
    }

    public string FullName
    {
        get; set;
    } = string.Empty;


    public string EMail
    {
        get; set;
    } = string.Empty;


    public void SetPassword(string password)
    {
        _PasswordHash = _HashPassword(UserName, password);
    }

    public override void Save()
    {
        if(!_New) { _EnsureAdminOrOwner(UserName); }

        // TODO: save user to database
        _PasswordHash = null;
        _EndEdit();
    }

    public override void Delete()
    {
        _EnsureAdminOrOwner(UserName);

        // TODO: delete user from database

        _EndEdit();
    }

    public override void Refresh()
    {
        // TODO: refresh user from database
        _EndEdit();
    }
}
