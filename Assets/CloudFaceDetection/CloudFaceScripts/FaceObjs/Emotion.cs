using System;


[Serializable]
public class EmotionCollection
{
	public Emotion[] emotions;
}


[Serializable]
public class Emotion
{
    /// <summary>
    /// Gets or sets the face rectangle.
    /// </summary>
    /// <value>
    /// The face rectangle.
    /// </value>
	public FaceRectangle faceRectangle;

    /// <summary>
    /// Gets or sets the emotion scores.
    /// </summary>
    /// <value>
    /// The emotion scores.
    /// </value>
	public Scores scores;


    public override bool Equals(object o)
    {
        if (o == null) return false;

        var other = o as Emotion;

        if (other == null) return false;

        if (this.faceRectangle == null)
        {
            if (other.faceRectangle != null) return false;
        }
        else
        {
            if (!this.faceRectangle.Equals(other.faceRectangle)) return false;
        }

        if (this.scores == null)
        {
            return other.scores == null;
        }
        else
        {
            return this.scores.Equals(other.scores);
        }
    }

    public override int GetHashCode()
    {
        int r = (faceRectangle == null) ? 0x33333333 : faceRectangle.GetHashCode();
        int s = (scores == null) ? 0xccccccc : scores.GetHashCode();

        return r ^ s;
    }

}


