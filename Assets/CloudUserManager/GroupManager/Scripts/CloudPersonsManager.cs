using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine.EventSystems;
using System.Linq;

public class CloudPersonsManager : MonoBehaviour 
{
    public GameObject personsListPanel;
    public GameObject personDetailsPanel;

	public GameObject personPanelPrefab;

	private Text listHeaderText;
    private RectTransform personsListContent;
    private ModalPanel modalPanel;

    private List<Person> persons;
    private Person selectedPerson;
    private Dictionary<string, GameObject> personsPanels = new Dictionary<string, GameObject>();

    // Checks whether personsListPanel has been loaded
    private bool isPanelLoaded = false;

    void Awake()
    {
        modalPanel = ModalPanel.Instance();
        //personPanelPrefab = Resources.LoadAssetAtPath<GameObject>("Assets/GroupManagement/Prefabs/PersonListItem.prefab");

        personsListContent = personsListPanel.FindComponentInChildWithTag<RectTransform>("ListViewContent");
		listHeaderText = personsListPanel.FindComponentInChildWithTag<Text>("ListHeaderText");
    }

    // Use this for initialization
    void Start()
    {
        personsListPanel.SetActive(false);
    }

    public void ToggleStudentManager() {
        if (!isPanelLoaded) {
            StartCoroutine(LoadPersons());
            StartCoroutine(CheckGroupStatus());
            isPanelLoaded = true;
        }

        if (personsListPanel.activeInHierarchy ==  true)
            personsListPanel.SetActive(false);
        else
            personsListPanel.SetActive(true);
    }

    public void OnAddNewPerson()
    {
        ShowPersonDetail(true);
    }

    public void OnCancelPerson()
    {        
        HidePersonDetails();
        selectedPerson = null;
    }

    public void OnSavePerson()
    {
        string personName = PersonNameInputValue;
        string userData = PersonUserDataInputValue;
        
		userData = userData.Replace("\r\n", "|").Trim();
        userData = userData.Replace("\r", "|").Trim();
        userData = userData.Replace("\n", "|").Trim();

        if (personName.Trim().Length == 0)
        {
            modalPanel.ShowMessage("Please enter a name!");
            return;
        }

        if (selectedPerson != null)
        {
            StartCoroutine(UpdatePerson(selectedPerson, personName, userData));
            selectedPerson = null;
        }
        else
        {
            StartCoroutine(CreatePerson(personName));
        }
    }

    public void OnDeletePerson()
    {
        if (selectedPerson != null)
        {
            modalPanel.ShowYesNoDialog(string.Format("Are you sure you want to delete {0}?", selectedPerson.name), () => {
                StartCoroutine(DeletePerson(selectedPerson));

                selectedPerson = null;
            });
        }
    }

    public void OnReloadPersons()
    {
        StartCoroutine(LoadPersons());
    }

    public void OnTrainGroup()
    {
        StartCoroutine(TrainGroup());
    }

    private IEnumerator TrainGroup()
    {
        AsyncTask<bool> task = new AsyncTask<bool>(() =>
        {
			CloudUserManager groupMgr = CloudUserManager.Instance;
            return groupMgr.StartGroupTraining();
        });

        bool abort = false;
        modalPanel.ShowProgress("Started group training. Please wait.", () => abort = true);

        task.Start();
        yield return null;

        while (task.State == TaskState.Running)
            yield return null;

        if (!task.Result)
        {
			if(!string.IsNullOrEmpty(task.ErrorMessage))
				modalPanel.ShowMessage(task.ErrorMessage);
			else
            	modalPanel.ShowMessage("Group training failed. Please, try again later.");
			
            yield return null;
        }
        else if(!abort)
        {
            task = new AsyncTask<bool>(() =>
            {
				CloudUserManager groupMgr = CloudUserManager.Instance;

                bool isTrained = false;
                int retries = 0;
                while (!isTrained && retries++ < 3 && !abort)
                {
                    Thread.Sleep(5000);
                    isTrained = groupMgr.IsGroupTrained();
                }

                return isTrained;
            });

            task.Start();
            yield return null;

            while (task.State == TaskState.Running)
                yield return null;

            if (!abort)
            {
                if (!task.Result)
                {
					if(!string.IsNullOrEmpty(task.ErrorMessage))
						modalPanel.ShowMessage(task.ErrorMessage);
					else
                    	modalPanel.ShowMessage("Group training failed. Please, try again later.");
                }
                else
                {
                    modalPanel.ShowMessage("Group training succeeded.");
                    TrainGroupButton.SetActive(false);
                }
            }
        }

        yield return null;
    }

