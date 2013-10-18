using UnityEngine;
using System.Collections;

public class FollowRoad : MonoBehaviour {
    ///  Forces
    private Forces forces;
    //// Variables de FollowRoad.cs
    public float speed = 0.0f, minSpeed, maxSpeed;
    public float percentageSpeed, percentage = 0.0f, percentageViewOffset = 0.01f;   // Percentage speed est calculée en fonction de speed
    public Highway road;
    /// Sortie de la piste
    public bool isOut = false;
    /// Position horizontale sur le circuit
    public float positionHorizontale = 0.0f;   
    /// Orientation des Vector
    public Vector3 vecOldGaucheCentre, vecNewGaucheCentre, vecCentres, UpCenter, UpVector;








	
    // Use this for initialization
	void Start () {
        // Initialisation
        this.transform.position = Highway.PointOnPath(road.nodes.ToArray(), percentage);
        transform.forward = Highway.PointOnPath(road.nodes.ToArray(), percentage + percentageViewOffset) - transform.position;
        // Forces
        forces = GetComponent<Forces>();
	}
	



	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOut)
            {
                isOut = false;
                percentage = 0.0f;
                positionHorizontale = 0.0f;
                speed = minSpeed;

                Forces forces = GetComponent<Forces>();
                if (forces != null)
                    forces.StopImpulse();
            }
            else
            {
                isOut = true;
                Forces forces = GetComponent<Forces>();
                if (forces != null)
                    forces.AddImpulse();

                GestionCamera cameraManager = GetComponent<GestionCamera>();
                if (cameraManager != null)
                    cameraManager.Unblock();
            }
        }




        
        
        if (!isOut)
        {
            // Sauvegarde pour ce tour
            Vector3 oldPos = this.transform.position;


            if (Input.GetKey(KeyCode.UpArrow))          /// TEST --> Changer pour du Input.GetAxis("Vertical")
            {
                speed += 1.2f * Time.deltaTime;
                if (speed > maxSpeed)
                    speed = maxSpeed;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                speed -= 2.0f * Time.deltaTime;
                if (speed < minSpeed)
                    speed = minSpeed;
            }
            else if (speed > minSpeed)
            {
                speed -= 0.2f * Time.deltaTime;
                if (speed < minSpeed)
                    speed = minSpeed;
            }


            /// On recalcule la vitesse
            setSpeed(speed);

            // Anciennes positions
            Vector3 gauche0 = Highway.PointOnPath(road.sideRoad.nodes.ToArray(), percentage);
            Vector3 centre0 = Highway.PointOnPath(road.nodes.ToArray(), percentage);
            // Mise à jour du pourcentage
                    /// percentage = (percentage + percentageSpeed) % 1.0f;
            percentage = (percentage + percentageSpeed);
            // Nouvelles positions
            Vector3 gauche1 = Highway.PointOnPath(road.sideRoad.nodes.ToArray(), percentage);
            Vector3 centre1 = Highway.PointOnPath(road.nodes.ToArray(), percentage);

            // Calcul des Vecteurs
            vecOldGaucheCentre = centre0 - gauche0;
            vecNewGaucheCentre = centre1 - gauche1;
            vecCentres = centre1 - centre0;

            // Mise à jour
            this.transform.position = centre1;

            if (vecCentres.magnitude > 0.0f)
            {
                Vector3 upVector = Vector3.Cross(vecNewGaucheCentre.normalized, vecCentres.normalized);
                UpCenter = centre1;
                UpVector = upVector;
                transform.rotation = Quaternion.LookRotation(centre1 - centre0, UpVector);
            }

            /// Position horizontale
            positionHorizontale += Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            transform.position = transform.position - positionHorizontale * vecNewGaucheCentre.normalized;
            /// Ajout de la force centrifuge
            if(forces != null)
                positionHorizontale -= forces.centripedalForce * Time.deltaTime;

            /// Sortie de piste
            if (Mathf.Abs(positionHorizontale) > Mathf.Abs(vecNewGaucheCentre.magnitude))
            {
                isOut = true;   // Sortie de piste
                
                if (forces != null)
                    forces.AddImpulse();    // Appel de l'impulsion sensée simuler la continuité de l'envolée
            }
        }
	}


    public void OnDrawGizmos()
    {
        Gizmos.DrawLine(UpCenter, UpCenter + transform.forward.normalized);
        Gizmos.DrawLine(UpCenter, UpCenter + UpVector);
    }


    // ERAILE
    public void setSpeed(float kmHour)
    {
        float speedKmH_current = kmHour;
        float speedMpS = kmHour * 0.277778f;

        if (road != null)
            percentageSpeed = (speedMpS / Highway.PathLength(road.nodes.ToArray()));
        else
            percentageSpeed = 0.0f;
    }
}