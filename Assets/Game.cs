﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets;
using Assets.Fileparsers;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Material TextureMaterialPrefab;

    public string GamePath;
    public Texture2D SurfaceTexture;
    public Material TerrainMaterial;
    public Material SkyMaterial;

    public Texture2D[] HeightmapTextures;
    public Terrain[,] TerrainPatches;
    public Vector2 RealTerrainGrid;

    public Color32[] Palette;

    public string SdfToLoad;
    public string MapToLoad;

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        VirtualFilesystem.Instance.Init(GamePath);

        Palette = ActPaletteParser.ReadActPalette("t11.ACT");

        //var path = @"e:\i76\extracted\map";
        //Directory.CreateDirectory(path);
        //foreach (var mapFile in VirtualFilesystem.Instance.FindAllWithExtension(".map"))
        //{
        //    File.WriteAllBytes(Path.Combine(path, mapFile), MapTextureParser.ReadMapTexture(mapFile, Palette).EncodeToPNG());
        //}
        //path = @"e:\i76\extracted\vqm";
        //Directory.CreateDirectory(path);
        //foreach (var mapFile in VirtualFilesystem.Instance.FindAllWithExtension(".vqm"))
        //{
        //    File.WriteAllBytes(Path.Combine(path, mapFile), VqmTextureParser.ReadVqmTexture(mapFile, Palette).EncodeToPNG());
        //}

        var levelManager = new LevelManager();
        levelManager.TextureMaterialPrefab = TextureMaterialPrefab;
        levelManager.Palette = Palette;

        levelManager.ImportSdf(SdfToLoad, null, null, Vector3.zero, Quaternion.identity);

        if (!string.IsNullOrEmpty(MapToLoad))
            GetComponent<GUITexture>().texture = MapTextureParser.ReadMapTexture(MapToLoad, Palette);

        TerrainPatches = new Terrain[80, 80];
        var textures = new List<Texture2D>();
        var mdef = MsnMissionParser.ReadMsnMission("T05.msn");

        var palette = ActPaletteParser.ReadActPalette(mdef.PaletteFilePath);
        SurfaceTexture = MapTextureParser.ReadMapTexture(mdef.SurfaceTextureFilePath, palette);

        var skyTexture = MapTextureParser.ReadMapTexture(mdef.SkyTextureFilePath, palette);
        SkyMaterial.mainTexture = skyTexture;

        var splatPrototypes = new SplatPrototype[1]
        {
            new SplatPrototype
            {
                texture = SurfaceTexture,
                tileSize = new Vector2(SurfaceTexture.width, SurfaceTexture.height),
                metallic = 0,
                smoothness = 0
            }
        };
        for (int z = 0; z < 80; z++)
        {
            for (int x = 0; x < 80; x++)
            {
                if (mdef.TerrainPatches[x, z] == null)
                    continue;

                var patchGameObject = new GameObject("Ter " + x + ", " + z);
                patchGameObject.transform.position = new Vector3(x * 640, 0, z * 640);
                patchGameObject.SetActive(false);

                var terrain = patchGameObject.AddComponent<Terrain>();
                terrain.terrainData = mdef.TerrainPatches[x, z].TerrainData;
                terrain.terrainData.splatPrototypes = splatPrototypes;
                terrain.materialTemplate = TerrainMaterial;
                terrain.materialType = Terrain.MaterialType.Custom;

                var terrainCollider = patchGameObject.AddComponent<TerrainCollider>();
                terrainCollider.terrainData = terrain.terrainData;

                foreach (var odef in mdef.TerrainPatches[x, z].Objects)
                {
                    //Debug.Log("Load " + odef.Label + " id: " + odef.Id);
                    if(odef.ClassId == 4)
                        levelManager.ImportSdf(odef.Label + ".sdf", odef.Label, patchGameObject.transform, odef.LocalPosition, odef.LocalRotation);
                }


                var texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                for (int iz = 0; iz < 128; iz++)
                {
                    for (int ix = 0; ix < 128; ix++)
                    {
                        var h = terrain.terrainData.GetHeight(ix, iz);
                        texture.SetPixel(ix, iz, Color.white * h);
                    }
                }
                texture.Apply();
                textures.Add(texture);

                TerrainPatches[x, z] = terrain;
                RealTerrainGrid = new Vector2(x, z);
            }
        }
        HeightmapTextures = textures.ToArray();

        RepositionCurrentTerrainPatch(RealTerrainGrid);
    }

    // Update is called once per frame
    void Update()
    {
        var newx = Camera.main.transform.position.x;
        var newz = Camera.main.transform.position.z;
        bool changed = false;
        var newTerrainGrid = RealTerrainGrid;

        if (newx > 640) // Moved right
        {
            newTerrainGrid.x += 1;
            newx = 0;
            changed = true;
        }
        else if (newx < 0) // Moved left
        {
            newTerrainGrid.x -= 1;
            newx = 640;
            changed = true;
        }
        if (newz > 640) // Moved back
        {
            newTerrainGrid.y += 1;
            newz = 0;
            changed = true;

        }
        else if (newz < 0.0f) // Moved forward
        {
            newTerrainGrid.y -= 1;
            newz = 640;
            changed = true;
        }

        if (changed)
        {
            Camera.main.transform.position = new Vector3(newx, Camera.main.transform.position.y, newz);
            RepositionCurrentTerrainPatch(newTerrainGrid);
        }
    }

    private void RepositionCurrentTerrainPatch(Vector2 newTerrainGrid)
    {
        for (int z = -1; z <= 1; z++)
        {
            var tpZ = (int)(RealTerrainGrid.y + z);
            if (tpZ < 0 || tpZ > 79)
                continue;
            for (int x = -1; x <= 1; x++)
            {
                var tpX = (int)(RealTerrainGrid.x + x);
                if (tpX < 0 || tpX > 79)
                    continue;
                var tp = TerrainPatches[tpX, tpZ];
                if (tp == null)
                    continue;
                tp.gameObject.SetActive(false);
            }
        }

        for (int z = -1; z <= 1; z++)
        {
            var tpZ = (int)(newTerrainGrid.y + z);
            if (tpZ < 0 || tpZ > 79)
                continue;
            for (int x = -1; x <= 1; x++)
            {
                var tpX = (int)(newTerrainGrid.x + x);
                if (tpX < 0 || tpX > 79)
                    continue;
                var tp = TerrainPatches[tpX, tpZ];
                if (tp == null)
                    continue;
                tp.gameObject.SetActive(true);
                tp.transform.position = new Vector3(x * 640, 0, z * 640);
            }
        }

        RealTerrainGrid = newTerrainGrid;
    }
}
