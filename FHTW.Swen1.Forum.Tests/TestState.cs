using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using FHTW.Swen1.Forum.System;

namespace FHTW.Swen1.Forum.Tests;

internal static class TestState
{
    public static void ResetAll()
    {
        ResetMediaStore();
        ResetRatingStore();
        ResetFavoriteStore();
        ResetSessionStore();
        ResetUserStore();
    }

    private static void ResetMediaStore()
    {
        var mediaField = typeof(MediaStore).GetField("_media", BindingFlags.NonPublic | BindingFlags.Static)!;
        var nextIdField = typeof(MediaStore).GetField("_nextId", BindingFlags.NonPublic | BindingFlags.Static)!;

        var dict = (ConcurrentDictionary<int, MediaEntry>)mediaField.GetValue(null)!;
        dict.Clear();
        nextIdField.SetValue(null, 1);
    }

    private static void ResetRatingStore()
    {
        var ratingsField = typeof(RatingStore).GetField("_ratings", BindingFlags.NonPublic | BindingFlags.Static)!;
        var indexField = typeof(RatingStore).GetField("_index", BindingFlags.NonPublic | BindingFlags.Static)!;
        var nextIdField = typeof(RatingStore).GetField("_NextId", BindingFlags.NonPublic | BindingFlags.Static)!;

        ((ConcurrentDictionary<int, Rating>)ratingsField.GetValue(null)!).Clear();
        ((ConcurrentDictionary<string, int>)indexField.GetValue(null)!).Clear();
        nextIdField.SetValue(null, 1);
    }

    private static void ResetFavoriteStore()
    {
        var favoritesField = typeof(FavoriteStore).GetField("_favorites", BindingFlags.NonPublic | BindingFlags.Static)!;
        ((ConcurrentDictionary<string, HashSet<int>>)favoritesField.GetValue(null)!).Clear();
    }

    private static void ResetSessionStore()
    {
        var sessionsField = typeof(Session).GetField("_Sessions", BindingFlags.NonPublic | BindingFlags.Static)!;
        var dict = (Dictionary<string, Session>)sessionsField.GetValue(null)!;
        lock (dict)
        {
            dict.Clear();
        }
    }

    private static void ResetUserStore()
    {
        var usersField = typeof(UserStore).GetField("_users", BindingFlags.NonPublic | BindingFlags.Static)!;
        var dict = (ConcurrentDictionary<string, UserStore.UserRecord>)usersField.GetValue(null)!;
        dict.Clear();

        dict.TryAdd("admin", new UserStore.UserRecord(
            UserName: "admin",
            FullName: "Administrator",
            EMail: "admin@local",
            FavoriteGenre: "",
            PasswordHash: Hash("admin", "admin")
        ));
    }

    internal static string Hash(string userName, string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(userName + password));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
