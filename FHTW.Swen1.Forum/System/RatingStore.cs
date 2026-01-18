using System.Collections.Concurrent;

namespace FHTW.Swen1.Forum.System;

public static class RatingStore
{
    private static int _NextId = 1;
    private static readonly ConcurrentDictionary<int, Rating> _ratings = new();
    private static readonly ConcurrentDictionary<string, int> _index = new();

    private static string Key(int mediaId, string userName)
        => $"{mediaId}:{userName.ToLowerInvariant()}";

    public static List<Rating> GetForUser(string userName)
    {
        return _ratings.Values
            .Where(r => r.CreatedBy.Equals(userName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Id)
            .ToList();
    }


    public static Rating AddOrUpdate(int mediaId, string userName, int stars, string comment)
    {
        var key = Key(mediaId, userName);

        if (_index.TryGetValue(key, out int existingId) &&
            _ratings.TryGetValue(existingId, out var existing))
        {
            existing.Stars = stars;
            existing.Comment = comment;
            existing.IsConfirmed = false;
            return existing;
        }

        var rating = new Rating
        {
            Id = Interlocked.Increment(ref _NextId),
            MediaId = mediaId,
            CreatedBy = userName,
            Stars = stars,
            Comment = comment,
            IsConfirmed = false
        };

        _ratings[rating.Id] = rating;
        _index[key] = rating.Id;

        return rating;
    }

    public static List<Rating> GetForMedia(int mediaId, bool onlyConfirmed = true)
    {
        return _ratings.Values
            .Where(r => r.MediaId == mediaId && (!onlyConfirmed || r.IsConfirmed))
            .OrderByDescending(r => r.Id)
            .ToList();
    }

    public static Rating? Get(int ratingId)
        => _ratings.TryGetValue(ratingId, out var r) ? r : null;

    public static bool Confirm(int ratingId, string userName)
    {
        var r = Get(ratingId);
        if (r is null) return false;
        if (!r.CreatedBy.Equals(userName, StringComparison.OrdinalIgnoreCase)) return false;

        r.IsConfirmed = true;
        return true;
    }

    public static bool Like(int ratingId, string userName)
    {
        var r = Get(ratingId);
        if (r is null) return false;
        if (r.CreatedBy.Equals(userName, StringComparison.OrdinalIgnoreCase)) return false;

        return r.LikedBy.Add(userName.ToLowerInvariant());
    }

    public static bool Delete(int ratingId, string userName, bool isAdmin)
    {
        var r = Get(ratingId);
        if (r is null) return false;
        if (!isAdmin && !r.CreatedBy.Equals(userName, StringComparison.OrdinalIgnoreCase)) return false;

        _ratings.TryRemove(ratingId, out _);
        _index.TryRemove(Key(r.MediaId, r.CreatedBy), out _);
        return true;
    }

    public static List<(string UserName, int RatingCount)> GetLeaderboard()
    {
        return _ratings.Values
            .GroupBy(r => r.CreatedBy, StringComparer.OrdinalIgnoreCase)
            .Select(g => (UserName: g.Key, RatingCount: g.Count()))
            .OrderByDescending(x => x.RatingCount)
            .ThenBy(x => x.UserName)
            .ToList();
    }

}
