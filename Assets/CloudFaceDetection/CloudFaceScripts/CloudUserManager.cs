using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System;

public class CloudUserManager : MonoBehaviour 
{
	[Tooltip("ID (short name) of the user group, containing the face-identified users. It will be created, if not found.")]
	public string userGroupId = "demo-users";

	[Tooltip("Whether group existence should be checked at start.")]
	public bool checkGroupAtStart = true;

	[Tooltip("GUI text used for debug and status messages.")]
	public GUIText debugText;

	// the face manager
	private CloudFaceManager faceManager = null;

	// the person group object
	private PersonGroup personGroup = null;

	private const int threadWaitLoops = 25;  // 25 * 200ms = 5.0s
	private const int threadWaitMs = 200;

	private static CloudUserManager instance = null;
	private string initedGroupId = string.Empty;



	/// <summary>
	/// Gets the CloudUserManager instance.
	/// </summary>
	/// <value>The CloudUserManager instance.</value>
	public static CloudUserManager Instance
	{
		get
		{
			return instance;
		}
	}
	
	
	/// <summary>
	/// Determines whether the UserGroupManager is initialized.
	/// </summary>
	/// <returns><c>true</c> if the UserGroupManager is initialized; otherwise, <c>false</c>.</returns>
	public bool IsInitialized()
	{
		return (userGroupId == initedGroupId);
	}
	
	
	void Start() 
	{
		try 
		{
			instance = this;
			
			if(string.IsNullOrEmpty(userGroupId))
			{
				throw new Exception("Please set the user-group name.");
			}
			
			faceManager = CloudFaceManager.Instance;
			if(faceManager != null)
			{
				if(string.IsNullOrEmpty(faceManager.faceSubscriptionKey))
				{
					throw new Exception("Please set your face-subscription key.");
				}
			}
			else
			{
				throw new Exception("FaceManager-component not found.");
			}

			// get the user group info
			if(checkGroupAtStart) 
			{
				AsyncTask<bool> task = new AsyncTask<bool>(() => {
					GetOrCreateUserGroup();
					return (userGroupId == initedGroupId);
				});

				task.Start();

				int waitounter = threadWaitLoops;
				while (task.State == TaskState.Running && waitounter > 0)
				{
					Thread.Sleep(threadWaitMs);
					waitounter--;
				}

				if(!string.IsNullOrEmpty(task.ErrorMessage))
				{
					throw new Exception(task.ErrorMessage);
				}
			}
		} 
		catch (Exception ex) 
		{
			Debug.LogError(ex.Message + '\n' + ex.StackTrace);

			if(debugText != null)
			{
				debugText.text = ex.Message;
			}
		}
	}
	
	void Update () 
	{
	}


	/// <summary>
	/// Starts group training.
	/// </summary>
	/// <returns><c>true</c>, if group training was started successfully, <c>false</c> otherwise.</returns>
	public bool StartGroupTraining()
	{
		// create the user-group if needed
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return false;

		if(faceManager != null)
		{
			return faceManager.TrainPersonGroup(userGroupId);
		}

		return false;
	}


	/// <summary>
	/// Gets the group training status.
	/// </summary>
	/// <returns>The training status (may be null).</returns>
	public TrainingStatus GetTrainingStatus()
	{
		// create the user-group if needed
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return null;
		
		// get the training status
		TrainingStatus training = null;
		if(faceManager != null)
		{
			training = faceManager.GetPersonGroupTrainingStatus(userGroupId);
		}
		
		return training;
	}
	
	
	/// <summary>
	/// Determines whether the group training is finished.
	/// </summary>
	/// <returns><c>true</c> if the group training is finished; otherwise, <c>false</c>.</returns>
	public bool IsGroupTrained()
	{
		// create the user-group if needed
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return false;
		
		if(faceManager != null)
		{
			return faceManager.IsPersonGroupTrained(userGroupId);
		}
		
		return false;
	}
	
	
	/// <summary>
	/// Identifies the users on the image.
	/// </summary>
	/// <returns><c>true</c>, if identification completed successfully, <c>false</c> otherwise.</returns>
	/// <param name="texImage">Image texture.</param>
	/// <param name="faces">Array of faces.</param>
	/// <param name="results">Array of identification results.</param>
	public bool IdentifyUsers(Texture2D texImage, ref Face[] faces, ref IdentifyResult[] results)
	{
		if(texImage == null)
			return false;
		
		byte[] imageBytes = texImage.EncodeToJPG();
		return IdentifyUsers(imageBytes, ref faces, ref results);
	}


