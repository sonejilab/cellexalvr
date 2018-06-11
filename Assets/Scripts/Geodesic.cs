using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Geodesic : MonoBehaviour
{
    public GameObject linePrefab;
    public ReferenceManager referenceManager;

    private LayerMask layerMask;
    private Dictionary<GraphPoint, Node> nodes;

    private void Start()
    {
        CellexalEvents.GraphsLoaded.AddListener(DrawPath);
    }

    private void DrawPath()
    {
        var graph = referenceManager.graphManager.FindGraph("DDRtree");
        Init(graph);

        var path = AStar(graph.points["HSPC_001"], graph.points["Prog_456"]);

        print("path:");
        foreach (var node in path)
            print(node.thisPoint.label);
    }

    public void Init(Graph graph)
    {
        layerMask = LayerMask.NameToLayer("GraphPointLayer");
        nodes = new Dictionary<GraphPoint, Node>();

        foreach (var point in graph.points.Values)
        {
            var newNode = new Node(point);
            nodes[point] = newNode;
        }

        foreach (var point in graph.points.Values)
        {
            var radius = 0.05f;
            var closeGraphPoints = Physics.OverlapSphere(point.transform.position, radius, ~layerMask, QueryTriggerInteraction.Collide);
            while (closeGraphPoints.Length < 5)
            {
                radius += 0.03f;
                closeGraphPoints = Physics.OverlapSphere(point.transform.position, radius, ~layerMask, QueryTriggerInteraction.Collide);
            }
            foreach (var closePoint in closeGraphPoints)
            {
                var graphPoint = closePoint.gameObject.GetComponent<GraphPoint>();
                if (graphPoint != null)
                {
                    nodes[point].neighbours.Add(nodes[graphPoint]);
                }
            }
        }
    }

    private class Node
    {
        public GraphPoint thisPoint;
        public List<Node> neighbours;
        public Vector3 pos;
        public Node(GraphPoint thisPoint)
        {
            this.thisPoint = thisPoint;
            pos = thisPoint.transform.position;
            neighbours = new List<Node>();
        }
    }

    private List<Node> AStar(GraphPoint from, GraphPoint to)
    {
        var start = nodes[from];
        var goal = nodes[to];
        var closedSet = new HashSet<Node>();
        var openSet = new HashSet<Node>();
        openSet.Add(start);
        var cameFrom = new Dictionary<Node, Node>();

        var gScore = new Dictionary<Node, float>();
        gScore[start] = 0;
        var fScore = new Dictionary<Node, float>();
        fScore[start] = Vector3.Distance(start.pos, goal.pos);

        foreach (var node in nodes.Values)
        {
            gScore[node] = float.PositiveInfinity;
            fScore[node] = float.PositiveInfinity;
        }

        while (openSet.Count > 0)
        {
            // get the node in openset with the lowest fscore
            Node current = openSet.First();
            foreach (var node in openSet)
            {
                if (fScore[node] < fScore[current])
                {
                    current = node;
                }
            }
            print("eval " + current.thisPoint.label);

            if (ReferenceEquals(current, goal))
            {
                return Path(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbour in current.neighbours)
            {
                // ignore already evaluated nodes
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                // discover new node
                if (!openSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);
                }

                // tentativeGScore is the distance from the start to this neighbour
                var tentativeGScore = gScore[current] + Vector3.Distance(current.pos, neighbour.pos);
                if (tentativeGScore >= gScore[neighbour])
                {
                    continue;
                }

                cameFrom[neighbour] = current;
                gScore[neighbour] = tentativeGScore;
                fScore[neighbour] = gScore[neighbour] + Vector3.Distance(neighbour.pos, goal.pos);
            }
        }

        return null;
    }

    private List<Node> Path(Dictionary<Node, Node> cameFrom, Node current)
    {
        var path = new List<Node>();
        path.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        return path;
    }

    private float CostOfPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        float cost = 0f;
        while (cameFrom.ContainsKey(current))
        {
            cost += Vector3.Distance(current.pos, cameFrom[current].pos);
            current = cameFrom[current];
        }
        return cost;
    }
}

