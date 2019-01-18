using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System;
using System.IO;
using System.Net;
using System.Net.Security;

public class CloudWebTools 
{

	// calls web service with url, content and headers
	public static HttpWebResponse DoWebRequest(string requestUrl, string method, string contentType, byte[] content, Dictionary<string, string> headers, bool bAwaitResponse, bool bStopOnError)
	{
		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
		HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUrl);

		if(contentType != string.Empty && content != null)
		{
			webRequest.ContentType = contentType;
			webRequest.ContentLength = content.Length;
		}

		foreach(string hName in headers.Keys)
		{
			webRequest.Headers.Add(hName, headers[hName]);
		}

		webRequest.Method = !string.IsNullOrEmpty(method) ? method : "POST";

		if(content != null)
		{
			//using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
			using (Stream stream = webRequest.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}
		}

		// 'bAwaitResponse = false' - not yet implemented
		HttpWebResponse httpResponse = null;

		try 
		{
			httpResponse = (HttpWebResponse)webRequest.GetResponse();
		} 
		catch (WebException ex) 
		{
			httpResponse = (HttpWebResponse)ex.Response;

			if(bStopOnError)
			{
				throw new Exception(ex.Message + " - " + requestUrl);
			}
		}

		return httpResponse;
	}

	private static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
	{
		bool isOk = true;

		// If there are errors in the certificate chain, look at each error to determine the cause.
		if (sslPolicyErrors != SslPolicyErrors.None) 
		{
			for (int i = 0; i < chain.ChainStatus.Length; i++) 
			{
				if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown) 
				{
					chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
					chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
					chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
					bool chainIsValid = chain.Build ((X509Certificate2)certificate);

					if (!chainIsValid) 
					{
						isOk = false;
					}
				}
			}
		}
		return isOk;
	}


	// returns the response status code
	public static int GetStatusCode(HttpWebResponse response)
	{
		int status = -1;
		
		if(response != null)
		{
			status = (int)response.StatusCode;
		}
		
		return status;
	}
	
	// checks if the response status is error
	public static bool IsErrorStatus(HttpWebResponse response)
	{
		int status = GetStatusCode(response);
		return (status >= 300);
	}
	
	// returns the response status message
	public static string GetStatusMessage(HttpWebResponse response)
	{
		string message = string.Empty;
		
		if(response != null)
		{
			message = response.StatusDescription;
		}
		
		return message.Trim();
	}
	
	

}
