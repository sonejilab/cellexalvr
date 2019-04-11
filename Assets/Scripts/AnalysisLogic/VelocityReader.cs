using UnityEngine;
using System.IO;
using System.Collections;
using CellexalVR.DesktopUI;

namespace CellexalVR.AnalysisLogic
{
    public class VelocityReader : MonoBehaviour
    {
        public GameObject parentPrefab;
        public GameObject arrowPrefab;

        [ConsoleCommand("velocityReader", "rvf")]
        public void ReadVelocityFile()
        {
            ReadVelocityFile(Directory.GetCurrentDirectory() + @"\Data\Mouse_HSPC\Diff.velo");
        }

        public void ReadVelocityFile(string path)
        {
            StreamReader streamReader = new StreamReader(path);

            GameObject parent = Instantiate(parentPrefab);
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                string[] words = line.Split(null);
                string cellname = words[0];

                Vector3 start = new Vector3(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
                Vector3 end = new Vector3(float.Parse(words[4]), float.Parse(words[5]), float.Parse(words[6]));

                GameObject arrow = Instantiate(arrowPrefab);
                arrow.transform.parent = parent.transform;
                arrow.transform.localPosition = Vector3.zero;
                var lineRenderer = arrow.GetComponentInChildren<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPositions(new Vector3[] { start, end });
                var head = arrow.transform.Find("head");
                head.localPosition = end;
                head.LookAt(arrow.transform.TransformPoint(start));
                head.Rotate(new Vector3(180f, 0f, 0f));
                //arrow.transform.localPosition = pos;

                parent.transform.position = new Vector3(1f, 1f, 1f);
            }

            streamReader.Close();
        }
    }
}