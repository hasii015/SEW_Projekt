using System.Collections.Concurrent;

namespace FHTW.Swen1.Forum.System;

public static class FavoriteStore
{
    private static readonly ConcurrentDictionary<string, HashSet<int>> _favorites = new();

    public static bool Add(string userName, int mediaId)
    {
        var key = userName.ToLowerInvariant();
        var set = _favorites.GetOrAdd(key, _ => new HashSet<int>());
        lock (set) { return set.Add(mediaId); }
    }

    public static bool Remove(string userName, int mediaId)
    {
        var key = userName.ToLowerInvariant();
        if (!_favorites.TryGetValue(key, out var set)) return false;
        lock (set) { return set.Remove(mediaId); }
    }

    public static List<int> Get(string userName)
    {
        var key = userName.ToLowerInvariant();
        if (!_favorites.TryGetValue(key, out var set)) return new();
        lock (set) { return set.ToList(); }
    }
}
