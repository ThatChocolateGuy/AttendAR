using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.IO;
using System.Threading;
//using Newtonsoft.Json.Serialization;
//using Newtonsoft.Json;


public class CloudFaceManager : MonoBehaviour 
{
	[Tooltip("Service location for Face API.")]
	public string faceServiceLocation = "westus";

	[Tooltip("Subscription key for Face API.")]
	public string faceSubscriptionKey;

	[Tooltip("Service location for Emotion API.")]
	public string emotionServiceLocation = "westus";

	[Tooltip("Subscription key for Emotion API.")]
	public string emotionSubscriptionKey;

	//private const string FaceServiceHost = "https://api.projectoxford.ai/face/v1.0";
	//private const string EmotionServiceHost = "https://api.projectoxford.ai/emotion/v1.0";
	private const string FaceServiceHost = "https://[location].api.cognitive.microsoft.com/face/v1.0";
	private const string EmotionServiceHost = "https://[location].api.cognitive.microsoft.com/emotion/v1.0";

	private const int threadWaitLoops = 25;  // 25 * 200ms = 5.0s
	private const int threadWaitMs = 200;

	private static CloudFaceManager instance = null;
	private bool isInitialized = false;


	void Awake() 
	{
		instance = this;

		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("Please set your face-subscription key.");
		}

