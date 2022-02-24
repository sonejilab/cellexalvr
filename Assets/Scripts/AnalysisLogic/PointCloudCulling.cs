using System;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    public class PointCloudCulling : MonoBehaviour
    {
        public static PointCloudCulling instance;

        private void Awake()
        {
            instance = this;
        }
        
    }
}