using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Multiplayer.PlayMode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Vector3 = UnityEngine.Vector3;

public class WheelController : MonoBehaviour
{
    [Header("Steering and Driving Config")]
    public bool drivenWheel;
    public bool steeredWheel;
    public bool isFrontWheel;

    [Header("Suspension and Physics Config")]
    [SerializeField]
    private RaycastSuspension associatedSuspension; // The Associated Wheel Raycast Suspension Class the Car Controller Will Work With

    [SerializeField]
    private Rigidbody vehicleRigidBody;

    // Variables Related to Positioning and Vectors for Applying Forces to the Vehicle RigidBody
    private Vector3 prevPosition;
    private Vector3 curPosition;
    private Vector3 resultantForce;
    private float currentVelocity;
    private float previousVelocity;
    private float currentAcceleration;

    // Physics Variables That Can be Tweaked for Testing / Adjusting Vehicle Behaviour

    private float reactionForce;
    private float shearFrictionCo;
    private float rollingFrictionCo;
    private float brakeStrength;
    private float frontBrakeMultiplier;
    private float rearBrakeMultiplier;
    private float brakeForceDirection;

    // Variables Related to User / ML Input

    private Vector3 throttleForce; // Input Values to be taken from a seperate User or ML input script
    private float steerAngle;
    private Vector3 brakeForce;
    private bool handbrakeEnabled;

    // Start is called before the first frame update
    void Start()
    {
        vehicleRigidBody = transform.root.GetComponent<Rigidbody>(); // Finds the rigid body (Root Component of the Car)
        associatedSuspension = GetComponent<RaycastSuspension>();
        shearFrictionCo = transform.root.GetComponent<VehicleConfiguration>().shearFrictionCo;
        rollingFrictionCo = transform.root.GetComponent<VehicleConfiguration>().rollingFrictionCo;
        frontBrakeMultiplier = transform
            .root.GetComponent<VehicleConfiguration>()
            .frontBrakeMultiplier;
        rearBrakeMultiplier = transform
            .root.GetComponent<VehicleConfiguration>()
            .rearBrakeMultiplier;
        brakeStrength = transform.root.GetComponent<VehicleConfiguration>().brakeStrength;
        transform.localRotation = UnityEngine.Quaternion.Euler(0, 0, 0); // Ensures the wheels are straight when loading in
        prevPosition = transform.position;
        currentVelocity = 0f;
        previousVelocity = 0f;
        handbrakeEnabled = false;
    }

    // Update is called once per frame

