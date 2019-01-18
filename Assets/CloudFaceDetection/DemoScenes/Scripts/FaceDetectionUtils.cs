using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public static class FaceDetectionUtils 
{
    private static readonly Color[] faceColors = new Color[] { Color.green, Color.yellow, Color.cyan, Color.magenta, Color.red };
    private static readonly string[] faceColorNames = new string[] { "Green", "Yellow", "Cyan", "Magenta", "Red", };
                                      

    public static Texture2D ImportImage()
    {
        Texture2D tex = null;

#if UNITY_EDITOR
		string filePath = UnityEditor.EditorUtility.OpenFilePanel("Open image file", "", "jpg");  // string.Empty; // 
#else
		string filePath = string.Empty;
#endif

        if (!string.IsNullOrEmpty(filePath))
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            tex = new Texture2D(2, 2);
            tex.LoadImage(fileBytes);
        }

        return tex;
    }

    public static string FaceToString(Face face, string faceColorName)
    {
        StringBuilder sbResult = new StringBuilder();

        sbResult.Append(string.Format("{0} face:", faceColorName)).AppendLine();
        sbResult.Append(string.Format("  • Gender: {0}", face.faceAttributes.gender)).AppendLine();
		sbResult.Append(string.Format("  • Age: {0}", face.faceAttributes.age)).AppendLine();
		sbResult.Append(string.Format("  • Smile: {0:F0}%", face.faceAttributes.smile * 100f)).AppendLine();

//			sbResult.Append(string.Format("  • Beard: {0}", face.FaceAttributes.FacialHair.Beard)).AppendLine();
//			sbResult.Append(string.Format("  • Moustache: {0}", face.FaceAttributes.FacialHair.Moustache)).AppendLine();
//			sbResult.Append(string.Format("  • Sideburns: {0}", face.FaceAttributes.FacialHair.Sideburns)).AppendLine().AppendLine();

		if(face.emotion != null && face.emotion.scores != null)
			sbResult.Append(string.Format("  • Emotion: {0}", GetEmotionScoresAsString(face.emotion))).AppendLine();

		sbResult.AppendLine();

        return sbResult.ToString();
    }

	
	/// <summary>
	/// Gets the emotion scores as string.
	/// </summary>
	/// <returns>The emotion as string.</returns>
	/// <param name="emotion">Emotion.</param>
	public static string GetEmotionScoresAsString(Emotion emotion)
	{
		if(emotion == null || emotion.scores == null)
			return string.Empty;
		
		Scores es = emotion.scores; 
		StringBuilder emotStr = new StringBuilder();
		
		if(es.anger >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% angry,", es.anger * 100f);
		if(es.contempt >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% contemptuous,", es.contempt * 100f);
		if(es.disgust >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% disgusted,", es.disgust * 100f);
		if(es.fear >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% scared,", es.fear * 100f);
		if(es.happiness >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% happy,", es.happiness * 100f);
		if(es.neutral >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% neutral,", es.neutral * 100f);
		if(es.sadness >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% sad,", es.sadness * 100f);
		if(es.surprise >= 0.01f) 
			emotStr.AppendFormat(" {0:F0}% surprised,", es.surprise * 100f);
		
		if(emotStr.Length > 0)
		{
			emotStr.Remove(0, 1);
			emotStr.Remove(emotStr.Length - 1, 1);
		}
		
		return emotStr.ToString();
	}
	
	
	/// <summary>
	/// Gets the emotion scores as list of strings.
	/// </summary>
	/// <returns>The emotion as string.</returns>
	/// <param name="emotion">Emotion.</param>
	public static List<string> GetEmotionScoresList(Emotion emotion)
	{
		List<string> alScores = new List<string>();
		if(emotion == null || emotion.scores == null)
			return alScores;
		
		Scores es = emotion.scores; 
		
		if(es.anger >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% angry", es.anger * 100f));
		if(es.contempt >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% contemptuous", es.contempt * 100f));
		if(es.disgust >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% disgusted,", es.disgust * 100f));
		if(es.fear >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% scared", es.fear * 100f));
		if(es.happiness >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% happy", es.happiness * 100f));
		if(es.neutral >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% neutral", es.neutral * 100f));
		if(es.sadness >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% sad", es.sadness * 100f));
		if(es.surprise >= 0.01f) 
			alScores.Add(string.Format("{0:F0}% surprised", es.surprise * 100f));
		
		return alScores;
	}

    public static Color[] FaceColors { get { return faceColors; } }
    public static string[] FaceColorNames { get { return faceColorNames; } }
}
