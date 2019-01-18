using System;

[Serializable]
public class IdentityRequest
{
	public string personGroupId;
	public string[] faceIds;
	public int maxNumOfCandidates;

	public IdentityRequest(string personGroupId, string[] faceIds, int maxNumOfCandidates)
	{
		this.personGroupId = personGroupId;
		this.faceIds = faceIds;
		this.maxNumOfCandidates = maxNumOfCandidates;
	}
}


[Serializable]
public class IdentifyResultCollection
{
	public IdentifyResult[] identityResults;
}


/// <summary>
/// The identification result.
/// </summary>
[Serializable]
public class IdentifyResult
{
	
    /// <summary>
    /// Gets or sets the face identifier.
    /// </summary>
    /// <value>
    /// The face identifier.
    /// </value>
	public string faceId;

    /// <summary>
    /// Gets or sets the candidates.
    /// </summary>
    /// <value>
    /// The candidates.
    /// </value>
	public Candidate[] candidates;

}

