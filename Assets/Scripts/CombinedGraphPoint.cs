public class CombinedGraphPoint
{
    public string Label { get; set; }
    public UnityEngine.Vector3 Position { get; set; }

    public CombinedGraphPoint(string label, float x, float y, float z)
    {
        Label = label;
        Position = new UnityEngine.Vector3(x, y, z);
    }

    //public override int GetHashCode()
    //{
    //    return Position.GetHashCode();
    //}
}
