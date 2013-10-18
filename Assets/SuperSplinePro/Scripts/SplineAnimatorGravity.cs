using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//This class applies gravity towards a spline to rigidbodies that this script is attached to
public class SplineAnimatorGravity : MonoBehaviour
{
	public Spline spline;
	
	public float gravityConstant = 9.81f;
	public int iterations = 5;
	
	void FixedUpdate( ) 
	{
		if( rigidbody == null || spline == null )
			return;
		
		Vector3 force = spline.GetShortestConnection( rigidbody.position, iterations );
		
		//Calculate gravity force according to Newton's law of universal gravity
		rigidbody.AddForce( force * ( Mathf.Pow( force.magnitude, -3 ) * gravityConstant * rigidbody.mass) );
	}
}
