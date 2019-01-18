using System;

[Serializable]
public class PersonGroupRequest
{
	public string name;
	public string userData;

	public PersonGroupRequest(string name, string userData)
	{
		this.name = name;
		this.userData = userData;
	}
}


/// <summary>
/// The person group entity.
/// </summary>
[Serializable]
public class PersonGroup
{
	
    /// <summary>
    /// Gets or sets the person group identifier.
    /// </summary>
    /// <value>
    /// The person group identifier.
    /// </value>
	public string personGroupId;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name of the person group.
    /// </value>
	public string name;

    /// <summary>
    /// Gets or sets the user data.
    /// </summary>
    /// <value>
    /// The user data.
    /// </value>
	public string userData;

}

