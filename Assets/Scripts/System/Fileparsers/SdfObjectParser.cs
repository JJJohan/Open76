using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.System.Fileparsers
{
    public class Sdf
    {
        public string Name { get; set; }
        public uint Health { get; set; }
        public string DestroySoundName { get; set; }
        public Xdf Xdf { get; set; }
        public GeometryDefinition[] Parts { get; set; }
        public GeometryDefinition WreckedPart { get; set; }
    }

    public class SdfObjectParser
    {
        private static readonly Dictionary<string, Sdf> SdfCache = new Dictionary<string, Sdf>();

        public static Sdf LoadSdf(string filename, bool canWreck)
        {
            filename = filename.ToLower();
            if (SdfCache.ContainsKey(filename))
                return SdfCache[filename];

            using (Bwd2Reader br = new Bwd2Reader(filename))
            {
                Sdf sdf = new Sdf();

                br.FindNext("SDFC");
                sdf.Name = br.ReadCString(16);
                uint one = br.ReadUInt32();
                Vector3 size = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                uint unk1 = br.ReadUInt32();
                uint unk2 = br.ReadUInt32();
                sdf.Health = br.ReadUInt32();
                string xdfName = br.ReadCString(13) + ".xdf";
                string soundName = br.ReadCString(13);
                if (!string.IsNullOrEmpty(soundName) && soundName.ToLower() != "null")
                {
                    sdf.DestroySoundName = soundName;
                }

                if (VirtualFilesystem.Instance.FileExists(xdfName))
                {
                    sdf.Xdf = XdfParser.ParseXdf(xdfName);
                }

                br.FindNext("SGEO");
                uint numParts = br.ReadUInt32();
                sdf.Parts = new GeometryDefinition[numParts];
                for (int i = 0; i < numParts; i++)
                {
                    sdf.Parts[i] = GeometryDefinition.Read(br);
                    br.Position += 56;
                }

                if (canWreck)
                {
                    GeometryDefinition wreckedPart = GetDestroyedPart(br);
                    if (wreckedPart == null)
                    {
                        wreckedPart = GetDestroyedPart(br);
                    }

                    sdf.WreckedPart = wreckedPart;
                }

                SdfCache.Add(filename, sdf);
                return sdf;
            }
        }
        
        private static GeometryDefinition GetDestroyedPart(Bwd2Reader br)
        {
            string partName = br.ReadCString(8);
            if (string.IsNullOrEmpty(partName) || partName.ToLower() == "null")
            {
                br.Position += 112;
                return null;
            }

            
            GeometryDefinition wreckedPart = new GeometryDefinition();
            wreckedPart.Name = partName;
            wreckedPart.Right = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            wreckedPart.Up = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            wreckedPart.Forward = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            wreckedPart.Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            wreckedPart.ParentName = br.ReadCString(8);
            br.Position += 56;

            return wreckedPart;
        }
    }
}
