﻿using Assets;
using Assets.Fileparsers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

    public string MissionFile;
    public string VcfToLoad;

    // Use this for initialization
    void Start () {
        var levelLoader = GetComponent<LevelLoader>();
        levelLoader.LoadLevel(MissionFile);

        var cacheManager = FindObjectOfType<CacheManager>();
        var importedVcf = cacheManager.ImportVcf(VcfToLoad);
        importedVcf.transform.position = new Vector3(339, 0.2f, 325);
        importedVcf.transform.localRotation = Quaternion.Euler(0, 96, 0);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
