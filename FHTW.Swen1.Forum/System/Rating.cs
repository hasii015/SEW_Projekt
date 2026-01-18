namespace FHTW.Swen1.Forum.System;

/// <summary>
/// Represents a rating given by a user to a specific media entry.
/// </summary>
public sealed class Rating
{
    public int Id { get; set; }
    public int MediaId { get; set; }
    public string CreatedBy { get; set; } = "";

    public int Stars { get; set; }            
    public string Comment { get; set; } = "";  
    public DateTime Timestamp { get; set; }     

    public bool IsConfirmed { get; set; } = false;

    public HashSet<string> LikedBy { get; set; } = new();
}
