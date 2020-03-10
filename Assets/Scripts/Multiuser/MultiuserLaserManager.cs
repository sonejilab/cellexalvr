using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Multiuser
{
    public class MultiuserLaserManager : MonoBehaviour
    {
        public Material laserMaterial;
        private List<GameObject> lasers = new List<GameObject>();

        public LineRenderer GetLaser(int id)
        {
            foreach (GameObject laser in lasers)
            {
                if (laser.gameObject.name == id.ToString())
                {
                    return laser.GetComponent<LineRenderer>();
                }
            }
            return null;
        }

        public LineRenderer AddLaser(int id)
        {
            GameObject newLaser = new GameObject();
            newLaser.transform.parent = transform;
            newLaser.gameObject.name = id.ToString();
            LineRenderer lr = newLaser.AddComponent<LineRenderer>();
            lr.material = laserMaterial;
            lr.startWidth = lr.endWidth = 0.015f;
            lasers.Add(newLaser);
            return lr;
        }
    }
}