	/// <summary>
	/// Identifies the users on the image.
	/// </summary>
	/// <returns><c>true</c>, if identification completed successfully, <c>false</c> otherwise.</returns>
	/// <param name="imageBytes">Image bytes.</param>
	/// <param name="faces">Array of faces.</param>
	/// <param name="results">Array of identification results.</param>
	public bool IdentifyUsers(byte[] imageBytes, ref Face[] faces, ref IdentifyResult[] results)
	{
		// create the user-group if needed
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return false;

		// detect and identify user faces
		faces = null;
		results = null;

		if(faceManager != null)
		{
			faces = faceManager.DetectFaces(imageBytes);

			// get the training status
			TrainingStatus training = faceManager.GetPersonGroupTrainingStatus(userGroupId);
			bool bEmptyGroup = false;

			if(training != null)
			{
				if(training.status == Status.Failed)
				{
					// check if there are persons in this group
					List<Person> listPersons = GetUsersList();

					if(listPersons.Count > 0)
					{
						// retrain the group
						faceManager.TrainPersonGroup(userGroupId);
					}
					else
					{
						// empty group - always returns 'training failed'
						training.status = Status.Succeeded;
						bEmptyGroup = true;
					}
				}
				else if(training.status == Status.Succeeded && training.message.StartsWith("There is no person"))
				{
					// the group exists but it's empty
					bEmptyGroup = true;
				}
			}

			DateTime waitTill = DateTime.Now.AddSeconds(5);
			while((training == null || training.status != Status.Succeeded) && (DateTime.Now < waitTill))
			{
				// wait for training to succeed
				System.Threading.Thread.Sleep(1000);
				training = faceManager.GetPersonGroupTrainingStatus(userGroupId);
			}

			if(bEmptyGroup)
			{
				// nothing to check
				return true;
			}

			if(faces != null && faces.Length > 0)
			{
				results = faceManager.IdentifyFaces(userGroupId, ref faces, 1);
				faceManager.MatchCandidatesToFaces(ref faces, ref results, userGroupId);
				return true;
			}
		}

		return false;
	}


	/// <summary>
	/// Gets the list of users in this group.
	/// </summary>
	/// <returns>The users list.</returns>
	public List<Person> GetUsersList()
	{
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return null;
		
		if(faceManager != null && !string.IsNullOrEmpty(userGroupId))
		{
			Person[] persons = faceManager.ListPersonsInGroup(userGroupId);
			
			if(persons != null)
			{
				return new List<Person>(persons);
			}
		}
		
		return null;
	}


	/// <summary>
	/// Gets the user by ID.
	/// </summary>
	/// <returns>The user or null.</returns>
	/// <param name="personId">Person ID</param>
	public Person GetUserById(string personId)
	{
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return null;

		Person person = null;
		if(faceManager != null && !string.IsNullOrEmpty(userGroupId))
		{
			person = faceManager.GetPerson(userGroupId, personId);
		}
		
		return person;
	}
	
	
	/// <summary>
	/// Adds the user to group.
	/// </summary>
	/// <returns>Person or null.</returns>
	/// <param name="userName">User name.</param>
	/// <param name="userData">User data.</param>
	/// <param name="texImage">Image texture.</param>
	/// <param name="faceRect">Face rectangle.</param>
	public Person AddUserToGroup(string userName, string userData, Texture2D texImage, FaceRectangle faceRect)
	{
		if(texImage == null)
			return null;
		
		byte[] imageBytes = texImage != null ? texImage.EncodeToJPG() : null;
		return AddUserToGroup(userName, userData, imageBytes, faceRect);
	}


