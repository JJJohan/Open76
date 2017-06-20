﻿using System.Collections;
using System.Collections.Generic;
using Assets;
using Assets.Fileparsers;
using UnityEngine;

public class Sky : MonoBehaviour
{
	public string TextureFilename;
    public Vector2 Speed;
    public float Height;
    private Material _material;

    // Use this for initialization
	void Start ()
	{
	    _material = GetComponent<MeshRenderer>().material;


        var cacheManager = FindObjectOfType<CacheManager>();
		_material.mainTexture = TextureParser.ReadMapTexture(TextureFilename, cacheManager.Palette);
	}
	
	// Update is called once per frame
	void Update ()
	{
	    _material.mainTextureOffset += Speed*Time.deltaTime;
	    transform.position = Camera.main.transform.position + Vector3.up* Height;
	}
}
