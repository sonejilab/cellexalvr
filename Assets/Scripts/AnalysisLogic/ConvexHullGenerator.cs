using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using CellexalVR.DesktopUI;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Linq;
using CellexalVR.Extensions;
using System;
using System.IO;

namespace CellexalVR.AnalysisLogic
{
    public class ConvexHullGenerator : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject convexHullPrefab;
        public GameObject[] debugSpheres;

        private float result;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        #region 2D_helper_functions

        // claims points for the hashset outside from the hashset unclaimed, given two points on a line
        private void ClaimPoints(Vector2 pointOnLine1, Vector2 pointOnLine2, HashSet<int> outside,
                                    HashSet<int> unclaimed, List<Vector2> pos)
        {
            foreach (int j in unclaimed)
            {
                if (RightOfLine(pointOnLine1, pointOnLine2, pos[j]))
                {
                    outside.Add(j);
                }
            }

            foreach (int j in outside)
            {
                unclaimed.Remove(j);
            }
        }

        private Vector3Int FindInitTriangle(List<Vector2> pos)
        {
            Vector3Int initTri = new Vector3Int(0, 0, 0);
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            for (int i = 0; i < pos.Count; ++i)
            {
                Vector2 point = pos[i];
                if (point.x < minX)
                {
                    minX = point.x;
                    initTri.x = i;
                }
                if (point.x > maxX)
                {
                    maxX = point.x;
                    initTri.y = i;
                }
            }

            Vector2 initTriPos1 = pos[initTri.x];
            Vector2 initTriPos2 = pos[initTri.y];
            float maxArea = 0;
            for (int i = 0; i < pos.Count; ++i)
            {
                Vector2 point = pos[i];
                float area = Mathf.Abs((initTriPos1.x - point.x) * (initTriPos2.y - initTriPos1.y) - (initTriPos1.x - initTriPos2.x) * (point.y - initTriPos1.y)) / 2f;
                if (area > maxArea)
                {
                    maxArea = area;
                    initTri.z = i;
                }
            }

            return initTri;
        }