    private IEnumerator CheckGroupStatus()
    {
        AsyncTask<bool> task = new AsyncTask<bool>(() =>
        {
			CloudUserManager groupMgr = CloudUserManager.Instance;

			// wait for the group manager to start
			int waitPeriods = 10;
			while(groupMgr == null && waitPeriods > 0)
			{
				Thread.Sleep(500);
				waitPeriods--;

				groupMgr = CloudUserManager.Instance;
			}

			return groupMgr ? groupMgr.IsGroupTrained() : false;
        });

        task.Start();
        yield return null;

        while (task.State == TaskState.Running)
            yield return null;

        if (!task.Result)
        {
			if(!string.IsNullOrEmpty(task.ErrorMessage))
				Debug.LogError(task.ErrorMessage);
			
            TrainGroupButton.SetActive(true);
        }

		CloudUserManager userManager = CloudUserManager.Instance;
		if(userManager && listHeaderText)
		{
			listHeaderText.text = "Group: " + userManager.userGroupId;
		}

        yield return null;
    }

    private IEnumerator LoadPersons()
    {
        modalPanel.ShowProgress("Loading users. Please wait ...");

        // Clear persons from the list
        if(persons != null) persons.Clear();
        foreach(GameObject panel in personsPanels.Values)
        {
            DestroyPersonPanel(panel);
        }

        personsPanels.Clear();

        AsyncTask<List<Person>> task = new AsyncTask<List<Person>>(() =>
        {
            // load persons here
			CloudUserManager groupMgr = CloudUserManager.Instance;

			// wait for the group manager to start
			int waitPeriods = 10;
			while(groupMgr == null && waitPeriods > 0)
			{
				Thread.Sleep(500);
				waitPeriods--;

				groupMgr = CloudUserManager.Instance;
			}

			return groupMgr ? groupMgr.GetUsersList() : null;
        });

        task.Start();
        yield return null;

        while (task.State == TaskState.Running)
            yield return null;

        modalPanel.Hide();
		persons = task.Result;

		if(persons != null)
		{
			// sort the person names alphabetically
			persons = persons.OrderBy(p => p.name).ToList();

			foreach (Person p in persons)
			{
				InstantiatePersonPanel(p);
			}
		}
		else
		{
			if(!string.IsNullOrEmpty(task.ErrorMessage))
				Debug.LogError(task.ErrorMessage);
			else
				Debug.LogError("Error loading users' list. Check the FaceManager- and UserGroupManager-components.");
		}

        yield return null;
    }

    private void InstantiatePersonPanel(Person p)
    {
		if(!personPanelPrefab)
		{
			Debug.LogError("PersonPanel-prefab not set.");
			return;
		}

        GameObject personPanelInstance = Instantiate<GameObject>(personPanelPrefab);

		GameObject personNameObj = personPanelInstance.transform.Find("PersonName").gameObject;
		Text personNameTxt = personNameObj.GetComponent<Text>(); // personPanelInstance.GetComponentInChildren<Text>();
		personNameTxt.text = p.name;

		GameObject personIDObj = personPanelInstance.transform.Find("PersonID").gameObject;
		Text personIDTxt = personIDObj.GetComponent<Text>();
		personIDTxt.text = "ID: " + p.personId;

        personPanelInstance.transform.SetParent(personsListContent, false);
        AddPersonPanelClickListener(personPanelInstance, p);
        personsPanels.Add(p.personId, personPanelInstance);
    }

    private void DestroyPersonPanel(GameObject panel)
    {
        panel.transform.SetParent(null, false);
        Destroy(panel);
    }

