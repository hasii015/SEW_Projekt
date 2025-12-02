using System.Collections.Concurrent;

namespace FHTW.Swen1.Forum.System;

public static class MediaStore
{
    private static readonly ConcurrentDictionary<int, MediaEntry> _media = new();
    private static int _nextId = 1;

    public static MediaEntry Add(MediaEntry entry)
    {
        entry.Id = _nextId++;
        _media[entry.Id] = entry;
        return entry;
    }

    public static ICollection<MediaEntry> GetAll()
    => _media.Values;

    public static MediaEntry? GetById(int id)
        => _media.TryGetValue(id, out var m) ? m : null;

    public static bool Update(MediaEntry entry)
    {
        if (!_media.ContainsKey(entry.Id)) return false;
        _media[entry.Id] = entry;
        return true;
    }

    public static bool Delete(int id)
        => _media.TryRemove(id, out _);
}
