using System;

/// <summary>
/// The identified candidate entity.
/// </summary>
[Serializable]
public class Candidate
{

    /// <summary>
    /// Gets or sets the person identifier.
    /// </summary>
    /// <value>
    /// The person identifier.
    /// </value>
    public string personId;

    /// <summary>
    /// Gets or sets the confidence.
    /// </summary>
    /// <value>
    /// The confidence.
    /// </value>
    public double confidence;

	/// <summary>
	/// Gets or sets the person.
	/// </summary>
	/// <value>The person.</value>
	public Person person;
	
}