    private IEnumerator UpdatePerson(Person p, string name, string userData)
    {
        modalPanel.ShowProgress("Saving data, Please Wait ...");

        AsyncTask<bool> task = new AsyncTask<bool>(() =>
        {
            try
            {
                // update data in the cloud
				CloudUserManager groupMgr = CloudUserManager.Instance;

				if(groupMgr != null && p != null)
				{
					p.name = name;
                    p.userData = userData;
					groupMgr.UpdateUserData(p);

					return true;
				}

                return false;
            }
            catch (Exception ex)
            {
				Debug.LogError("Failed to process task: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        });

        task.Start();
        yield return null;

        while (task.State == TaskState.Running)
            yield return null;

        modalPanel.Hide();

        if (!task.Result)
        {
            modalPanel.ShowMessage("Error saving data!");
        }
        else 
		{
            HidePersonDetails();

            if (p != null)
            {
                GameObject personPanelInstance = personsPanels[p.personId];
                Text personName = personPanelInstance.GetComponentInChildren<Text>();
                personName.text = name;
            }
        }  

        yield return null;
    }

    private IEnumerator CreatePerson(string name)
    {
        modalPanel.ShowProgress("Saving data, Please Wait ...");
		Person p = null;

        AsyncTask<bool> task = new AsyncTask<bool>(() =>
        {
            try
            {
                // update data in the cloud
				CloudUserManager groupMgr = CloudUserManager.Instance;

				if(groupMgr != null && persons != null)
				{
					p = groupMgr.AddUserToGroup(name, string.Empty);
					return true;
				}
				
                return false;
            }
            catch (Exception ex)
            {
				Debug.LogError("Failed to process task: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        });

        task.Start();
        yield return null;

        while (task.State == TaskState.Running)
            yield return null;

        modalPanel.Hide();

        if (!task.Result)
        {
            modalPanel.ShowMessage("Error saving data!");
        }
        else 
		{
            HidePersonDetails();

			if(p != null)
			{
            	persons.Add(p);
				InstantiatePersonPanel(p);
			}
        }

        yield return null;
    }

    private IEnumerator DeletePerson(Person p)
    {
        modalPanel.ShowProgress("Deleting user, Please Wait ...");

        AsyncTask<bool> task = new AsyncTask<bool>(() =>
        {
            try
            {
                // Debug.Log("Trying to delete user");

                // update data in the cloud
				CloudUserManager groupMgr = CloudUserManager.Instance;

                // Debug.Log(String.Format(
                //     "Group Manager: {0}\n" +
                //     "Person: {1}\n" +
                //     "Persons: {2}",
                //     groupMgr != null ? "exists" : null, p.name, persons[0]));
				
				if(groupMgr != null && p != null && persons != null)
				{
                    Debug.Log("About to delete user");
					groupMgr.DeleteUser(p);
                    Debug.Log("User Deleted");
					return true;
				}

                return false;
            }
            catch (Exception ex)
            {
				Debug.LogError("Failed to process task: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        });

        task.Start();
        yield return null;

        while (task.State == TaskState.Running)
            yield return null;

        modalPanel.Hide();

        if (!task.Result)
        {
            modalPanel.ShowMessage("Error deleting user!");
        }
        else 
		{
            HidePersonDetails();

            if (p != null)
            {
                persons.Remove(p);
                
                GameObject personPanelInstance = personsPanels[p.personId];
                DestroyPersonPanel(personPanelInstance);
                personsPanels.Remove(p.personId);
            }
        }

        yield return null;
    }

    private void LoadPersonDetails(Person p)
    {
        PersonNameInputValue = p != null ? p.name : "";

		string userInfo = p != null && p.userData != null ? p.userData.Replace("|", "\r\n") : "";
		PersonUserDataInputValue = userInfo;

        PersonFaceIdText = p != null && p.persistedFaceIds != null && p.persistedFaceIds.Length > 0 ? ("FaceID: " + p.persistedFaceIds[0].ToString()) : "No Face ID";
        PersonIdText = p != null && p.personId != string.Empty ? p.personId : "";
    }

    private string PersonNameInputValue
    {
        get
        {
            InputField personName = FindComponent<InputField>(personDetailsPanel, "InputFieldName");
            if (personName)
                return personName.text;
            else
                return "";
        }
        set
        {
            InputField personName = FindComponent<InputField>(personDetailsPanel, "InputFieldName"); ;
            if(personName)
                personName.text = value;
        }
    }

    private string PersonUserDataInputValue
    {
        get
        {
            InputField personUserData = FindComponent<InputField>(personDetailsPanel, "InputFieldUserData");
            if (personUserData)
                return personUserData.text;
            else
                return "";
        }
        set
        {
            InputField personUserData = FindComponent<InputField>(personDetailsPanel, "InputFieldUserData");
            if (personUserData)
                personUserData.text = value ?? "";
        }
    }

    private string PersonFaceIdText
    {
        set
        {
            Text personFaceID = FindComponent<Text>(personDetailsPanel, "PersonFaceID");
            if (personFaceID)
                personFaceID.text = value ?? "";
        }
    }

    private string PersonIdText
    {
        set
        {
            Text personID = FindComponent<Text>(personDetailsPanel, "TextPersonID");
            if (personID)
                personID.text = value ?? "";
        }
    }

    private GameObject TrainGroupButton
    {
        get
        {
            return personsListPanel.FindChildWithTag("TrainGroupButton");
        }
    }

    private T FindComponent<T>(GameObject parent, string componentName) where T: UnityEngine.Object
    {
        return parent.GetComponentsInChildren<T>().FirstOrDefault(x => x.name == componentName);
    }

    private void AddPersonPanelClickListener(GameObject panel, Person p)
    {
        EventTrigger trigger = panel.GetComponentInParent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { OnPersonClick(p); });
        trigger.triggers.Add(entry);
    }

    private void OnPersonClick(Person p)
    {
        selectedPerson = p;
        ShowPersonDetail();
        LoadPersonDetails(p);
    }

    private void ShowPersonDetail(bool newPerson = false)
    {
        personsListPanel.SetActive(false);
        personDetailsPanel.SetActive(true);

        GameObject deletePersonButton = personDetailsPanel.FindChildWithTag("DeletePersonButton");
        deletePersonButton.SetActive(!newPerson);
    }

    private void HidePersonDetails()
    {
        LoadPersonDetails(null);
        personDetailsPanel.SetActive(false);
        personsListPanel.SetActive(true);        
    }
}
