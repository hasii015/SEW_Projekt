using FHTW.Swen1.Forum.System;
using Xunit;

namespace FHTW.Swen1.Forum.Tests;

public sealed class RatingStoreTests : ResettableTestBase
{
    [Fact]
    public void AddOrUpdate_CreatesNewRating_UnconfirmedWithCorrectFields()
    {
        var rating = RatingStore.AddOrUpdate(mediaId: 5, userName: "Alice", stars: 4, comment: "nice");

        Assert.True(rating.Id > 0);
        Assert.Equal(5, rating.MediaId);
        Assert.Equal("Alice", rating.CreatedBy);
        Assert.Equal(4, rating.Stars);
        Assert.Equal("nice", rating.Comment);
        Assert.False(rating.IsConfirmed);
    }

    [Fact]
    public void AddOrUpdate_UpdatesExistingRating_ForSameMediaAndUser_ResetsConfirmed()
    {
        var r1 = RatingStore.AddOrUpdate(7, "Alice", 5, "great");
        Assert.False(r1.IsConfirmed);

        RatingStore.Confirm(r1.Id, "Alice");
        Assert.True(RatingStore.Get(r1.Id)!.IsConfirmed);

        var r2 = RatingStore.AddOrUpdate(7, "ALICE", 2, "changed");

        Assert.Equal(r1.Id, r2.Id);
        Assert.Equal(2, r2.Stars);
        Assert.Equal("changed", r2.Comment);
        Assert.False(r2.IsConfirmed); 
    }

    [Fact]
    public void GetForUser_ReturnsRatingsInDescendingIdOrder()
    {
        var r1 = RatingStore.AddOrUpdate(1, "Alice", 3, "");
        var r2 = RatingStore.AddOrUpdate(2, "Alice", 4, "");
        var r3 = RatingStore.AddOrUpdate(3, "Alice", 5, "");

        var list = RatingStore.GetForUser("alice");
        Assert.Equal(new[] { r3.Id, r2.Id, r1.Id }, list.Select(r => r.Id).ToArray());
    }

    [Fact]
    public void Confirm_FailsForNonOwner_AndDoesNotChangeState()
    {
        var r = RatingStore.AddOrUpdate(1, "Alice", 5, "");
        var ok = RatingStore.Confirm(r.Id, "Bob");

        Assert.False(ok);
        Assert.False(RatingStore.Get(r.Id)!.IsConfirmed);
    }

    [Fact]
    public void Like_FailsWhenUserLikesOwnRating()
    {
        var r = RatingStore.AddOrUpdate(1, "Alice", 5, "");
        Assert.False(RatingStore.Like(r.Id, "Alice"));
    }

    [Fact]
    public void Like_AddsOnlyOnce_PerUser_AndIsCaseInsensitive()
    {
        var r = RatingStore.AddOrUpdate(1, "Alice", 5, "");

        Assert.True(RatingStore.Like(r.Id, "Bob"));
        Assert.False(RatingStore.Like(r.Id, "BOB"));

        var stored = RatingStore.Get(r.Id)!;
        Assert.Single(stored.LikedBy);
        Assert.Contains("bob", stored.LikedBy);
    }

    [Fact]
    public void Delete_FailsForNonOwnerAndNonAdmin()
    {
        var r = RatingStore.AddOrUpdate(1, "Alice", 5, "");
        Assert.False(RatingStore.Delete(r.Id, userName: "Bob", isAdmin: false));
        Assert.NotNull(RatingStore.Get(r.Id));
    }

    [Fact]
    public void Delete_SucceedsForAdmin()
    {
        var r = RatingStore.AddOrUpdate(1, "Alice", 5, "");
        Assert.True(RatingStore.Delete(r.Id, userName: "admin", isAdmin: true));
        Assert.Null(RatingStore.Get(r.Id));
    }

    [Fact]
    public void GetForMedia_OnlyConfirmedFilter_Works()
    {
        var r1 = RatingStore.AddOrUpdate(42, "Alice", 5, "");
        var r2 = RatingStore.AddOrUpdate(42, "Bob", 4, "");

        RatingStore.Confirm(r1.Id, "Alice");

        var confirmedOnly = RatingStore.GetForMedia(42, onlyConfirmed: true);
        Assert.Single(confirmedOnly);
        Assert.Equal(r1.Id, confirmedOnly[0].Id);

        var all = RatingStore.GetForMedia(42, onlyConfirmed: false);
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void GetLeaderboard_OrdersByCountDesc_ThenUserNameAsc()
    {
        RatingStore.AddOrUpdate(1, "Bob", 3, "");
        RatingStore.AddOrUpdate(2, "Bob", 4, "");
        RatingStore.AddOrUpdate(3, "Alice", 5, "");
        RatingStore.AddOrUpdate(4, "Alice", 2, "");
        RatingStore.AddOrUpdate(5, "Charlie", 1, "");

        var lb = RatingStore.GetLeaderboard();

        Assert.Equal(("Alice", 2), lb[0]);
        Assert.Equal(("Bob", 2), lb[1]);
        Assert.Equal(("Charlie", 1), lb[2]);
    }
}
