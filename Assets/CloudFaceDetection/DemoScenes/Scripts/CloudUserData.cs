using UnityEngine;
using System.Collections;

public class CloudUserData : MonoBehaviour 
{
	[Tooltip("The selected user, if any.")]
	public Face selectedUser;


	// singleton class instance
	private static CloudUserData instance = null;


	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <value>The singleton instance.</value>
	public static CloudUserData Instance
	{
		get
		{
			return instance;
		}
	}


	/// <summary>
	/// Clears the selected user.
	/// </summary>
	public void ClearSelectedUser()
	{
		selectedUser = null;
	}


	private void Awake()
	{
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this);
		}
		else if(this != instance)
		{
			Destroy(gameObject);
		}
	}

}