    void FixedUpdate()
    {
        // TODO: Steering Code, Turn the Empty By a given amount per frame, tyre friction model will take care of the rest.

        // Refactored Code
        resultantForce = Vector3.zero; // Zero out the resultant force at the start of every tick to avoid unwanted acceleration
        curPosition = transform.position;
        if (associatedSuspension.isContactingGround == true)
        { // These calculations are only run when the wheel touches the ground, as without touching the ground there is no friction to apply forces onto the car
            // Calculate Velocity for Sliding Direction

            reactionForce = associatedSuspension.springForce;

            Vector3 worldVelocity = vehicleRigidBody.GetPointVelocity(
                associatedSuspension.hit.point
            );
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

            // Transform Both Vectors to Use Local Directions to account for steering

            resultantForce = transform.InverseTransformDirection(resultantForce);

            // Friction Acts in the Opposite Direction

            float lateralForce = -localVelocity.x * shearFrictionCo * reactionForce; // Refactored lateral and rolling force calculations to be far easier to read, changed variable names to make code easier to read too (x and z are just too vague)
            float rollingForce = -localVelocity.z * rollingFrictionCo * reactionForce;

            // The Z axis is the direction the wheels will roll, whereas the X axis will be the wheel sliding (shearing force)

            // Clamp Frictional Force to the Maximum Frictional Force (F=uN)

            float maxLateralForce = reactionForce * shearFrictionCo; // A Tyre's ability to grip is dependent on the force acting upon it (the reaction force), these scalers are used to calculate how much force the tyre is able to apply in both x and z directions (lateral and rolling).
            float maxLongitudinalForce = reactionForce * rollingFrictionCo;

            resultantForce.x = Mathf.Clamp(lateralForce, -maxLateralForce, maxLateralForce); // Using Unity's built in clamping feature, far easier to implement than the algebraic system used previously.
            resultantForce.z = Mathf.Clamp(
                rollingForce,
                -maxLongitudinalForce,
                maxLongitudinalForce
            );

            // The Z axis is the direction the wheels will roll, whereas the X axis will be the wheel sliding (shearing force)

            // Apply Driving Inputs

            if (steeredWheel == true)
            {
                transform.localRotation = UnityEngine.Quaternion.Euler(0, steerAngle, 0); // Steer()
                Debug.DrawRay(curPosition, transform.forward, Color.yellow); // Shows the Direction the wheels are pointing
            }
            if (drivenWheel == true)
            {
                vehicleRigidBody.AddForceAtPosition(throttleForce, associatedSuspension.hit.point); // Drive()
                if (throttleForce.z != 0)
                {
                    Debug.DrawRay(curPosition, throttleForce, Color.blue); // Shows the Direction the Driving Force is in
                }
            }

            if (localVelocity.z < 0)
            {
                brakeForceDirection = -1f;
            }
            else
            {
                brakeForceDirection = 1f;
            }
            if (isFrontWheel == true) // Front Wheel Braking Logic
            {
                brakeForce = brakeForceDirection * brakeForce * frontBrakeMultiplier;
            }
            else // Rear Wheel Braking Logic
            {
                if (handbrakeEnabled == true)
                {
                    brakeForce = brakeForceDirection * -transform.forward * rearBrakeMultiplier; // If the Handbrake is Enabled, Apply the Brakes to The Rear Wheels
                }
                else
                {
                    brakeForce = brakeForceDirection * brakeForce * rearBrakeMultiplier; // If not, apply the brake pressure from the pedal
                }
            }

            vehicleRigidBody.AddForceAtPosition(brakeForce, associatedSuspension.hit.point); // Brake()
            if (brakeForce.z != 0)
            {
                Debug.DrawRay(curPosition, brakeForce, Color.blue); // Shows the Direction the Braking Force is in
            }

            Debug.DrawRay(curPosition, brakeForce, Color.blue); // Shows the Direction the Braking Force is in

            // Apply Frictional Force

            Vector3 localForce = new Vector3(resultantForce.x, 0f, resultantForce.z);
            Vector3 worldForce = transform.TransformDirection(localForce);

            vehicleRigidBody.AddForceAtPosition(worldForce, associatedSuspension.hit.point);

            // Debugging

            resultantForce.Normalize(); // Gets the unit vector of the frictional force applied by the tyres onto the ground
            Debug.DrawRay(curPosition, worldForce, Color.red); // Shows the Direction the Friction Force is being applied to

            currentVelocity = (curPosition.z - prevPosition.z) / Time.fixedDeltaTime; // Z axis is the forward axis for the car
            currentAcceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;

            prevPosition = curPosition;
            previousVelocity = currentVelocity;
        }
    }

    public void SetInput(
        Vector3 throttleForceInput,
        Vector3 brakeForceInput,
        float steerAngleInput,
        bool handbrakeEnabledInput
    ) // used to set the input values in this script from the seperate user / ML input script
    {
        throttleForce = throttleForceInput;
        steerAngle = steerAngleInput;
        brakeForce = brakeForceInput;
        handbrakeEnabled = handbrakeEnabledInput;
    }

    public float GetWheelVelocity()
    {
        return currentVelocity;
    }

    public float GetWheelAcceleration()
    {
        return currentAcceleration;
    }
}
