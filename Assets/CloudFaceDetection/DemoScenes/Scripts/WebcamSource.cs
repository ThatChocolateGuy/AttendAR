using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Text;


public class WebcamSource : MonoBehaviour, ImageSourceInterface
{
	[Tooltip("Whether the web-camera output needs to be flipped horizontally or not.")]
	public bool flipHorizontally = false;

	[Tooltip("Selected web-camera name, if any.")]
	public string webcamName;

	// the web-camera texture
	private WebCamTexture webcamTex;

	// whether the output aspect ratio is set
	private bool bTexResolutionSet = false;


	public virtual void Awake () 
	{
		WebCamDevice[] devices = WebCamTexture.devices;

		if (devices != null && devices.Length > 0)
		{
			// print available webcams
			StringBuilder sbWebcams = new StringBuilder();
			sbWebcams.Append("Available webcams:").AppendLine();

			foreach(WebCamDevice device in devices)
			{
				sbWebcams.Append(device.name).AppendLine();
			}

			Debug.Log(sbWebcams.ToString());

			// get 1st webcam name, if not set
			if(string.IsNullOrEmpty(webcamName))
			{
				webcamName = devices[0].name;
			}

			// create webcam tex
			webcamTex = new WebCamTexture(webcamName);

			OnApplyTexture(webcamTex);

			bTexResolutionSet = false;
		}

		if (flipHorizontally)
		{
			Vector3 scale = transform.localScale;
			transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
		}

		if (HasCamera())
		{
			webcamTex.Play();
		}
	}


	void Update()
	{
		if (!bTexResolutionSet && webcamTex != null && webcamTex.isPlaying)
		{
			OnSetAspectRatio(webcamTex.width, webcamTex.height);

			bTexResolutionSet = true;
		}
	}


	void OnDisable()
	{
		if(webcamTex)
		{
			webcamTex.Stop();
			webcamTex = null;
		}
	}


	/// <summary>
	/// Gets the image as texture2d.
	/// </summary>
	/// <returns>The image.</returns>
	public Texture2D GetImage()
	{
		Texture2D snap = new Texture2D(webcamTex.width, webcamTex.height, TextureFormat.ARGB32, false);

		if (webcamTex)
		{
			snap.SetPixels(webcamTex.GetPixels());
			snap.Apply();

			if (flipHorizontally)
			{
				snap = CloudTexTools.FlipTexture(snap);
			}
		}

		return snap;
	}


	// Check if there is web camera
	public bool HasCamera()
	{
		return webcamTex && !string.IsNullOrEmpty(webcamTex.deviceName);
	}


	public void OnApplyTexture(Texture tex)
    {
        RawImage rawimage = GetComponent<RawImage>();
        if (rawimage)
        {
			rawimage.texture = tex;
			//rawimage.material.mainTexture = tex;
        }
    }

	public void OnSetAspectRatio(int width, int height)
    {
        AspectRatioFitter ratioFitter = GetComponent<AspectRatioFitter>();
        if (ratioFitter)
        {
            ratioFitter.aspectRatio = (float)width / (float)height;
        }
    }
}
