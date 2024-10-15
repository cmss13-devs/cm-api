namespace CmApi.Records;

/// <summary>
/// A ticket action, as held in the database.
/// </summary>
public record Ticket
{
    /// <summary>
    /// The ID of the action.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Which ticket number the action belongs to.
    /// </summary>
    public int TicketId { get; set; }
    /// <summary>
    /// The action type taken.
    /// </summary>
    public required string Action { get; set; }
    /// <summary>
    /// The message involved with the action.
    /// </summary>
    public required string Message { get; set; }
    /// <summary>
    /// Who this was sent to.
    /// </summary>
    public string? Recipient { get; set; }
    /// <summary>
    /// Who sent this.
    /// </summary>
    public string? Sender { get; set; }
    /// <summary>
    /// The Round ID that this was sent in.
    /// </summary>
    public int? RoundId { get; set; }
    /// <summary>
    /// The time that this was sent.
    /// </summary>
    public required DateTime Time { get; set; }
    /// <summary>
    /// If the sender clicked the "Urgent" box
    /// </summary>
    public bool Urgent { get; set; }
};