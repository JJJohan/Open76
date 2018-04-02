﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Fileparsers;
using UnityEngine;
using Object = UnityEngine.Object;
using Assets.Car;

namespace Assets.System
{
    class CacheManager : MonoBehaviour
    {        
        public string GamePath;
        public GameObject _3DObjectPrefab;
        public GameObject NoColliderPrefab;
        public RaySusp SteerWheelPrefab;
        public RaySusp DriveWheelPrefab;
        public GameObject CarBodyPrefab;
        public NewCar CarPrefab;

        public Material ColorMaterialPrefab;
        public Material TextureMaterialPrefab;
        public Material TransparentMaterialPrefab;
        public Color32[] Palette;


        private static readonly Dictionary<string, GeoMeshCacheEntry> _meshCache = new Dictionary<string, GeoMeshCacheEntry>();
        private readonly Dictionary<string, GameObject> _sdfCache = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();

        void Awake()
        {
            VirtualFilesystem.Instance.Init(GamePath);
            _materialCache["default"] = Instantiate(TextureMaterialPrefab);
        }

        public Material GetTextureMaterial(string textureName, bool transparent)
        {
            if (!_materialCache.ContainsKey(textureName))
            {
                Texture2D texture;
                if (VirtualFilesystem.Instance.FileExists(textureName + ".vqm"))
                {
                    texture = TextureParser.ReadVqmTexture(textureName + ".vqm", Palette);
                }
                else if (VirtualFilesystem.Instance.FileExists(textureName + ".map"))
                {
                    texture = TextureParser.ReadMapTexture(textureName + ".map", Palette);
                }
                else
                {
                    throw new Exception("Texture not found: " + textureName);
                }
                var material = Instantiate(transparent ? TransparentMaterialPrefab : TextureMaterialPrefab);
                material.mainTexture = texture;
                material.name = textureName;
                _materialCache[textureName] = material;
            }

            return _materialCache[textureName];
        }

        public Material GetColorMaterial(string materialName, Color32 color)
        {
            if (_materialCache.ContainsKey(materialName))
                return _materialCache[materialName];

            var material = Instantiate(ColorMaterialPrefab);
            material.color = color;
            _materialCache[materialName] = material;

            return material;
        }

        private Material GetMaterial(GeoFace geoFace, Vtf vtf)
        {

            if (geoFace.TextureName != null)
            {
                var textureName = geoFace.TextureName;
                if (vtf != null && geoFace.TextureName.StartsWith("V"))
                {
                    if (geoFace.TextureName.EndsWith("BO DY"))
                    {
                        textureName = vtf.Maps[12];
                    }
                    else
                    {
                        var key = geoFace.TextureName.Substring(1).Replace(" ", "").Replace("LF", "LT") + ".TMT";

                        if (vtf.Tmts.ContainsKey(key))
                        {
                            //Debug.Log("Vehicle tmt reference: " + geoFace.TextureName + " decoded: " + key);
                            var tmt = vtf.Tmts[key];
                            textureName = tmt.TextureNames[0];
                        }
                    }
                }
                return GetTextureMaterial(textureName, geoFace.SurfaceFlags2 == 5 || geoFace.SurfaceFlags2 == 7);
                //Debug.Log(geoFace.TextureName + "color=" + geoFace.Color + " flag1=" + geoFace.SurfaceFlags1 + " flag2=" + geoFace.SurfaceFlags2, mat);
            }
            return GetColorMaterial("color" + geoFace.Color, geoFace.Color);
        }

        class GeoMeshCacheEntry
        {
            public GeoMesh GeoMesh { get; set; }
            public Mesh Mesh { get; set; }
            public Material[] Materials { get; set; }
        }

