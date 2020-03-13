using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace CellexalVR.Multiuser
{
    public class MultiuserLaserManager : MonoBehaviour
    {
        public Material laserMaterial;
        [HideInInspector]
        public Dictionary<int, Transform> laserTransforms = new Dictionary<int, Transform>();
        // lasers contain a list of the gameobjects and the origin point.
        [HideInInspector]
        public Dictionary<int, LineRenderer> lasersLineRends = new Dictionary<int, LineRenderer>();
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
            if (player == null) return null;
            newLaser.transform.parent = player.transform;
            newLaser.gameObject.name = id.ToString();
            LineRenderer lr = newLaser.AddComponent<LineRenderer>();
            lr.material = laserMaterial;
            lr.startWidth = lr.endWidth = 0.002f;
            lasersLineRends[id] = lr;
            laserTransforms[id] = player.GetComponent<PlayerManager>().rightHand;
            lr.SetPosition(0, laserTransforms[id].position);
            lr.SetPosition(1, laserTransforms[id].position + Vector3.forward * 10);
            // var views = PhotonNetwork.playerList;
            // lasers.Add(new Tuple<GameObject, Vector3>(newLaser, originPoint));
            return lr;
        }
    }
}