namespace CellexalExtensions
{

    public static class Definitions
    {
        public enum Measurement { INVALID, GENE, ATTRIBUTE, FACS }
        public static string ToString(this Measurement m)
        {
            switch (m)
            {
                case Measurement.INVALID:
                    return "invalid";
                case Measurement.GENE:
                    return "gene";
                case Measurement.ATTRIBUTE:
                    return "attribute";
                case Measurement.FACS:
                    return "facs";
                default:
                    return "";
            }
        }
    }

    public static class Extensions
    {
        public static UnityEngine.Color[] InterpolateColors(UnityEngine.Color color1, UnityEngine.Color color2, int numColors)
        {
            var colors = new UnityEngine.Color[numColors];
            if (numColors == 0)
            {
                CellexalLog.Log("ERROR: Can't create 0 colors");
                return null;
            }

            float lowMidDeltaR = (color2.r * color2.r - color1.r * color1.r) / numColors;
            float lowMidDeltaG = (color2.g * color2.g - color1.g * color1.g) / numColors;
            float lowMidDeltaB = (color2.b * color2.b - color1.b * color1.b) / numColors;


            for (int i = 0; i < numColors; ++i)
            {
                float r = color1.r * color1.r + lowMidDeltaR * i;
                float g = color1.g * color1.g + lowMidDeltaG * i;
                float b = color1.b * color1.b + lowMidDeltaB * i;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                colors[i] = new UnityEngine.Color(UnityEngine.Mathf.Sqrt(r), UnityEngine.Mathf.Sqrt(g), UnityEngine.Mathf.Sqrt(b));
            }

            return colors;
        }
    }
}