        private GeoMeshCacheEntry ImportMesh(string filename, Vtf vtf)
        {
            if (_meshCache.ContainsKey(filename))
                return _meshCache[filename];

            var geoMesh = GeoParser.ReadGeoMesh(filename);

            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            var facesGroupedByMaterial = geoMesh.Faces.GroupBy(face => GetMaterial(face, vtf)).ToArray();
            mesh.subMeshCount = facesGroupedByMaterial.Length;
            var submeshTriangles = new Dictionary<Material, List<int>>();
            foreach (var faceGroup in facesGroupedByMaterial)
            {
                submeshTriangles[faceGroup.Key] = new List<int>();
                foreach (var face in faceGroup)
                {
                    var numTriangles = face.VertexRefs.Length - 3 + 1;
                    var viStart = vertices.Count;
                    foreach (var vertexRef in face.VertexRefs)
                    {
                        vertices.Add(geoMesh.Vertices[vertexRef.VertexIndex]);
                        normals.Add(geoMesh.Normals[vertexRef.VertexIndex] * -1);
                        uvs.Add(vertexRef.Uv);
                    }
                    for (var t = 1; t <= numTriangles; t++)
                    {
                        submeshTriangles[faceGroup.Key].Add(viStart + t);
                        submeshTriangles[faceGroup.Key].Add(viStart);
                        submeshTriangles[faceGroup.Key].Add(viStart + t + 1);
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.normals = normals.ToArray();
            var i = 0;
            foreach (var submeshTriangle in submeshTriangles)
            {
                mesh.SetTriangles(submeshTriangles[submeshTriangle.Key].ToArray(), i);
                i++;
            }
            mesh.RecalculateBounds();

            var cacheEntry = new GeoMeshCacheEntry
            {
                GeoMesh = geoMesh,
                Mesh = mesh,
                Materials = facesGroupedByMaterial.Select(x => x.Key).ToArray()
            };
            _meshCache.Add(filename, cacheEntry);

            return cacheEntry;
        }

        public GameObject ImportGeo(string filename, Vtf vtf, GameObject prefab)
        {
            var meshCacheEntry = ImportMesh(filename, vtf);

            var obj = Instantiate(prefab);
            obj.gameObject.name = meshCacheEntry.GeoMesh.Name;

            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = meshCacheEntry.Mesh;
                obj.GetComponent<MeshRenderer>().materials = meshCacheEntry.Materials;
            }
            var collider = obj.GetComponent<MeshCollider>();
            if (collider != null)
                collider.sharedMesh = meshCacheEntry.Mesh;

            return obj.gameObject;
        }


        public GameObject ImportSdf(string filename, Transform parent, Vector3 localPosition, Quaternion rotation)
        {
            if (_sdfCache.ContainsKey(filename))
            {
                var obj = Instantiate(_sdfCache[filename], parent);
                obj.transform.localPosition = localPosition;
                obj.transform.localRotation = rotation;
                return obj;
            }

            var sdf = SdfObjectParser.LoadSdf(filename);

            var sdfObject = new GameObject(sdf.Name);
            sdfObject.transform.parent = parent;
            sdfObject.transform.localPosition = localPosition;
            sdfObject.transform.rotation = rotation;

            var partDict = new Dictionary<string, GameObject> { { "WORLD", sdfObject } };

            foreach (var sdfPart in sdf.Parts)
            {
                var partObj = ImportGeo(sdfPart.Name + ".geo", null, _3DObjectPrefab);
                partObj.transform.parent = partDict[sdfPart.ParentName].transform;
                partObj.transform.localPosition = sdfPart.Position;
                partObj.transform.localRotation = Quaternion.identity;
                partDict.Add(sdfPart.Name, partObj);
            }

            _sdfCache.Add(filename, sdfObject);
            return sdfObject;
        }

        public GameObject ImportVcf(string filename)
        {
            var vcf = VcfParser.ParseVcf(filename);
            var vdf = VdfParser.ParseVdf(vcf.VdfFilename);
            var vtf = VtfParser.ParseVtf(vcf.VtfFilename);
            
            var carObject = Instantiate(CarPrefab); //ImportGeo(vdf.SOBJGeoName + ".geo", vtf, CarPrefab.gameObject).GetComponent<ArcadeCar>();
            carObject.gameObject.name = vdf.Name + " (" + vcf.VariantName + ")";

            foreach (var hLoc in vdf.HLocs)
            {
                var hlocGo = new GameObject("HLOC");
                hlocGo.transform.parent = carObject.transform;
                hlocGo.transform.localRotation = Quaternion.LookRotation(hLoc.Forward, hLoc.Up);
                hlocGo.transform.localPosition = hLoc.Position;
            }
            foreach (var vLoc in vdf.VLocs)
            {
                var vlocGo = new GameObject("VLOC");
                vlocGo.transform.parent = carObject.transform;
                vlocGo.transform.localRotation = Quaternion.LookRotation(vLoc.Forward, vLoc.Up);
                vlocGo.transform.localPosition = vLoc.Position;
            }

            var chassis = new GameObject("Chassis");
            chassis.transform.parent = carObject.transform;

            var thirdPerson = new GameObject("ThirdPerson");
            thirdPerson.transform.parent = chassis.transform;

            var firstPerson = new GameObject("FirstPerson");
            firstPerson.transform.parent = chassis.transform;
            firstPerson.SetActive(false);
                        
            ImportCarParts(thirdPerson, vtf, vdf.PartsThirdPerson[0], NoColliderPrefab);
            ImportCarParts(firstPerson, vtf, vdf.PartsFirstPerson, NoColliderPrefab, true);
                        
            var meshFilters = thirdPerson.GetComponentsInChildren<MeshFilter>();
            var bounds = new Bounds();
            bounds.SetMinMax(Vector3.one * float.MaxValue, Vector3.one * float.MinValue);
            foreach (var meshFilter in meshFilters)
            {
                var min = Vector3.Min(bounds.min, meshFilter.transform.position + meshFilter.sharedMesh.bounds.min) - thirdPerson.transform.position;
                var max = Vector3.Max(bounds.max, meshFilter.transform.position + meshFilter.sharedMesh.bounds.max) - thirdPerson.transform.position;
                bounds.SetMinMax(min, max);
            }

            var chassisCollider = new GameObject("ChassisColliders");
            chassisCollider.transform.parent = carObject.transform;
            ImportCarParts(chassisCollider, vtf, vdf.PartsThirdPerson[0], CarBodyPrefab);


            //ImportCarParts(chassisCollider, vtf, vdf.PartsThirdPerson[3], CarBodyPrefab);
            //chassisCollider.transform.localPosition = new Vector3(0, bounds.center.y - (chassisCollider.GetComponentInChildren<MeshCollider>().sharedMesh.bounds.size.y / 2.0f), 0);

            //var destroyed = ImportCarParts(car, vtf, vdf.PartsThirdPerson[3]);
            //destroyed.gameObject.SetActive(false);

            if (vcf.FrontWheelDef != null)
            {
                var frontWheels = CreateWheelPair("Front", 0, carObject.gameObject, vdf, vtf, vcf.FrontWheelDef.Parts);
                carObject.FrontWheels = frontWheels;
            }
            if (vcf.MidWheelDef != null)
            {
                CreateWheelPair("Mid", 2, carObject.gameObject, vdf, vtf, vcf.MidWheelDef.Parts);
            }
            if (vcf.BackWheelDef != null)
            {
                var rearWheels = CreateWheelPair("Back", 4, carObject.gameObject, vdf, vtf, vcf.BackWheelDef.Parts);
                carObject.RearWheels = rearWheels;
            }
            carObject.Chassis = chassis.transform;

            return carObject.gameObject;
        }

        private RaySusp[] CreateWheelPair(string placement, int wheelIndex, GameObject car, Vdf vdf, Vtf vtf, SdfPart[] parts)
        {
            var wheel1Name = placement + "Right";
            var wheel = Instantiate(placement == "Front" ? SteerWheelPrefab : DriveWheelPrefab, car.transform);
            wheel.gameObject.name = wheel1Name;
            ImportCarParts(wheel.transform.Find("Mesh").gameObject, vtf, parts, NoColliderPrefab, true);
            wheel.transform.localPosition = vdf.WheelLoc[wheelIndex].Position;

            var wheel2 = Instantiate(wheel, car.transform);
            wheel2.name = placement + "Left";
            wheel2.transform.localPosition = vdf.WheelLoc[wheelIndex + 1].Position;
            wheel2.transform.Find("Mesh").localScale = new Vector3(-1, 1, 1);

            return new[] { wheel, wheel2 };
        }

        private void ImportCarParts(GameObject parent, Vtf vtf, SdfPart[] sdfParts, GameObject prefab, bool forgetParentPosition = false)
        {
            var partDict = new Dictionary<string, GameObject> { { "WORLD", parent } };

            foreach (var sdfPart in sdfParts)
            {
                if (sdfPart.Name == "NULL")
                    continue;

                if (sdfPart.Name.EndsWith("3") || sdfPart.Name.EndsWith("1"))
                    continue;

                //GameObject prefab = NoColliderPrefab;
                //if (!wheel && sdfPart.Name.Substring(0, sdfPart.Name.Length - 1).EndsWith("BDY"))
                //    prefab = CarBodyPrefab;

                var partObj = ImportGeo(sdfPart.Name + ".geo", vtf, prefab);
                var parentName = sdfPart.ParentName;
                if (!partDict.ContainsKey(parentName))
                {
                    Debug.Log("Cant find parent '" + sdfPart.ParentName + "' for '" + sdfPart.Name + "'");
                    parentName = "WORLD";
                }

                if(!forgetParentPosition)
                    partObj.transform.parent = partDict[parentName].transform;
                partObj.transform.right = sdfPart.Right;
                partObj.transform.up = sdfPart.Up;
                partObj.transform.forward = sdfPart.Forward;
                partObj.transform.localPosition = sdfPart.Position;
                if(forgetParentPosition)
                    partObj.transform.parent = partDict[parentName].transform;
                partDict.Add(sdfPart.Name, partObj);
            }
        }

        public void ClearCache()
        {
            _materialCache.Clear();
            _sdfCache.Clear();
            _meshCache.Clear();
        }
    }
}
