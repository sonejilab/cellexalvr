using System;
using CellexalVR.MarchingCubes;
using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.MarchingCubes
{
    public class DensitySphere : MonoBehaviour
    {
        public int[] index = new int[3];

        public ChunkManager chunkManager;


        // For Mesh Slicing
        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.gameObject.GetComponent<LightSaberSliceCollision>() != null)
        //    {
        //        // Vector3 pos = transform.localPosition;
        //        // chunkManager.setDensity((int) pos.x, (int) pos.y, (int) pos.z, 0);
        //        chunkManager.setDensity(index[0], index[1], index[2], 2);
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (other.gameObject.GetComponent<LightSaberSliceCollision>() != null)
        //    {
        //        // chunkManager.toggleSurfaceLevelandUpdateCubes(0, chunkManager.chunks);
        //    }

        //}
    }
    
}