	/// <summary>
	/// Adds the user to group.
	/// </summary>
	/// <returns>Person or null.</returns>
	/// <param name="userName">User name.</param>
	/// <param name="userData">User data.</param>
	/// <param name="imageBytes">Image bytes.</param>
	/// <param name="faceRect">Face rectangle.</param>
	public Person AddUserToGroup(string userName, string userData, byte[] imageBytes, FaceRectangle faceRect)
	{
		// create the user-group if needed
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return null;
		
		if(faceManager != null)
		{
			// add person
			Person person = faceManager.AddPersonToGroup(userGroupId, userName, userData);

			if(person != null)
			{
//				if(faceRect != null)
//				{
//					faceRect.Left -= 10;
//					faceRect.Top -= 10;
//					faceRect.Width += 20;
//					faceRect.Height += 20;
//				}

				PersonFace personFace = null;
				if(imageBytes != null)
				{
					personFace = faceManager.AddFaceToPerson(userGroupId, person.personId, string.Empty, faceRect, imageBytes);
				}

				if(personFace != null)
				{
					person.persistedFaceIds = new string[1];
					person.persistedFaceIds[0] = personFace.persistedFaceId;

					// train the group
					faceManager.TrainPersonGroup(userGroupId);

					// wait for training to complete
					bool isTrained = false;
					int retries = 0;

					while (!isTrained && retries++ < 5)
					{
						Thread.Sleep(1000);
						isTrained = faceManager.IsPersonGroupTrained(userGroupId);
					}

				}
			}

			return person;
		}

		return null;
	}


	/// <summary>
	/// Adds the user to group.
	/// </summary>
	/// <returns>Person or null.</returns>
	/// <param name="userName">User name.</param>
	/// <param name="userData">User data.</param>
	public Person AddUserToGroup(string userName, string userData)
	{
		// create the user-group if needed
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return null;

		Person person = null;
		if(faceManager != null)
		{
			// add person
			person = faceManager.AddPersonToGroup(userGroupId, userName, userData);
		}
		
		return person;
	}


	/// <summary>
	/// Adds the face to user.
	/// </summary>
	/// <returns>User face ID.</returns>
	/// <param name="person">Person.</param>
	/// <param name="texImage">Image texture.</param>
	/// <param name="faceRect">Face rectangle.</param>
	public string AddFaceToUser(Person person, Texture2D texImage, FaceRectangle faceRect)
	{
		if(texImage == null)
			return string.Empty;
		
		byte[] imageBytes = texImage != null ? texImage.EncodeToJPG() : null;
		return AddFaceToUser(person, imageBytes, faceRect);
	}


