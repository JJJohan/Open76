﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.CarSystems;
using Assets.Scripts.Entities;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts.System
{
    internal class CacheManager
    {
        public GameObject ProjectilePrefab { get; private set; }
        public Color32[] Palette { get; set; }

        private readonly GameObject _3DObjectPrefab;
        private readonly GameObject _noColliderPrefab;
        private readonly RaySusp _steerWheelPrefab;
        private readonly RaySusp _driveWheelPrefab;
        private readonly GameObject _carBodyPrefab;
        private readonly Car _carPrefab;
        
        private readonly Material _colorMaterialPrefab;
        private readonly Material _textureMaterialPrefab;
        private readonly Material _transparentMaterialPrefab;
        private readonly Material _unlitTextureMaterialPrefab;
        private readonly Material _unlitTransparentMaterialPrefab;
        private readonly Material _carMirrorMaterialPrefab;

        private readonly string[] _bannedNames =
        {
            "51CMP3",
            "51SPC3",
            "51SYS3",
            "51WEP3",
            "51RTC1",
            "SWHL" // Steering wheel, not animated yet
        };

        private static CacheManager _instance;
        public static CacheManager Instance
        {
            get { return _instance ?? (_instance = new CacheManager()); }
        }

        private static readonly Dictionary<string, GeoMeshCacheEntry> MeshCache = new Dictionary<string, GeoMeshCacheEntry>();
        private readonly Dictionary<string, Sdf> _sdfCache = new Dictionary<string, Sdf>();
        private readonly Dictionary<string, Xdf> _xdfCache = new Dictionary<string, Xdf>();
        private readonly Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();


        private CacheManager()
        {
            _colorMaterialPrefab = Resources.Load<Material>("Materials/AlphaMaterial");
            _textureMaterialPrefab = Resources.Load<Material>("Materials/TextureMaterial");
            _transparentMaterialPrefab = Resources.Load<Material>("Materials/AlphaMaterial");
            _unlitTextureMaterialPrefab = Resources.Load<Material>("Materials/UnlitTextureMaterial");
            _unlitTransparentMaterialPrefab = Resources.Load<Material>("Materials/UnlitAlphaMaterial");
            _carMirrorMaterialPrefab = Resources.Load<Material>("Materials/CarMirror");

            ProjectilePrefab = Resources.Load<GameObject>("Prefabs/ProjectilePrefab");
            _3DObjectPrefab = Resources.Load<GameObject>("Prefabs/ObjectPrefab");
            _noColliderPrefab = Resources.Load<GameObject>("Prefabs/NoColliderPrefab");
            _steerWheelPrefab = Resources.Load<RaySusp>("Prefabs/SteerWheelPrefab");
            _driveWheelPrefab = Resources.Load<RaySusp>("Prefabs/DriveWheelPrefab");
            _carBodyPrefab = Resources.Load<GameObject>("Prefabs/CarBodyPrefab");

            VirtualFilesystem.Instance.Init();
            _materialCache["default"] = Object.Instantiate(_textureMaterialPrefab);
            Palette = ActPaletteParser.ReadActPalette("p02.act");
        }

        public AudioClip GetAudioClip(string soundName)
        {
            string filename = Path.GetFileNameWithoutExtension(soundName);
            if (VirtualFilesystem.Instance.FileExists(filename + ".wav"))
            {
                return VirtualFilesystem.Instance.GetAudioClip(filename + ".wav");
            }

            if (VirtualFilesystem.Instance.FileExists(filename + ".gpw"))
            {
                return GpwParser.ParseGpw(filename + ".gpw").Clip;
            }
            
            Debug.LogWarning("Sound file not found: " + soundName);
            return null;
        }

        public AudioSource GetAudioSource(GameObject rootObject, string soundName)
        {
            const float maxVolume = 0.8f; // Set a default volume since high values cause bad distortion.

            AudioClip audioClip = GetAudioClip(soundName);
            if (audioClip == null)
            {
                return null;
            }

            AudioSource audioSource = rootObject.AddComponent<AudioSource>();
            audioSource.volume = maxVolume;
            audioSource.playOnAwake = false;
            audioSource.clip = audioClip;

            if (audioClip.name.EndsWith(".wav"))
            {
                audioSource.spatialize = false;
                audioSource.spatialBlend = 0.0f;
            }
            else if (audioClip.name.EndsWith(".gpw"))
            {
                audioSource.spatialize = true;
                audioSource.spatialBlend = 1.0f;
                audioSource.minDistance = 5f;
                audioSource.maxDistance = 75f;
            }

            return audioSource;
        }

        public Texture2D GetTexture(string textureName)
        {
            string filename = Path.GetFileNameWithoutExtension(textureName);
            Texture2D texture = null;
            if (VirtualFilesystem.Instance.FileExists(filename + ".vqm"))
            {
                texture = TextureParser.ReadVqmTexture(filename + ".vqm", Palette);
            }
            else if (VirtualFilesystem.Instance.FileExists(filename + ".map"))
            {
                texture = TextureParser.ReadMapTexture(filename + ".map", Palette);
            }
            else
            {
                Debug.LogWarning("Texture not found: " + textureName);
            }
            return texture;
        }

        public Material GetTextureMaterial(string textureName, bool transparent, bool unlit)
        {
            if (!_materialCache.ContainsKey(textureName))
            {
                Texture2D texture = GetTexture(textureName);
                Material material;
                if (unlit)
                {
                    material = Object.Instantiate(transparent ? _unlitTransparentMaterialPrefab : _unlitTextureMaterialPrefab);
                }
                else
                {
                    material = Object.Instantiate(transparent ? _transparentMaterialPrefab : _textureMaterialPrefab);
                }
                material.mainTexture = texture ?? Texture2D.blackTexture;
                material.name = textureName;
                _materialCache[textureName] = material;
            }

            return _materialCache[textureName];
        }

        public Material GetColorMaterial(string materialName, Color32 color)
        {
            if (_materialCache.ContainsKey(materialName))
                return _materialCache[materialName];

            Material material = Object.Instantiate(_colorMaterialPrefab);
            material.color = color;
            _materialCache[materialName] = material;

            return material;
        }

        private void AddMaterial(GeoFace geoFace, string textureName, Vtf vtf, int textureGroup, bool unlit, Dictionary<string, List<GeoFace>> facesGroupedByMaterial, Dictionary<string, HashSet<Material>> materialGroups)
        {
            Material material;

            string materialGroupName = geoFace.TextureName;
            if (textureName != null)
            {
                textureName = Path.GetFileNameWithoutExtension(textureName);

                if (vtf != null && textureName[0] == 'V')
                {
                    if (textureName.EndsWithFast("BO DY"))
                    {
                        textureName = vtf.Maps[12];
                    }
                    else
                    {
                        string key = textureName.Substring(1).Replace(" ", "").Replace("LF", "LT") + ".TMT";

                        if (vtf.Tmts.TryGetValue(key, out Tmt tmt))
                        {
                            textureName = tmt.TextureNames[textureGroup];
                            materialGroupName = textureName;
                        }
                    }
                }
                material = GetTextureMaterial(textureName, geoFace.SurfaceFlags2 == 5 || geoFace.SurfaceFlags2 == 7, unlit);
            }
            else
            {
                textureName = "color" + geoFace.Color;
                material = GetColorMaterial(textureName, geoFace.Color);
                materialGroupName = textureName;
            }

            if (materialGroups.TryGetValue(materialGroupName, out HashSet<Material> materialGroup))
            {
                if (!materialGroup.Contains(material))
                {
                    materialGroup.Add(material);
                }
            }
            else
            {
                materialGroups.Add(materialGroupName, new HashSet<Material>
                {
                    material
                });
            }

            if (facesGroupedByMaterial.TryGetValue(materialGroupName, out List<GeoFace> faceGroup))
            {
                faceGroup.Add(geoFace);
            }
            else
            {
                facesGroupedByMaterial.Add(materialGroupName, new List<GeoFace>
                {
                    geoFace
                });
            }
        }

        public class GeoMeshCacheEntry
        {
            public GeoMesh GeoMesh { get; set; }
            public Mesh Mesh { get; set; }
            public Material[][] Materials { get; set; }
        }

        private void LoadMaterial()
        {

        }

        public GeoMeshCacheEntry ImportMesh(string filename, Vtf vtf, int textureGroup, int frameCount, bool unlit)
        {
            if (MeshCache.TryGetValue(filename, out GeoMeshCacheEntry cacheEntry))
            {
                return cacheEntry;
            }

            GeoMesh geoMesh = GeoParser.ReadGeoMesh(filename);

            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            
            Dictionary<string, List<GeoFace>> facesGroupedByMaterial = new Dictionary<string, List<GeoFace>>();
            Dictionary<string, HashSet<Material>> materialGroups = new Dictionary<string, HashSet<Material>>();

            GeoFace[] faces = geoMesh.Faces;
            for (int i = 0; i < faces.Length; ++i)
            {
                GeoFace face = faces[i];
                string textureName = face.TextureName;

                bool materialAdded = false;
                if (!string.IsNullOrEmpty(textureName))
                {
                    string pixFileName = Path.GetFileNameWithoutExtension(textureName) + ".pix";
                    if (VirtualFilesystem.Instance.TryGetFileInfo(pixFileName, out VirtualFilesystem.ZFSFileInfo fileInfo))
                    {
                        List<string> bundle = fileInfo.PixBundle;
                        int bundleCount = bundle.Count;
                        for (int j = 0; j < bundleCount; ++j)
                        {
                            AddMaterial(face, bundle[j], vtf, textureGroup, unlit, facesGroupedByMaterial, materialGroups);
                        }

                        materialAdded = true;
                        frameCount = bundleCount;
                    }
                }
                
                if (!materialAdded)
                {
                    for (int j = 0; j < frameCount; ++j)
                    {
                        AddMaterial(face, textureName, vtf, textureGroup, unlit, facesGroupedByMaterial, materialGroups);
                    }
                }
            }

            mesh.subMeshCount = facesGroupedByMaterial.Count;
            Dictionary<Material, List<int>> submeshTriangles = new Dictionary<Material, List<int>>();
            foreach (KeyValuePair<string, List<GeoFace>> faceGroup in facesGroupedByMaterial)
            {
                Material firstMaterial = materialGroups[faceGroup.Key].First();
                submeshTriangles[firstMaterial] = new List<int>();
                List<GeoFace> faceGroupValues = faceGroup.Value;
                foreach (GeoFace face in faceGroupValues)
                {
                    int numTriangles = face.VertexRefs.Length - 3 + 1;
                    int viStart = vertices.Count;
                    foreach (GeoVertexRef vertexRef in face.VertexRefs)
                    {
                        vertices.Add(geoMesh.Vertices[vertexRef.VertexIndex]);
                        normals.Add(geoMesh.Normals[vertexRef.VertexIndex] * -1);
                        uvs.Add(vertexRef.Uv);
                    }
                    for (int t = 1; t <= numTriangles; t++)
                    {
                        submeshTriangles[firstMaterial].Add(viStart + t);
                        submeshTriangles[firstMaterial].Add(viStart);
                        submeshTriangles[firstMaterial].Add(viStart + t + 1);
                    }
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            int subMeshIndex = 0;
            foreach (KeyValuePair<Material, List<int>> submeshTriangle in submeshTriangles)
            {
                mesh.SetTriangles(submeshTriangles[submeshTriangle.Key], subMeshIndex++);
            }
            mesh.RecalculateBounds();

            int maxFrames = 0;
            int groupCount = materialGroups.Count;
            Material[][] materialSubGroups = new Material[groupCount][];
            int groupIndex = 0;
            foreach (HashSet<Material> materialGroup in materialGroups.Values)
            {
                materialSubGroups[groupIndex++] = materialGroup.ToArray();
                if (materialGroup.Count > maxFrames)
                {
                    maxFrames = materialGroup.Count;
                }
            }
            
            Material[][] matGroups = new Material[maxFrames][];
            for (int i = 0; i < maxFrames; ++i)
            {
                matGroups[i] = new Material[groupCount];
            }
            
            for (int i = 0; i < maxFrames; ++i)
            {
                for (int j = 0; j < groupCount; ++j)
                {
                    if (materialSubGroups[j].Length <= i)
                    {
                        continue;
                    }

                    matGroups[i][j] = materialSubGroups[j][i];
                }
            }

            cacheEntry = new GeoMeshCacheEntry
            {
                GeoMesh = geoMesh,
                Mesh = mesh,
                Materials = matGroups
            };
            MeshCache.Add(filename, cacheEntry);

            return cacheEntry;
        }

        public GameObject ImportGeo(string filename, Vtf vtf, GameObject prefab, int textureGroup, int frameCount, bool unlit, bool autoAnimate)
        {
            GeoMeshCacheEntry meshCacheEntry = ImportMesh(filename, vtf, textureGroup, frameCount, unlit);

            GameObject obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            obj.gameObject.name = meshCacheEntry.GeoMesh.Name;
            
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = meshCacheEntry.Mesh;
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                meshRenderer.materials = meshCacheEntry.Materials[0];

                if (autoAnimate && meshCacheEntry.Materials.Length > 1)
                {
                    AnimatedTexture.AnimateObject(obj, meshCacheEntry.Materials, meshRenderer);
                }
            }
            MeshCollider collider = obj.GetComponent<MeshCollider>();
            if (collider != null)
                collider.sharedMesh = meshCacheEntry.Mesh;

            return obj.gameObject;
        }

        private bool TryGetMaskTexture(string filename, out Texture2D maskTexture)
        {
            if (MeshCache.TryGetValue(filename, out GeoMeshCacheEntry cacheEntry))
            {
                foreach (GeoFace face in cacheEntry.GeoMesh.Faces)
                {
                    string textureName = face.TextureName + ".MAP";
                    if (TextureParser.MaskTextureCache.TryGetValue(textureName, out maskTexture))
                    {
                        return true;
                    }
                }
            }

            maskTexture = null;
            return false;
        }

        private GameObject LoadGeometryDefinition(GeometryDefinition geometryDefinition, Transform parent, bool collider, int frameCount, bool unlit, bool autoAnimate)
        {
            string geoFilename = geometryDefinition.Name + ".geo";
            if (!VirtualFilesystem.Instance.FileExists(geoFilename))
            {
                Debug.LogWarning("File does not exist: " + geoFilename);
                return null;
            }

            GameObject partObj = ImportGeo(geoFilename, null, collider ? _3DObjectPrefab : _noColliderPrefab, 0, frameCount, unlit, autoAnimate);
            partObj.transform.parent = parent;
            partObj.transform.localPosition = geometryDefinition.Position;
            partObj.transform.localRotation = Quaternion.identity;
            return partObj;
        }

        public Effect ImportXdf(string fileName, Transform parent)
        {
            if (!_xdfCache.TryGetValue(fileName, out Xdf xdf))
            {
                xdf = XdfParser.ParseXdf(fileName);
                _xdfCache.Add(fileName, xdf);
            }

            GameObject xdfObject = new GameObject(fileName);
            xdfObject.transform.parent = parent;
            xdfObject.transform.localPosition = Vector3.zero;
            xdfObject.transform.localRotation = Quaternion.identity;

            Effect effect = new Effect(xdfObject);
            effect.Initialise(xdf);
            
            Dictionary<string, GameObject> partDict = new Dictionary<string, GameObject> { { "WORLD", xdfObject } };

            GeometryDefinition[] parts = xdf.Parts;
            for (int i = 0; i < parts.Length; ++i)
            {
                GeometryDefinition part = parts[i];
                GameObject partObj = LoadGeometryDefinition(part, partDict[part.ParentName].transform, false, xdf.Frames, true, true);
                if (partObj == null)
                {
                    continue;
                }

                partObj.SetActive(true);
                partDict.Add(part.Name, partObj);
            }

            return effect;
        }

        public GameObject ImportSdf(string filename, Transform parent, Vector3 localPosition, Quaternion rotation, bool canWreck, out Sdf sdf, out GameObject wreckedPart)
        {
            wreckedPart = null;
            sdf = null;

            if (!_sdfCache.TryGetValue(filename, out sdf))
            {
                sdf = SdfObjectParser.LoadSdf(filename, canWreck);
                _sdfCache.Add(filename, sdf);
            }

            GameObject sdfObject = new GameObject(sdf.Name);
            sdfObject.transform.parent = parent;
            sdfObject.transform.localPosition = localPosition;
            sdfObject.transform.rotation = rotation;

            Dictionary<string, GameObject> partDict = new Dictionary<string, GameObject> { { "WORLD", sdfObject } };

            foreach (GeometryDefinition sdfPart in sdf.Parts)
            {
                GameObject partObj = LoadGeometryDefinition(sdfPart, partDict[sdfPart.ParentName].transform, true, 1, false, true);
                if (partObj == null)
                {
                    continue;
                }
                
                partObj.SetActive(true);
                partDict.Add(sdfPart.Name, partObj);
            }

            if (canWreck && sdf.WreckedPart != null)
            {
                wreckedPart = LoadGeometryDefinition(sdf.WreckedPart, partDict[sdf.WreckedPart.ParentName].transform, true, 1, false, true);
                if (wreckedPart != null)
                {
                    wreckedPart.SetActive(false);
                    Rigidbody rigidBody = sdfObject.AddComponent<Rigidbody>();
                    rigidBody.isKinematic = true;
                }
            }

            return sdfObject;
        }

        public GameObject ImportVcf(string filename, bool importFirstPerson, out Car car, out Vdf vdf)
        {
            Vcf vcf = VcfParser.ParseVcf(filename);
            vdf = VdfParser.ParseVdf(vcf.VdfFilename);
            Vtf vtf = VtfParser.ParseVtf(vcf.VtfFilename);

            GameObject carObject = new GameObject(vdf.Name + " (" + vcf.VariantName + ")");
            car = EntityManager.Instance.CreateEntity<Car>(carObject);

            foreach (VLoc vLoc in vdf.VLocs)
            {
                GameObject vlocGo = new GameObject("VLOC");
                vlocGo.transform.parent = car.Transform;
                vlocGo.transform.localRotation = Quaternion.LookRotation(vLoc.Forward, vLoc.Up);
                vlocGo.transform.localPosition = vLoc.Position;
            }

            GameObject chassis = new GameObject("Chassis");
            chassis.transform.parent = car.Transform;

            GameObject thirdPerson = new GameObject("ThirdPerson");
            thirdPerson.transform.parent = chassis.transform;

            Transform[] weaponMountTransforms = new Transform[vdf.HLocs.Count];
            for (int i = 0; i < vdf.HLocs.Count; ++i)
            {
                HLoc hloc = vdf.HLocs[i];
                Transform mountPoint = new GameObject(hloc.Label).transform;
                mountPoint.parent = thirdPerson.transform;
                mountPoint.localRotation = Quaternion.LookRotation(hloc.Forward, hloc.Up);
                mountPoint.localPosition = hloc.Position;
                weaponMountTransforms[i] = mountPoint;
            }

            Dictionary<string, GameObject> partDict = new Dictionary<string, GameObject>();

            for (int i = 0; i < vdf.PartsThirdPerson.Count; ++i)
            {
                GameObject healthObject = new GameObject("Health " + i);
                healthObject.transform.SetParent(thirdPerson.transform);
                ImportCarParts(partDict, healthObject, vtf, vdf.PartsThirdPerson[i], _noColliderPrefab, false, false, i);
                if (i != 0)
                {
                    healthObject.SetActive(false);
                }
            }

            MeshFilter[] meshFilters = thirdPerson.GetComponentsInChildren<MeshFilter>();
            Bounds bounds = new Bounds();
            bounds.SetMinMax(Vector3.one * float.MaxValue, Vector3.one * float.MinValue);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                Vector3 min = Vector3.Min(bounds.min, meshFilter.transform.position + meshFilter.sharedMesh.bounds.min) - thirdPerson.transform.position;
                Vector3 max = Vector3.Max(bounds.max, meshFilter.transform.position + meshFilter.sharedMesh.bounds.max) - thirdPerson.transform.position;
                bounds.SetMinMax(min, max);
            }

            GameObject chassisCollider = new GameObject("ChassisColliders");
            chassisCollider.transform.parent = car.Transform;
            ImportCarParts(partDict, chassisCollider, vtf, vdf.PartsThirdPerson[0], _carBodyPrefab, true);
            
            for (int i = 0; i < vcf.Weapons.Count; ++i)
            {
                VcfWeapon weapon = vcf.Weapons[i];
                int mountPoint = weapon.MountPoint;
                HLoc hloc = vdf.HLocs[mountPoint];
                weapon.RearFacing = hloc.FacingDirection == 2;

                GeometryDefinition[] partsArray;
                switch (hloc.MeshType)
                {
                    case HardpointMeshType.Top:
                        partsArray = weapon.Gdf.TopParts;
                        break;
                    case HardpointMeshType.Side:
                        partsArray = weapon.Gdf.SideParts;
                        break;
                    case HardpointMeshType.Inside:
                        partsArray = weapon.Gdf.InsideParts;
                        break;
                    case HardpointMeshType.Turret:
                        partsArray = weapon.Gdf.TurretParts;
                        break;
                    default:
                        partsArray = null;
                        break;
                }

                if (partsArray != null)
                {
                    Transform weaponTransform = new GameObject(weapon.Gdf.Name).transform;
                    weaponTransform.SetParent(weaponMountTransforms[i]);
                    weaponTransform.localPosition = Vector3.zero;
                    weaponTransform.localRotation = Quaternion.identity;
                    ImportCarParts(partDict, weaponTransform.gameObject, vtf, partsArray, _noColliderPrefab, false);
                    weapon.Transform = weaponTransform;

                    // Disable depth test for 'inside' weapons, otherwise they are obscured.
                    if (hloc.MeshType == HardpointMeshType.Inside)
                    {
                        MeshRenderer weaponRenderer = weaponTransform.GetComponentInChildren<MeshRenderer>();
                        if (weaponRenderer != null)
                        {
                            weaponRenderer.sharedMaterial.shader = Shader.Find("Custom/CutOutWithoutZ");
                        }
                    }
                }
                else
                {
                    weapon.Transform = chassis.transform;
                }
            }
            
            // Note: The following is probably how I76 does collision detection. Two large boxes that encapsulate the entire vehicle.
            // Right now this won't work with Open76's raycast suspension, so I'm leaving this off for now. Investigate in the future.
            //var innerBox = chassisCollider.AddComponent<BoxCollider>();
            //innerBox.center = vdf.BoundsInner.center;
            //innerBox.size = vdf.BoundsInner.size;

            //var outerBox = chassisCollider.AddComponent<BoxCollider>();
            //outerBox.center = vdf.BoundsOuter.center;
            //outerBox.size = vdf.BoundsOuter.size;

            RaySusp[] frontWheels = null;
            if (vcf.FrontWheelDef != null)
            {
                frontWheels = CreateWheelPair(partDict, "Front", 0, carObject, vdf, vtf, vcf.FrontWheelDef);
                car.Movement.FrontWheels = frontWheels;
            }
            if (vcf.MidWheelDef != null)
            {
                CreateWheelPair(partDict, "Mid", 2, carObject, vdf, vtf, vcf.MidWheelDef);
            }

            RaySusp[] rearWheels = null;
            if (vcf.BackWheelDef != null)
            {
                rearWheels = CreateWheelPair(partDict, "Back", 4, carObject, vdf, vtf, vcf.BackWheelDef);
                car.Movement.RearWheels = rearWheels;
            }
            
            if (importFirstPerson)
            {
                GameObject firstPerson = new GameObject("FirstPerson");
                firstPerson.transform.parent = chassis.transform;
                ImportCarParts(partDict, firstPerson, vtf, vdf.PartsFirstPerson, _noColliderPrefab, false, true, 0, LayerMask.NameToLayer("FirstPerson"));

                car.InitPanels();
                firstPerson.SetActive(false);
            }

            car.Movement.Initialise(chassis.transform, frontWheels, rearWheels);
            car.Configure(vdf, vcf);

            return carObject;
        }

        private RaySusp[] CreateWheelPair(Dictionary<string, GameObject> partDict, string placement, int wheelIndex, GameObject car, Vdf vdf, Vtf vtf, Wdf wheelDef)
        {
            string wheel1Name = placement + "Right";
            RaySusp wheel = Object.Instantiate(placement == "Front" ? _steerWheelPrefab : _driveWheelPrefab, car.transform);
            wheel.gameObject.name = wheel1Name;
            wheel.WheelRadius = wheelDef.Radius;  // This is not correct - find out where the radius is really stored
            wheel.SpringLength = wheelDef.Radius;

            ImportCarParts(partDict, wheel.transform.Find("Mesh").gameObject, vtf, wheelDef.Parts, _noColliderPrefab, false);
            wheel.transform.localPosition = vdf.WheelLoc[wheelIndex].Position;

            RaySusp wheel2 = Object.Instantiate(wheel, car.transform);
            wheel2.name = placement + "Left";
            wheel2.transform.localPosition = vdf.WheelLoc[wheelIndex + 1].Position;
            wheel2.transform.Find("Mesh").localScale = new Vector3(-1, 1, 1);

            return new[] { wheel, wheel2 };
        }

        private void LoadCarPart(GeometryDefinition geometryDefinition, GameObject parent, Dictionary<string, GameObject> partDict, List<GeometryDefinition> deferredParts, Vtf vtf, GameObject prefab, bool justChassis, bool forgetParentPosition, int textureGroup, int layerMask)
        {
            if (geometryDefinition == null || geometryDefinition.Name == "NULL")
                return;

            if (_bannedNames.Any(b => geometryDefinition.Name.EndsWithFast(b)))
                return;

            GameObject parentObj;
            if (geometryDefinition.ParentName == "WORLD")
            {
                parentObj = parent;
            }
            else if (!partDict.TryGetValue(geometryDefinition.ParentName, out parentObj))
            {
                if (deferredParts != null)
                {
                    deferredParts.Add(geometryDefinition);
                    return;
                }

                Debug.Log("Cant find parent '" + geometryDefinition.ParentName + "' for '" + geometryDefinition.Name + "'");
                parentObj = parent;
            }

            if (justChassis && !(geometryDefinition.Name.Contains("BDY") || geometryDefinition.Name.EndsWithFast("CHAS")))
                return;

            string geoFilename = geometryDefinition.Name + ".geo";
            if (!VirtualFilesystem.Instance.FileExists(geoFilename))
            {
                Debug.LogWarning("File does not exist: " + geoFilename);
                return;
            }

            GameObject partObj = ImportGeo(geoFilename, vtf, prefab, textureGroup, 1, false, false);
            
            Transform partTransform = partObj.transform;
            if (!forgetParentPosition)
                partTransform.SetParent(parentObj.transform);
            partTransform.right = geometryDefinition.Right;
            partTransform.up = geometryDefinition.Up;
            partTransform.forward = geometryDefinition.Forward;
            partTransform.localPosition = geometryDefinition.Position;
            partTransform.localRotation = Quaternion.identity;
            if (forgetParentPosition)
                partTransform.parent = parentObj.transform;
            partObj.SetActive(true);

            if (partDict.ContainsKey(geometryDefinition.Name))
            {
                partDict[geometryDefinition.Name] = partObj;
            }
            else
            {
                partDict.Add(geometryDefinition.Name, partObj);
            }
            

            if (layerMask != 0)
            {
                partObj.layer = layerMask;
            }

            // Special case for mirrors.
            if (geometryDefinition.Name.Contains("MIRI") && TryGetMaskTexture(geoFilename, out Texture2D maskTexture))
            {
                RenderTexture renderTexture = new RenderTexture(256, 128, 24);

                GameObject mirrorCameraObj = Object.Instantiate(partObj);
                Transform mirrorObjTransform = mirrorCameraObj.transform;
                mirrorObjTransform.SetParent(partObj.transform);
                mirrorObjTransform.localPosition = Vector3.zero;
                mirrorObjTransform.localRotation = Quaternion.identity;

                Material mirrorMaterial = Object.Instantiate(_carMirrorMaterialPrefab);
                MeshRenderer meshRenderer = partObj.transform.GetComponent<MeshRenderer>();
                mirrorMaterial.mainTexture = meshRenderer.material.mainTexture;
                mirrorMaterial.SetTexture("_MaskTex", maskTexture);
                meshRenderer.material = mirrorMaterial;

                GameObject cameraPivotObj = new GameObject("Camera Pivot");
                UnityEngine.Camera mirrorCamera = cameraPivotObj.AddComponent<UnityEngine.Camera>();
                mirrorCamera.cullingMask = ~LayerMask.GetMask("FirstPerson");
                mirrorCamera.targetTexture = renderTexture;
                Transform pivotTransform = cameraPivotObj.transform;
                pivotTransform.SetParent(mirrorObjTransform);
                pivotTransform.localPosition = Vector3.zero;
                pivotTransform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                Material cameraMaterial = Object.Instantiate(_carMirrorMaterialPrefab);
                cameraMaterial.mainTexture = renderTexture;
                MeshRenderer mirrorRenderer = mirrorCameraObj.GetComponent<MeshRenderer>();
                cameraMaterial.SetTexture("_MaskTex", maskTexture);
                mirrorRenderer.material = cameraMaterial;
            }
        }

        private void ImportCarParts(Dictionary<string, GameObject> partDict, GameObject parent, Vtf vtf, GeometryDefinition[] geometryDefinitions, GameObject prefab, bool justChassis, bool forgetParentPosition = false, int textureGroup = 0, int layerMask = 0)
        {
            List<GeometryDefinition> deferredParts = new List<GeometryDefinition>();

            foreach (GeometryDefinition sdfPart in geometryDefinitions)
            {
                LoadCarPart(sdfPart, parent, partDict, deferredParts, vtf, prefab, justChassis, forgetParentPosition, textureGroup, layerMask);
            }

            int deferredPartCount = deferredParts.Count;
            for (int i = 0; i < deferredPartCount; ++i)
            {
                LoadCarPart(deferredParts[i], parent, partDict, null, vtf, prefab, justChassis, forgetParentPosition, textureGroup, layerMask);
            }
        }

        public void ClearCache()
        {
            _materialCache.Clear();
            _sdfCache.Clear();
            MeshCache.Clear();
        }
    }
}
