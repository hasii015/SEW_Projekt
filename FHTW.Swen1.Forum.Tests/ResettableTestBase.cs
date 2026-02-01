namespace FHTW.Swen1.Forum.Tests;

public abstract class ResettableTestBase
{
    protected ResettableTestBase()
    {
        TestState.ResetAll();
    }
}
