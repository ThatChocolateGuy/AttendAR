using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class RollPlayerController : MonoBehaviour 
{
	public float speed = 1;

	public Text counterText;
	public Text userText;
	public RawImage userImage;

	private Rigidbody rb;
	private int counter = 0;


	void Start () 
	{
		rb = GetComponent<Rigidbody> ();

		if(counterText)
			counterText.text = "Collect the cubes";

		// show the logged-in user
		CloudUserData userData = CloudUserData.Instance;

		string userLogged = "No user logged in";
		Texture2D userLoggedImage = null;

		if(userData && userData.selectedUser != null)
		{
			// user was selected on previous scene
			userLogged = userData.selectedUser.candidate.person.name;
			userLoggedImage = userData.selectedUser.faceImage;
		}

		// show the user name and image
		if(userText)
			userText.text = userLogged;
		if(userImage)
			userImage.texture = userLoggedImage;
	}


	void Update () 
	{
		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");

		rb.AddForce (h * speed, 0, v * speed);

		if (Input.GetButtonDown ("Jump")) 
		{
			rb.AddForce (0, 300, 0);
		}
	}


	void OnTriggerEnter(Collider other) 
	{
		Destroy(other.gameObject);

		counter = counter + 1;
		if(counterText)
			counterText.text = "Collected " + counter + " cubes";

		if (counter == 8) 
		{
			if(counterText)
				counterText.text = "YOU WIN!";
		}
	}


	// invoked when the logout button gets pressed
	public void OnLogoutPressed()
	{
		CloudUserData userData = CloudUserData.Instance;

		if(userData && userData.selectedUser != null)
		{
			userData.ClearSelectedUser();
		}

		// show the user name and image
		if(userText)
			userText.text = "No user logged in";
		if(userImage)
			userImage.texture = null;

		// go back tp the login-scene
		SceneManager.LoadScene(0);
	}

}
