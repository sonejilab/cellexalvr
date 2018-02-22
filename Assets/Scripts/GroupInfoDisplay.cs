using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents the group info display on the right controller. It shows how many cells that currently belong to a group (have a certain color).
/// </summary>
public class GroupInfoDisplay : MonoBehaviour
{
    public SelectionToolHandler selectionToolHandler;
    public TextMesh status;
    public List<MeshRenderer> coloredSquares;
    private Dictionary<int, int> groups = new Dictionary<int, int>();
    private Color[] colors;

    /// <summary>
    /// Sets the colors that should be used. Must be called once before any calls to ChangeGroupsInfo.
    /// </summary>
    /// <param name="colors"> An array of Color that should be used. </param>
    public void SetColors(Color[] colors)
    {
        this.colors = colors;
        for (int i = 0; i < colors.Length; i++)
        {
            Color col = colors[i];
            groups[i] = 0;
            coloredSquares[i].material.color = col;
            //print("Color added " + col.r + " " + col.g + " " + col.b + " ints: " + ColorComparer.Bits(col.r) + " " + ColorComparer.Bits(col.g) + " " + ColorComparer.Bits(col.b));
        }
    }

    /// <summary>
    /// Changes the text on the display by adding (or subtracting) the specifed number of cells from the specified color.
    /// </summary>
    /// <param name="group"> The group's number that should be changed. </param>
    /// <param name="n"> How much the number should change by. 1 if adding 1 cell to the color. -1 if subtracting 1 cell from the color. </param>
    public void ChangeGroupsInfo(int group, int n)
    {
        groups[group] += n;
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < colors.Length; i++)
        {
            builder.Append(groups[i]).Append('\n');
        }
        status.text = builder.ToString();
    }

    /// <summary>
    /// Reset the display to only zeroes.
    /// </summary>
    public void ResetGroupsInfo()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < colors.Length; i++)
        {
            Color col = colors[i];
            groups[i] = 0;
            builder.Append("0\n");
        }
        status.text = builder.ToString();
    }

    /// <summary>
    /// Helper class to compare colors.
    /// </summary>
   /* private class ColorComparer : IEqualityComparer<Color>
    {
        public bool Equals(Color c1, Color c2)
        {
            // Two colors are equal if they have the same rgb values.
            //int c1r = FloatToBitRepresentation(c1.r) & 0x7ffffff8;
            //int c1g = FloatToBitRepresentation(c1.g) & 0x7ffffff8;
            //int c1b = FloatToBitRepresentation(c1.b) & 0x7ffffff8;
            //int c2r = FloatToBitRepresentation(c2.r) & 0x7ffffff8;
            //int c2g = FloatToBitRepresentation(c2.g) & 0x7ffffff8;
            //int c2b = FloatToBitRepresentation(c2.b) & 0x7ffffff8;
            return c1.r.Equals(c2.r) && c1.g.Equals(c2.g) && c1.b.Equals(c2.b);
        }

        public int GetHashCode(Color obj)
        {
            //int r = FloatToBitRepresentation(obj.r) & 0x7ffffff8;
            //int g = FloatToBitRepresentation(obj.g) & 0x7ffffff8;
            //int b = FloatToBitRepresentation(obj.b) & 0x7ffffff8;
            return (r.GetHashCode() ^ (g.GetHashCode() << 2) ^ (b.GetHashCode() >> 2));
        }

        public static int FloatToBitRepresentation(float f)
        {
            byte[] bytes = BitConverter.GetBytes(f);
            int result = 0;
            result = bytes[0];
            result |= (bytes[1] << 8);
            result |= (bytes[2] << 16);
            result |= (bytes[3] << 24);
            return result;
        }

        public static string Bits(float f)
        {
            uint bits = BitConverter.ToUInt32(BitConverter.GetBytes(f), 0);
            uint mask = 0x80000000;
            StringBuilder builder = new StringBuilder();
            for (int i = 31; i >= 0; --i)
            {
                uint bit = (bits & mask) >> i;
                builder.Append(bit);
                mask = mask >> 1;
            }
            return builder.ToString();
        }
    }*/
}
