using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Multiuser
{
    public class MultiuserLaserManager : MonoBehaviour
    {
        private List<GameObject> lasers = new List<GameObject>();

        public LineRenderer GetLaser(int id)
        {
            print("Get Laser");
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
            print("Add Laser");
            GameObject newLaser = new GameObject();
            newLaser.transform.parent = transform;
            newLaser.gameObject.name = id.ToString();
            lasers.Add(newLaser);
            return newLaser.AddComponent<LineRenderer>();
        }
    }
}