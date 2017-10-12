using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// This class represents the group info display ont he roght controller. It shows how many cells that currently belong to a group (have a certain color).
/// </summary>
public class GroupInfoDisplay : MonoBehaviour
{
    public SelectionToolHandler selectionToolHandler;
    public TextMesh status;
    public List<MeshRenderer> coloredSquares;
    private Dictionary<Color, int> groups = new Dictionary<Color, int>(new ColorComparer());
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
            groups[colors[i]] = 0;
            coloredSquares[i].material.color = colors[i];
        }
    }

    /// <summary>
    /// Changes the text on the display by adding (or subtracting) the specifed number of cells from the specified color.
    /// </summary>
    /// <param name="col"> The color which's number should be changed. </param>
    /// <param name="n"> How much the number should change by. 1 if adding 1 cell to the color. -1 if subtracting 1 cell from the color. </param>
    public void ChangeGroupsInfo(Color col, int n)
    {
        try
        {
            groups[col] += n;
        }
        catch (KeyNotFoundException e)
        {
            print("Color not found " + col.r + " " + col.g + " " + col.b + " ints: " + ColorComparer.FloatToBitRepresentation(col.r) + " " + ColorComparer.FloatToBitRepresentation(col.g) + " " + ColorComparer.FloatToBitRepresentation(col.b));
            print(e);
        }
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < colors.Length; i++)
        {
            builder.Append(groups[colors[i]]).Append('\n');
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
            groups[col] = 0;
            builder.Append("0\n");
        }
        status.text = builder.ToString();
    }

    /// <summary>
    /// Helper class to compare colors.
    /// </summary>
    private class ColorComparer : IEqualityComparer<Color>
    {
        public bool Equals(Color c1, Color c2)
        {
            // Two colors are equal if they have the same rgb values.
            return c1.r.Equals(c2.r) && c1.g.Equals(c2.g) && c1.b.Equals(c2.b);
        }

        public int GetHashCode(Color obj)
        {
            int r = FloatToBitRepresentation(obj.r) & (-1 << 3);
            int g = FloatToBitRepresentation(obj.g) & (-1 << 3);
            int b = FloatToBitRepresentation(obj.b) & (-1 << 3);
            return (r.GetHashCode() ^ (g.GetHashCode() << 2) ^ (b.GetHashCode() >> 2));
        }

        public static int FloatToBitRepresentation(float f)
        {
            byte[] bytes = BitConverter.GetBytes(f);
            int result = bytes[0];
            result |= (bytes[1] << 8);
            result |= (bytes[2] << 16);
            result |= (bytes[3] << 24);
            return result;
        }
    }
}
