//#define DEBUG_SPACE_MESH_CREATOR

using UnityEngine;
using System.Collections.Generic;

namespace CurvedVRKeyboard {
    public class SpaceMeshCreator {
        KeyboardCreator creator;
        UvSlicer uvSlicer;

        List<Vector3> verticiesArray;
        private bool isFrontFace;

        //-----BuildingData-----
        private Vector2 boundary = new Vector2(2f, 0.5f);
        private int verticiesCount = 32;
        private int trianglesIteration = 39;
        private float rowSize = 4;
        private float verticiesSpacing;

        public SpaceMeshCreator(KeyboardCreator creator) {
            this.creator = creator;
        }

        public void Recalculate9Slice(Sprite texture,float referencedPixels) {
            if(uvSlicer == null) {
                uvSlicer = new UvSlicer();
            }
            uvSlicer.referencedPixels = referencedPixels;
            uvSlicer.ChangeSprite(texture);    
        }

        /// <summary>
        /// Builds mesh for space bar
        /// </summary>
        /// <param name="renderer"> Renderer to get nesh from</param>
        /// <param name="frontFace"> True if front face needs to be rendered. False if back face</param>
        public void BuildFace(Renderer renderer, bool frontFace) {
            verticiesSpacing = rowSize / (verticiesCount / rowSize);
            isFrontFace = frontFace;
            Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            List<int> trainglesArray = new List<int>();

            BuildVerticies();
            BuildQuads(trainglesArray);

            renderer.gameObject.GetComponent<MeshFilter>().sharedMesh = RebuildMesh(mesh, verticiesArray, trainglesArray);

            CalculatePosition(verticiesArray);
            mesh.vertices = verticiesArray.ToArray();

#if DEBUG_SPACE_MESH_CREATOR
            LogList(verticiesArray);
#endif
            mesh.RecalculateNormals();
        }

        private void BuildVerticies() {
            verticiesArray = new List<Vector3>();
            for(float currentX = -boundary.x; currentX <= boundary.x; currentX += verticiesSpacing) {
                AddWholeColumn(new Vector3(currentX, 0, 0));
                uvSlicer.CheckVerticalBorders(currentX, verticiesSpacing, this);
            }
        }

        public void AddWholeColumn(Vector3 toAdd) {
            for(int row = 0; row < rowSize; row++) {
                verticiesArray.Add(toAdd);
            }
        }

        /// <summary>
        /// Builds triangles from array of integers
        /// </summary>
        /// <param name="trianglesArray"> Array to be builded</param>
        private void BuildQuads(List<int> trianglesArray) {
            if(isFrontFace) {
                for(int i = 0; i < trianglesIteration; i++) {
                    trianglesArray.Add(i + 4);
                    trianglesArray.Add(i + 1);
                    trianglesArray.Add(i);

                    trianglesArray.Add(i + 1);
                    trianglesArray.Add(i + 4);
                    trianglesArray.Add(i + 5);
                    if(i % rowSize == 2) { //we must skip every 3rd iteration
                        i++;
                    }
                }
            }
        }

        private Vector2[] BuildUV() {
            Vector2[] uv = new Vector2[verticiesCount + 2];
            float border = 1f / (verticiesCount / 2f);
            uv[0] = new Vector2(0f, 1f);
            uv[1] = new Vector2(0, 0f);
            uv[2] = new Vector2(border, 1);
            uv[3] = new Vector2(border, 0f);
            for(int i = 0; i < verticiesCount; i += 4) {
                float uvPoint = border * (i / 2f);
                uv[i] = new Vector2(uvPoint, 1);
                uv[i + 1] = new Vector2(uvPoint, 0);
                uvPoint += border;
                uv[i + 2] = new Vector2(uvPoint, 1);
                uv[i + 3] = new Vector2(uvPoint, 0);
            }
            uv[verticiesCount] = new Vector2(1, 1);
            uv[verticiesCount + 1] = new Vector2(1, 0);
            return uv;
        }
        
        /// <summary>
        /// Calculates position for verticies
        /// </summary>
        /// <param name="verticiesArray"> Array of verticies</param>
        private void CalculatePosition(List<Vector3> verticiesArray) {
            float offset = 0;
            for (int i = 0; i < verticiesArray.Count; i += 4)
            {
                float degree = creator.CalculateRotation(rowSize, offset);
                Vector3 calculatedVertex = creator.CalculatePositionOnCylinder(degree);

                if (i + 4 < verticiesArray.Count)
                {//if there is next value in array
                    offset += verticiesArray[i + 4].x - verticiesArray[i].x;
                }

                calculatedVertex.z -= creator.centerPointDistance;

                calculatedVertex.y = boundary.y;
                this.verticiesArray[i] = calculatedVertex;

                calculatedVertex.y = uvSlicer.objectBorderInUnits.top;
                this.verticiesArray[i + 1] = calculatedVertex;

                calculatedVertex.y = uvSlicer.objectBorderInUnits.bottom;
                this.verticiesArray[i + 2] = calculatedVertex;

                calculatedVertex.y = -boundary.y;
                this.verticiesArray[i + 3] = calculatedVertex;

            }
        }

        /// <summary>
        /// Apply all changes 
        /// </summary>
        /// <param name="mesh"> Mesh to be changed</param>
        /// <param name="verticiesArray"> Calculated positions of verticies</param>
        /// <param name="trainglesArray"> Calculated triangles </param>
        /// <returns></returns>
        private Mesh RebuildMesh(Mesh mesh, List<Vector3> verticiesArray, List<int> trainglesArray) {
            mesh.triangles = trainglesArray.ToArray();
            mesh.uv = uvSlicer.BuildUV(verticiesArray, boundary);
            return mesh;
        }

#if DEBUG_SPACE_MESH_CREATOR
        private void LogList(List<Vector3> list) {
            string output = "";
            foreach(Vector3 element in list) {
                output += string.Format("x:{0}, y{1}:, z{2} \n", element.x, element.y, element.z);
            }
            Debug.Log(output);
            Debug.Log(list.Count);
        }

        private void LogArray(Vector2[] list) {
            string output = "";
            foreach(Vector3 element in list) {
                output += element.ToString();
                output += " :: ";
            }
            Debug.Log(output);
            Debug.Log(list.Length);
        }
#endif
    }
}