﻿using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour {
	//Wheel Colliders
	public WheelCollider wheelFL;//front left
	public WheelCollider wheelFR;//front right
	public WheelCollider wheelRL;//rear left
	public WheelCollider wheelRR;//rear right
	
	//Transforms
	public Transform wheelFLTrans;//front left
	public Transform wheelFRTrans;//front right
	public Transform wheelRLTrans;//rear left
	public Transform wheelRRTrans;//rear right
	
	//Skidding variables
	private float slipSidewayFriction;
	private float slipForwardFriction;
	
	//torque
	private float maxTorque;
	
	//deceleration by itself
	public int deceleration;
	
	//max speed
	public int maxSpeed;
	
	//the MAGIC VALUE
	private float magicValue = 0.05f;//controls the time it takes for car to recover from drift:low->longer, high->faster

	public Transform forceUp;
	
	/*
	 * TODO
	 * When handbrake is pressed cars rear wheels should stop spinning
	 * remote control missile
	 * bullet that changes other players car controls temporarily
	 */
	
	// Use this for initialization
	void Start () {
		rigidbody.centerOfMass = new Vector3 (0.0f, -0.9f, 0.0f);
		//print(rigidbody.centerOfMass);
		slipForwardFriction = 0.05f;
		slipSidewayFriction = 0.018f;
		
	}
	
	void Update(){
		
		//make wheels spin
		wheelFLTrans.Rotate (0,0,wheelFL.rpm / 60 * 360 * Time.deltaTime);
		wheelFRTrans.Rotate (0,0,wheelFL.rpm / 60 * 360 * Time.deltaTime);
		wheelRLTrans.Rotate (0,0,wheelFL.rpm / 60 * 360 * Time.deltaTime);
		wheelRRTrans.Rotate (0, 0, wheelFL.rpm / 60 * 360 * Time.deltaTime);
		
		//wheel rotate
		wheelFLTrans.localEulerAngles = new Vector3 (wheelFLTrans.localEulerAngles.x, wheelFL.steerAngle, wheelFLTrans.localEulerAngles.z);//changing just y and leaving alone x & z
		wheelFRTrans.localEulerAngles = new Vector3 (wheelFRTrans.localEulerAngles.x, wheelFR.steerAngle, wheelFRTrans.localEulerAngles.z);
		
		//print (rigidbody.velocity.magnitude);
	}
	
	
	void WheelPosition(){
		RaycastHit hit;
		Vector3 wheelPos;
		
		//Raycast(object, direction, what it hits, length of raycast);
		if(Physics.Raycast(wheelFL.transform.position, -wheelFL.transform.up, out hit,wheelFL.radius + wheelFL.suspensionDistance)){
			//hit.point is the point the raycast is hitting
			wheelPos = hit.point + wheelFL.transform.up * wheelFL.radius;
			//Debug.DrawRay(wheelFL.transform.position, new Vector3(0, -(wheelFL.radius + wheelFL.suspensionDistance), 0), Color.red);
		} else{
			wheelPos = wheelFL.transform.position - wheelFL.transform.up * wheelFL.suspensionDistance;
		}
		wheelFLTrans.position = wheelPos;
		
		//Raycast(object, direction, what it hits, length of raycast);
		if(Physics.Raycast(wheelFR.transform.position, -wheelFR.transform.up, out hit,wheelFR.radius + wheelFR.suspensionDistance)){
			//hit.point is the point the raycast is hitting
			wheelPos = hit.point + wheelFR.transform.up * wheelFR.radius;
		} else{
			wheelPos = wheelFR.transform.position - wheelFR.transform.up * wheelFR.suspensionDistance;
		}
		wheelFRTrans.position = wheelPos;
		
		
		//Raycast(object, direction, what it hits, length of raycast);
		if(Physics.Raycast(wheelRL.transform.position, -wheelRL.transform.up, out hit,wheelRL.radius + wheelRL.suspensionDistance)){
			//hit.point is the point the raycast is hitting
			wheelPos = hit.point + wheelRL.transform.up * wheelRL.radius;
		} else{
			wheelPos = wheelRL.transform.position - wheelRL.transform.up * wheelRL.suspensionDistance;
		}
		wheelRLTrans.position = wheelPos;
		
		
		//Raycast(object, direction, what it hits, length of raycast);
		if(Physics.Raycast(wheelRR.transform.position, -wheelRR.transform.up, out hit,wheelRR.radius + wheelRR.suspensionDistance)){
			//hit.point is the point the raycast is hitting
			wheelPos = hit.point + wheelRR.transform.up * wheelRR.radius;
		} else{
			wheelPos = wheelRR.transform.position - wheelRR.transform.up * wheelRR.suspensionDistance;
		}
		wheelRRTrans.position = wheelPos;
		
		
	}
	
	
	//set slip function
	void MakeSlip (float forwardFriction , float sidewayFriction){
		/*<WheelFrictionCurve> is a struct so we cant change it directly
		 * So first we get the <forwardFriction> var type from it
		 * then <stiffness> attribute gets copied and is assigned a value
		 * We at the end 'wheelRR.forwardFriction = t2;' assign the <forwardFriction> type the attribute copy we had
		 */
		WheelFrictionCurve t1 = wheelRR.forwardFriction;
		t1.stiffness = forwardFriction;
		
		WheelFrictionCurve t2 = wheelRL.forwardFriction;
		t2.stiffness = forwardFriction;
		
		WheelFrictionCurve t3 = wheelRR.sidewaysFriction;
		t3.stiffness = sidewayFriction;
		
		WheelFrictionCurve t4 = wheelRL.sidewaysFriction;
		t4.stiffness = sidewayFriction;
		
		wheelRR.forwardFriction = t1;
		wheelRL.forwardFriction = t2;
		wheelRR.sidewaysFriction = t3;
		wheelRL.sidewaysFriction = t4;
	}
	
	//simple function for organization that determines the slip value for drfiting
	void CalcStiffness(){
		int carSpeed = (int)rigidbody.velocity.magnitude;
		if(carSpeed <= 70f && carSpeed >= 15f){
			slipSidewayFriction = 0.019f;
			MakeSlip(slipForwardFriction, slipSidewayFriction);
		} else if(carSpeed > 70f && carSpeed <= maxSpeed){
			slipSidewayFriction = 0.022f;
			MakeSlip(slipForwardFriction, slipSidewayFriction);
		} else if(carSpeed < 15f){
			MakeSlip(Mathf.Lerp(wheelRL.forwardFriction.stiffness, 1f, Time.deltaTime*magicValue), Mathf.Lerp(wheelRL.sidewaysFriction.stiffness, 1f, Time.deltaTime*magicValue));
		}
		//print ("SIDEWAYS FRIC: "+wheelRL.sidewaysFriction.stiffness+" FORWARD FRIC: "+wheelRL.forwardFriction.stiffness);
	}
	
	void Drift(){
		/*
		 * For car speed 0-70 slipSidewayFriction = 0.01;
		 * speed more than 70 - 100 slipSidewayFriction = 0.05
		 * speed more than 100 - 150 slipSidewayFriction = 0.09
		 */
		
		if(Input.GetAxis("Vertical") == 0){
			
			if(Input.GetButton("Brake")){
				wheelRL.brakeTorque = 100f;
				wheelRR.brakeTorque = 100f;
				CalcStiffness();
			} else{
				wheelRL.brakeTorque = deceleration;
				wheelRR.brakeTorque = deceleration;
				MakeSlip(Mathf.Lerp(wheelRL.forwardFriction.stiffness, 1f, Time.deltaTime*magicValue), Mathf.Lerp(wheelRL.sidewaysFriction.stiffness, 1f, Time.deltaTime*magicValue));
			}
		} else if(Input.GetAxis("Vertical") != 0f){
			if(Input.GetButton("Brake")){
				wheelRL.brakeTorque = 150f;//this value should be greater than brake torque on brake pressed when no accleratopm
				wheelRR.brakeTorque = 150f;
				CalcStiffness();
			} else{
				wheelRL.brakeTorque = 0f;
				wheelRR.brakeTorque = 0f;
				MakeSlip(Mathf.Lerp(wheelRL.forwardFriction.stiffness, 1f, Time.deltaTime*magicValue), Mathf.Lerp(wheelRL.sidewaysFriction.stiffness, 1f, Time.deltaTime*magicValue));
			}
		}
		
		//print (wheelRL.brakeTorque);
		//print ("SIDEWAYS FRIC: "+wheelRL.sidewaysFriction.stiffness+" FORWARD FRIC: "+wheelRL.forwardFriction.stiffness);
	}
	
	void CalcDownForceOnCar(){
		RaycastHit hit;
		int carSpeed = (int) rigidbody.velocity.magnitude;
		//Raycast(object, direction, what it hits, length of raycast);
		if(Physics.Raycast(wheelFL.transform.position, -wheelFL.transform.up, out hit,wheelFL.radius + wheelFL.suspensionDistance)){
			//hit.point is the point the raycast is hitting
			rigidbody.AddForce(0,carSpeed*1000*-1,0);
			//print ("1000");
		} else{
			int force = 175;
			rigidbody.AddForce(0,carSpeed*force*-1,0);
			rigidbody.AddForceAtPosition(new Vector3(0, carSpeed*6,0), forceUp.position);
			//print (force);
		}
	}
	
	//is called multiple times per frame ;)
	void FixedUpdate(){
		//if(Drift()){ slow down car -> add very little opposing force to slow it down}
		//if car currently drifting and only Input.GetAxis("Vertical") is pressed then speed it to make it go forward
		Drift ();
		WheelPosition ();
		CalcDownForceOnCar ();
		
		//check for car moving if no key pressed start slowing down
		//this is also for brakes
		
		
		//max speed
		//int carSpeed = (int) Mathf.Abs(2 * Mathf.PI * wheelRR.radius * wheelRR.rpm * 60 / 1000);
		int carSpeed = (int) rigidbody.velocity.magnitude;
		//print (carSpeed);
		if(carSpeed < maxSpeed){
			maxTorque = 30f;
			wheelRL.motorTorque = maxTorque * Input.GetAxis ("Vertical");
			wheelRR.motorTorque = maxTorque * Input.GetAxis ("Vertical");
		} else{
			wheelRL.motorTorque = 0;
			wheelRR.motorTorque = 0;
		}
		
		//rigidbody.AddForce(0,carSpeed*1000*-1,0);
		
		//steer Control -> more steer angle if car is slow and less if fast //default for fast and more for slow
		/*
		 * steer angle should be from 7 -> 10
		 * create function that computes value from 7 -> 10
		 * car speed goes from 0 -> 85
		 * max speed = 85
		 * (maxspeed - carspeed) + some number to give value close to 7->10
		 */
		
		float steerAngleforCar = (((maxSpeed - carSpeed)+1f)/10f)/2f;
		steerAngleforCar = steerAngleforCar + 8f;
		//print (steerAngleforCar);
		wheelFL.steerAngle = steerAngleforCar * Input.GetAxis ("Horizontal");
		wheelFR.steerAngle = steerAngleforCar * Input.GetAxis ("Horizontal");
		
		//decelerating when oppsite direction button pressed to direction of motion
		//wheel.rpm and wheel.motortorque
		//print (wheelRR.motorTorque);
		
		if(wheelRR.rpm > 0 && wheelRR.motorTorque < 0  ||  wheelRR.rpm < 0 && wheelRR.motorTorque > 0){
			//if car is moving forward but has negative torque getting applied to it && other way -> slow the car down
			maxTorque = 35.0f;//make 35 later
			wheelRL.motorTorque = maxTorque * Input.GetAxis ("Vertical");
			wheelRR.motorTorque = maxTorque * Input.GetAxis ("Vertical");
		}
		
		//controlling the max back speed
		if(wheelRR.rpm != 0){
			if (wheelRR.rpm < 0 && wheelRR.motorTorque < 0){
				if(carSpeed < 30){
					maxTorque = 25.0f;
					wheelRL.motorTorque = maxTorque * Input.GetAxis ("Vertical");
					wheelRR.motorTorque = maxTorque * Input.GetAxis ("Vertical");
				} else{
					wheelRL.motorTorque = 0;
					wheelRR.motorTorque = 0;
				}
			}
		}
		
		//reset
		if(Input.GetKeyDown("q")){
			transform.position = new Vector3(0, 15, 0);
			transform.rotation = Quaternion.identity;
			rigidbody.Sleep ();
		}
		/*
		 * When max speed reached and if you hold back key without letting go of front key car will lock at max speed
		 * 
		 */
	}
}