		isInitialized = true;
	}

	/// <summary>
	/// Gets the CloudFaceManager instance.
	/// </summary>
	/// <value>The CloudFaceManager instance.</value>
	public static CloudFaceManager Instance
	{
		get
		{
			return instance;
		}
	}

	/// <summary>
	/// Determines whether the FaceManager is initialized.
	/// </summary>
	/// <returns><c>true</c> if the FaceManager is initialized; otherwise, <c>false</c>.</returns>
	public bool IsInitialized()
	{
		return isInitialized;
	}

	/// <summary>
	/// Gets the face service URL.
	/// </summary>
	/// <returns>The face service URL.</returns>
	public string GetFaceServiceUrl()
	{
		string faceServiceUrl = FaceServiceHost.Replace("[location]", faceServiceLocation);
		return faceServiceUrl;
	}

	/// <summary>
	/// Gets the emotion service URL.
	/// </summary>
	/// <returns>The emotion service URL.</returns>
	public string GetEmotionServiceUrl()
	{
		string emotServiceUrl = EmotionServiceHost.Replace("[location]", emotionServiceLocation);
		return emotServiceUrl;
	}



	/// <summary>
	/// Detects the faces in the given image.
	/// </summary>
	/// <returns>List of detected faces.</returns>
	/// <param name="texImage">Image texture.</param>
	public Face[] DetectFaces(Texture2D texImage)
	{
		if(texImage == null)
			return null;

		byte[] imageBytes = texImage.EncodeToJPG();
		return DetectFaces(imageBytes);
	}
	
	
	/// <summary>
	/// Detects the faces in the given image.
	/// </summary>
	/// <returns>List of detected faces.</returns>
	/// <param name="imageBytes">Image bytes.</param>
	public Face[] DetectFaces(byte[] imageBytes)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}

		string requestUrl = string.Format("{0}/detect?returnFaceId={1}&returnFaceLandmarks={2}&returnFaceAttributes={3}", 
			GetFaceServiceUrl(), true, false, "age,gender,smile,headPose");

		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);

		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "POST", "application/octet-stream", imageBytes, headers, true, false);

		Face[] faces = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			//faces = JsonConvert.DeserializeObject<Face[]>(reader.ReadToEnd(), jsonSettings);
			string newJson = "{ \"faces\": " + reader.ReadToEnd() + "}";
			FaceCollection facesCollection = JsonUtility.FromJson<FaceCollection>(newJson);
			faces = facesCollection.faces;
		}
		else
		{
			ProcessFaceError(response);
		}

		return faces;
	}


	/// <summary>
	/// Recognizes the emotions.
	/// </summary>
	/// <returns>The array of recognized emotions.</returns>
	/// <param name="texImage">Image texture.</param>
	/// <param name="faceRects">Detected face rectangles, or null.</param>
	public Emotion[] RecognizeEmotions(Texture2D texImage, FaceRectangle[] faceRects)
	{
		if(texImage == null)
			return null;
		
		byte[] imageBytes = texImage.EncodeToJPG();
		return RecognizeEmotions(imageBytes, faceRects);
	}


	/// <summary>
	/// Recognizes the emotions.
	/// </summary>
	/// <returns>The array of recognized emotions.</returns>
	/// <param name="imageBytes">Image bytes.</param>
	/// <param name="faceRects">Detected face rectangles, or null.</param>
	public Emotion[] RecognizeEmotions(byte[] imageBytes, FaceRectangle[] faceRects)
	{
		if(string.IsNullOrEmpty(emotionSubscriptionKey))
		{
			throw new Exception("The emotion-subscription key is not set.");
		}

		StringBuilder faceRectsStr = new StringBuilder();
		if(faceRects != null)
		{
			foreach(FaceRectangle rect in faceRects)
			{
				faceRectsStr.AppendFormat("{0},{1},{2},{3};", rect.left, rect.top, rect.width, rect.height);
			}

			if(faceRectsStr.Length > 0)
			{
				faceRectsStr.Remove(faceRectsStr.Length - 1, 1); // drop the last semicolon
			}
		}

		string requestUrl = string.Format("{0}/recognize??faceRectangles={1}", GetEmotionServiceUrl(), faceRectsStr);

		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", emotionSubscriptionKey);

		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "POST", "application/octet-stream", imageBytes, headers, true, false);

		Emotion[] emotions = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			//emotions = JsonConvert.DeserializeObject<Emotion[]>(reader.ReadToEnd(), jsonSettings);
			string newJson = "{ \"emotions\": " + reader.ReadToEnd() + "}";
			EmotionCollection emotionCollection = JsonUtility.FromJson<EmotionCollection>(newJson);
			emotions = emotionCollection.emotions;
		}
		else
		{
			ProcessFaceError(response);
		}

		return emotions;
	}
	
	
	/// <summary>
	/// Matches the recognized emotions to faces.
	/// </summary>
	/// <returns>The number of matched emotions.</returns>
	/// <param name="faces">Array of detected Faces.</param>
	/// <param name="emotions">Array of recognized Emotions.</param>
	public int MatchEmotionsToFaces(ref Face[] faces, ref Emotion[] emotions)
	{
		int matched = 0;
		if(faces == null || emotions == null)
			return matched;
		
		foreach(Emotion emot in emotions)
		{
			FaceRectangle emotRect = emot.faceRectangle;
			
			for(int i = 0; i < faces.Length; i++)
			{
				if(Mathf.Abs(emotRect.left - faces[i].faceRectangle.left) <= 2 &&
				   Mathf.Abs(emotRect.top - faces[i].faceRectangle.top) <= 2)
				{
					faces[i].emotion = emot;
					matched++;
					break;
				}
			}
		}
		
		return matched;
	}
	
	

	/// <summary>
	/// Gets the standard face colors.
	/// </summary>
	/// <returns>The face colors.</returns>
	public static Color[] GetFaceColors()
	{
		Color[] faceColors = new Color[5];

		faceColors[0] = Color.green;
		faceColors[1] = Color.yellow;
		faceColors[2] = Color.cyan;
		faceColors[3] = Color.magenta;
		faceColors[4] = Color.red;

		return faceColors;
	}


	/// <summary>
	/// Gets the standard face color names.
	/// </summary>
	/// <returns>The face color names.</returns>
	public static string[] GetFaceColorNames()
	{
		string[] faceColorNames = new string[5];

		faceColorNames[0] = "Green";
		faceColorNames[1] = "Yellow";
		faceColorNames[2] = "Cyan";
		faceColorNames[3] = "Magenta";
		faceColorNames[4] = "Red";

		return faceColorNames;
	}
	
	
	// draw face rectangles
	/// <summary>
	/// Draws the face rectacgles in the given texture.
	/// </summary>
	/// <param name="faces">List of faces.</param>
	/// <param name="tex">The camera shot texture</param>
	public static void DrawFaceRects(Texture2D tex, Face[] faces, Color[] faceColors)
	{
		for(int i = 0; i < faces.Length; i++)
		{
			Face face = faces[i];
			Color faceColor = faceColors[i % faceColors.Length];
			
			FaceRectangle rect = face.faceRectangle;
			CloudTexTools.DrawRect(tex, rect.left, rect.top, rect.width, rect.height, faceColor);
		}
		
		tex.Apply();
	}


	/// <summary>
	/// Gets the face-only texture from the given texture.
	/// </summary>
	/// <returns>The face texture.</returns>
	/// <param name="tex">The camera shot texture</param>
	/// <param name="face">The detected face</param>
	public static Texture2D GetFaceTexture(Texture2D tex, ref Face face)
	{
		if(tex != null && face != null)
		{
			FaceRectangle rect = face.faceRectangle;
			int texY = tex.height - rect.top - rect.height;

			return CloudTexTools.GetTexturePart(tex, rect.left, texY, rect.width, rect.height);
		}

		return null;
	}
	

	/// <summary>
	/// Matches the face images.
	/// </summary>
	/// <returns><c>true</c>, if face images were matched, <c>false</c> otherwise.</returns>
	/// <param name="tex">The camera shot texture</param>
	/// <param name="faces">The array of detected faces.</param>
	public static bool MatchFaceImages(Texture2D tex, ref Face[] faces)
	{
		if(tex == null || faces == null)
			return false;

		for(int i = 0; i < faces.Length; i++)
		{
			faces[i].faceImage = GetFaceTexture(tex, ref faces[i]);
		}

		return true;
	}


	/// <summary>
	/// Creates a person group.
	/// </summary>
	/// <returns><c>true</c>, if person group was created, <c>false</c> otherwise.</returns>
	/// <param name="groupId">Person-group ID.</param>
	/// <param name="name">Group name (max 128 chars).</param>
	/// <param name="userData">User data (max 16K).</param>
	public bool CreatePersonGroup(string groupId, string groupName, string userData)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}", GetFaceServiceUrl(), groupId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);

		//string sJsonContent = JsonConvert.SerializeObject(new { name = groupName, userData = userData }, jsonSettings);
		string sJsonContent = JsonUtility.ToJson(new PersonGroupRequest(groupName, userData));
		byte[] btContent = Encoding.UTF8.GetBytes(sJsonContent);
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "PUT", "application/json", btContent, headers, true, false);
		
		if(CloudWebTools.IsErrorStatus(response))
		{
			ProcessFaceError(response);
			return false;
		}
		
		return true;
	}
	

	/// <summary>
	/// Gets the person group.
	/// </summary>
	/// <returns>The person group.</returns>
	/// <param name="groupId">Group ID.</param>
	public PersonGroup GetPersonGroup(string groupId)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}", GetFaceServiceUrl(), groupId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "GET", "application/json", null, headers, true, false);
		
		PersonGroup group = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			//group = JsonConvert.DeserializeObject<PersonGroup>(reader.ReadToEnd(), jsonSettings);
			group = JsonUtility.FromJson<PersonGroup>(reader.ReadToEnd());
		}
		else
		{
			ProcessFaceError(response);
		}
		
		return group;
	}
	
	
	/// <summary>
	/// Lists the people in a person-group.
	/// </summary>
	/// <returns>The people in group.</returns>
	/// <param name="groupId">Person-group ID.</param>
	public Person[] ListPersonsInGroup(string groupId)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}/persons", GetFaceServiceUrl(), groupId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "GET", "application/json", null, headers, true, false);

		Person[] persons = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			//persons = JsonConvert.DeserializeObject<Person[]>(reader.ReadToEnd(), jsonSettings);
			string newJson = "{ \"persons\": " + reader.ReadToEnd() + "}";
			PersonCollection personColl = JsonUtility.FromJson<PersonCollection>(newJson);
			persons = personColl.persons;
		}
		else
		{
			ProcessFaceError(response);
		}

		return persons;
	}
	

	/// <summary>
	/// Adds the person to a group. (CREATE PersonGroup Person)
	/// </summary>
	/// <returns>The person to group.</returns>
	/// <param name="groupId">Person-group ID.</param>
	/// <param name="personName">Person name (max 128 chars).</param>
	/// <param name="userData">User data (max 16K).</param>
	public Person AddPersonToGroup(string groupId, string personName, string userData)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}/persons", GetFaceServiceUrl(), groupId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		string sJsonContent = JsonUtility.ToJson(new PersonRequest(personName, userData));
		byte[] btContent = Encoding.UTF8.GetBytes(sJsonContent);
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "POST", "application/json", btContent, headers, true, false);
		
		Person person = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			person = JsonUtility.FromJson<Person>(reader.ReadToEnd());

			//if(person.PersonId != null)
			{
				person.name = personName;
				person.userData = userData;
			}
		}
		else
		{
			ProcessFaceError(response);
		}

		return person;
	}
	

	/// <summary>
	/// Gets the person data.
	/// </summary>
	/// <returns>The person data.</returns>
	/// <param name="groupId">Group ID.</param>
	/// <param name="personId">Person ID.</param>
	public Person GetPerson(string groupId, string personId)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}/persons/{2}", GetFaceServiceUrl(), groupId, personId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "GET", "application/json", null, headers, true, false);
		
		Person person = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			person = JsonUtility.FromJson<Person>(reader.ReadToEnd());
		}
		else
		{
			ProcessFaceError(response);
		}
		
		return person;
	}
	
	
	/// <summary>
	/// Adds the face to a person in a person-group.
	/// </summary>
	/// <returns>The persisted face (only faceId is set).</returns>
	/// <param name="groupId">Person-group ID.</param>
	/// <param name="personId">Person ID.</param>
	/// <param name="userData">User data.</param>
	/// <param name="faceRect">Face rect or null.</param>
	/// <param name="texImage">Image Texture.</param>
	public PersonFace AddFaceToPerson(string groupId, string personId, string userData, FaceRectangle faceRect, Texture2D texImage)
	{
		if(texImage == null)
			return null;
		
		byte[] imageBytes = texImage.EncodeToJPG();
		return AddFaceToPerson(groupId, personId, userData, faceRect, imageBytes);
	}
	
	
	/// <summary>
	/// Adds the face to a person in a person-group.
	/// </summary>
	/// <returns>The persisted face (only faceId is set).</returns>
	/// <param name="groupId">Person-group ID.</param>
	/// <param name="personId">Person ID.</param>
	/// <param name="userData">User data.</param>
	/// <param name="faceRect">Face rect or null.</param>
	/// <param name="imageBytes">Image bytes.</param>
	public PersonFace AddFaceToPerson(string groupId, string personId, string userData, FaceRectangle faceRect, byte[] imageBytes)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}

		string sFaceRect = faceRect != null ? string.Format("{0},{1},{2},{3}", faceRect.left, faceRect.top, faceRect.width, faceRect.height) : string.Empty;
		string requestUrl = string.Format("{0}/persongroups/{1}/persons/{2}/persistedFaces?userData={3}&targetFace={4}", GetFaceServiceUrl(), groupId, personId, userData, sFaceRect);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "POST", "application/octet-stream", imageBytes, headers, true, false);
		
		PersonFace face = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			face = JsonUtility.FromJson<PersonFace>(reader.ReadToEnd());
		}
		else
		{
			ProcessFaceError(response);
		}
		
		return face;
	}


	/// <summary>
	/// Updates the person's name or userData field.
	/// </summary>
	/// <param name="groupId">Person-group ID.</param>
	/// <param name="personId">Person ID.</param>
	public void UpdatePersonData(string groupId, Person person)
	{
		if(person == null)
			return;

		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}/persons/{2}", GetFaceServiceUrl(), groupId, person.personId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		string sJsonContent = JsonUtility.ToJson(new PersonRequest(person.name, person.userData));
		byte[] btContent = Encoding.UTF8.GetBytes(sJsonContent);
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "PATCH", "application/json", btContent, headers, true, false);
		
		if(CloudWebTools.IsErrorStatus(response))
		{
			ProcessFaceError(response);
		}
	}
	
	
	/// <summary>
	/// Deletes existing person from a person group. Persisted face images of the person will also be deleted. 
	/// </summary>
	/// <param name="groupId">Person-group ID.</param>
	/// <param name="personId">Person ID.</param>
	public void DeletePerson(string groupId, string personId)
	{
		Debug.Log("DeletePerson_Starting User Deletion\n");

		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}/persons/{2}", GetFaceServiceUrl(), groupId, personId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "DELETE", "application/json", null, headers, true, false);
		
		if(CloudWebTools.IsErrorStatus(response))
		{
			Debug.Log("DeletePerson_Error - Response Below\n");
			ProcessFaceError(response);
		}

		Debug.Log("DeletePerson_User Deleted From Cloud\n");
	}
	
	
	/// <summary>
	/// Trains the person-group.
	/// </summary>
	/// <returns><c>true</c>, if person-group training was successfully started, <c>false</c> otherwise.</returns>
	/// <param name="groupId">Group identifier.</param>
	public bool TrainPersonGroup(string groupId)
	{
		Debug.Log("TrainPersonGroup_Starting Group Training\n");

		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}/train", GetFaceServiceUrl(), groupId);
		
		// Dictionary<string, string> headers = new Dictionary<string, string>();
		// headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		HttpWebRequest request= WebRequest.CreateHttp(requestUrl);

		request.Headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		request.Method = "POST";
		request.ContentLength = 0;
		request.ContentType = "application/json";

		HttpWebResponse response = (HttpWebResponse) request.GetResponse();

		// HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "POST", "application/json", null, headers, true, false);

		if(CloudWebTools.IsErrorStatus(response))
		{
			Debug.Log("TrainPersonGroup_Error - Response Below\n");
			Debug.Log(response.StatusCode);

			ProcessFaceError(response);
			return false;
		}
		
		Debug.Log("TrainPersonGroup_Group Trained in Cloud\n");
		return true;
	}
	

	/// <summary>
	/// Determines whether the person-group's training is finished.
	/// </summary>
	/// <returns><c>true</c> if the person-group's training is finished; otherwise, <c>false</c>.</returns>
	/// <param name="groupId">Person-group ID.</param>
	public bool IsPersonGroupTrained(string groupId)
	{
		TrainingStatus status = GetPersonGroupTrainingStatus(groupId);
		bool bSuccess = (status != null && status.status == Status.Succeeded);
		
		return bSuccess;
	}


	/// <summary>
	/// Gets the person-group's training status.
	/// </summary>
	/// <returns>The group's training status.</returns>
	/// <param name="groupId">Person-group ID.</param>
	public TrainingStatus GetPersonGroupTrainingStatus(string groupId)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}
		
		string requestUrl = string.Format("{0}/persongroups/{1}/training", GetFaceServiceUrl(), groupId);
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "GET", "", null, headers, true, false);
		
		TrainingStatus status = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			status = JsonUtility.FromJson<TrainingStatus>(reader.ReadToEnd());
		}
		else
		{
			ProcessFaceError(response);
		}

		return status;
	}
	

	/// <summary>
	/// Identifies the given faces.
	/// </summary>
	/// <returns>Array of identification results.</returns>
	/// <param name="groupId">Group ID.</param>
	/// <param name="faces">Array of detected faces.</param>
	/// <param name="maxCandidates">Maximum allowed candidates per face.</param>
	public IdentifyResult[] IdentifyFaces(string groupId, ref Face[] faces, int maxCandidates)
	{
		if(string.IsNullOrEmpty(faceSubscriptionKey))
		{
			throw new Exception("The face-subscription key is not set.");
		}

		string[] faceIds = new string[faces.Length];
		for(int i = 0; i < faces.Length; i++)
		{
			faceIds[i] = faces[i].faceId;
		}

		if(maxCandidates <= 0)
		{
			maxCandidates = 1;
		}
		
		string requestUrl = string.Format("{0}/identify", GetFaceServiceUrl());
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("ocp-apim-subscription-key", faceSubscriptionKey);
		
		string sJsonContent = JsonUtility.ToJson(new IdentityRequest(groupId, faceIds, maxCandidates));
		byte[] btContent = Encoding.UTF8.GetBytes(sJsonContent);
		HttpWebResponse response = CloudWebTools.DoWebRequest(requestUrl, "POST", "application/json", btContent, headers, true, false);
		
		IdentifyResult[] results = null;
		if(!CloudWebTools.IsErrorStatus(response))
		{
			StreamReader reader = new StreamReader(response.GetResponseStream());
			string newJson = "{ \"identityResults\": " + reader.ReadToEnd() + "}";
			IdentifyResultCollection resultCollection = JsonUtility.FromJson<IdentifyResultCollection>(newJson);
			results = resultCollection.identityResults;
		}
		else
		{
			ProcessFaceError(response);
		}
		
		return results;
	}
	

	/// <summary>
	/// Matchs the identity candidates to faces.
	/// </summary>
	/// <returns>The number of matched identities.</returns>
	/// <param name="faces">Array of detected faces.</param>
	/// <param name="identities">Array of recognized identities.</param>
	public int MatchCandidatesToFaces(ref Face[] faces, ref IdentifyResult[] identities, string groupId)
	{
		int matched = 0;
		if(faces == null || identities == null)
			return matched;

		// clear face identities
		for(int i = 0; i < faces.Length; i++)
		{
			faces[i].candidate = null;
		}

		foreach(IdentifyResult ident in identities)
		{
			string faceId = ident.faceId;
			
			for(int i = 0; i < faces.Length; i++)
			{
				if(faces[i].faceId == faceId)
				{
					if(ident.candidates != null && ident.candidates.Length > 0)
					{
						faces[i].candidate = ident.candidates[0];

						if(faces[i].candidate != null)
						{
							faces[i].candidate.person = GetPerson(groupId, faces[i].candidate.personId);
						}
					}

					matched++;
					break;
				}
			}
		}
		
		return matched;
	}
	
	
	// --------------------------------------------------------------------------------- //
	
	// processes the error status in response
	private void ProcessFaceError(HttpWebResponse response)
	{
		StreamReader reader = new StreamReader(response.GetResponseStream());
		string responseText = reader.ReadToEnd();

		//ClientError ex = JsonConvert.DeserializeObject<ClientError>(responseText);
		ClientError ex = JsonUtility.FromJson<ClientError>(responseText);

		if (ex.error != null && ex.error.code != null)
		{
			string sErrorMsg = !string.IsNullOrEmpty(ex.error.code) && ex.error.code != "Unspecified" ?
				ex.error.code + " - " + ex.error.message : ex.error.message;
			throw new System.Exception(sErrorMsg);
		}
		else
		{
			//ServiceError serviceEx = JsonConvert.DeserializeObject<ServiceError>(responseText);
			ServiceError serviceEx = JsonUtility.FromJson<ServiceError>(responseText);

			if (serviceEx != null && serviceEx.statusCode != null)
			{
				string sErrorMsg = !string.IsNullOrEmpty(serviceEx.statusCode) && serviceEx.statusCode != "Unspecified" ?
					serviceEx.statusCode + " - " + serviceEx.message : serviceEx.message;
				throw new System.Exception(sErrorMsg);
			}
			else
			{
				throw new System.Exception("Error " + CloudWebTools.GetStatusCode(response) + ": " + CloudWebTools.GetStatusMessage(response) + "; Url: " + response.ResponseUri);
			}
		}
	}
	
	
//	private JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
//	{
//		DateFormatHandling = DateFormatHandling.IsoDateFormat,
//		NullValueHandling = NullValueHandling.Ignore,
//		ContractResolver = new CamelCasePropertyNamesContractResolver()
//	};


}
