using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;

public class AdManager : MonoBehaviour
{

	public class AdInfo
	{
		public string gameInfo;
		public string appstoreUrl;
		public string weburl;
		public string videourl;
	}

	List<AdInfo> ads;
	public VideoPlayer player;
	private bool ready = false;
	public VideoClip backupClip;
	// Use this for initialization
	public string adinfo = "http://www.pastille.se/ads/kicka_ads.txt";
	public Button buy;
	public Button visit;
	int index = 0;

	void Start ()
	{
		ads = new List<AdInfo> ();
		if (buy != null) {
			buy.enabled = false;
			buy.onClick.AddListener (BuyOnClick);
		} 
		if (visit != null) {
			visit.enabled = false;

			visit.onClick.AddListener (VisitOnClick);
		}
		StartCoroutine (LoadData (adinfo));
	}

	public void PlayAd ()
	{
		if (ads.Count <= 0) {
			ready = false;
			StartCoroutine (LoadData (adinfo));
		} else {
			index = Random.Range (0, ads.Count);

			if (ads [index].videourl.Length <= 0) {
				index = 0;
			}

			string url = ads [index].videourl;
			string fileName = Application.persistentDataPath + GetNameFromUrl (url);

			if (FileExistInCache (fileName)) {
				player.url = fileName;
				player.Play ();
			} else {
				StartCoroutine (SaveClip (url));
			}
		}
	}

	string GetNameFromUrl (string url)
	{
		string[] tokens = url.Split ('/');
		if (tokens != null) {
			return "/" + tokens [tokens.Length - 1];
		}
		return null;
	}

	bool FileExistInCache (string clipPath)
	{
		return File.Exists (clipPath);
	}

	IEnumerator SaveClip (string clipURL)
	{
		var www = new WWW (clipURL);
		yield return www;
		string name = GetNameFromUrl (clipURL);
		File.WriteAllBytes (Application.persistentDataPath + name, www.bytes);

		player.url = Application.persistentDataPath + name;
		player.Play ();
	}

	public void BuyOnClick ()
	{
		Application.OpenURL (ads [index].appstoreUrl);
	}

	public void VisitOnClick ()
	{
		Application.OpenURL (ads [index].weburl);
	}


	void Update ()
	{
		if (ready) {
			ready = false;
			PlayAd ();
			if (buy != null) {
				buy.enabled = true;
			}
			if (visit != null) {
				visit.enabled = true;
			}
			ready = false;
		}
	}


	IEnumerator LoadData (string url)
	{
		WWW www = new WWW (url);

		yield return www;
		string text = www.text;
		if (text == null) {
			
			yield return null;
		}
		if (text != null && text.Length > 1) {
			string[] lines = text.Split ('\n');
			foreach (string line in lines) {
				string[] info = line.Split ('|');
				if (info.Length > 4) {
					AdInfo ad = new AdInfo ();
					ad.gameInfo = info [0];
					ad.appstoreUrl = info [1];
					ad.weburl = info [2];
					ad.videourl = info [3];
					ads.Add (ad);
					#if UNITY_ANDROID
					if (info.Length >= 5) {
					ad.appstoreUrl = info [4];
					}
					#endif
				}
			}
		
			ready = true;

		} else {
			Debug.Log ("Could not load from www");
			if (backupClip != null) {
				player.clip = backupClip;
				player.Play ();
			}

		}


	}
}
