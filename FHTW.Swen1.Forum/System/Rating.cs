namespace FHTW.Swen1.Forum.System;

/// <summary>
/// Represents a rating given by a user to a specific media entry.
/// </summary>
public sealed class Rating
{
    public int Id { get; set; }

    public int MediaId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public int Stars { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsConfirmed { get; set; } = true;  
    public int Likes { get; set; } = 0;
}
