using FHTW.Swen1.Forum.System;
using Xunit;

namespace FHTW.Swen1.Forum.Tests;

public sealed class MediaStoreTests : ResettableTestBase
{
    [Fact]
    public void Add_AssignsIncrementingIds_AndStoresEntry()
    {
        var m1 = MediaStore.Add(new MediaEntry { Title = "A" });
        var m2 = MediaStore.Add(new MediaEntry { Title = "B" });

        Assert.Equal(1, m1.Id);
        Assert.Equal(2, m2.Id);

        Assert.Equal("A", MediaStore.GetById(1)!.Title);
        Assert.Equal("B", MediaStore.GetById(2)!.Title);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenEntryDoesNotExist()
    {
        var ok = MediaStore.Update(new MediaEntry { Id = 999, Title = "Nope" });
        Assert.False(ok);
    }

    [Fact]
    public void Delete_RemovesEntry_AndGetByIdReturnsNull()
    {
        var m = MediaStore.Add(new MediaEntry { Title = "DeleteMe" });

        Assert.True(MediaStore.Delete(m.Id));
        Assert.Null(MediaStore.GetById(m.Id));
    }
}
