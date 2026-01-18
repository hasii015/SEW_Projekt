using System.Collections.Concurrent;

namespace FHTW.Swen1.Forum.System;

public static class UserStore
{
    // Username -> stored user data
    private static readonly ConcurrentDictionary<string, UserRecord> _users =
        new(StringComparer.OrdinalIgnoreCase);

    // Seed an admin account so you can always login as admin/admin while testing
    static UserStore()
    {
        if (!_users.ContainsKey("admin"))
        {
            _users["admin"] = new UserRecord(
                UserName: "admin",
                FullName: "Administrator",
                EMail: "admin@local",
                FavoriteGenre: "",
                PasswordHash: User._HashPassword("admin", "admin")
            );
        }
    }

    public static bool Exists(string userName)
        => _users.ContainsKey(userName);

    public static bool TryGet(string userName, out UserRecord? record)
    {
        if (_users.TryGetValue(userName, out var r))
        {
            record = r;
            return true;
        }

        record = null;
        return false;
    }

    /// <summary>
    /// Register: only succeeds if username not taken.
    /// </summary>
    public static bool Create(UserRecord record)
        => _users.TryAdd(record.UserName, record);

    /// <summary>
    /// Update profile (and optionally password).
    /// </summary>
    public static bool Update(UserRecord record)
    {
        if (!_users.ContainsKey(record.UserName)) return false;
        _users[record.UserName] = record;
        return true;
    }

    public static bool Delete(string userName)
        => _users.TryRemove(userName, out _);

    public static bool VerifyCredentials(string userName, string password)
    {
        if (!_users.TryGetValue(userName, out var record)) return false;

        string hash = User._HashPassword(userName, password);
        return string.Equals(hash, record.PasswordHash, StringComparison.OrdinalIgnoreCase);
    }

    // Stored user data
    public sealed record UserRecord(
        string UserName,
        string FullName,
        string EMail,
        string FavoriteGenre,
        string PasswordHash
    );
}
