using System;

/// <summary>
/// Enumeration represents status of training
/// </summary>
public enum Status
{
    /// <summary>
    /// Training succeeded
    /// </summary>
    Succeeded,

    /// <summary>
    /// Training failed
    /// </summary>
    Failed,

    /// <summary>
    /// Training still in progress
    /// </summary>
    Running
}

/// <summary>
/// The training status entity.
/// </summary>
[Serializable]
public class TrainingStatus
{
	
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    /// <value>
    /// The status.
    /// </value>
	public Status status;

    /// <summary>
    /// Gets or sets the create time.
    /// </summary>
    /// <value>
    /// The create time.
    /// </value>
	public DateTime createdDateTime;

    /// <summary>
    /// Gets or sets the last action time.
    /// </summary>
    /// <value>
    /// The last action time.
    /// </value>
	public DateTime lastActionDateTime;

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    /// <value>
    /// The message.
    /// </value>
	public string message;

}

