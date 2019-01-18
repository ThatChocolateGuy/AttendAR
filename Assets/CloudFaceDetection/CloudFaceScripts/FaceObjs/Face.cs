using System;

[Serializable]
public class FaceCollection
{
	public Face[] faces;
}


/// <summary>
/// The detected face entity.
/// </summary>
[Serializable]
public class Face
{
	
    /// <summary>
    /// Gets or sets the face identifier.
    /// </summary>
    /// <value>
    /// The face identifier.
    /// </value>
	public string faceId;

    /// <summary>
    /// Gets or sets the face rectangle.
    /// </summary>
    /// <value>
    /// The face rectangle.
    /// </value>
	public FaceRectangle faceRectangle;

    /// <summary>
    /// Gets or sets the face landmarks.
    /// </summary>
    /// <value>
    /// The face landmarks.
    /// </value>
	public FaceLandmarks faceLandmarks;

    /// <summary>
    /// Gets or sets the face attributes.
    /// </summary>
    /// <value>
    /// The face attributes.
    /// </value>
	public FaceAttributes faceAttributes;

	/// <summary>
	/// Gets or sets the emotion.
	/// </summary>
	/// <value>The emotion.</value>
	public Emotion emotion;

	/// <summary>
	/// Gets or sets the identified candidate.
	/// </summary>
	/// <value>The identified candidate.</value>
	public Candidate candidate;

	/// <summary>
	/// Gets or sets the face image texture.
	/// </summary>
	/// <value>The face image texture.</value>
	public UnityEngine.Texture2D faceImage;

}
