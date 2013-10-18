using UnityEngine;
using System.Collections;

public class GestionCamera : MonoBehaviour {
    public Camera camera;
    private FollowRoad followRoad;
    public Vector3 originalRelativePos, originalRelativeRot;
    public Vector3 lastPos, lastRot;

	// Use this for initialization
	void Start () {
        followRoad = gameObject.GetComponent<FollowRoad>();
        // Relative
        originalRelativePos = camera.transform.localPosition;
        originalRelativeRot = camera.transform.localRotation.eulerAngles;
	}
	



	// Update is called once per frame
	void Update () {
        if (followRoad != null)
        {
            if (!followRoad.isOut)
            {
                lastPos = camera.transform.position;
                lastRot = camera.transform.rotation.eulerAngles;

                // Replacement de la caméra ---> FIX --> trop de call
                camera.transform.localPosition = originalRelativePos;
                camera.transform.localRotation = Quaternion.Euler(originalRelativeRot);
            }
            else
            {
                camera.transform.position = lastPos;
                camera.transform.rotation = Quaternion.Euler(lastRot);
            }
        }
	}



    /// Appelé par FollowRoad.cs
    public void Unblock()
    {
        camera.transform.position = originalRelativePos;
        camera.transform.rotation = Quaternion.Euler(originalRelativeRot);
    }
}
