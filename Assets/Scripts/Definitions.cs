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
}