        // returns true if p is to the right the line that goes from a to b
        private bool RightOfLine(Vector2 a, Vector2 b, Vector2 p)
        {
            return ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) < 0f;
        }

        // returns the line in the correct orientation so that point is to the right (or left if right = false) of the line
        private Vector2Int CorrectOrientation(List<Vector2> pos, Vector2Int line, Vector2 point, bool right)
        {
            if (RightOfLine(pos[line.x], pos[line.y], point) ^ right)
            {
                return new Vector2Int(line.y, line.x);
            }
            else
            {
                return line;
            }
        }

        // returns the index of the point in outside that is the furthest from the line
        private int IndexOfFurthestPoint(List<Vector2> pos, HashSet<int> outside, Vector2Int line)
        {
            float maxDist = float.MinValue;
            int maxDistIndex = -1;
            Vector2 linePos1 = pos[line.x];
            Vector2 linePos2 = pos[line.y];
            Vector2 lineVec = linePos2 - linePos1;
            float lineLength = lineVec.sqrMagnitude;

            foreach (int i in outside)
            {

                Vector2 point = pos[i];
                // distance
                float t = Mathf.Clamp(Vector2.Dot(point - linePos1, point - linePos2) / lineLength, 0f, 1f);
                Vector2 projected = linePos1 + t * lineVec;
                float dist = (point - projected).magnitude;
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxDistIndex = i;
                }
            }
            return maxDistIndex;
        }

        private bool IsAlmostEqual(float f1, float f2)
        {
            return Mathf.Abs(f1 - f2) < 1e-6f;
        }

        // returns true if the line segments l1 and l2 intersect
        private void LineSegementsIntersect(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2, out bool intersect, out Vector2 intersectPoint)
        {
            float A1 = l1p2.y - l1p1.y;
            float B1 = l1p1.x - l1p2.x;
            float C1 = A1 * l1p1.x + B1 * l1p1.y;

            float A2 = l2p2.y - l2p1.y;
            float B2 = l2p1.x - l2p2.x;
            float C2 = A2 * l2p1.x + B2 * l2p1.y;

            //lines are parallel
            float det = A1 * B2 - A2 * B1;
            if (IsAlmostEqual(det, 0f))
            {
                //parallel lines
                intersect = false;
                intersectPoint = Vector2.zero;
            }
            else
            {
                float x = (B2 * C1 - B1 * C2) / det;
                float y = (A1 * C2 - A2 * C1) / det;
                bool online1 = ((Mathf.Min(l1p1.x, l1p2.x) < x || IsAlmostEqual(Mathf.Min(l1p1.x, l1p2.x), x))
                                && (Mathf.Max(l1p1.x, l1p2.x) > x || IsAlmostEqual(Mathf.Max(l1p1.x, l1p2.x), x))
                                && (Mathf.Min(l1p1.y, l1p2.y) < y || IsAlmostEqual(Mathf.Min(l1p1.y, l1p2.y), y))
                                && (Mathf.Max(l1p1.y, l1p2.y) > y || IsAlmostEqual(Mathf.Max(l1p1.y, l1p2.y), y))
                    );
                bool online2 = ((Mathf.Min(l2p1.x, l2p2.x) < x || IsAlmostEqual(Mathf.Min(l2p1.x, l2p2.x), x))
                                && (Mathf.Max(l2p1.x, l2p2.x) > x || IsAlmostEqual(Mathf.Max(l2p1.x, l2p2.x), x))
                                && (Mathf.Min(l2p1.y, l2p2.y) < y || IsAlmostEqual(Mathf.Min(l2p1.y, l2p2.y), y))
                                && (Mathf.Max(l2p1.y, l2p2.y) > y || IsAlmostEqual(Mathf.Max(l2p1.y, l2p2.y), y))
                    );

                if (online1 && online2)
                {
                    intersect = true;
                    intersectPoint = new Vector2(x, y);
                }
                else
                {
                    intersect = false;
                    intersectPoint = Vector2.zero;
                }
            }

        }

        private void MinMaxCoords(List<Vector2> points, out Vector2 min, out Vector2 max)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Vector2 p in points)
            {
                if (p.x < minX)
                    minX = p.x;
                if (p.x > maxX)
                    maxX = p.x;
                if (p.y < minY)
                    minY = p.y;
                if (p.y > maxY)
                    maxY = p.y;
            }

            min = new Vector2(minX, minY);
            max = new Vector2(maxX, maxY);
        }

        private bool BoundsInsideBounds(Vector2 b1min, Vector2 b1max, Vector2 b2min, Vector2 b2max)
        {
            return b1min.x > b2min.x && b1max.x < b2max.x
                                     && b1min.y > b2min.y && b1max.y < b2max.y;
        }

        private bool BoundsIntersect(Vector2 b1min, Vector2 b1max, Vector2 b2min, Vector2 b2max)
        {
            return !(b1min.x > b2max.x || b2min.x > b1max.x
                                       || b1min.y > b2max.y || b2min.y > b1max.y);
        }

        private float AreaOf(List<Vector2> points)
        {
            if (points.Count == 0)
            {
                return 0f;
            }
            // loop over triangles and sum areas
            float area = 0f;
            Vector2 a = points[0];
            for (int i = 1; i < points.Count - 1; ++i)
            {
                Vector3 b = points[i];
                Vector3 c = points[i + 1];
                area += Mathf.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2f;
            }
            return area;
        }

        public List<Vector2> SortHull(List<Vector2Int> hullInd, List<Vector2> pos)
        {
            List<Vector2Int> hullIndCopy = new List<Vector2Int>(hullInd);
            int first = hullIndCopy[0].x;
            int second = hullIndCopy[0].y;
            int third = hullIndCopy[1].x;

            if (third == first || third == second)
            {
                third = hullIndCopy[1].y;
            }
            //print("sort hull: first: " + first + " second " + second + " third " + third);

            //bool dir = Vector2.SignedAngle(pos[first] - pos[third], pos[second] - pos[third]) > 0f;
            Vector2Int correctStart = CorrectOrientation(pos, hullInd[0], pos[third], false);

            List<Vector2> sortedHull = new List<Vector2>(hullIndCopy.Capacity);
            int lastAdded = correctStart.y;
            sortedHull.Add(pos[correctStart.y]);
            //if (dir)
            //{
            //    sortedHull.Add(pos[first]);
            //    lastAdded = first;
            //}
            //else
            //{
            //    sortedHull.Add(pos[second]);
            //    lastAdded = second;
            //}
            hullIndCopy.RemoveAt(0);

            for (int i = 0; i < hullIndCopy.Count; ++i)
            {
                if (hullIndCopy[i].x == lastAdded)
                {
                    sortedHull.Add(pos[hullIndCopy[i].y]);
                    lastAdded = hullIndCopy[i].y;
                    hullIndCopy.RemoveAt(i);
                    i = -1;
                }
                else if (hullIndCopy[i].y == lastAdded)
                {
                    sortedHull.Add(pos[hullIndCopy[i].x]);
                    lastAdded = hullIndCopy[i].x;
                    hullIndCopy.RemoveAt(i);
                    i = -1;
                }
            }
            return sortedHull;
        }

        #endregion

        #region 3D_helper_functions
        /// <summary>
        /// Removes points that lie too far away
        /// </summary>
        /// <param name="pos">A list of positions to filter</param>
        /// <param name="threshold">The number of standard deviations away from the mean in all axes combined a point must be to be removed</param>
        private void FilterOutliers(List<Vector3> pos, float threshold)
        {
            float meanX = 0f;
            float meanY = 0f;
            float meanZ = 0f;

            foreach (Vector3 v in pos)
            {
                meanX += v.x;
                meanY += v.y;
                meanZ += v.z;
            }

            meanX /= pos.Count;
            meanY /= pos.Count;
            meanZ /= pos.Count;
            // standard deviation = sqrt( mean( sum( (x_i - mean(x))^2 ) ) )
            float standardDeviationX = 0f;
            float standardDeviationY = 0f;
            float standardDeviationZ = 0f;

            foreach (Vector3 v in pos)
            {
                standardDeviationX += (v.x - meanX) * (v.x - meanX);
                standardDeviationY += (v.y - meanY) * (v.y - meanY);
                standardDeviationZ += (v.z - meanZ) * (v.z - meanZ);
            }
            standardDeviationX /= pos.Count;
            standardDeviationY /= pos.Count;
            standardDeviationZ /= pos.Count;
            standardDeviationX = Mathf.Sqrt(standardDeviationX);
            standardDeviationY = Mathf.Sqrt(standardDeviationY);
            standardDeviationZ = Mathf.Sqrt(standardDeviationZ);

            for (int i = 0; i < pos.Count; ++i)
            {
                if (Mathf.Abs(pos[i].x - meanX) / standardDeviationX +
                    Mathf.Abs(pos[i].y - meanY) / standardDeviationY +
                    Mathf.Abs(pos[i].z - meanZ) / standardDeviationZ > threshold)
                {
                    pos.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Removes points that lie too far away
        /// </summary>
        /// <param name="pos">A list of positions to filter</param>
        /// <param name="threshold">The number of standard deviations away from the mean in all axes combined a point must be to be removed</param>
        private void FilterOutliers(List<Vector2> pos, float threshold)
        {
            float meanX = 0f;
            float meanY = 0f;

            foreach (Vector3 v in pos)
            {
                meanX += v.x;
                meanY += v.y;
            }

            meanX /= pos.Count;
            meanY /= pos.Count;
            // standard deviation = sqrt( mean( sum( (x_i - mean(x))^2 ) ) )
            float standardDeviationX = 0f;
            float standardDeviationY = 0f;

            foreach (Vector2 v in pos)
            {
                standardDeviationX += (v.x - meanX) * (v.x - meanX);
                standardDeviationY += (v.y - meanY) * (v.y - meanY);
            }
            standardDeviationX /= pos.Count;
            standardDeviationY /= pos.Count;
            standardDeviationX = Mathf.Sqrt(standardDeviationX);
            standardDeviationY = Mathf.Sqrt(standardDeviationY);

            for (int i = 0; i < pos.Count; ++i)
            {
                if (Mathf.Abs(pos[i].x - meanX) / standardDeviationX +
                    Mathf.Abs(pos[i].y - meanY) / standardDeviationY > threshold)
                {
                    pos.RemoveAt(i);
                    i--;
                }
            }
        }

        // returns the index of the furhtest point from a triangle given by three points
        private int IndexOfFurthestPoint(List<Vector3> pos, HashSet<int> outside, Vector3 faceVert1, Vector3 faceVert2, Vector3 faceVert3)
        {
            // algorithm: https://www.geometrictools.com/Documentation/DistancePoint3Triangle3.pdf
            //
            // divide the area around and inside the triangle into 7 regions
            //           .region 2.
            //            .      .
            //             .    .
            //              .  .
            //               ..
            //               /\
            // region  3    /  \ region 1
            //             /    \
            //            /      \
            //           /region 0\
            //. . . . . ------------ . . . . . .
            //region 4 .   region 5  . region 6
            //        .               .
            //       .                 .

            float greatestDist = float.MinValue;
            int greatestDistIndex = -1;
            // two sides on the triangle
            Vector3 side1 = faceVert1 - faceVert3;
            Vector3 side2 = faceVert2 - faceVert3;
            float a = Vector3.Dot(side1, side1);
            float b = Vector3.Dot(side1, side2);
            float c = Vector3.Dot(side2, side2);
            float det = a * c - b * b;

            foreach (int i in outside)
            {

                Vector3 point = pos[i];
                Vector3 closestPointOnTriangle;
                Vector3 D = faceVert3 - point;

                float d = Vector3.Dot(side1, D);
                float e = Vector3.Dot(side2, D);
                //float f = Vector3.Dot(D, D);
                float s = b * e - c * d;
                float t = b * d - a * e;

                if (s + t <= det)
                {
                    if (s < 0)
                    {
                        if (t < 0)
                        {
                            // region 4
                            if (d < 0)
                            {
                                t = 0;
                                if (-d >= a)
                                {
                                    s = 1;
                                }
                                else
                                {
                                    s = -d / a;
                                }
                            }
                            else
                            {
                                s = 0;
                                if (e >= 0)
                                {
                                    t = 0;
                                }
                                else if (-e >= c)
                                {
                                    t = 1;
                                }
                                else
                                {
                                    t = -e / c;
                                }
                            }
                        }
                        else
                        {
                            // region 3
                            s = 0;
                            if (e >= 0)
                            {
                                t = 0;
                            }
                            else if (-e >= c)
                            {
                                t = 1;
                            }
                            else
                            {
                                t = -e / c;
                            }
                        }
                    }
                    else if (t < 0)
                    {
                        // region 5
                        t = 0;
                        if (d >= 0)
                        {
                            s = 0;
                        }
                        else if (-d >= a)
                        {
                            s = 1;
                        }
                        else
                        {
                            s = -d / a;
                        }
                    }
                    else
                    {
                        // region 0
                        s /= det;
                        t /= det;
                    }
                }
                else
                {
                    if (s < 0)
                    {
                        // region 2
                        float temp0 = b + d;
                        float temp1 = c + e;
                        if (temp1 > temp0)
                        {
                            float numer = temp1 - temp0;
                            float denom = a - 2 * b + c;
                            if (numer >= denom)
                            {
                                s = 1;
                            }
                            else
                            {
                                s = numer / denom;
                            }
                            t = 1 - s;
                        }
                        else
                        {
                            s = 0;
                            if (temp1 <= 0)
                            {
                                t = 1;
                            }
                            else if (e >= 0)
                            {
                                t = 0;
                            }
                            else
                            {
                                t = -e / c;
                            }
                        }
                    }
                    else if (t < 0)
                    {
                        // region 6
                        float temp0 = b + e;
                        float temp1 = a + d;
                        if (temp1 > temp0)
                        {
                            float numer = temp1 - temp0;
                            float denom = a - 2 * b + c;
                            if (numer >= denom)
                            {
                                t = 1;
                            }
                            else
                            {
                                t = numer / denom;
                            }
                            s = 1 - t;
                        }
                        else
                        {
                            t = 0;
                            if (temp1 <= 0)
                            {
                                s = 1;
                            }
                            else if (e >= 0)
                            {
                                s = 0;
                            }
                            else
                            {
                                s = -d / a;
                            }
                        }
                    }
                    else
                    {
                        // region 1
                        float numer = (c + e) - (b + d);
                        if (numer <= 0)
                        {
                            s = 0;
                        }
                        else
                        {
                            float denom = a - 2 * b + c;
                            if (numer >= denom)
                            {
                                s = 1;
                            }
                            else
                            {
                                s = numer / denom;
                            }
                        }
                        t = 1 - s;
                    }
                }

                closestPointOnTriangle = faceVert3 + side1 * s + side2 * t;
                float dist = (closestPointOnTriangle - point).magnitude;
                if (dist > greatestDist)
                {
                    greatestDist = dist;
                    greatestDistIndex = i;
                }
            }
            return greatestDistIndex;
        }

        // returns true if a point is "above" a plane (if the point is on the side of the plane that the plane's normal is pointing at)
        // second arg is a vector4 where xyz is the plane's normal and w is how far the plane is translated along that normal
        private bool AbovePlane(Vector3 point, Vector4 plane)
        {
            return point.x * plane.x + point.y * plane.y + point.z * plane.z - plane.w > 0;
        }

        // returns true if the edge between the two first args is the same as the edge between the last two args
        private bool EqualEdge(int x1, int y1, int x2, int y2)
        {
            return x1 == x2 && y1 == y2 || x1 == y2 && y1 == x2;
        }

        // returns true if a triangle includes an edge, first two args is the edge, second arg is the triangle
        private bool IncludesEdge(int v1, int v2, Vector3Int tri)
        {
            return EqualEdge(v1, v2, tri.x, tri.y) ||
                   EqualEdge(v1, v2, tri.x, tri.z) ||
                   EqualEdge(v1, v2, tri.y, tri.z);
        }

        // returns true if the specified triangles share at least one edge
        private bool SharesEdge(Vector3Int tri1, Vector3Int tri2)
        {
            return IncludesEdge(tri1.x, tri1.y, tri2) ||
                   IncludesEdge(tri1.y, tri1.z, tri2) ||
                   IncludesEdge(tri1.y, tri1.z, tri2);
        }

        // returns true if an edge is in any of the given triangles
        private bool EdgeInAnyTri(int v1, int v2, int exclude, List<Vector3Int> tris)
        {
            bool included = false;
            for (int k = 0; k < tris.Count && !included; ++k)
            {
                if (exclude == k)
                {
                    continue;
                }
                Vector3Int tri2 = tris[k];
                included = IncludesEdge(v1, v2, tri2);
            }
            return included;
        }

        // flips the orientation of a triangle if the specified point is not above it
        private Vector3Int CorrectOrientation(List<Vector3> pos, Vector3Int tri, Vector3 point, bool above)
        {
            if (AbovePlane(point, PlaneFromTriangle(pos[tri.x], pos[tri.y], pos[tri.z])) ^ above)
            {
                // point that is supposed to be above was below or point that was supposed to be below was above, flip the triangle
                return new Vector3Int(tri.z, tri.y, tri.x);
            }
            else
            {
                // point is on the correct side already
                return tri;
            }
        }

        // finds 4 vertics that make a big tetrahedron that include many points
        private Vector4Int FindInitTetrahedron(List<Vector3> pos, HashSet<int> outside)
        {
            Vector4Int result = new Vector4Int(0, 0, 0, 0);
            // find maximum length between points in x axis
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            for (int i = 0; i < pos.Count; ++i)
            {
                Vector3 point = pos[i];
                if (point.x < minX)
                {
                    minX = point.x;
                    result.x = i;
                }
                if (point.x > maxX)
                {
                    maxX = point.x;
                    result.y = i;
                }
            }

            // find furthest point from the line
            float maxDist = float.MinValue;
            for (int i = 0; i < pos.Count; ++i)
            {
                float squaredDist = (Vector3.Cross(pos[result.y] - pos[result.x], pos[result.x] - pos[i]).magnitude) / (pos[result.y] - pos[result.x]).magnitude;
                if (squaredDist > maxDist)
                {
                    maxDist = squaredDist;
                    result.z = i;
                }

            }

            // find furthest point from the triangle
            result.w = IndexOfFurthestPoint(pos, outside, pos[result.x], pos[result.y], pos[result.z]);
            if (!AbovePlane(pos[result.w], PlaneFromTriangle(pos[result.x], pos[result.y], pos[result.z])))
            {
                int temp = result.x;
                result.x = result.z;
                result.z = temp;
            }
            return result;
        }

        // returns a plane on the format that AbovePlane expects, given three points that lie in the plane
        private Vector4 PlaneFromTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 cross = Vector3.Cross(v1 - v3, v2 - v3);
            if (cross.sqrMagnitude == 0f)
            {
                print("degenerate triangle at " + v1 + ", " + v2 + ", " + v3);
            }
            return new Vector4(cross.x, cross.y, cross.z, Vector3.Dot(cross, v1));
        }

        // adds a triangle to the convex hull if it should be there
        // the x and y of visibleTri will be connected to indexOfFurthestPoint
        private void AddTriangleToConvexHull(Vector3Int visibleTri, List<Vector3> pos, HashSet<int> unclaimed, List<Vector3Int> hull, List<HashSet<int>> outsides, int indexOfFurthestPoint, List<Vector3Int> visibleTriangles, int j)
        {
            if (!EdgeInAnyTri(visibleTri.x, visibleTri.y, j, visibleTriangles))
            {
                Vector3Int tri = CorrectOrientation(pos, new Vector3Int(visibleTri.x, visibleTri.y, indexOfFurthestPoint), pos[visibleTri.z], false);
                hull.Add(tri);
                outsides.Add(new HashSet<int>());
                Vector4 plane = PlaneFromTriangle(pos[tri.x], pos[tri.y], pos[tri.z]);

                foreach (int k in unclaimed)
                {
                    if (AbovePlane(pos[k], plane))
                    {
                        outsides[outsides.Count - 1].Add(k);
                    }
                }

                foreach (int k in outsides[outsides.Count - 1])
                {
                    unclaimed.Remove(k);
                }
            }
        }

        // checks if a point is inside a polygon
        private bool PointInsidePolygon(Vector2 point, List<Vector2> poly)
        {
            Vector2 poly0 = poly[0];
            Vector2 v2 = point - poly0;

            for (int i = 1; i < poly.Count - 1; ++i)
            {
                Vector2 v0 = poly[i] - poly0;
                Vector2 v1 = poly[i + 1] - poly0;

                // Compute dot products
                float dot00 = Vector2.Dot(v0, v0);
                float dot01 = Vector2.Dot(v0, v1);
                float dot02 = Vector2.Dot(v0, v2);
                float dot11 = Vector2.Dot(v1, v1);
                float dot12 = Vector2.Dot(v1, v2);

                // Compute barycentric coordinates
                float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
                float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                // Check if point is in triangle
                if ((u >= -0.00001f) && (v >= -0.00001f) && (u + v <= 1.00001f))
                {
                    return true;
                }
            }
            return false;
        }

        public Mesh CreateHullMesh(List<Vector3Int> hull, List<Vector3> pos)
        {
            Mesh mesh = new Mesh();
            Vector3[] verts = new Vector3[hull.Count * 3];
            int[] tris = new int[hull.Count * 3];
            int vertIndex = 0;

            for (int i = 0; i < hull.Count; ++i, vertIndex += 3)
            {
                verts[vertIndex] = pos[hull[i].x];
                verts[vertIndex + 1] = pos[hull[i].y];
                verts[vertIndex + 2] = pos[hull[i].z];
                tris[vertIndex] = vertIndex;
                tris[vertIndex + 1] = vertIndex + 1;
                tris[vertIndex + 2] = vertIndex + 2;
            }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public Mesh CreateHullMesh(List<Vector2> pos)
        {
            Mesh mesh = new Mesh();
            Vector3[] verts = new Vector3[pos.Count * 3];
            int[] tris = new int[pos.Count * 3];
            int vertIndex = 0;
            Vector3 firstPos = pos[0];
            for (int i = 0; i < pos.Count; ++i, vertIndex += 3)
            {
                verts[vertIndex] = pos[i];
                verts[vertIndex + 1] = pos[(i + 1) % pos.Count];
                verts[vertIndex + 2] = firstPos;
                tris[vertIndex] = vertIndex;
                tris[vertIndex + 1] = vertIndex + 1;
                tris[vertIndex + 2] = vertIndex + 2;
            }

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
        #endregion

        [ConsoleCommand("convexHullGenerator", aliases: new string[] { "qh", "quickhull" })]
        public void QuickHull(string graphName)
        {
            Graph graph = referenceManager.graphManager.FindGraph(graphName);
            List<Vector3> pos = new List<Vector3>(graph.points.Count);
            foreach (var gp in graph.points.Values)
            {
                pos.Add(gp.Position);
            }
            QuickHull(graph, pos, Color.red, "");
        }

        public void QuickHull(Graph graph, List<Vector3> pos, Color color, string attributeName)
        {
            if (graph is null)
            {
                graph = referenceManager.graphManager.FindGraph("");
            }

            CellexalLog.Log("Started quickhull of " + pos.Count + " points");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            int pointsBefore = pos.Count;
            CellexalLog.Log("Removed " + (pointsBefore - pos.Count) + " outliers");

            GameObject hullGameObject = Instantiate(convexHullPrefab);
            hullGameObject.transform.parent = graph.transform;
            hullGameObject.transform.localPosition = Vector3.zero;
            hullGameObject.transform.localRotation = Quaternion.identity;
            //Destroy(hullDebugGO.GetComponent<SphereCollider>());
            //CreateDebugHull(hull, pos, hullGameObject);
            List<Vector3Int> hull = QuickHull(pos);

            stopwatch.Stop();
            hullGameObject.gameObject.GetComponent<MeshFilter>().mesh = CreateHullMesh(hull, pos);
            color.a = 0.5f;
            hullGameObject.gameObject.GetComponent<MeshRenderer>().material.color = color;
            referenceManager.cellManager.convexHulls[graph.GraphName + "_" + attributeName] = hullGameObject;
            CellexalLog.Log("Finished quickhull in " + stopwatch.Elapsed.ToString());
        }

        public List<Vector3Int> QuickHull(List<Vector3> pos)
        {
            if (pos.Count < 4)
            {
                CellexalLog.Log("Can not create 3D convex hull with less than 4 points");
                return null;
            }
            // list with indices of the points that are still outside the hull
            HashSet<int> unclaimed = new HashSet<int>();
            for (int i = 0; i < pos.Count; ++i)
            {
                unclaimed.Add(i);
            }
            List<Vector3Int> hull = new List<Vector3Int>();

            Vector4Int initTetra = FindInitTetrahedron(pos, unclaimed);

            Vector3Int initTri1 = CorrectOrientation(pos, new Vector3Int(initTetra.x, initTetra.y, initTetra.z), pos[initTetra.w], false);
            Vector3Int initTri2 = CorrectOrientation(pos, new Vector3Int(initTetra.y, initTetra.z, initTetra.w), pos[initTetra.x], false);
            Vector3Int initTri3 = CorrectOrientation(pos, new Vector3Int(initTetra.x, initTetra.y, initTetra.w), pos[initTetra.z], false);
            Vector3Int initTri4 = CorrectOrientation(pos, new Vector3Int(initTetra.x, initTetra.z, initTetra.w), pos[initTetra.y], false);

            hull.Add(initTri1);
            hull.Add(initTri2);
            hull.Add(initTri3);
            hull.Add(initTri4);

            unclaimed.Remove(initTetra.x);
            unclaimed.Remove(initTetra.y);
            unclaimed.Remove(initTetra.z);
            unclaimed.Remove(initTetra.w);


            List<HashSet<int>> outsides = new List<HashSet<int>>() { new HashSet<int>(), new HashSet<int>(), new HashSet<int>(), new HashSet<int>() };

            Vector4[] originalPlanes = new Vector4[] {
                PlaneFromTriangle(pos[initTri1.x], pos[initTri1.y], pos[initTri1.z]),
                PlaneFromTriangle(pos[initTri2.x], pos[initTri2.y], pos[initTri2.z]),
                PlaneFromTriangle(pos[initTri3.x], pos[initTri3.y], pos[initTri3.z]),
                PlaneFromTriangle(pos[initTri4.x], pos[initTri4.y], pos[initTri4.z])
            };

            // let the initial tetrahedron's triangles claim their points
            for (int i = 0; i < 4; ++i)
            {
                HashSet<int> outside_i = outsides[i];
                Vector4 plane_i = originalPlanes[i];

                foreach (int j in unclaimed)
                {
                    if (AbovePlane(pos[j], plane_i))
                    {
                        outside_i.Add(j);
                    }
                }

                foreach (int j in outside_i)
                {
                    unclaimed.Remove(j);
                }
            }

            // rest of the points are inside the tetrahedrons
            unclaimed.Clear();

            // find non-empty outside set
            for (int i = 0; i < outsides.Count; ++i)
            {
                if (outsides[i].Count > 0)
                {

                    int indexOfFurthestPoint = IndexOfFurthestPoint(pos, outsides[i], pos[hull[i].x], pos[hull[i].y], pos[hull[i].z]);
                    Vector3 addedPoint = pos[indexOfFurthestPoint];
                    outsides[i].Remove(indexOfFurthestPoint);
                    // get all visible faces
                    List<Vector3Int> visibleTriangles = new List<Vector3Int>();
                    for (int j = 0; j < hull.Count; ++j)
                    {
                        Vector3Int triOnHull = hull[j];
                        if (AbovePlane(addedPoint, PlaneFromTriangle(pos[triOnHull.x], pos[triOnHull.y], pos[triOnHull.z])))
                        {
                            // remove the face and mark the face's outside points as unclaimed
                            visibleTriangles.Add(triOnHull);
                            foreach (int k in outsides[j])
                            {
                                unclaimed.Add(k);
                            }
                            hull.RemoveAt(j);
                            outsides.RemoveAt(j);
                            j--;
                        }

                    }

                    // add the new triangles to the hull
                    for (int j = 0; j < visibleTriangles.Count; ++j)
                    {
                        Vector3Int tri = visibleTriangles[j];
                        AddTriangleToConvexHull(new Vector3Int(tri.x, tri.y, tri.z), pos, unclaimed, hull, outsides, indexOfFurthestPoint, visibleTriangles, j);
                        AddTriangleToConvexHull(new Vector3Int(tri.y, tri.z, tri.x), pos, unclaimed, hull, outsides, indexOfFurthestPoint, visibleTriangles, j);
                        AddTriangleToConvexHull(new Vector3Int(tri.z, tri.x, tri.y), pos, unclaimed, hull, outsides, indexOfFurthestPoint, visibleTriangles, j);
                    }

                    // start over until there are no non-empty outside sets
                    i = -1;
                    unclaimed.Clear();
                }
            }

            return hull;
        }

        public List<Vector2Int> QuickHull(List<Vector2> pos)
        {
            if (pos.Count < 3)
            {
                CellexalLog.Log("Can not create 2D convex hull with less than 3 points");
                return null;
            }
            HashSet<int> unclaimed = new HashSet<int>();
            for (int i = 0; i < pos.Count; ++i)
            {
                unclaimed.Add(i);
            }
            List<Vector2Int> hull = new List<Vector2Int>();

            // find a large triangle
            Vector3Int initTri = FindInitTriangle(pos);

            hull.Add(CorrectOrientation(pos, new Vector2Int(initTri.x, initTri.y), pos[initTri.z], false));
            hull.Add(CorrectOrientation(pos, new Vector2Int(initTri.y, initTri.z), pos[initTri.x], false));
            hull.Add(CorrectOrientation(pos, new Vector2Int(initTri.z, initTri.x), pos[initTri.y], false));

            unclaimed.Remove(initTri.x);
            unclaimed.Remove(initTri.y);
            unclaimed.Remove(initTri.z);

            List<HashSet<int>> outsides = new List<HashSet<int>>() { new HashSet<int>(), new HashSet<int>(), new HashSet<int>() };

            // let the initial triangle's lines calim their points
            for (int i = 0; i < 3; ++i)
            {
                HashSet<int> outside_i = outsides[i];
                Vector2 pointOnLine1 = pos[hull[i].x];
                Vector2 pointOnLine2 = pos[hull[i].y];

                ClaimPoints(pointOnLine1, pointOnLine2, outside_i, unclaimed, pos);
            }

            // rest of the points are inside the triangle
            unclaimed.Clear();

            // find non-empty outside set
            for (int i = 0; i < outsides.Count; ++i)
            {
                if (outsides[i].Count > 0)
                {

                    int indexOfFurthestPoint = IndexOfFurthestPoint(pos, outsides[i], hull[i]);
                    Vector2 addedPoint = pos[indexOfFurthestPoint];
                    outsides[i].Remove(indexOfFurthestPoint);

                    // get all visible faces
                    List<Vector2Int> visibleLines = new List<Vector2Int>();
                    for (int j = 0; j < hull.Count; ++j)
                    {
                        Vector2Int lineOnHull = hull[j];
                        if (RightOfLine(pos[lineOnHull.x], pos[lineOnHull.y], addedPoint))
                        {
                            // remove the face and mark the face's outside points as unclaimed
                            visibleLines.Add(lineOnHull);
                            foreach (int k in outsides[j])
                            {
                                unclaimed.Add(k);
                            }
                            hull.RemoveAt(j);
                            outsides.RemoveAt(j);
                            j--;
                        }

                    }

                    // find which two vertices we should connect the new lines to
                    HashSet<int> verts = new HashSet<int>();
                    for (int j = 0; j < visibleLines.Count; ++j)
                    {
                        Vector2Int line = visibleLines[j];
                        if (!verts.Contains(line.x))
                        {
                            verts.Add(line.x);
                        }
                        else
                        {
                            verts.Remove(line.x);
                        }

                        if (!verts.Contains(line.y))
                        {
                            verts.Add(line.y);
                        }
                        else
                        {
                            verts.Remove(line.y);
                        }
                    }

                    // add the new lines to the hull
                    int[] newVerts = verts.ToArray();

                    hull.Add(CorrectOrientation(pos, new Vector2Int(newVerts[0], indexOfFurthestPoint), pos[newVerts[1]], false));
                    outsides.Add(new HashSet<int>());
                    ClaimPoints(pos[hull[hull.Count - 1].x], pos[hull[hull.Count - 1].y], outsides[outsides.Count - 1], unclaimed, pos);

                    hull.Add(CorrectOrientation(pos, new Vector2Int(newVerts[1], indexOfFurthestPoint), pos[newVerts[0]], false));
                    outsides.Add(new HashSet<int>());
                    ClaimPoints(pos[hull[hull.Count - 1].x], pos[hull[hull.Count - 1].y], outsides[outsides.Count - 1], unclaimed, pos);

                    // start over until there are no non-empty outside sets
                    i = -1;
                    unclaimed.Clear();
                }
            }
            return hull;
        }

        /*
        public float IntersectingVolume(Mesh mesh1, Mesh mesh2)
        {
            if (!mesh1.bounds.Intersects(mesh2.bounds))
            {
                return 0f;
            }

            float totalVolume = 0f;
            Vector3[] verts1 = mesh1.vertices;
            Vector3[] verts2 = mesh2.vertices;
            int[] tris1 = mesh1.triangles;
            int[] tris2 = mesh2.triangles;

            if (verts1.Length == 0 || verts2.Length == 0)
            {
                return 0f;
            }

            Vector3 firstVert1 = verts1[0];
            Vector3 firstVert2 = verts2[0];

            for (int i = 0; i < tris1.Length; ++i)
            {
                for (int j = 0; j < tris2.Length; ++j)
                {

                }
            }

            return totalVolume;
        }
        */

        public List<Vector2> ProjectTo2D(Mesh mesh, Vector3 dir, Vector3 up)
        {
            List<Vector3> verts = new List<Vector3>(mesh.vertices);
            return ProjectTo2D(verts, dir, up);
        }

        public List<Vector2> ProjectTo2D(List<Vector3> pos, Vector3 dir, Vector3 up)
        {

            List<Vector2> result = new List<Vector2>();
            // create the transformation matrix that turns a 3d coordinate to a 2d coordinate in the (2d) coordinate system defined by the plane
            Vector3 A = Vector3.zero;
            Vector3 B = up;
            Vector3 N = dir;
            Vector3 U = (B - A).normalized;
            Vector3 uN = N.normalized;
            Vector3 V = Vector3.Cross(U, uN);
            Vector3 u = A + U;
            Vector3 v = A + V;
            Vector3 n = A + uN;
            Matrix4x4 S = new Matrix4x4(
                new Vector4(A.x, A.y, A.z, 1f),
                new Vector4(u.x, u.y, u.z, 1f),
                new Vector4(v.x, v.y, v.z, 1f),
                new Vector4(n.x, n.y, n.z, 1f));

            Matrix4x4 S_inv = Matrix4x4.Inverse(S);

            Matrix4x4 transformationMatrix = new Matrix4x4(
                new Vector4(0f, 0f, 0f, 1f),
                new Vector4(1f, 0f, 0f, 1f),
                new Vector4(0f, 1f, 0f, 1f),
                new Vector4(0f, 0f, 1f, 1f)) * S_inv;


            // multiply each point with the transformation matrix
            for (int i = 0; i < pos.Count; ++i)
            {
                Vector3 vert = pos[i];
                Vector3 projected = vert - Vector3.Project(vert, dir);
                Vector3 projectedInPlaneCoords = transformationMatrix.MultiplyPoint(projected);
                result.Add(new Vector2(projectedInPlaneCoords.x, projectedInPlaneCoords.y));
            }

            return result;
        }

        //private void InterpolateDirections(Vector3 start, Vector3 end, Vector3 up, List<Vector3> dirs, List<Vector3> upDirs, int xAngles, int yAngles, int xStart, int yStart)
        private void InterpolateDirections(List<Vector3> dirs, List<Vector3> upDirs)
        {
            int numPoints = 4096; // gives us ~2048 valid points
            float thetaMul = Mathf.PI * (1 + Mathf.Sqrt(5));
            for (int i = 0; i < numPoints; ++i)
            {
                float index = i + 0.5f;
                float phi = Mathf.Acos(1f - 2f * index / numPoints);
                float theta = thetaMul * index;

                float sinPhi = Mathf.Sin(phi);
                float y = Mathf.Sin(theta) * sinPhi;
                if (y < 0f)
                {
                    continue;
                }
                Vector3 dir = new Vector3(Mathf.Cos(theta) * sinPhi, y, Mathf.Cos(phi));
                dirs.Add(dir);

                Vector3 upDir = Vector3.Cross(dir, Vector3.up).normalized;
                if (upDir.sqrMagnitude == 0)
                {
                    upDir = Vector3.forward;
                }

                upDirs.Add(upDir);

            }

        }

        [ConsoleCommand("convexHullGenerator", aliases: new string[] { "coa" })]
        public void CalculateOverlapAll()
        {
            // GASTRULATION
            //Directory.CreateDirectory("2d_gas_tsne_unfiltered");
            //CalculateOverlap2D("2d_tsne", false, "2d_gas_tsne_unfiltered");
            //Directory.CreateDirectory("2d_gas_umap_unfiltered");
            //CalculateOverlap2D("2d_umap", false, "2d_gas_umap_unfiltered");
            //Directory.CreateDirectory("3d_gas_tsne_unfiltered");
            //CalculateOverlap3D("3d_tsne", false, "3d_gas_tsne_unfiltered");
            //Directory.CreateDirectory("3d_gas_umap_unfiltered");
            //CalculateOverlap3D("3d_umap", false, "3d_gas_umap_unfiltered");

            //Directory.CreateDirectory("2d_gas_tsne_filtered");
            //CalculateOverlap2D("2d_tsne", true, "2d_gas_tsne_filtered");
            //Directory.CreateDirectory("2d_gas_umap_filtered");
            //CalculateOverlap2D("2d_umap", true, "2d_gas_umap_filtered");
            //Directory.CreateDirectory("3d_gas_tsne_filtered");
            //CalculateOverlap3D("3d_tsne", true, "3d_gas_tsne_filtered");
            //Directory.CreateDirectory("3d_gas_umap_filtered");
            //CalculateOverlap3D("3d_umap", true, "3d_gas_umap_filtered");

            // MCA
            //Directory.CreateDirectory("2d_mca_tsne_unfiltered");
            //CalculateOverlap2D("TSNE2d", false, "2d_mca_tsne_unfiltered");
            //Directory.CreateDirectory("2d_mca_umap_unfiltered");
            //CalculateOverlap2D("UMAP2d", false, "2d_mca_umap_unfiltered");
            //Directory.CreateDirectory("3d_mca_tsne_unfiltered");
            //CalculateOverlap3D("TSNE3d", false, "3d_mca_tsne_unfiltered");
            //Directory.CreateDirectory("3d_mca_umap_unfiltered");
            //CalculateOverlap3D("UMAP3d", false, "3d_mca_umap_unfiltered");

            //Directory.CreateDirectory("2d_mca_tsne_filtered");
            //CalculateOverlap2D("TSNE2d", true, "2d_mca_tsne_filtered");
            //Directory.CreateDirectory("2d_mca_umap_filtered");
            //CalculateOverlap2D("UMAP2d", true, "2d_mca_umap_filtered");
            //Directory.CreateDirectory("3d_mca_tsne_filtered");
            //CalculateOverlap3D("TSNE3d", true, "3d_mca_tsne_filtered");
            //Directory.CreateDirectory("3d_mca_umap_filtered");
            //CalculateOverlap3D("UMAP3d", true, "3d_mca_umap_filtered");

            // ORGANOGENISES
            Directory.CreateDirectory("2d_org_tsne_unfiltered");
            CalculateOverlap2D("TSNE2d", false, "2d_org_tsne_unfiltered");
            Directory.CreateDirectory("2d_org_umap_unfiltered");
            CalculateOverlap2D("UMAP2d", false, "2d_org_umap_unfiltered");
            Directory.CreateDirectory("3d_org_tsne_unfiltered");
            CalculateOverlap3D("TSNE3d", false, "3d_org_tsne_unfiltered");
            Directory.CreateDirectory("3d_org_umap_unfiltered");
            CalculateOverlap3D("UMAP3d", false, "3d_org_umap_unfiltered");

            Directory.CreateDirectory("2d_org_tsne_filtered");
            CalculateOverlap2D("TSNE2d", true, "2d_org_tsne_filtered");
            Directory.CreateDirectory("2d_org_umap_filtered");
            CalculateOverlap2D("UMAP2d", true, "2d_org_umap_filtered");
            Directory.CreateDirectory("3d_org_tsne_filtered");
            CalculateOverlap3D("TSNE3d", true, "3d_org_tsne_filtered");
            Directory.CreateDirectory("3d_org_umap_filtered");
            CalculateOverlap3D("UMAP3d", true, "3d_org_umap_filtered");
        }

        [ConsoleCommand("convexHullGenerator", aliases: new string[] { "co2d", "calculateoverlap2d" })]
        public void CalculateOverlap2D(string graphName, bool filter, string dirPath)
        {
            CellexalLog.Log("Started calculating overlap");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var graph = referenceManager.graphManager.FindGraph(graphName);
            Dictionary<string, List<Vector2>> pos = new Dictionary<string, List<Vector2>>();
            Dictionary<string, List<Vector2>> hulls = new Dictionary<string, List<Vector2>>();

            GameObject debugParent = Instantiate(new GameObject(), graph.transform);

            string[] attributes = referenceManager.cellManager.AttributesNames.ToArray();
            //string[] attributes = new string[] { "Tissue@Bladder", "Tissue@Embryonic-Mesenchyme" };

            foreach (string attribute in attributes)
            {
                pos[attribute] = new List<Vector2>();
                string lowerAttribute = attribute.ToLower();
                int attributeIndex = attributes.IndexOf(lowerAttribute, (s1, s2) => s1.ToLower() == s2.ToLower());
                Color attributeColor = CellexalConfig.Config.SelectionToolColors[attributeIndex % CellexalConfig.Config.SelectionToolColors.Length];
                attributeColor.a = 0.5f;

                pos.Clear();
                //foreach (CellexalVR.AnalysisLogic.Cell cell in referenceManager.cellManager.GetCells())
                //{
                //    if (cell.Attributes.ContainsKey(lowerAttribute))
                //    {
                //        Vector3 pos3d = graph.points[cell.Label].Position;
                //        pos[attribute].Add(new Vector2(pos3d.x, pos3d.y));
                //    }
                //}

                foreach (KeyValuePair<string, HashSet<Cell>> kvp in ReferenceManager.instance.cellManager.Attributes)
                {
                    foreach (Cell cell in kvp.Value)
                    {
                        pos[kvp.Key].Add(graph.points[cell.Label].Position);
                    }
                }

                if (filter)
                    FilterOutliers(pos[attribute], 3.5f);

                if (pos[attribute].Count < 3)
                {
                    continue;
                }


                List<Vector2Int> projectedHull = QuickHull(pos[attribute]);
                List<Vector2> sortedHull = SortHull(projectedHull, pos[attribute]);
                hulls[attribute] = sortedHull;

                //Mesh projectedMesh = CreateHullMesh(sortedHull);
                //GameObject projectedHullGameObject = Instantiate(convexHullPrefab);
                //projectedHullGameObject.GetComponent<MeshFilter>().mesh = projectedMesh;
                //projectedHullGameObject.GetComponent<MeshRenderer>().material.color = attributeColor;
                //projectedHullGameObject.transform.parent = debugParent.transform;
                //projectedHullGameObject.transform.localPosition = Vector3.zero;
                //projectedHullGameObject.transform.LookAt(graph.transform);
                //projectedHullGameObject.name = "2D hull " + attribute;

            }

            Dictionary<string, StreamWriter> streams = new Dictionary<string, StreamWriter>();
            var keys = hulls.Keys;

            for (int i = 0; i < keys.Count; ++i)
            {
                string attribute1 = keys.ElementAt(i);
                string attributeType = attribute1.Substring(0, attribute1.IndexOf('@'));
                string attributeName1 = attribute1.Substring(attributeType.Length + 1, attribute1.Length - attributeType.Length - 1);

                if (!streams.ContainsKey(attributeType))
                {
                    streams[attributeType] = new StreamWriter(new FileStream(dirPath + "\\overlapping_areas_result_" + attributeType + ".txt", FileMode.Create));
                }

                for (int j = i + 1; j < keys.Count; ++j)
                {
                    string attribute2 = keys.ElementAt(j);
                    // only compare attributes of the same type
                    if (attributeType != attribute2.Substring(0, attribute2.IndexOf('@')))
                    {
                        continue;
                    }

                    string attributeName2 = attribute2.Substring(attributeType.Length + 1, attribute2.Length - attributeType.Length - 1);
                    //StartCoroutine(OverlappingArea(hulls[attribute1], hulls[attribute2], graph.gameObject));

                    List<Vector2> intersection = IntersectionOf(hulls[attribute1], hulls[attribute2], null);
                    float area = AreaOf(intersection);
                    float intersectingOfAttribute1 = area / AreaOf(hulls[attribute1]);
                    float intersectingOfAttribute2 = area / AreaOf(hulls[attribute2]);

                    int attr1PointsInside = 0;
                    int attr2PointsInside = 0;

                    if (intersection.Count != 0)
                    {

                        foreach (var p in pos[attribute1])
                        {
                            if (PointInsidePolygon(p, intersection))
                            {
                                attr1PointsInside++;
                            }
                        }

                        foreach (var p in pos[attribute2])
                        {
                            if (PointInsidePolygon(p, intersection))
                            {
                                attr2PointsInside++;
                            }
                        }
                    }

                    streams[attributeType].WriteLine(attributeName1 + " " + attributeName2
                                                     + " " + attr1PointsInside
                                                     + " " + attr2PointsInside
                                                     + " " + (attr1PointsInside + attr2PointsInside)
                                                     + " " + area
                                                     + " " + intersectingOfAttribute1 + " " + intersectingOfAttribute2
                    );
                    streams[attributeType].Flush();
                }
            }


            foreach (var stream in streams.Values)
            {
                stream.Flush();
                stream.Close();
            }

            stopwatch.Stop();
            CellexalLog.Log("Finished calculating overlap in " + stopwatch.Elapsed.ToString());
            CellexalEvents.CommandFinished.Invoke(true);

        }


        [ConsoleCommand("convexHullGenerator", aliases: new string[] { "co", "calculateoverlap" })]
        public void CalculateOverlap3D(string graphName, bool filter, string dirPath)
        {
            CellexalLog.Log("Started calculating overlap");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            List<Vector3> dirs = new List<Vector3>();
            List<Vector3> upDirs = new List<Vector3>();

            //dirs.Add(new Vector3(-0.8645056f, 0.3613501f, -0.3493653f));
            //upDirs.Add(new Vector3(0.3746825f, 0f, -0.9271532f));
            InterpolateDirections(dirs, upDirs);

            var graph = referenceManager.graphManager.FindGraph(graphName);
            List<Vector3> pos = new List<Vector3>(graph.points.Count);
            Dictionary<string, List<List<Vector2>>> projectedHulls = new Dictionary<string, List<List<Vector2>>>();
            Dictionary<string, List<List<Vector2>>> rawProjections = new Dictionary<string, List<List<Vector2>>>();
            //Dictionary<string, GameObject> projectionsGO = new Dictionary<string, GameObject>();
            Dictionary<string, StreamWriter> streams = new Dictionary<string, StreamWriter>();
            Cell[] cells = referenceManager.cellManager.GetCells();

            // Mesenchymal-Stem-Cell-Cultured Trophoblast-Stem-Cell 0 0 0 0.01117841 0.05680383 -0.8645056 0.3613501 -0.3493653 0.3746825 0 -0.9271532

            //string[] debugAttributes = new string[] { "celltype@Epiblast", "celltype@Primitive.Streak", "celltype@NA", "celltype@ExE.ectoderm" };
            //foreach (string attribute in debugAttributes)

            foreach (string attribute in referenceManager.cellManager.AttributesNames)
            {
                string lowerAttribute = attribute.ToLower();

                pos.Clear();
                //foreach (CellexalVR.AnalysisLogic.Cell cell in cells)
                //{
                //    if (cell.Attributes.ContainsKey(lowerAttribute))
                //    {
                //        pos.Add(graph.points[cell.Label].Position);
                //    }
                //}

                foreach (KeyValuePair<string, HashSet<Cell>> kvp in ReferenceManager.instance.cellManager.Attributes)
                {
                    foreach (Cell cell in kvp.Value)
                    {
                        pos.Add(graph.points[cell.Label].Position);
                    }
                }

                if (filter)
                    FilterOutliers(pos, 5f);

                if (pos.Count < 3)
                {
                    //projections[attribute].Add(new List<Vector2>());
                    continue;
                }

                projectedHulls[attribute] = new List<List<Vector2>>();
                rawProjections[attribute] = new List<List<Vector2>>();

                //int attributeIndex = referenceManager.cellManager.Attributes.IndexOf(attribute, (s1, s2) => s1.ToLower() == s2.ToLower());
                //Color attributeColor = CellexalConfig.Config.SelectionToolColors[attributeIndex % CellexalConfig.Config.SelectionToolColors.Length];
                //List<Vector3Int> hull = QuickHull(pos);
                //Mesh mesh = CreateHullMesh(hull, pos);
                //GameObject hullGameObject = Instantiate(convexHullPrefab);
                //hullGameObject.GetComponent<MeshFilter>().mesh = mesh;
                //hullGameObject.GetComponent<MeshRenderer>().material.color = attributeColor;
                //hullGameObject.transform.parent = graph.transform;
                //hullGameObject.transform.localPosition = Vector3.zero;
                //hullGameObject.transform.localRotation = Quaternion.identity;
                //hullGameObject.name = "3D hull " + attribute;
                //projectionsGO[attribute] = hullGameObject;


                for (int i = 0; i < dirs.Count; ++i)
                {


                    //if (projections.Count > 2)
                    //{
                    //    break;
                    //}



                    List<Vector2> projected = ProjectTo2D(pos, dirs[i], upDirs[i]);
                    rawProjections[attribute].Add(projected);
                    List<Vector2Int> projectedHull = QuickHull(projected);
                    List<Vector2> sortedHull = SortHull(projectedHull, projected);
                    projectedHulls[attribute].Add(sortedHull);
                    //print(sortedHull.Count);

                    //Mesh projectedMesh = CreateHullMesh(sortedHull);
                    //GameObject projectedHullGameObject = Instantiate(convexHullPrefab);
                    //projectedHullGameObject.GetComponent<MeshFilter>().mesh = projectedMesh;
                    //projectedHullGameObject.GetComponent<MeshRenderer>().material.color = attributeColor;
                    //projectedHullGameObject.transform.parent = graph.transform;
                    //projectedHullGameObject.transform.localPosition = Vector3.zero;
                    //projectedHullGameObject.transform.Translate(dirs[i]);
                    //projectedHullGameObject.transform.LookAt(graph.transform);
                    //projectedHullGameObject.name = "2D hull " + attribute + " " + i;
                    //projectionsGO[attribute] = projectedHullGameObject;

                    //GameObject updirGO = Instantiate(convexHullPrefab);
                    //updirGO.transform.parent = graph.transform;
                    //updirGO.transform.localRotation = Quaternion.identity;
                    //updirGO.transform.localPosition = upDirs[i];

                }
            }

            var attributes = projectedHulls.Keys;
            for (int i = 0; i < attributes.Count; ++i)
            {
                string attribute1 = attributes.ElementAt(i);
                string attributeType = attribute1.Substring(0, attribute1.IndexOf('@'));
                string attributeName1 = attribute1.Substring(attributeType.Length + 1, attribute1.Length - attributeType.Length - 1);

                if (!streams.ContainsKey(attributeType))
                {
                    streams[attributeType] = new StreamWriter(new FileStream(dirPath + "\\overlapping_areas_result_" + attributeType + ".txt", FileMode.Create));
                }


                for (int j = i + 1; j < attributes.Count; ++j)
                {
                    string attribute2 = attributes.ElementAt(j);
                    // only compare attributes of the same type
                    if (attributeType != attribute2.Substring(0, attribute2.IndexOf('@')))
                    {
                        continue;
                    }

                    string attributeName2 = attribute2.Substring(attributeType.Length + 1, attribute2.Length - attributeType.Length - 1);
                    int attr1MinPointsInIntersection = int.MaxValue;
                    int attr2MinPointsInIntersection = int.MaxValue;
                    int minPointsInIntersection = int.MaxValue;
                    int minPointsDir = -1;

                    for (int dir = 0; dir < dirs.Count && minPointsInIntersection > 0; ++dir)
                    {
                        //debugSpheres[0].transform.parent = projectionsGO[attribute1][dir].transform;
                        //debugSpheres[1].transform.parent = projectionsGO[attribute1][dir].transform;
                        //debugSpheres[2].transform.parent = projectionsGO[attribute1][dir].transform;
                        //debugSpheres[3].transform.parent = projectionsGO[attribute1][dir].transform;
                        //print(dir + " " + projections[attribute1].Count + " " + projections[attribute2].Count);

                        //StartCoroutine(OverlappingArea(projections[attribute1][dir], projections[attribute2][dir], graph.gameObject));
                        //float area = OverlappingArea(projections[attribute1][dir], projections[attribute2][dir], null);
                        List<Vector2> intersection = IntersectionOf(projectedHulls[attribute1][dir], projectedHulls[attribute2][dir], null);

                        int attr1PointsInside = 0;
                        int attr2PointsInside = 0;
                        if (intersection.Count != 0)
                        {
                            List<Vector2> rawProjection1 = rawProjections[attribute1][dir];
                            List<Vector2> rawProjection2 = rawProjections[attribute2][dir];

                            for (int k = 0; k < rawProjection1.Count && attr1PointsInside < minPointsInIntersection; ++k)
                            {
                                Vector2 p = rawProjection1[k];

                                if (PointInsidePolygon(p, intersection))
                                {
                                    attr1PointsInside++;
                                }
                            }

                            for (int k = 0; k < rawProjection2.Count && attr1PointsInside + attr2PointsInside < minPointsInIntersection; ++k)
                            {
                                Vector2 p = rawProjection2[k];

                                if (PointInsidePolygon(p, intersection))
                                {
                                    attr2PointsInside++;
                                }
                            }
                        }

                        if (attr1PointsInside + attr2PointsInside < minPointsInIntersection)
                        {
                            attr1MinPointsInIntersection = attr1PointsInside;
                            attr2MinPointsInIntersection = attr2PointsInside;
                            minPointsInIntersection = attr1PointsInside + attr2PointsInside;
                            minPointsDir = dir;
                        }
                    }

                    //int attributeIndex = referenceManager.cellManager.Attributes.IndexOf(attribute1, (s1, s2) => s1.ToLower() == s2.ToLower());
                    //Color attributeColor = CellexalConfig.Config.SelectionToolColors[attributeIndex % CellexalConfig.Config.SelectionToolColors.Length];
                    //Mesh projectedMesh = CreateHullMesh(projections[attribute1][minAreaDir]);
                    //GameObject projectedHullGameObject = Instantiate(convexHullPrefab);
                    //projectedHullGameObject.GetComponent<MeshFilter>().mesh = projectedMesh;
                    //projectedHullGameObject.GetComponent<MeshRenderer>().material.color = attributeColor;
                    //projectedHullGameObject.transform.parent = projectionsGO[attribute1].transform;
                    //projectedHullGameObject.transform.localPosition = Vector3.zero;
                    //projectedHullGameObject.transform.Translate(dirs[minAreaDir]);
                    //projectedHullGameObject.transform.LookAt(graph.transform);
                    //projectedHullGameObject.name = "2D hull min area " + attribute1 + " " + minAreaDir;

                    //projectedMesh = CreateHullMesh(projections[attribute2][minAreaDir]);
                    //projectedHullGameObject = Instantiate(convexHullPrefab);
                    //projectedHullGameObject.GetComponent<MeshFilter>().mesh = projectedMesh;
                    //projectedHullGameObject.GetComponent<MeshRenderer>().material.color = attributeColor;
                    //projectedHullGameObject.transform.parent = projectionsGO[attribute2].transform;
                    //projectedHullGameObject.transform.localPosition = Vector3.zero;
                    //projectedHullGameObject.transform.Translate(dirs[minAreaDir]);
                    //projectedHullGameObject.transform.LookAt(graph.transform);
                    //projectedHullGameObject.name = "2D hull min area " + attribute2 + " " + minAreaDir;
                    //OverlappingArea(projections[attribute1][minAreaDir], projections[attribute2][minAreaDir], projectedHullGameObject);

                    if (minPointsDir != -1)
                    {
                        float areaOfAttribute1 = AreaOf(projectedHulls[attribute1][minPointsDir]);
                        float areaOfAttribute2 = AreaOf(projectedHulls[attribute2][minPointsDir]);
                        float minArea = AreaOf(IntersectionOf(projectedHulls[attribute1][minPointsDir], projectedHulls[attribute2][minPointsDir], null));
                        float intersectingOfAttribute1 = minArea / areaOfAttribute1;
                        float intersectingOfAttribute2 = minArea / areaOfAttribute2;
                        Vector3 dir = dirs[minPointsDir];
                        Vector3 upDir = upDirs[minPointsDir];
                        streams[attributeType].WriteLine(attributeName1 + " " + attributeName2
                                                         + " " + attr1MinPointsInIntersection
                                                         + " " + attr2MinPointsInIntersection
                                                         + " " + minPointsInIntersection
                                                         + " " + minArea
                                                         + " " + intersectingOfAttribute1 + " " + intersectingOfAttribute2
                                                         + " " + areaOfAttribute1 + " " + areaOfAttribute2
                                                         + " " + dir.x + " " + dir.y + " " + dir.z
                                                         + " " + upDir.x + " " + upDir.y + " " + upDir.z);
                        streams[attributeType].Flush();
                    }
                }

                //Color attributeColor = CellexalConfig.Config.SelectionToolColors[attributeIndex % CellexalConfig.Config.SelectionToolColors.Length];
                //attributeColor.a = 0.5f;

            }

            CellexalLog.Log("Finished generating convex hulls after " + stopwatch.Elapsed.ToString());

            foreach (var stream in streams.Values)
            {
                stream.Flush();
                stream.Close();
            }

            //StartCoroutine(OverlappingArea(projections[kvp.ElementAt(0)][0], projections[kvp.ElementAt(1)][0]));
            //OverlappingArea(projections[kvp.ElementAt(0)][0], projections[kvp.ElementAt(1)][0]);

            stopwatch.Stop();
            CellexalLog.Log("Finished calculating overlap in " + stopwatch.Elapsed.ToString());
            CellexalEvents.CommandFinished.Invoke(true);
        }

        //private IEnumerator OverlappingArea(List<Vector2> hull1, List<Vector2> hull2, GameObject parent)
        private float OverlappingArea(List<Vector2> hull1, List<Vector2> hull2, GameObject parent)
        {
            return AreaOf(IntersectionOf(hull1, hull2, parent));
        }

        private List<Vector2> IntersectionOf(List<Vector2> hull1, List<Vector2> hull2, GameObject parent)
        {
            MinMaxCoords(hull1, out Vector2 hull1MinCoords, out Vector2 hull1MaxCoords);
            MinMaxCoords(hull2, out Vector2 hull2MinCoords, out Vector2 hull2MaxCoords);

            if (!BoundsIntersect(hull1MinCoords, hull1MaxCoords, hull2MinCoords, hull2MaxCoords))
            {
                return new List<Vector2>();
                //print(0f);
                //yield break;
            }

            List<Vector2> intersection = new List<Vector2>();
            int hull1index = 0;
            int hull2index = 0;
            bool inside1 = false;
            bool inside2 = false;

            while (hull1index < hull1.Count * 2 && hull2index < hull2.Count * 2)
            {
                //yield return null;
                //while (!Input.GetKeyDown(KeyCode.T))
                //{
                //    yield return null;
                //}
                //print(hull1index + " " + hull1.Count * 2 + " " + hull2index + " " + hull2.Count * 2);

                Vector2 line1_1 = hull1[hull1index % hull1.Count];
                Vector2 line1_2 = hull1[(hull1index + 1) % hull1.Count];
                Vector2 line2_1 = hull2[hull2index % hull2.Count];
                Vector2 line2_2 = hull2[(hull2index + 1) % hull2.Count];

                //debugSpheres[0].transform.parent = parent.transform;
                //debugSpheres[1].transform.parent = parent.transform;
                //debugSpheres[2].transform.parent = parent.transform;
                //debugSpheres[3].transform.parent = parent.transform;

                //debugSpheres[0].transform.localPosition = line1_1;
                //debugSpheres[1].transform.localPosition = line1_2;
                //debugSpheres[2].transform.localPosition = line2_1;
                //debugSpheres[3].transform.localPosition = line2_2;

                LineSegementsIntersect(line1_1, line1_2, line2_1, line2_2, out bool lineSegmentsIntersect, out Vector2 intersectPoint);

                if (lineSegmentsIntersect)
                {
                    // prevent vertices being added twice when too close too an edge
                    if (!(intersection.Count > 0 && intersectPoint == intersection[intersection.Count - 1]))
                    {
                        if (intersection.Count > 3 && (intersectPoint == intersection[0]))
                        {
                            break;
                        }
                        intersection.Add(intersectPoint);
                    }
                    // since we are iterating over the shapes counter clock wise, the end point of line 2 being to the right of line 1 means that line 2 is outside hull 1
                    if (RightOfLine(line1_1, line1_2, line2_2))
                    {
                        inside1 = true;
                        inside2 = false;
                    }
                    else
                    {
                        inside2 = true;
                        inside1 = false;
                    }
                }

                bool line2RightOfLine1 = RightOfLine(line1_1, line1_2, line2_2);
                bool anglePositive = Vector2.SignedAngle(line1_2 - line1_1, line2_2 - line2_1) > 0f;
                // a line segment point towards another line segment if:
                // the end point of the other line segment is to the right of the line segment AND the signed angle between them is negative
                // OR the end point of the other line segment is to the left of the line segment AND the signed angle between them is positive
                bool line2PointsTowardsLine1 = line2RightOfLine1 == anglePositive;
                bool line1PointsTowardsLine2 = RightOfLine(line2_1, line2_2, line1_2) != anglePositive;
                //print("line2RightOfLine1 " + line2RightOfLine1 + " line1RightOfLine2 " + RightOfLine(line2_1, line2_2, line1_2) + " anglePositive " + anglePositive + " intersection " + lineSegmentsIntersect + "\nline2PointsTowardsLine1 " + line2PointsTowardsLine1 + " line1PointsTowardsLine2 " + line1PointsTowardsLine2 + " inside1 " + inside1 + " inside2 " + inside2);

                // increment one of the lines and add it to the intersection if it is inside the other hull
                if (line1PointsTowardsLine2 == line2PointsTowardsLine1)
                {
                    if (line2RightOfLine1)
                    {
                        hull2index++;
                        if (inside2)
                        {
                            if (intersection.Count > 3 && (line2_2 == intersection[0]))
                            {
                                break;
                            }
                            intersection.Add(line2_2);
                        }
                    }
                    else
                    {
                        hull1index++;
                        if (inside1)
                        {
                            if (intersection.Count > 3 && (line1_2 == intersection[0]))
                            {
                                break;
                            }
                            intersection.Add(line1_2);
                        }
                    }
                }
                else if (line1PointsTowardsLine2)
                {
                    hull1index++;
                    if (inside1)
                    {
                        if (intersection.Count > 3 && (line1_2 == intersection[0]))
                        {
                            break;
                        }
                        intersection.Add(line1_2);
                    }
                }
                else
                {
                    // line2PointsTowardsLine1 must be true
                    hull2index++;
                    if (inside2)
                    {
                        if (intersection.Count > 3 && (line2_2 == intersection[0]))
                        {
                            break;
                        }
                        intersection.Add(line2_2);
                    }
                }
            }

            if (intersection.Count == 0)
            {

                if (BoundsInsideBounds(hull1MinCoords, hull1MaxCoords, hull2MinCoords, hull2MaxCoords)
                    && PointInsidePolygon(hull1[0], hull2))
                {
                    return hull1;
                }
                else if (BoundsInsideBounds(hull2MinCoords, hull2MaxCoords, hull1MinCoords, hull1MaxCoords)
                         && PointInsidePolygon(hull2[0], hull1))
                {
                    return hull2;
                }
                else
                {
                    return new List<Vector2>();
                }
            }

            //foreach (Vector2 p in intersection)
            //{
            //    print(GraphGenerator.V2S(p));
            //}

            //if (parent != null)
            //{
            //    GameObject debugIntersectionGameObject = Instantiate(convexHullPrefab);
            //    debugIntersectionGameObject.GetComponent<MeshFilter>().mesh = CreateHullMesh(intersection);
            //    debugIntersectionGameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0f, 0.5f);
            //    debugIntersectionGameObject.transform.parent = parent.transform;
            //    debugIntersectionGameObject.transform.localPosition = Vector3.zero;
            //    debugIntersectionGameObject.transform.localRotation = Quaternion.identity;
            //    debugIntersectionGameObject.name = "Intersection";
            //}

            return intersection;
            //print(AreaOf(intersection));
        }

        private void OnDrawGizmos()
        {
            //Vector3 diff = debugSpheres[1].transform.position - debugSpheres[0].transform.position;
            //Gizmos.color = debugSpheres[0].GetComponent<MeshRenderer>().material.color;
            //Gizmos.DrawLine(debugSpheres[0].transform.position + diff * 100f, debugSpheres[1].transform.position - diff * 100f);

            //diff = debugSpheres[3].transform.position - debugSpheres[2].transform.position;
            //Gizmos.color = debugSpheres[2].GetComponent<MeshRenderer>().material.color;
            //Gizmos.DrawLine(debugSpheres[2].transform.position + diff * 100f, debugSpheres[3].transform.position - diff * 100f);
        }

        [ConsoleCommand("convexHullGenerator", aliases: new string[] { "dt", "delaunaytriangulation" })]
        public void DelaunayTriangulation(string graphName)
        {
            Graph graph = referenceManager.graphManager.FindGraph(graphName);

            if (graph is null)
            {
                graph = referenceManager.graphManager.FindGraph("");
            }

            List<Vector3> pos = new List<Vector3>(graph.points.Count);
            foreach (var gp in graph.points.Values)
            {
                pos.Add(gp.Position);
            }
            DelaunayTriangulation(graph, pos, Color.white, "all");
            CellexalEvents.CommandFinished.Invoke(true);
            //StartCoroutine(DelaunayTriangulationCoroutine(graph, pos, Color.white, "all"));
        }

        public void DelaunayTriangulation(Graph graph, List<Vector3> pos, Color color, string attributeName)
        {
            StartCoroutine(DelaunayTriangulationCoroutine(graph, pos, color, attributeName));
        }

        private IEnumerator DelaunayTriangulationCoroutine(Graph graph, List<Vector3> pos, Color color, string attributeName)
        {
            //return;
            CellexalLog.Log("Started delaunay triangulation of " + pos.Count + " graph points");

            System.Diagnostics.Stopwatch stopwatchTotal = new System.Diagnostics.Stopwatch();
            stopwatchTotal.Start();
            int pointsBefore = pos.Count;
            // remove the 5% furthest away points
            FilterOutliers(pos, 5f);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            foreach (Vector3 v in pos)
            {
                if (v.x < minX) minX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.z < minZ) minZ = v.z;
                if (v.x > maxX) maxX = v.x;
                if (v.y > maxY) maxY = v.y;
                if (v.z > maxZ) maxZ = v.z;
            }



            CellexalLog.Log("Removed " + (pointsBefore - pos.Count) + " outliers");

            // delaunay triangulation
            List<Vector4Int> tetras = new List<Vector4Int>();
            List<float> circumRadiusesSqr = new List<float>();
            List<Vector3> circumCenters = new List<Vector3>();
            List<Vector4Int> badTetras = new List<Vector4Int>();
            float maxDist = 99999f;

            // make debug parents
            GameObject empty = new GameObject();
            GameObject lineParent = Instantiate(empty, graph.transform);
            lineParent.name = "Lines";
            GameObject sphereParent = Instantiate(empty, graph.transform);
            sphereParent.name = "Spheres";
            Destroy(empty);

            #region helper_functions
            // helper functions for later
            void AddTetra(Vector4Int v)
            {
                Vector3 sideA = pos[v.y] - pos[v.x];
                Vector3 sideB = pos[v.z] - pos[v.x];
                Vector3 sideC = pos[v.w] - pos[v.x];
                float a = sideA.magnitude; // side 1
                float b = sideB.magnitude; // side 2
                float c = sideC.magnitude; // side 3
                Vector3 circumSphereCenter = pos[v.x] + ((a * a * Vector3.Cross(sideB, sideC) + b * b * Vector3.Cross(sideC, sideA) + c * c * Vector3.Cross(sideA, sideB))
                                                         / (2 * Vector3.Dot(sideA, Vector3.Cross(sideB, sideC))));
                float circumSphereRadius = (pos[v.x] - circumSphereCenter).sqrMagnitude;
                tetras.Add(v);
                circumRadiusesSqr.Add(circumSphereRadius);
                circumCenters.Add(circumSphereCenter);
            }

            // first arg is an index in pos, second arg is an index in circumCenters / circumRadiuses
            bool InsideCircumSphere(int p, int c)
            {
                float dist = (circumCenters[c] - pos[p]).sqrMagnitude;
                return (p < 3 || dist < maxDist) && dist < circumRadiusesSqr[c];
            }
            // returns true if the edge between the two first args is the same as the edge between the last two args
            bool EqualEdge(int x1, int x2, int y1, int y2)
            {
                return x1 == y1 && x2 == y2 || x1 == y2 && x2 == y1;
            }

            // returns true if an edge is shared in another tetrahedron. first two args are the vertices defining the edge, third arg is the other tetrahedron
            bool SharesEdge(int v1, int v2, Vector4Int other)
            {
                return EqualEdge(v1, v2, other.x, other.y) ||
                       EqualEdge(v1, v2, other.x, other.z) ||
                       EqualEdge(v1, v2, other.x, other.w) ||
                       EqualEdge(v1, v2, other.y, other.z) ||
                       EqualEdge(v1, v2, other.y, other.w) ||
                       EqualEdge(v1, v2, other.z, other.w);
            }

            // returns true if a triangle is shared in another tetrahedron, first three args are the vertices of the triangle, fourth arg is the other tetrahedron
            bool SharesTri(int v1, int v2, int v3, Vector4Int other)
            {
                return SharesEdge(v1, v2, other) &&
                       SharesEdge(v1, v3, other) &&
                       SharesEdge(v2, v3, other);

            }

            // returns true if a tetrahedron contains a vertex that was part of the original tetrahedron, i.e. is part of the edge
            bool HasEdgeVert(Vector4Int tetra)
            {
                return tetra.x <= 3 || tetra.y <= 3 || tetra.z <= 3 || tetra.w <= 3;
            }

            bool HasOnlyOneEdgeVert(Vector4Int tetra)
            {
                if (tetra.x <= 3)
                {
                    return tetra.y > 3 && tetra.z > 3 && tetra.w > 3;
                }
                else if (tetra.y <= 3)
                {
                    return tetra.x > 3 && tetra.z > 3 && tetra.w > 3;
                }
                else if (tetra.z <= 3)
                {
                    return tetra.x > 3 && tetra.y > 3 && tetra.w > 3;
                }
                else
                {
                    return tetra.x > 3 && tetra.y > 3 && tetra.z > 3;
                }

            }

            // returns the three indices of a tetra that is not on the edge, assuming that _one_ of the vertices are on the edge and the other three are not
            Vector3Int NonEdgeVertices(Vector4Int tetra)
            {
                if (tetra.x <= 3)
                {
                    return new Vector3Int(tetra.y, tetra.z, tetra.w);
                }
                else if (tetra.y <= 3)
                {
                    return new Vector3Int(tetra.x, tetra.z, tetra.w);
                }
                else if (tetra.z <= 3)
                {
                    return new Vector3Int(tetra.x, tetra.y, tetra.w);
                }
                else
                {
                    return new Vector3Int(tetra.x, tetra.y, tetra.z);
                }
            }

            void DebugPrint(string s)
            {
                //print(s);
                CellexalLog.Log(s);
            }

            void DebugInstantiateObjects()
            {
                // remove old debug objects
                //foreach (Transform t in sphereParent.transform)
                //{
                //    Destroy(t.gameObject);
                //}
                //foreach (Transform t in lineParent.transform)
                //{
                //    Destroy(t.gameObject);
                //}
                //UnityEngine.Random.InitState(0);
                //for (int j = 0; j < tetras.Count; ++j)
                //{
                //    Vector4Int tetra = tetras[j];
                //    GameObject go = Instantiate(triprefab, lineParent.transform);
                //    go.transform.localPosition = Vector3.zero;
                //    LineRenderer lineRenderer = go.GetComponent<LineRenderer>();
                //    Vector3[] scaled = new Vector3[] { pos[tetra.x], pos[tetra.y], pos[tetra.z], pos[tetra.w] };
                //    // move the lines slightly away from eachother
                //    float halfWidth = lineRenderer.startWidth / 2f;
                //    scaled[0] -= (scaled[1] + scaled[2] + scaled[3]).normalized * halfWidth;
                //    scaled[1] -= (scaled[0] + scaled[2] + scaled[3]).normalized * halfWidth;
                //    scaled[2] -= (scaled[0] + scaled[1] + scaled[3]).normalized * halfWidth;
                //    scaled[3] -= (scaled[0] + scaled[1] + scaled[2]).normalized * halfWidth;
                //    lineRenderer.SetPositions(new Vector3[] { scaled[0], scaled[1], scaled[2], scaled[0], scaled[3], scaled[2], scaled[3], scaled[1] });
                //    lineRenderer.startColor = lineRenderer.endColor = UnityEngine.Random.ColorHSV(0, 1, 0.6f, 1, 0.6f, 1, 1, 1);

                //    GameObject sphere = Instantiate(sphereprefab, sphereParent.transform);
                //    sphere.transform.localPosition = circumCenters[j];
                //    float radius = Mathf.Sqrt(circumRadiusesSqr[j]);
                //    sphere.transform.localScale = new Vector3(radius, radius, radius) * 2f;
                //    //print(" radius " + radius);
                //    Material m = new Material(sphere.GetComponent<MeshRenderer>().material);
                //    m.color = UnityEngine.Random.ColorHSV(0, 1, 0.6f, 1, 0.6f, 1, 0.2f, 0.2f);

                //    sphere.GetComponent<MeshRenderer>().material = m;
                //}
            }
            #endregion

            // add a tetrahedron that contains all points
            pos.InsertRange(0, new Vector3[] {
                new Vector3(  0,  -2f,  -1f),
                new Vector3( 3f,   2f,  -1f),
                new Vector3(-3f,   2f,  -1f),
                new Vector3(  0,    0,   3f)
            });
            AddTetra(new Vector4Int(0, 1, 2, 3));

            int debugFrameCount = 0;

            for (int i = 4; i < pos.Count; ++i)
            {
                // find bad tetras
                badTetras.Clear();
                for (int j = 0; j < tetras.Count; ++j)
                {
                    if (InsideCircumSphere(i, j))
                    {
                        badTetras.Add(tetras[j]);
                        tetras.RemoveAt(j);
                        circumCenters.RemoveAt(j);
                        circumRadiusesSqr.RemoveAt(j);
                        j--;
                    }
                }

                // find the boundary of the polygonal hole
                List<int> polygon = new List<int>();
                for (int j = 0; j < badTetras.Count; ++j)
                {
                    bool sharedTrixyz = false;
                    bool sharedTrixyw = false;
                    bool sharedTrixzw = false;
                    bool sharedTriyzw = false;
                    for (int k = 0; k < badTetras.Count; ++k)
                    {
                        if (k == j)
                        {
                            continue;
                        }
                        Vector4Int v1 = badTetras[j];
                        Vector4Int v2 = badTetras[k];
                        // add any triangle that is not shared in any other tetra in badTetras
                        if (!sharedTrixyz)
                        {
                            sharedTrixyz = SharesTri(v1.x, v1.y, v1.z, v2);
                        }
                        if (!sharedTrixyw)
                        {
                            sharedTrixyw = SharesTri(v1.x, v1.y, v1.w, v2);
                        }
                        if (!sharedTrixzw)
                        {
                            sharedTrixzw = SharesTri(v1.x, v1.z, v1.w, v2);
                        }
                        if (!sharedTriyzw)
                        {
                            sharedTriyzw = SharesTri(v1.y, v1.z, v1.w, v2);
                        }

                    }
                    if (!sharedTrixyz)
                    {
                        polygon.Add(badTetras[j].x);
                        polygon.Add(badTetras[j].y);
                        polygon.Add(badTetras[j].z);
                    }
                    if (!sharedTrixyw)
                    {
                        polygon.Add(badTetras[j].x);
                        polygon.Add(badTetras[j].y);
                        polygon.Add(badTetras[j].w);
                    }
                    if (!sharedTrixzw)
                    {
                        polygon.Add(badTetras[j].x);
                        polygon.Add(badTetras[j].z);
                        polygon.Add(badTetras[j].w);
                    }
                    if (!sharedTriyzw)
                    {
                        polygon.Add(badTetras[j].y);
                        polygon.Add(badTetras[j].z);
                        polygon.Add(badTetras[j].w);
                    }
                }

                for (int j = 0; j < polygon.Count; j += 3)
                {
                    Vector4Int newTetra = new Vector4Int(polygon[j], polygon[j + 1], polygon[j + 2], i);
                    if (HasEdgeVert(newTetra))
                    {
                        AddTetra(newTetra);
                    }
                }

                DebugInstantiateObjects();
                yield return null;
                while (debugFrameCount == 0)
                {
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        debugFrameCount = 1;
                    }
                    else if (Input.GetKey(KeyCode.Y))
                    {
                        debugFrameCount = 10;
                    }
                    else if (Input.GetKeyDown(KeyCode.U))
                    {
                        debugFrameCount = 1000;
                    }
                    yield return null;
                }

                debugFrameCount--;

            }

            CellexalLog.Log("Tetrahedrons count after triangulation: " + tetras.Count);

            GameObject convexHullGameObject = Instantiate(convexHullPrefab);
            MeshFilter meshFilter = convexHullGameObject.GetComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            Vector3[] verts = new Vector3[tetras.Count * 3];
            int[] tris = new int[tetras.Count * 3];
            for (int i = 0, tetraIndex = 0; tetraIndex < tetras.Count; i += 3, tetraIndex += 1)
            {
                if (!HasOnlyOneEdgeVert(tetras[tetraIndex]))
                {
                    continue;
                }

                Vector3Int nonEdgeVerts = NonEdgeVertices(tetras[tetraIndex]);
                verts[i] = pos[nonEdgeVerts.x];
                verts[i + 1] = pos[nonEdgeVerts.y];
                verts[i + 2] = pos[nonEdgeVerts.z];
                tris[i] = i;
                tris[i + 1] = i + 1;
                tris[i + 2] = i + 2;
            }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.Optimize();
            CellexalLog.Log("Mesh vertices after optimization: " + mesh.vertexCount);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            convexHullGameObject.name = graph.GraphName + "_" + attributeName;
            convexHullGameObject.transform.position = graph.transform.position;
            convexHullGameObject.transform.rotation = graph.transform.rotation;
            convexHullGameObject.transform.parent = graph.transform;
            color.a = 0.5f;
            convexHullGameObject.gameObject.GetComponent<MeshRenderer>().material.color = color;
            referenceManager.cellManager.convexHulls[graph.GraphName + "_" + attributeName] = convexHullGameObject;

            stopwatchTotal.Stop();
            CellexalLog.Log("Finished delaunay triangulation in " + stopwatchTotal.Elapsed.ToString());
        }

    }

    /// <summary>
    /// Helper struct that defines a vector with 4 integer components
    /// </summary>
    struct Vector4Int
    {
        public int x;
        public int y;
        public int z;
        public int w;

        public Vector4Int(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Vector4Int operator +(Vector4Int v1, Vector4Int v2)
        {
            return new Vector4Int(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }
    }

    /// <summary>
    /// Helper struct that defines a 3 by 3 matrix
    /// </summary>
    class Matrix3x3
    {
        public float m1_1;
        public float m1_2;
        public float m1_3;
        public float m2_1;
        public float m2_2;
        public float m2_3;
        public float m3_1;
        public float m3_2;
        public float m3_3;

        public Matrix3x3(float m1_1, float m1_2, float m1_3, float m2_1, float m2_2, float m2_3, float m3_1, float m3_2, float m3_3)
        {
            this.m1_1 = m1_1;
            this.m1_2 = m1_2;
            this.m1_3 = m1_3;
            this.m2_1 = m2_1;
            this.m2_2 = m2_2;
            this.m2_3 = m2_3;
            this.m3_1 = m3_1;
            this.m3_2 = m3_2;
            this.m3_3 = m3_3;
        }

        public void Invert()
        {
            float det = m1_1 * (m2_2 * m3_3 - m2_3 * m3_2)
                + m1_2 * (m2_1 * m3_3 - m2_3 * m3_1)
                + m1_3 * (m2_1 * m3_2 - m2_2 * m3_1);

            float i1_1 = (m2_2 * m3_3 - m2_3 * m3_2) / det;
            float i1_2 = (m1_3 * m3_2 - m1_2 * m3_3) / det;
            float i1_3 = (m1_2 * m2_3 - m1_3 * m2_2) / det;
            float i2_1 = (m2_3 * m3_1 - m2_1 * m3_3) / det;
            float i2_2 = (m1_1 * m3_3 - m1_3 * m3_1) / det;
            float i2_3 = (m1_3 * m2_1 - m1_1 * m2_3) / det;
            float i3_1 = (m2_1 * m3_2 - m2_2 * m3_1) / det;
            float i3_2 = (m1_2 * m3_1 - m1_1 * m3_2) / det;
            float i3_3 = (m1_1 * m2_2 - m1_2 * m2_1) / det;

            m1_1 = i1_1;
            m1_2 = i1_2;
            m1_3 = i1_3;
            m2_1 = i2_1;
            m2_2 = i2_2;
            m2_3 = i2_3;
            m3_1 = i3_1;
            m3_2 = i3_2;
            m3_3 = i3_3;
        }

        public static Vector3 operator *(Matrix3x3 m, Vector3 v)
        {
            return new Vector3(
                m.m1_1 * v.x + m.m1_2 * v.y + m.m1_3 * v.z,
                m.m2_1 * v.x + m.m2_2 * v.y + m.m2_3 * v.z,
                m.m3_1 * v.x + m.m3_2 * v.y + m.m3_3 * v.z
                );
        }
    }
}
