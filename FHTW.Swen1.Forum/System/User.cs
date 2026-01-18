using System.Security.Cryptography;
using System.Text;

namespace FHTW.Swen1.Forum.System;

public sealed class User : Atom, IAtom
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
        // Only admin or the user themselves may load this user object.
        if (session is null || !session.Valid)
            throw new UnauthorizedAccessException("Invalid session.");

        if (!(session.IsAdmin || session.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)))
            throw new UnauthorizedAccessException("Admin or owner privileges required.");

        if (!UserStore.TryGet(userName, out var record) || record is null)
            throw new ArgumentException("User not found.");

        var user = new User(session)
        {
            FullName = record.FullName,
            EMail = record.EMail,
            FavoriteGenre = record.FavoriteGenre
        };

        user._UserName = record.UserName;
        user._New = false;
        user._PasswordHash = null;

        return user;
    }

    public string UserName
    {
        get { return _UserName ?? string.Empty; }
        set
        {
            if (!_New) { throw new InvalidOperationException("User name cannot be changed."); }
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("User name must not be empty."); }

            _UserName = value.Trim();
        }
    }

    internal static string _HashPassword(string userName, string password)
    {
        StringBuilder rval = new();
        foreach (byte i in SHA256.HashData(Encoding.UTF8.GetBytes(userName + password)))
        {
            rval.Append(i.ToString("x2"));
        }
        return rval.ToString();
    }

    public string FullName { get; set; } = string.Empty;

    public string EMail { get; set; } = string.Empty;

    // ✅ SPEC FIELD
    public string FavoriteGenre { get; set; } = string.Empty;

    public void SetPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password must not be empty.");

        _PasswordHash = _HashPassword(UserName, password);
    }

    public override void Save()
    {
        // Editing existing user => must be admin or owner
        if (!_New) { _EnsureAdminOrOwner(UserName); }

        if (string.IsNullOrWhiteSpace(UserName))
            throw new ArgumentException("Username must not be empty.");

        // Creating a new user requires a password
        if (_New && string.IsNullOrWhiteSpace(_PasswordHash))
            throw new ArgumentException("Password must be set for new users.");

        // If user exists and we are "new", then it's a duplicate registration
        if (_New && UserStore.Exists(UserName))
            throw new ArgumentException("Username already exists.");

        // If we're updating, we keep old password unless a new one was set
        string passwordHashToStore;

        if (UserStore.TryGet(UserName, out var existing) && existing is not null)
        {
            passwordHashToStore = string.IsNullOrWhiteSpace(_PasswordHash)
                ? existing.PasswordHash
                : _PasswordHash!;
        }
        else
        {
            passwordHashToStore = _PasswordHash!;
        }

        // ✅ MUST MATCH NEW UserRecord CONSTRUCTOR (FavoriteGenre added)
        var record = new UserStore.UserRecord(
            UserName: UserName,
            FullName: FullName ?? string.Empty,
            EMail: EMail ?? string.Empty,
            FavoriteGenre: FavoriteGenre ?? string.Empty,
            PasswordHash: passwordHashToStore
        );

        bool ok = _New
            ? UserStore.Create(record)
            : UserStore.Update(record);

        if (!ok)
            throw new InvalidOperationException("Could not save user.");

        _New = false;
        _PasswordHash = null;
        _EndEdit();
    }

    public override void Delete()
    {
        _EnsureAdminOrOwner(UserName);

        if (!UserStore.Delete(UserName))
            throw new ArgumentException("User not found.");

        _EndEdit();
    }

    public override void Refresh()
    {
        if (string.IsNullOrWhiteSpace(UserName))
            throw new ArgumentException("UserName is missing.");

        if (!UserStore.TryGet(UserName, out var record) || record is null)
            throw new ArgumentException("User not found.");

        FullName = record.FullName;
        EMail = record.EMail;
        FavoriteGenre = record.FavoriteGenre;

        _New = false;
        _PasswordHash = null;
        _EndEdit();
    }
}