	/// <summary>
	/// Adds face to the user.
	/// </summary>
	/// <returns>User face ID.</returns>
	/// <param name="person">Person.</param>
	/// <param name="imageBytes">Image bytes.</param>
	/// <param name="faceRect">Face rectangle.</param>
	public string AddFaceToUser(Person person, byte[] imageBytes, FaceRectangle faceRect)
	{
		// create the user-group if needed
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return string.Empty;
		
		if(faceManager != null && person != null && imageBytes != null)
		{
			PersonFace personFace = faceManager.AddFaceToPerson(userGroupId, person.personId, string.Empty, faceRect, imageBytes);

			if(personFace != null)
			{
				faceManager.TrainPersonGroup(userGroupId);
				return personFace.persistedFaceId;
			}
		}
		
		return string.Empty;
	}
	
	
	/// <summary>
	/// Updates the person's name or userData field.
	/// </summary>
	/// <param name="person">Person to be updated.</param>
	public void UpdateUserData(Person person)
	{
		if(userGroupId != initedGroupId)
			GetOrCreateUserGroup();
		if(userGroupId != initedGroupId)
			return;
		
		if(faceManager != null && !string.IsNullOrEmpty(userGroupId) && person != null)
		{
			faceManager.UpdatePersonData(userGroupId, person);
		}
	}
	
	
	/// <summary>
	/// Deletes existing person from a person group. Persisted face images of the person will also be deleted. 
	/// </summary>
	/// <param name="person">Person to be deleted.</param>
	public void DeleteUser(Person person)
	{
		Debug.Log("DeleteUser_Mark1"); //pass

		if(userGroupId != initedGroupId){
			Debug.Log("DeleteUser_Mark2"); //fail
			GetOrCreateUserGroup();
		}
		if(userGroupId != initedGroupId){
			Debug.Log("DeleteUser_Mark3"); //fail
			return;
		}
		Debug.Log("DeleteUser_Mark4"); //pass
		if(faceManager != null && !string.IsNullOrEmpty(userGroupId) && person != null)
		{
			Debug.Log("DeleteUser_Mark5"); //pass
			faceManager.DeletePerson(userGroupId, person.personId);
			Debug.Log("DeleteUser_Mark6"); //pass
			faceManager.TrainPersonGroup(userGroupId);
		}
	}


	/// <summary>
	/// Converts user info string to dictionary.
	/// </summary>
	/// <returns>The user information dictionary.</returns>
	/// <param name="userInfo">User info.</param>
	public static Dictionary<string, string> ConvertInfoToDict(string userInfo)
	{
		Dictionary<string, string> dUserInfos = new Dictionary<string, string>();

		if(!string.IsNullOrEmpty(userInfo))
		{
			string[] asUserInfo = userInfo.Split("|".ToCharArray());

			for(int i = 0; i < asUserInfo.Length; i++)
			{
				int iIndex = asUserInfo[i].IndexOf("=");

				if(iIndex > 0)
				{
					string sName = asUserInfo[i].Substring(0, iIndex);
					string sValue = asUserInfo[i].Substring(iIndex + 1);

					dUserInfos[sName] = sValue;
				}
				else if(asUserInfo[i].Length > 0 && !dUserInfos.ContainsKey("UserInfo"))
				{
					dUserInfos["UserInfo"] = asUserInfo[i];
				}
			}
		}

		return dUserInfos;
	}


	/// <summary>
	/// Converts the dictionary of user infos to string.
	/// </summary>
	/// <returns>The user info as string.</returns>
	/// <param name="dUserInfos">Dictionary of user infos.</param>
	public static string ConvertDictToInfo(Dictionary<string, string> dUserInfos)
	{
		StringBuilder sbUserInfo = new StringBuilder();

		if(dUserInfos != null)
		{
			foreach(string sName in dUserInfos.Keys)
			{
				if(sbUserInfo.Length > 0)
					sbUserInfo.Append("|");

				string sValue = dUserInfos[sName];
				sbUserInfo.AppendFormat("{0}={1}", sName, sValue);
			}
		}

		return sbUserInfo.ToString();
	}
	
	
	// gets the person group info
	private void GetOrCreateUserGroup()
	{
		if(!string.IsNullOrEmpty(userGroupId) && faceManager != null)
		{
			try 
			{
				Debug.Log("GOCUG_Mark - try block");
				personGroup = faceManager.GetPersonGroup(userGroupId);
			} 
			catch (Exception ex) 
			{
				Debug.Log("GOCUG_Mark - catch block");
				Debug.Log(ex.Message);
				Debug.Log("Trying to create user-group '" + userGroupId + "'...");

				if(faceManager.CreatePersonGroup(userGroupId, userGroupId, string.Empty))
				{
					faceManager.TrainPersonGroup(userGroupId);
					personGroup = faceManager.GetPersonGroup(userGroupId);
				}

				Debug.Log("User-group '" + userGroupId + "' created.");
			}
			
			Debug.Log("GOCUG_MarkInited");
			initedGroupId = (personGroup != null) ? personGroup.personGroupId : string.Empty;
		}
	}

}
