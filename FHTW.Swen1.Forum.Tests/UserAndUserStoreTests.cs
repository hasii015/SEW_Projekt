using FHTW.Swen1.Forum.System;
using Xunit;

namespace FHTW.Swen1.Forum.Tests;

public sealed class UserAndUserStoreTests : ResettableTestBase
{
    [Fact]
    public void UserStore_Create_AndVerifyCredentials_Works()
    {
        var record = new UserStore.UserRecord(
            UserName: "berta",
            FullName: "Berta",
            EMail: "berta@example.com",
            FavoriteGenre: "sci-fi",
            PasswordHash: TestState.Hash("berta", "pw")
        );

        Assert.True(UserStore.Create(record));
        Assert.True(UserStore.VerifyCredentials("berta", "pw"));
        Assert.False(UserStore.VerifyCredentials("berta", "wrong"));
    }

    [Fact]
    public void User_Save_NewUserRequiresPassword()
    {
        var u = new User();
        u.UserName = "alice";
        u.FullName = "Alice Example";

        var ex = Assert.Throws<ArgumentException>(() => u.Save());
        Assert.Contains("Password must be set", ex.Message);
    }

    [Fact]
    public void User_Get_RequiresAdminOrOwner()
    {
        UserStore.Create(new UserStore.UserRecord(
            UserName: "alice",
            FullName: "Alice",
            EMail: "a@x",
            FavoriteGenre: "",
            PasswordHash: TestState.Hash("alice", "pw")
        ));
        UserStore.Create(new UserStore.UserRecord(
            UserName: "bob",
            FullName: "Bob",
            EMail: "b@x",
            FavoriteGenre: "",
            PasswordHash: TestState.Hash("bob", "pw")
        ));

        var bobSession = Session.Create("bob", "pw")!;
        Assert.Throws<UnauthorizedAccessException>(() => User.Get("alice", bobSession));
    }

    [Fact]
    public void UserName_CannotBeChanged_AfterUserWasSaved()
    {
        var u = new User();
        u.UserName = "alice";
        u.SetPassword("pw");
        u.Save();

        Assert.Throws<InvalidOperationException>(() => u.UserName = "newname");
    }
}
