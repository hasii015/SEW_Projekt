using FHTW.Swen1.Forum.System;
using Xunit;

namespace FHTW.Swen1.Forum.Tests;

public sealed class FavoriteStoreTests : ResettableTestBase
{
    [Fact]
    public void Add_IsCaseInsensitive_AndDoesNotDuplicate()
    {
        Assert.True(FavoriteStore.Add("Alice", 10));
        Assert.False(FavoriteStore.Add("alice", 10));

        var favs = FavoriteStore.Get("ALICE");
        Assert.Single(favs);
        Assert.Contains(10, favs);
    }

    [Fact]
    public void Remove_ReturnsFalse_WhenUserOrMediaMissing()
    {
        Assert.False(FavoriteStore.Remove("Bob", 999));
    }
}
