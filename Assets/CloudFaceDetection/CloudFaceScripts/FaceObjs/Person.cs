using System;

[Serializable]
public class PersonRequest
{
	public string name;
	public string userData;

	public PersonRequest(string name, string userData)
	{
		this.name = name;
		this.userData = userData;
	}
}


[Serializable]
public class PersonCollection
{
	public Person[] persons;
}


/// <summary>
/// The person entity.
/// </summary>
[Serializable]
public class Person
{
	
    /// <summary>
    /// Gets or sets the person identifier.
    /// </summary>
    /// <value>
    /// The person identifier.
    /// </value>
	public string personId;

    /// <summary>
    /// Gets or sets the persisted face ids.
    /// </summary>
    /// <value>
    /// The persisted face ids.
    /// </value>
	public string[] persistedFaceIds;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name of the person.
    /// </value>
	public string name;

    /// <summary>
    /// Gets or sets the profile.
    /// </summary>
    /// <value>
    /// The profile.
    /// </value>
	public string userData;

}

