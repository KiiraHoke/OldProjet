using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class MeshCourbure : MonoBehaviour
{
    // SIDES
    public bool regeneratesSides;
    public Curve[] sides;
    // MESH
    public GameObject meshCylinder;
    public GameObject mesh;
    public Vector3 meshFactor = new Vector3(1, 1, 1);
    // CIRCLE
    public float circleWidth = 3.0f;
    public float circleAngle = 0.0f;

    void OnDrawGizmos()
    {
        /*
        if (mesh == null)
        {
            regeneratesSides = false;
            DrawPathMesh(null, Color.blue, "gizmos");
        }
         */

        if (mesh == null)
        {
            // MESH
            mesh = new GameObject();
            mesh.name = "MESH";
            mesh.transform.parent = this.transform;
            // Variables pour la courbure
            List<Vector3> courbure = new List<Vector3>();
            float f = 0.0f;
            // Boucle de génération
            while (f < 1.0f)
            {
                // Génération du Path
                courbure = new List<Vector3>();
                for (int j = 0; j < sides.Length; j++)
                    courbure.Add(sides[j].PointOnPath(sides[j].positions, f));
                Vector3[] vector3s = PathControlPointGenerator(courbure.ToArray());


                //Line Draw:
                    //DrawPathHelper(courbure.ToArray(), Color.red, "gizmos");            
                DrawPathMesh(courbure.ToArray(), mesh, f);

                f += 0.001f;
            }
        }
        
    }


    #region CIRCLE
    public static Vector3 PointOnCircle(float radius, float angleInDegrees, Vector3 origin)
    {
        // Convert from degrees to radians via multiplication by PI/180        
        float x = (float)(radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F)) + origin.x;
        float y = (float)(radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180F)) + origin.y;
        //float z = (float)(radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F)) + origin.z;
        return new Vector3(x, y, origin.z);
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

    private void DrawPathMesh(Vector3[] path, GameObject parent, float percent)
    {
        GameObject cylinder;
        GameObject newMesh;
        // Destroy du mesh précédent
            //if (mesh != null)
            //    DestroyImmediate(mesh);
        newMesh = new GameObject();
        newMesh.name = percent.ToString();  // Changer le nom
        newMesh.transform.parent = parent.transform;
        newMesh.transform.localPosition = new Vector3(0, 0, 0);

        // Génération du Path
        Vector3[] vector3s = PathControlPointGenerator(path);

        //Line Draw:
        Vector3 prevPt = Interp(vector3s, 0);
        int SmoothAmount = path.Length *5;
        for (int i = 1; i <= SmoothAmount; i++)
        {
            float pm = (float)i / SmoothAmount;
            Vector3 currPt = Interp(vector3s, pm);
            ///////////////////////////////
            // -- Draw du mesh
            float width = 1.0f;
            Vector3 offset = currPt - prevPt;
            Vector3 scale = new Vector3(width * meshFactor.x, (offset.magnitude / 2.0f) * meshFactor.y, width * meshFactor.z);
            Vector3 pos = prevPt + (offset / 2.0f);
            // Instantiation du mesh repère
            cylinder = Instantiate(meshCylinder, pos, Quaternion.identity) as GameObject;
            // Placement & orientation
            cylinder.transform.up = offset;
            // Redimensionnement
            cylinder.transform.localScale = scale;
            // Placement dans le GameObject parent
            cylinder.transform.parent = newMesh.transform;
            // Renommage
            cylinder.name = (i - 1).ToString();
            ///////////////////////////////
            // -- Draw du mesh
            prevPt = currPt;
        }

        newMesh.transform.parent = parent.transform;        
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

    private Vector3 Interp(Vector3[] pts, float t)
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

    public Vector3 PointOnPath(Vector3[] path, float percent)
    {
        return (Interp(PathControlPointGenerator(path), percent));
    }

    public float PathLength(Vector3[] path)
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
    #endregion
}
