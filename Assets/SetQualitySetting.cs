using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetQualitySetting : MonoBehaviour {
	public int memorySize = 800;
	// Use this for initialization
	void Start () {
		int MemorySizeInMB = SystemInfo.systemMemorySize;
		if (MemorySizeInMB > 0 && MemorySizeInMB < memorySize) {
			QualitySettings.SetQualityLevel (0, true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
