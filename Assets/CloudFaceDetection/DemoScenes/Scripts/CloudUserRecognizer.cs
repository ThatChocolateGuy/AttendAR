using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class CloudUserRecognizer : MonoBehaviour
{
    [Tooltip("Image source component used for taking camera shots.")]
	public WebcamSource imageSource;

    [Tooltip("Image component used for rendering camera shots.")]
    public RawImage cameraShot;

	[Tooltip("Text component used for displaying hints and status messages.")]
    public Text hintText;

	[Tooltip("Reference to the user list content.")]
	public RectTransform userListContent;

	[Tooltip("Reference to the user item prefab.")]
	public GameObject userItemPrefab;

    // whether webcamSource has been set or there is web camera at all
    private bool hasCamera = false;

    // initial hint message
    private string hintMessage;

    // AspectRatioFitter component;
    private AspectRatioFitter ratioFitter;

	// list of found persons
	private Dictionary<string, GameObject> personsPanels = new Dictionary<string, GameObject>();
	//private Face selectedPerson;

	// array of faces
	private Face[] faces = null;
	// array of identification results
	private IdentifyResult[] results = null;
	// camera shot texture
	private Texture2D texCamShot = null;

    void Start()
    {
        if (cameraShot)
        {
            ratioFitter = cameraShot.GetComponent<AspectRatioFitter>();
        }

		hasCamera = imageSource != null && imageSource.HasCamera();

        hintMessage = hasCamera ? "Tap on the camera image to take a shot" : "No Camera Found";
        
        SetHintText(hintMessage);
    }

    // camera panel onclick event handler
    public void OnCameraClick()
    {
        if (!hasCamera) 
			return;
        
        if (DoCameraShot())
        {
            StartCoroutine(DoUserRecognition());
        }        
    }

    // camera-shot panel onclick event handler
    public void OnShotClick()
    {
        if (DoImageImport())
        {
            StartCoroutine(DoUserRecognition());
        }
    }

    // camera shot step
    private bool DoCameraShot()
    {
        if (cameraShot != null && imageSource != null)
        {
            SetShotImageTexture(imageSource.GetImage());
            return true;
        }

        return false;
    }

    // imports image and displays it on the camera-shot object
    private bool DoImageImport()
    {
        Texture2D tex = FaceDetectionUtils.ImportImage();
        if (!tex) return false;

        SetShotImageTexture(tex);

        return true;
    }

	// display image on the camera-shot object
	private void SetShotImageTexture(Texture2D tex)
	{        
		if (ratioFitter)
		{
			ratioFitter.aspectRatio = (float)tex.width / (float)tex.height;
		}

		if (cameraShot)
		{
			cameraShot.texture = tex;
		}
	}

    // performs user recognition
    private IEnumerator DoUserRecognition()
    {
        // get the image to detect
        faces = null;
        texCamShot = null;

        if (cameraShot)
        {
			texCamShot = (Texture2D)cameraShot.texture;
            SetHintText("Wait...");
        }

		// get the user manager instance
		CloudUserManager userManager = CloudUserManager.Instance;

		if (!userManager)
        {
			if(hintText)
			{
				hintText.text = "Check if the CloudFaceManager and CloudUserManager components exist in the scene.";
			}
        }
        else if(texCamShot)
        {
			byte[] imageBytes = texCamShot.EncodeToJPG();
			yield return null;

			AsyncTask<bool> taskIdentify = new AsyncTask<bool>(() => {
				bool bSuccess = userManager.IdentifyUsers(imageBytes, ref faces, ref results);
				return bSuccess;
			});

			taskIdentify.Start();
			yield return null;

			while (taskIdentify.State == TaskState.Running)
			{
				yield return null;
			}

			if(string.IsNullOrEmpty(taskIdentify.ErrorMessage))
			{
				if(taskIdentify.Result)
				{
					CloudFaceManager.DrawFaceRects(texCamShot, faces, FaceDetectionUtils.FaceColors);
					yield return null;

					SetHintText("Select Student to Check In:");
				}
				else
				{
					SetHintText("No students detected.");
				}

				// show the identified users
				ShowIdentityResult();
				Debug.Log("ShowIdentityResult Triggered");
			}
			else
			{
				SetHintText(taskIdentify.ErrorMessage);
				Debug.Log(taskIdentify.ErrorMessage);
			}
        }

        yield return null;
    }

    // display identity results
    private void ShowIdentityResult()
    {
//        StringBuilder sbResult = new StringBuilder();
//
//        if (faces != null && faces.Length > 0)
//        {
//            for (int i = 0; i < faces.Length; i++)
//            {
//                Face face = faces[i];
//                string faceColorName = FaceDetectionUtils.FaceColorNames[i % FaceDetectionUtils.FaceColors.Length];
//
//                string res = FaceDetectionUtils.FaceToString(face, faceColorName);
//
//                sbResult.Append(string.Format("<color={0}>{1}</color>", faceColorName, res));
//            }
//        }
//
//        string result = sbResult.ToString();
//
//        if (resultText)
//        {
//            resultText.text = result;
//        }
//        else
//        {
//            Debug.Log(result);
//        }

		// clear current list
		ClearIdentityResult();
		Debug.Log("ClearIdentityResult Triggered - Clearing Current List");

		// create the new list
		if(faces != null)
		{
			// get face images
			CloudFaceManager.MatchFaceImages(texCamShot, ref faces);

			// show recognized persons
			for(int i = 0; i < faces.Length; i++)
			{
				Face face = faces[i];

				if(face.candidate != null && face.candidate.person != null)
				{
					InstantiateUserItem(i, face, face.candidate.person);
				}
			}

			// show unrecognized faces
			for(int i = 0; i < faces.Length; i++)
			{
				Face face = faces[i];

				if(face.candidate == null || face.candidate.person == null)
				{
					InstantiateUserItem(i, face, null);
				}
			}
		}

    }

	private void InstantiateUserItem(int i, Face f, Person p)
	{
		if(!userItemPrefab)
			return;

		GameObject userItemInstance = Instantiate<GameObject>(userItemPrefab);

		GameObject userImageObj = userItemInstance.transform.Find("UserImagePanel").gameObject;
		userImageObj.GetComponentInChildren<RawImage>().texture = f.faceImage;

        string faceColorName = FaceDetectionUtils.FaceColorNames[i % FaceDetectionUtils.FaceColors.Length];
		string userName = string.Format("<color={0}>{1}</color>", faceColorName, p != null ? p.name : faceColorName + " face");

		GameObject userNameObj = userItemInstance.transform.Find("UserName").gameObject;
		userNameObj.GetComponent<Text>().text = userName;

//		GameObject personIdObj = userItemInstance.transform.Find("PersonID").gameObject;
//		personIdObj.GetComponent<Text>().text = p != null ? "UserID: " + p.personId : string.Empty;

//		GameObject faceIdObj = userItemInstance.transform.Find("FaceID").gameObject;
//		faceIdObj.GetComponent<Text>().text = "FaceID: " + f.faceId;

		if(p != null)
		{
			Dictionary<string, string> dUserData = CloudUserManager.ConvertInfoToDict(p.userData);
			string sUserInfo = dUserData.ContainsKey("UserInfo") ? dUserData["UserInfo"] : string.Empty;

			GameObject userInfoObj = userItemInstance.transform.Find("UserInfo").gameObject;
			userInfoObj.GetComponent<Text>().text = sUserInfo;
			userInfoObj.SetActive(true);

			GameObject loginBtnObj = userItemInstance.transform.Find("LoginButton").gameObject;
			loginBtnObj.SetActive(true);

			Button loginButton = loginBtnObj.GetComponent<Button>();
			loginButton.onClick.AddListener(() => OnUserLoginClick(f, userItemInstance));

			// enable selectable panel
			userItemInstance.GetComponent<Selectable>().enabled = true;
			AddUserPanelClickListener(userItemInstance, f);
		}
		else
		{
//			GameObject nameHintObj = userItemInstance.transform.Find("NameHint").gameObject;
//			nameHintObj.SetActive(true);

			GameObject saveNameObj = userItemInstance.transform.Find("SaveName").gameObject;
			saveNameObj.SetActive(true);

//			GameObject infoHintObj = userItemInstance.transform.Find("InfoHint").gameObject;
//			infoHintObj.SetActive(true);

			GameObject saveInfoObj = userItemInstance.transform.Find("SaveInfo").gameObject;
			saveInfoObj.SetActive(true);

			GameObject saveBtnObj = userItemInstance.transform.Find("SaveButton").gameObject;
			saveBtnObj.SetActive(true);

			Button saveButton = saveBtnObj.GetComponent<Button>();
			InputField saveNameInput = saveNameObj.GetComponent<InputField>();
			InputField saveInfoInput = saveInfoObj.GetComponent<InputField>();
			saveButton.onClick.AddListener(() => OnSaveUserClick(f, saveNameInput, saveInfoInput));

			// disable selectable panel
			userItemInstance.GetComponent<Selectable>().enabled = false;
		}

		userItemInstance.transform.SetParent(userListContent, false);
		personsPanels.Add(f.faceId, userItemInstance);
	}

	private void AddUserPanelClickListener(GameObject panel, Face f)
	{
		EventTrigger trigger = panel.GetComponentInParent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();

		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener((eventData) => { OnUserLoginClick(f, panel); });

		trigger.triggers.Add(entry);
	}

	private void OnUserLoginClick(Face f, GameObject panelInstance)
	{
		CloudUserData userData = CloudUserData.Instance;
		if(userData)
		{
			userData.selectedUser = f;
		}

		// SetHintText("Selected: " + (userData ? userData.selectedUser.candidate.person.name : "-"));
		
		CloudUserManager userManager = CloudUserManager.Instance;
		CloudFaceManager cloudFaceManager = CloudFaceManager.Instance;
		Person person = userData.selectedUser.candidate.person;
		string groupId = userManager ? userManager.userGroupId : "";
		// CheckIn Timestamp on "Check In" Press
		string checkIn = System.DateTime.Now.ToString();
		
		person.userData += string.Format("|CheckIn={0}", checkIn);
		Debug.Log(person.userData);
		cloudFaceManager.UpdatePersonData(groupId, person);
		Debug.Log("Person Updated in Cloud");

		panelInstance.gameObject.SetActive(false);
		SetHintText(string.Format("{0} Checked In", person.name));

		// load the main scene
		// SceneManager.LoadScene(1);
	}

	private void OnSaveUserClick(Face f, InputField saveNameInput, InputField safeInfoInput)
	{
		// create user info string
		Dictionary<string, string> dUserInfo = new Dictionary<string, string>();

		if(safeInfoInput.text != null && safeInfoInput.text.Trim() != string.Empty)
		{
			dUserInfo["UserInfo"] = safeInfoInput.text.Trim();
		}

		if(f != null && f.faceAttributes != null)
		{
			dUserInfo["Gender"] = f.faceAttributes.gender;
			dUserInfo["Age"] = f.faceAttributes.age.ToString();
			//dUserInfo["Smile"] = string.Format("{0:F3}", f.faceAttributes.smile);
			dUserInfo["Created"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		}

		string sUserInfo = CloudUserManager.ConvertDictToInfo(dUserInfo);

		// create the user
		StartCoroutine(AddUserToGroup(f, saveNameInput.text, sUserInfo));
	}

	// creates new user and adds it to the user group
	private IEnumerator AddUserToGroup(Face face, string userName, string userInfo)
	{
		Debug.Log("AddUserToGroup_Creating New User...");
		CloudUserManager userManager = CloudUserManager.Instance;

		if(texCamShot && userManager && face != null && userName != null && userName.Trim() != string.Empty)
		{
			SetHintText("Wait...");
			yield return null;

			FaceRectangle faceRect = face.faceRectangle;
			byte[] imageBytes = texCamShot.EncodeToJPG();
			yield return null;

			AsyncTask<Person> task = new AsyncTask<Person>(() => {
				return userManager.AddUserToGroup(userName.Trim(), userInfo, imageBytes, faceRect);
			});

			task.Start();
			yield return null;

			while (task.State == TaskState.Running)
			{
				yield return null;
			}

			// get the resulting person
			Person person = task.Result;

			if(!string.IsNullOrEmpty(task.ErrorMessage))
			{
				SetHintText(task.ErrorMessage);
				Debug.LogError(task.ErrorMessage);
			}
			else if(person != null && person.persistedFaceIds != null && person.persistedFaceIds.Length > 0)
			{
				string faceId = face.faceId;
				bool bFaceFound = false;

				for(int i = 0; i < faces.Length; i++)
				{
					if(faces[i].faceId == faceId)
					{
						if(faces[i].candidate == null || faces[i].candidate.person == null)
						{
							faces[i].candidate = new Candidate();

							faces[i].candidate.personId = person.personId;
							faces[i].candidate.confidence = 1f;
							faces[i].candidate.person = person;
						}

						bFaceFound = true;
						break;
					}
				}

				if(!bFaceFound)
				{
					Debug.Log(string.Format("Face {0} not found.", faceId));
				}

				SetHintText(string.Format("'{0}' created successfully.", userName.Trim()));
				ShowIdentityResult();
			}
			else
			{
				SetHintText("User could not be created.");
			}
		}

		yield return null;
	}

    // clear the list of displayed identity results
    private void ClearIdentityResult()
    {
		foreach(GameObject panel in personsPanels.Values)
		{
			panel.transform.SetParent(null, false);
			Destroy(panel);
		}

		personsPanels.Clear();
    }

    // displays hint or status text
    private void SetHintText(string sHintText)
    {
        if (hintText)
        {
            hintText.text = sHintText;
        }
        else
        {
            Debug.Log(sHintText);
        }
    }

}
