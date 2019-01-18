using UnityEngine;
using System.Collections;

public class RollCameraController : MonoBehaviour 
{
	public GameObject player;
	Vector3 offset;

	void Start () 
	{
		offset = transform.position - player.transform.position;
	}
	
	void Update () 
	{
		transform.position = player.transform.position + offset;
	}
}
