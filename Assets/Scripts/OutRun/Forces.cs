using UnityEngine;
using System.Collections;

public class Forces : MonoBehaviour
{
    #region  Variables
        private Vector3 oldPosition;
        private Vector3 forcesExpulsion;
   
        ///  TEST    
        private Highway road;
        Vector3 centripedalVector;
        Vector3 oldRoadCenter, newRoadCenter;
        /// Poussée centrifuge
        Vector3 transposition;
        float transpositionAngle, transpositionSens, transpositionAngleSens;    
        /// Sa force
        public float centripedalForce, centripedalFactor = 2.0f;
    #endregion





    // Use this for initialization
	void Start () {
        oldPosition = transform.position;

        /// TEST
        FollowRoad infos = gameObject.GetComponent<FollowRoad>();
        if (infos != null)
        {
            road = infos.road;
            oldRoadCenter = Highway.PointOnPath(road.nodes.ToArray(), infos.percentage);
        }
	}
	
	// Update is called once per frame
	void Update () {
        /// Force "d'expulsion" (de poursuite)
        forcesExpulsion = transform.position - oldPosition;
        oldPosition = transform.position;

        /// Calcul des Forces à appliquer
        FollowRoad follow = GetComponent<FollowRoad>();
        if (follow != null)
        {
            /// Calcul de la force d'expulsion de la piste lorsqu'on en sort pour poursuivre le mouvement (& le simuler)
            forcesExpulsion *= follow.speed * 3.0f;


            /// Le vecteur utilisé pour le calcul de la force centrifuge sera le vecteur
            newRoadCenter = Highway.PointOnPath(road.nodes.ToArray(), follow.percentage + follow.percentageViewOffset);
            centripedalVector = newRoadCenter - oldRoadCenter;
            oldRoadCenter = newRoadCenter; 
        
            /// Transposition pour simuler la force centrifuge
            Vector3 transpoX = Vector3.Project(centripedalVector, follow.vecNewGaucheCentre);
            Vector3 transpoZ = Vector3.Project(centripedalVector, follow.vecCentres);
            /// Calcul de la transposition qui permet de calculer l'angle
            transposition = transpoX + transpoZ;

            /// A partir de cet angle, on peut définir de combien on doit simuler la force centrifuge
            transpositionAngle = Vector3.Angle(transposition, follow.vecCentres);

            transpositionAngleSens = Mathf.Round(Vector3.Angle(transpoX, follow.vecOldGaucheCentre));
            if ((transpositionAngleSens % 360) == 0)
            {
                transpositionSens = -1.0f;
            }
            else if ((transpositionAngleSens % 360) == 180)
            {
                transpositionSens = 1.0f;
            }
            // transpositionMagnitude = transposition.magnitude;
            // transpositionMagnitude *= transpositionSens;


            /// Définition de la force centrifuge
            centripedalForce = transposition.magnitude * transpositionAngle * transpositionSens;
            centripedalForce /= (centripedalFactor * follow.maxSpeed);
        }
	}


    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + transposition);
    }



    /// <summary>
    ///  Fonctions
    /// </summary>
    public void AddImpulse()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            //rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.AddForce(forcesExpulsion, ForceMode.Impulse);

            // Remove de la force centrifuge
            centripedalForce = 0.0f;
        }
    }

    public void StopImpulse()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Destroy(rb);
        }
    }
}








/*
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + forcesExpulsion);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + centripedalForce);


        /// TEST
        FollowRoad follow = GetComponent<FollowRoad>();
        if (follow != null)
        {
            Vector3 result = Vector3.Project(centripedalForce, follow.vecNewGaucheCentre);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + follow.vecNewGaucheCentre);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + result);

            transposition = result.normalized;
            transpositionMagnitude = result.magnitude;
            transpositionSens = Vector3.Angle(transposition, follow.vecNewGaucheCentre);
        }
        */
