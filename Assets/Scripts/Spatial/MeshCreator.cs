using UnityEngine;
using System.Collections;
using CellexalVR.General;
using System.IO;
using CellexalVR.MarchingCubes;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CellexalVR.AnalysisObjects
{
    public class MeshCreator : MonoBehaviour
    {
        public static MeshCreator instance;

        public GameObject chunkManagerPrefab;
        public GameObject contourParent;
        public Material opaqueMat;

        private Graph graph;
        private GameObject contour;

        private void Awake()
        {
            instance = this;
        }

        public IEnumerator CreateGeneMesh(string geneName)
        {
            string path = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder + @"\" + "gene1vertex" + ".mesh";
            List<List<Vector3>> meshes = new List<List<Vector3>>();
            List<Vector3> vertices = new List<Vector3>();
            CellexalLog.Log("Started reading " + path);
            //for (int i = 0; i < points.Count; ++i)
            //{
            //    vertices[i] = pointsPositions[i];
            //}

            ChunkManager chunkManager = GameObject.Instantiate(chunkManagerPrefab).GetComponent<ChunkManager>();
            yield return null;
            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);


            string header = streamReader.ReadLine();
            while (!streamReader.EndOfStream)
            {
                string[] coords = streamReader.ReadLine().Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                print(coords[0]);
                Graph.GraphPoint gp = graph.FindGraphPoint(coords[0]);
                int onConvHull = int.Parse(coords[2]);
                //if (coords.Length != 3)
                //    continue;
                //int density = (int)(float.Parse(coords[4]) + float.Parse(coords[5]) + float.Parse(coords[6]) / 3.0);
                //chunkManager.addDensity((int)float.Parse(coords[1]), (int)float.Parse(coords[2]), (int)float.Parse(coords[3]), onConvHull);
                //chunkManager.addDensity((int)gp.Position.x, (int)gp.Position.y, (int)gp.Position.z, onConvHull);
                //vertices.Add(new Vector3(float.Parse(coords[1]), float.Parse(coords[2]), float.Parse(coords[3])));
                //if (vertices.Count >= 65535 && !(streamReader.Peek() == -1))
                //{
                //    meshes.Add(new List<Vector3>(vertices));
                //    vertices.Clear();
                //}
            }
            streamReader.Close();
            fileStream.Close();
            //meshes.Add(vertices);
            //triangles = Enumerable.Range(0, vertices.Count).ToList();
            //chunkManager.toggleSurfaceLevelandUpdateCubes(0);

            foreach (MeshFilter mf in chunkManager.GetComponentsInChildren<MeshFilter>())
            {
                mf.mesh.RecalculateBounds();
                mf.mesh.RecalculateNormals();
            }



            //meshes.Add(vertices);
            //triangles = Enumerable.Range(0, vertices.Count).ToList();

            //contour = Instantiate(contourParent);
            chunkManager.transform.parent = contour.transform;
            contour.transform.localPosition = Vector3.zero;
            contour.transform.localScale = Vector3.one * 0.25f;
            //BoxCollider bc = contour.AddComponent<BoxCollider>();
            //bc.center = Vector3.one * 4;
            //bc.size = Vector3.one * 7;


            // if vertices are reaching the max nr for a mesh we need to subdivide into several meshes.
            //if (meshes.Count > 0)
            //{
            //    foreach (List<Vector3> vertsList in meshes)
            //    {
            //        var convexHull = Instantiate(contourMeshPrefab, contour.transform).GetComponent<MeshFilter>();
            //        convexHull.gameObject.name = "ConvexHull_" + this.name;
            //        convexHull.mesh = new Mesh()
            //        {
            //            vertices = vertsList.ToArray(),
            //            triangles = Enumerable.Range(0, vertsList.Count).ToArray() // triangles.ToArray()
            //        };
            //        //convexHull.transform.parent = contour.transform;
            //        //convexHull.transform.localRotation = contour.transform.localRotation;
            //        //convexHull.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
            //        convexHull.mesh.RecalculateBounds();
            //        convexHull.mesh.RecalculateNormals();
            //        convexHull.transform.localPosition = contour.transform.localPosition;
            //        if (geneName == "gene1")
            //        {
            //            convexHull.GetComponent<MeshRenderer>().material.color = new Color(255, 0, 0, 0.15f);
            //        }
            //        else if (geneName == "gene2")
            //        {
            //            convexHull.GetComponent<MeshRenderer>().material.color = new Color(0, 255, 0, 0.15f);
            //        }
            //        else
            //        {
            //            convexHull.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 255, 0.15f);
            //        }
            //        //convexHull.gameObject.AddComponent<BoxCollider>();
            //    }
            //}

        }


        public IEnumerator CreateMeshFromAShape(string geneName)
        {

            string path = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder + @"\" + "gene1triang" + ".hull";
            string vertPath = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder + @"\" + geneName + ".mesh";

            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);

            ChunkManager chunkManager = GameObject.Instantiate(chunkManagerPrefab).GetComponent<ChunkManager>();
            yield return null;
            List<List<Vector3>> meshes = new List<List<Vector3>>();
            List<Vector3> vertices = new List<Vector3>();
            using (StreamReader sr = new StreamReader(vertPath))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split(null);
                    var gp = graph.FindGraphPoint(line[1]);
                    //Color32 tex = texture.GetPixel(gp.textureCoord.x, gp.textureCoord.y);
                    //print(tex.r);
                    //float density = tex.r / 30;
                    //vertices.Add(gp.Position);
                    //chunkManager.addDensity((int)gp.Position.x, (int)gp.Position.y, (int)gp.Position.z, 1);
                    //chunkManager.addDensity((int)float.Parse(line[1]), (int)float.Parse(line[2]), (int)float.Parse(line[3]), 1);
                }
            }
            List<int> triangles = new List<int>();
            CellexalLog.Log("Started reading " + path);
            //int i = 0;
            //foreach (KeyValuePair<string, GraphPoint> tuple in points)
            //{
            //    vertices[i] = tuple.Value.Position;
            //    i++;
            //}

            meshes.Add(new List<Vector3>(vertices));
            //contour = Instantiate(contourParent);
            //chunkManager.toggleSurfaceLevelandUpdateCubes(0);



            foreach (MeshFilter mf in chunkManager.GetComponentsInChildren<MeshFilter>())
            {
                mf.gameObject.GetComponent<Renderer>().material = opaqueMat;
                mf.mesh.RecalculateBounds();
                mf.mesh.RecalculateNormals();
            }



            //meshes.Add(vertices);
            //triangles = Enumerable.Range(0, vertices.Count).ToList();

            //contour = Instantiate(contourParent);
            chunkManager.transform.parent = contour.transform;
            yield return null;
            chunkManager.transform.localScale = Vector3.one;
            chunkManager.transform.localPosition = Vector3.zero;
            chunkManager.transform.localRotation = Quaternion.identity;
            //string header = streamReader.ReadLine();
            //while (!streamReader.EndOfStream)
            //{
            //    string[] coords = streamReader.ReadLine().Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            //if (coords.Length != 3)
            //    continue;
            // subtract 1 because R is 1-indexed
            //vertices.Add(FindGraphPoint(coords[0]).Position);
            //if (vertices.Count >= 65535 && !(streamReader.Peek() == -1))
            //{
            //    meshes.Add(new List<Vector3>(vertices));
            //    vertices.Clear();
            //}
            //print(vertices[int.Parse(coords[1])]);
            //triangles.Add(int.Parse(coords[1]) - 1);
            //triangles.Add(int.Parse(coords[2]) - 1);
            //triangles.Add(int.Parse(coords[3]) - 1);


        }
        //if (meshes.Count > 0)
        //{
        //    foreach (List<Vector3> vertsList in meshes)
        //    {
        //        var convexHull = Instantiate(contourMeshPrefab, contour.transform).GetComponent<MeshFilter>();
        //        convexHull.gameObject.AddComponent<MeshCollider>();
        //        convexHull.gameObject.name = "ConvexHull_" + this.name;
        //        convexHull.mesh = new Mesh()
        //        {
        //            vertices = vertsList.ToArray(),
        //            triangles = triangles.ToArray()
        //            //triangles = Enumerable.Range(0, vertsList.Count).ToArray() // triangles.ToArray()
        //        };
        //        convexHull.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
        //        convexHull.mesh.RecalculateBounds();
        //        convexHull.mesh.RecalculateNormals();
        //        //convexHull.gameObject.AddComponent<BoxCollider>();
        //    }
        //}
        //meshes.Add(vertices);

        //CellexalLog.Log("Created convex hull with " + vertices.Count() + " vertices");
        //return contour.gameObject;

        public void CreateMesh(string graphName)
        {
            //string path = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder + @"\" + graphName + ".mds";
            //FileStream fileStream = new FileStream(path, FileMode.Open);
            //StreamReader streamReader = new StreamReader(fileStream);

            //ChunkManager chunkManager = GameObject.Instantiate(chunkManagerPrefab).GetComponent<ChunkManager>();
            //yield return null;
            ////Vector3[] vertices = new Vector3[points.Count];
            //List<List<Vector3>> meshes = new List<List<Vector3>>();
            //List<Vector3> vertices = new List<Vector3>();
            //CellexalLog.Log("Started reading " + path);
            ////for (int i = 0; i < points.Count; ++i)
            ////{
            ////    vertices[i] = pointsPositions[i];
            ////}

            //string header = streamReader.ReadLine();
            //while (!streamReader.EndOfStream)
            //{
            //    string[] coords = streamReader.ReadLine().Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            //    //if (coords.Length != 3)
            //    //    continue;
            //    chunkManager.addDensity((int)float.Parse(coords[1]), (int)float.Parse(coords[2]), (int)float.Parse(coords[3]), 1);

            //    //vertices.Add(new Vector3(float.Parse(coords[1]), float.Parse(coords[2]), float.Parse(coords[3])));
            //    //if (vertices.Count >= 65535 && !(streamReader.Peek() == -1))
            //    //{
            //    //    meshes.Add(new List<Vector3>(vertices));
            //    //    vertices.Clear();
            //    //}
            //}
            //chunkManager.toggleSurfaceLevelandUpdateCubes(0);



            //foreach (MeshFilter mf in chunkManager.GetComponentsInChildren<MeshFilter>())
            //{
            //    mf.mesh.RecalculateBounds();
            //    mf.mesh.RecalculateNormals();
            //}



            //meshes.Add(vertices);
            ////triangles = Enumerable.Range(0, vertices.Count).ToList();

            //streamReader.Close();
            //fileStream.Close();

            //contour = Instantiate(contourParent);
            //chunkManager.transform.parent = contour.transform;
            //contour.transform.localScale = Vector3.one * 0.25f;
            //BoxCollider bc = contour.AddComponent<BoxCollider>();
            //bc.center = Vector3.one * 4;
            //bc.size = Vector3.one * 6;


            // if vertices are reaching the max nr for a mesh we need to subdivide into several meshes.
            //if (meshes.Count > 0)
            //{
            //    foreach (List<Vector3> vertsList in meshes)
            //    {
            //        var convexHull = Instantiate(contourMeshPrefab, contour.transform).GetComponent<MeshFilter>();
            //        convexHull.gameObject.name = "ConvexHull_" + this.name;
            //        convexHull.mesh = new Mesh()
            //        {
            //            vertices = vertsList.ToArray(),
            //            triangles = Enumerable.Range(0, vertsList.Count).ToArray() // triangles.ToArray()
            //        };
            //        convexHull.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
            //        convexHull.mesh.RecalculateBounds();
            //        convexHull.mesh.RecalculateNormals();
            //        convexHull.gameObject.AddComponent<BoxCollider>();
            //    }
            //}
            //contour.AddComponent<BoxCollider>();
            //contour.AddComponent<MeshCollider>();
            //else
            //{
            //    var convexHull = Instantiate(skeletonPrefab).GetComponent<MeshFilter>();
            //    convexHull.gameObject.name = "ConvexHull_" + this.name;
            //    convexHull.mesh = new Mesh()
            //    {
            //        vertices = vertices.ToArray(),
            //        triangles = triangles.ToArray()
            //    };
            //    convexHull.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
            //    convexHull.mesh.RecalculateBounds();
            //    convexHull.mesh.RecalculateNormals();
            //}


            //if (gameManager.multiplayer)
            //{
            //    convexHull.transform.position = new Vector3(0, 1f, 0);
            //}
            //if (!gameManager.multiplayer)
            //{
            //    convexHull.transform.position = referenceManager.rightController.transform.position;
            //}
            // move the convexhull slightly out of the way of the graph
            // in a direction sort of pointing towards the middle.
            // otherwise it lags really bad when the skeleton is first 
            // moved out of the original graph
            //Vector3 moveDist = new Vector3(0f, 0.3f, 0f);
            //if (transform.position.x > 0) moveDist.x = -.2f;
            //if (transform.position.z > 0) moveDist.z = -.2f;
            //convexHull.transform.Translate(moveDist);
            //convexHull.transform.position += referenceManager.rightController.transform.forward * 1f;
            //convexHull.transform.rotation = transform.rotation;
            //convexHull.transform.localScale = transform.localScale;
            //convexHull.transform.position = Vector3.zero;
            //CellexalLog.Log("Created convex hull with " + vertices.Count() + " vertices");
            //return contour.gameObject;
        }
    }
}
