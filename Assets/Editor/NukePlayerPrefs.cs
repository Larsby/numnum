using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NUnit.Framework;
using System.IO;


internal static class NukePlayerPrefs  {

	private const string MenuRoot = "Pastille/"; 
	[MenuItem (MenuRoot + "Nuke Playerprefs")]
	static bool Nuke() {
		Debug.Log ("Nuking playerprefs baby");
		PlayerPrefs.DeleteAll ();
		return true;
	}


}
