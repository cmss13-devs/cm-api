using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CmApi.Classes;


/// <summary>
/// Sent from a form, the data for a request to add a note to the database.
/// </summary>
public class AddNoteRequest
{
    /// <summary>
    /// The CKEY that this note will be attached to.
    /// </summary>
    public required string Ckey { get; set; }
    
    /// <summary>
    /// The message that appears on this note.
    /// </summary>
    public required string Message { get; set; }
    
    /// <summary>
    /// The note category this appears in. 1 - Admin, 2 - Merit, 3 - Commander, 4 - Synthetic, 5 - Yautja
    /// </summary>
    public required int Category { get; set; }
    
    /// <summary>
    /// If the player can see this note.
    /// </summary>
    public required bool Confidential { get; set; }
}

