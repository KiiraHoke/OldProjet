using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Highway : MonoBehaviour
{
    // CURVES
    public Color couleur = Color.blue;
    public int nodeCount = 0;
    public List<GameObject> nodes = new List<GameObject>();
    public Vector3[] positions = new List<Vector3>().ToArray();

    public Highway sideRoad;
    public Vector3[] otherSideOfRoadNodes;



    void OnDrawGizmos()
    {
        // Modification du nombre de nodes
        ModifyNodes();

        // Update des positions
        NodesToArrayOfVector3();

        if (nodes.Count >= 4)
        {
            DrawPathHelper(positions, couleur, "gizmos");

            if (sideRoad != null)   // DRAW de l'orientation
            {
                Gizmos.color = couleur;
                List<Vector3> OtherSideNodes = new List<Vector3>();
                for (float i = 0.0f; i < 1.0f; i += 0.0025f)
                {
                    Vector3 sideNode = Highway.PointOnPath(sideRoad.nodes.ToArray(), i);
                    Vector3 roadNode = Highway.PointOnPath(nodes.ToArray(), i);
                    Vector3 newNode = (roadNode - sideNode) + roadNode;
                    OtherSideNodes.Add(newNode);

                    Gizmos.DrawLine(sideNode, newNode);
                }
                otherSideOfRoadNodes = OtherSideNodes.ToArray();

                DrawPathHelper(otherSideOfRoadNodes, couleur, "gizmos");     
            }
        }
    }

    public void ResetNodes(List<GameObject> newNodes)
    {
        nodes = newNodes;
        nodeCount = newNodes.Count;
    }

    #region PATH
    #region Autogénération
    void ModifyNodes()   //// TEST -> A AMELIORER
    {
        if (nodeCount > nodes.Count)
        {
            int difference = nodeCount - nodes.Count;

            for (int i = 0; i < difference; i++)
            {
                if (nodes.Count > 0)
                    nodes.Add(GenerateNode(nodes[nodes.Count - 1].transform.position, nodes.Count));
                else
                    nodes.Add(GenerateNode(this.transform.position, nodes.Count));
            }
        }
        else if (nodeCount < nodes.Count)
        {
            int difference = nodes.Count - nodeCount;

            for (int i = 0; i < difference; i++)
            {
                GameObject go = nodes[nodes.Count - 1];
                nodes.RemoveAt(nodes.Count - 1);
                GameObject.DestroyImmediate(go);
            }
        }
    }

    GameObject GenerateNode(Vector3 tPosition, int numero)
    {
        GameObject node = new GameObject();
        node.transform.position = tPosition;
        node.transform.parent = this.transform; // Make child

        node.name = numero.ToString();

        return node;
    }

    void NodesToArrayOfVector3()
    {
        List<Vector3> ligne = new List<Vector3>();

        for (int i = 0; i < nodes.Count; i++)
        {
            ligne.Add(nodes[i].transform.position);
        }

        positions = ligne.ToArray();
    }
    #endregion

    #region Courbe
    private /*static*/ void DrawLineHelper(Vector3[] line, Color color, string method)
    {
        Gizmos.color = color;
        for (int i = 0; i < line.Length - 1; i++)
        {
            if (method == "gizmos")
            {
                Gizmos.DrawLine(line[i], line[i + 1]); ;
            }
            else if (method == "handles")
            {
                Debug.LogError("iTween Error: Drawing a line with Handles is temporarily disabled because of compatability issues with Unity 2.6!");
                //UnityEditor.Handles.DrawLine(line[i], line[i+1]);
            }
        }
    }

    private void DrawPathHelper(Vector3[] path, Color color, string method)
    {
        Vector3[] vector3s = PathControlPointGenerator(path);

        //Line Draw:
        Vector3 prevPt = Interp(vector3s, 0);
        Gizmos.color = color;
        int SmoothAmount = path.Length * 20;
        for (int i = 1; i <= SmoothAmount; i++)
        {
            float pm = (float)i / SmoothAmount;
            Vector3 currPt = Interp(vector3s, pm);
            Gizmos.DrawLine(currPt, prevPt);
            prevPt = currPt;
        }
    }

    private static Vector3[] PathControlPointGenerator(Vector3[] path)
    {
        Vector3[] suppliedPath;
        Vector3[] vector3s;

        //create and store path points:
        suppliedPath = path;

        if (path.Length >= 4)    // ERAILE
        {
            //populate calculate path;
            int offset = 2;
            vector3s = new Vector3[suppliedPath.Length + offset];
            System.Array.Copy(suppliedPath, 0, vector3s, 1, suppliedPath.Length);

            //populate start and end control points:
            //vector3s[0] = vector3s[1] - vector3s[2];
            vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
            vector3s[vector3s.Length - 1] = vector3s[vector3s.Length - 2] + (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 3]);

            //is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
            if (vector3s[1] == vector3s[vector3s.Length - 2])
            {
                Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
                System.Array.Copy(vector3s, tmpLoopSpline, vector3s.Length);
                tmpLoopSpline[0] = tmpLoopSpline[tmpLoopSpline.Length - 3];
                tmpLoopSpline[tmpLoopSpline.Length - 1] = tmpLoopSpline[2];
                vector3s = new Vector3[tmpLoopSpline.Length];
                System.Array.Copy(tmpLoopSpline, vector3s, tmpLoopSpline.Length);
            }

            return (vector3s);
        }

        return null;
    }

    private static Vector3 Interp(Vector3[] pts, float t)
    {
        if (pts != null && pts.Length >= 4)        // ERAILE
        {
            int numSections = pts.Length - 3;
            int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
            float u = t * (float)numSections - (float)currPt;

            Vector3 a = pts[currPt];
            Vector3 b = pts[currPt + 1];
            Vector3 c = pts[currPt + 2];
            Vector3 d = pts[currPt + 3];

            return .5f * (
                (-a + 3f * b - 3f * c + d) * (u * u * u)
                + (2f * a - 5f * b + 4f * c - d) * (u * u)
                + (-a + c) * u
                + 2f * b
            );
        }

        return Vector3.zero;
    }

    public void PutOnPath(GameObject target, Vector3[] path, float percent)
    {
        target.transform.position = Interp(PathControlPointGenerator(path), percent);
    }

    public static Vector3 PointOnPath(Vector3[] path, float percent)
    {
        return (Interp(PathControlPointGenerator(path), percent));
    }

    public static Vector3 PointOnPath(GameObject[] path, float percent)
    {
        List<Vector3> nodes = new List<Vector3>();
        for (int i = 0; i < path.Length; i++)
            nodes.Add(path[i].transform.position);

        return (Interp(PathControlPointGenerator(nodes.ToArray()), percent));
    }

    public static float PathLength(Vector3[] path)
    {
        float pathLength = 0;

        Vector3[] vector3s = PathControlPointGenerator(path);

        //Line Draw:
        Vector3 prevPt = Interp(vector3s, 0);
        int SmoothAmount = path.Length * 20;
        for (int i = 1; i <= SmoothAmount; i++)
        {
            float pm = (float)i / SmoothAmount;
            Vector3 currPt = Interp(vector3s, pm);
            pathLength += Vector3.Distance(prevPt, currPt);
            prevPt = currPt;
        }

        return pathLength;
    }

    public static float PathLength(GameObject[] path)
    {
        float pathLength = 0;
        
            List<Vector3> nodes = new List<Vector3>();
            for (int i = 0; i < path.Length; i++)
                nodes.Add(path[i].transform.position);

        Vector3[] vector3s = PathControlPointGenerator(nodes.ToArray());

        //Line Draw:
        Vector3 prevPt = Interp(vector3s, 0);
        int SmoothAmount = path.Length * 20;
        for (int i = 1; i <= SmoothAmount; i++)
        {
            float pm = (float)i / SmoothAmount;
            Vector3 currPt = Interp(vector3s, pm);
            pathLength += Vector3.Distance(prevPt, currPt);
            prevPt = currPt;
        }

        return pathLength;
    }
    #endregion
    #endregion
}
