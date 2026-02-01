using FHTW.Swen1.Forum.System;
using Xunit;

namespace FHTW.Swen1.Forum.Tests;

public sealed class RecommendationServiceTests : ResettableTestBase
{
    [Fact]
    public void GetRecommendations_ByGenre_ExcludesRatedAndOrdersByScoreThenTitle()
    {
        var liked1 = MediaStore.Add(new MediaEntry { Title = "Movie A", Genres = "sci-fi, action", MediaType = "movie", AgeRestriction = 12 });
        var liked2 = MediaStore.Add(new MediaEntry { Title = "Movie B", Genres = "sci-fi", MediaType = "movie", AgeRestriction = 12 });

        var ratedButNotRecommended = MediaStore.Add(new MediaEntry { Title = "Already Rated", Genres = "sci-fi", MediaType = "movie", AgeRestriction = 12 });

        var top = MediaStore.Add(new MediaEntry { Title = "Flick", Genres = "sci-fi, action", MediaType = "movie", AgeRestriction = 12 }); // score 3
        var tie1 = MediaStore.Add(new MediaEntry { Title = "Alpha", Genres = "sci-fi", MediaType = "movie", AgeRestriction = 12 });         // score 2
        var tie2 = MediaStore.Add(new MediaEntry { Title = "Zulu", Genres = "sci-fi", MediaType = "movie", AgeRestriction = 12 });          // score 2
        var noMatch = MediaStore.Add(new MediaEntry { Title = "Drama", Genres = "drama", MediaType = "movie", AgeRestriction = 12 });

        RatingStore.AddOrUpdate(liked1.Id, "alice", 5, "");
        RatingStore.AddOrUpdate(liked2.Id, "alice", 4, "");
        RatingStore.AddOrUpdate(ratedButNotRecommended.Id, "alice", 3, "");

        var recs = RecommendationService.GetRecommendations("alice", "genre");

        Assert.DoesNotContain(recs, m => m.Id == ratedButNotRecommended.Id);
        Assert.DoesNotContain(recs, m => m.Id == liked1.Id || m.Id == liked2.Id);
        Assert.DoesNotContain(recs, m => m.Id == noMatch.Id);

        Assert.Equal(new[] { top.Id, tie1.Id, tie2.Id }, recs.Select(m => m.Id).ToArray());
    }
}
