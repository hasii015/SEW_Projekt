namespace FHTW.Swen1.Forum.System;

public sealed class MediaEntry
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public string Genres { get; set; } = string.Empty;

    public int AgeRestriction { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
