namespace Assets.Scripts.System.Fileparsers
{
    public class Xdf
    {
        public int Frames { get; set; }
        public float FrameRate { get; set; }
        public float LifeTime { get; set; }
        public float Unknown4 { get; set; }
        public float Unknown5 { get; set; }
        public float Unknown6 { get; set; }
        public int Unknown7 { get; set; }
        public GeometryDefinition[] Parts { get; set; }
    }

    internal class XdfParser
    {
        public static Xdf ParseXdf(string filename)
        {
            using (Bwd2Reader br = new Bwd2Reader(filename))
            {
                Xdf xdf = new Xdf();

                br.FindNext("XDFC");
                xdf.Frames = br.ReadInt32();
                xdf.FrameRate = br.ReadSingle();
                xdf.LifeTime = br.ReadSingle();
                br.ReadBytes(12);
                xdf.Unknown4 = br.ReadSingle();
                xdf.Unknown5 = br.ReadSingle();
                xdf.Unknown6 = br.ReadSingle();
                xdf.Unknown7 = br.ReadInt32();

                br.FindNext("XGEO");
                int partCount = br.ReadInt32();
                xdf.Parts = new GeometryDefinition[partCount];
                for (int i = 0; i < partCount; ++i)
                {
                    xdf.Parts[i] = GeometryDefinition.Read(br);
                    br.Position += 36;
                }

                return xdf;
            }
        }
    }
}
