using FHTW.Swen1.Forum.System;
using Xunit;

namespace FHTW.Swen1.Forum.Tests;

public sealed class SessionTests : ResettableTestBase
{
    [Fact]
    public void Create_ReturnsValidSession_With24CharToken_FromAllowedAlphabet()
    {
        UserStore.Create(new UserStore.UserRecord(
            UserName: "alice",
            FullName: "Alice",
            EMail: "a@x",
            FavoriteGenre: "",
            PasswordHash: TestState.Hash("alice", "pw")
        ));

        var s = Session.Create("alice", "pw");
        Assert.NotNull(s);
        Assert.True(s!.Valid);
        Assert.Equal(24, s.Token.Length);

        const string alphabet = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        Assert.All(s.Token, c => Assert.Contains(c, alphabet));
    }
}
