using UnityEngine;
using System.Collections;

namespace CellexalVR.Spatial
{
    public class GraphSliceCube : MonoBehaviour
    {
        [SerializeField] private GameObject leftWall;
        [SerializeField] private GameObject rightWall;
        [SerializeField] private GameObject topWall;
        [SerializeField] private GameObject bottomWall;
        [SerializeField] private GameObject frontWall;
        [SerializeField] private GameObject backWall;

        private void Start()
        {

        }

        private void Update()
        {

        }

        public void SetCube(Vector3 coordinates, Vector3 center)
        {
            Vector3 leftWallScale = leftWall.transform.localScale;
            leftWallScale.x = coordinates.x * 2;
            leftWallScale.x = coordinates.y * 2;
        }
    }
}
