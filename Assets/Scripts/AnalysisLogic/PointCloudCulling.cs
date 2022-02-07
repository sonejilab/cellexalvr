using System;
using UnityEngine;

namespace DefaultNamespace
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