using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Filter
{

    private string[] seperators = { "=", "!=", "<=", "<", ">=", ">" };

    public Filter(string filepath)
    {
        StreamReader streamReader = new StreamReader(filepath);
        while (!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine();
            if (line.Length == 0) continue;
            if (line[0] == '#') continue;
            int seperatorIndex = IndexOfAny(line, seperators);
            if (seperatorIndex == -1)
            {
                CellExAlLog.Log("WARNING: Ignoring line because no seperator was found in the filter " + filepath + " on the line " + line + ".");
                continue;
            }

        }
        streamReader.Close();
    }

    private int IndexOfAny(string s, string[] stringsToLookFor)
    {
        // loop to go through the string
        for (int i = 0; i < s.Length; ++i)
        {
            // loop to check all different strings to look for
            for (int j = 0; j < stringsToLookFor.Length; ++j)
            {
                // loop to check one thing to look for
                for (int k = 0; k < stringsToLookFor[j].Length; ++k)
                {
                    if (s[i + k] != stringsToLookFor[j][k])
                    {
                        // if it does not match, try the next one
                        break;
                    }
                    else if (k == stringsToLookFor[k].Length - 1)
                    {
                        // if it matched and this was the last character of the thing to look for, return the start index
                        return i;
                    }
                }
            }

        }
        // if nothing was found
        return -1;
    }

    public class FilterRule
    {

    }
}

