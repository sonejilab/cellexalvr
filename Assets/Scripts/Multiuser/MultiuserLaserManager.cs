using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace CellexalVR.Multiuser
{
    public class MultiuserLaserManager : MonoBehaviour
    {
        public Material laserMaterial;
        public Dictionary<int, Transform> laserDict = new Dictionary<int, Transform>();
        // lasers contain a list of the gameobjects and the origin point.
        public Dictionary<int, LineRenderer> lasers = new Dictionary<int, LineRenderer>();
        // private List<Tuple<GameObject, Vector3>> lasers = new List<Tuple<GameObject, Vector3>>();

        // public LineRenderer GetLaser(int id)
        // {
        //     foreach (LineRenderer laser in lasers)
        //     {
        //         if (laser.gameObject.name == id.ToString())
        //         {
        //             return laser;
        //         }
        //     }
        //     return null;
        // }

        public LineRenderer AddLaser(int id, string ownerName)
        {
            GameObject newLaser = new GameObject();
            GameObject player = GameObject.Find(ownerName);
            newLaser.transform.parent = player.transform;
            newLaser.gameObject.name = id.ToString();
            LineRenderer lr = newLaser.AddComponent<LineRenderer>();
            lr.material = laserMaterial;
            lr.startWidth = lr.endWidth = 0.002f;
            lasers.Add(lr);
            laserDict[id] = player.GetComponent<PlayerManager>().rightHand;
            // var views = PhotonNetwork.playerList;
            // lasers.Add(new Tuple<GameObject, Vector3>(newLaser, originPoint));
            return lr;
        }
    }
}