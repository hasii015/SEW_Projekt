using global::System.Collections.Generic;
using global::System.Linq;

namespace FHTW.Swen1.Forum.System;

public static class RecommendationService
{
    public static List<MediaEntry> GetRecommendations(string userName, string type)
    {
        var allMedia = MediaStore.GetAll();
        var userRatings = RatingStore.GetForUser(userName);

        var ratedMediaIds = userRatings.Select(r => r.MediaId).ToHashSet();

        var likedMediaIds = userRatings
            .Where(r => r.Stars >= 4)
            .Select(r => r.MediaId)
            .ToList();

        if (likedMediaIds.Count == 0)
            return new List<MediaEntry>();

        var likedMedia = likedMediaIds
            .Select(id => MediaStore.GetById(id))
            .Where(m => m is not null)
            .Cast<MediaEntry>()
            .ToList();

        var candidates = allMedia
            .Where(m => !ratedMediaIds.Contains(m.Id))
            .ToList();

        if (type == "genre")
            return RecommendByGenre(likedMedia, candidates);

        return RecommendByContentSimilarity(likedMedia, candidates);
    }

    private static List<MediaEntry> RecommendByGenre(List<MediaEntry> likedMedia, List<MediaEntry> candidates)
    {
        var genreScore = new Dictionary<string, int>();

        foreach (var m in likedMedia)
        {
            foreach (var g in SplitGenres(m.Genres))
            {
                if (!genreScore.ContainsKey(g)) genreScore[g] = 0;
                genreScore[g]++;
            }
        }

        return candidates
            .Select(m => new
            {
                Media = m,
                Score = SplitGenres(m.Genres).Sum(g => genreScore.TryGetValue(g, out int v) ? v : 0)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Media.Title)
            .Take(10)
            .Select(x => x.Media)
            .ToList();
    }

    private static List<MediaEntry> RecommendByContentSimilarity(List<MediaEntry> likedMedia, List<MediaEntry> candidates)
    {
        return candidates
            .Select(m => new
            {
                Media = m,
                Score = likedMedia.Max(baseMedia => SimilarityScore(baseMedia, m))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Media.Title)
            .Take(10)
            .Select(x => x.Media)
            .ToList();
    }

    private static int SimilarityScore(MediaEntry a, MediaEntry b)
    {
        int score = 0;

        if (a.MediaType.Equals(b.MediaType, global::System.StringComparison.OrdinalIgnoreCase))
            score += 2;

        if (a.AgeRestriction == b.AgeRestriction)
            score += 1;

        var ga = SplitGenres(a.Genres);
        var gb = SplitGenres(b.Genres);

        score += ga.Intersect(gb).Count();

        return score;
    }

    private static List<string> SplitGenres(string genres)
    {
        return (genres ?? "")
            .Split(',', global::System.StringSplitOptions.RemoveEmptyEntries | global::System.StringSplitOptions.TrimEntries)
            .Select(g => g.ToLowerInvariant())
            .ToList();
    }
}
