using UnityEngine;

namespace Assets.Scripts.System.Fileparsers
{
    public class GeometryDefinition
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Forward { get; set; }
        public string ParentName { get; set; }
        
        public static GeometryDefinition Read(Bwd2Reader br)
        {
            GeometryDefinition geometryDefinition = new GeometryDefinition();
            geometryDefinition.Name = br.ReadCString(8);
            geometryDefinition.Right = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            geometryDefinition.Up = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            geometryDefinition.Forward = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            geometryDefinition.Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            geometryDefinition.ParentName = br.ReadCString(8);
            return geometryDefinition;
        }
    }
}
