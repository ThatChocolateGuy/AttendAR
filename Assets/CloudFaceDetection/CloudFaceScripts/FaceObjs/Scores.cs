using System;
using System.Collections.Generic;


[Serializable]
public class Scores
{
    /// <summary>
    /// 
    /// </summary>
	public float anger;

    /// <summary>
    /// 
    /// </summary>
	public float contempt;

    /// <summary>
    /// 
    /// </summary>
	public float disgust;

    /// <summary>
    /// 
    /// </summary>
	public float fear;

    /// <summary>
    /// 
    /// </summary>
	public float happiness;

    /// <summary>
    /// 
    /// </summary>
	public float neutral;

    /// <summary>
    /// 
    /// </summary>
	public float sadness;

    /// <summary>
    /// 
    /// </summary>
	public float surprise;


    public override bool Equals(object o)
    {
        if (o == null) return false;

        var other = o as Scores;
        if (other == null) return false;

        return this.anger == other.anger &&
            this.disgust == other.disgust &&
            this.fear == other.fear &&
            this.happiness == other.happiness &&
            this.neutral == other.neutral &&
            this.sadness == other.sadness &&
            this.surprise == other.surprise;
    }

    public override int GetHashCode()
    {
        return anger.GetHashCode() ^
            disgust.GetHashCode() ^
            fear.GetHashCode() ^
            happiness.GetHashCode() ^
            neutral.GetHashCode() ^
            sadness.GetHashCode() ^
            surprise.GetHashCode();
    }
}


