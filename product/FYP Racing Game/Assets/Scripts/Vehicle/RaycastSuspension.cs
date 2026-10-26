using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class RaycastSuspension : MonoBehaviour
{
    [Header("Vehicle Configurations")]
    private Rigidbody vehicleRigidBody; // This is the RigidBody Forces are applied to, with my suspension model, it will be the parent of the suspension empties.
    private float restLength; // This holds the midpoint of the springs length
    private float springTravel; // This variable holds how far further or shorter the spring can extend
    private float springStiffness; // This variable controls how hard the spring is to compress
    private float dampingRate; // This variable controls how strong the damping of the suspension is (how fast energy is removed from the system)
    private float wheelRadius;

    [Header("Public Data Values")]
    public bool isContactingGround;
    public RaycastHit hit;
    public float springForce;

    // These Parameters are not to be exposed at all, these are just used to make the calculation of suspension forces easier to read / improve
    private float maxLength;
    private float minLength;
    private Vector3 suspensionForce;
    private float springLength;
    private float springVelocity;
    private float damperForce;
    private float lastFrameLength; // The Length of the "spring" from the last update tick, used to find the speed of oscillation for damping
    private float unclampedSpringLength;
    private SpringSpecs springSpecs;

    void Start()
    {
        vehicleRigidBody = transform.root.GetComponent<Rigidbody>(); // Finds the rigid body (Root Component of the Car)

        restLength = transform.root.GetComponent<VehicleConfiguration>().restLength; // Pulls the suspension info from VehicleConfiguration of the car, makes finetuning suspension much easier
        springTravel = transform.root.GetComponent<VehicleConfiguration>().springTravel;
        springStiffness = transform.root.GetComponent<VehicleConfiguration>().springStiffness;
        dampingRate = transform.root.GetComponent<VehicleConfiguration>().dampingRate;
        wheelRadius = transform.root.GetComponent<VehicleConfiguration>().wheelRadius;

        minLength = restLength - springTravel; // These two Variables determine the absolute minimum and maximum length of the "spring", this will be used to clamp the length to avoid "bottoming out" and other visual glitches.
        maxLength = restLength + springTravel;
        isContactingGround = false; // Always Assume the car is not contacting the ground, it will be updated in the next frame anyway.
        springSpecs = new SpringSpecs(restLength, springTravel, springStiffness, dampingRate);
        Debug.Log(
            "Spring Specs: "
                + springSpecs.restLength
                + " "
                + springSpecs.springTravel
                + " "
                + springSpecs.springStiffness
                + " "
                + springSpecs.dampingRate
        ); // Used to Check the springSpecs struct
    }

    void FixedUpdate()
    {
        Vector3 origin = transform.position; // Stores the position of the suspension empty
        float rayLength = maxLength + wheelRadius;
        if (Physics.Raycast(origin, -transform.up, out hit, rayLength))
        {
            lastFrameLength = unclampedSpringLength;
            unclampedSpringLength = hit.distance - wheelRadius; // The Length of the actual spring
            springLength = Mathf.Clamp(unclampedSpringLength, minLength, maxLength); // Ensures the spring doesnt over/under-extend
            springForce = CarCalculations.CalculateSpringForce(springLength, springSpecs);
            suspensionForce =
                CarCalculations.CalculateSuspensionForce(
                    lastFrameLength,
                    springLength,
                    Time.fixedDeltaTime,
                    springSpecs
                ) * transform.up;
            vehicleRigidBody.AddForceAtPosition(suspensionForce, hit.point); // Applying the force onto the rigidbody of the car, do note however, this is applied from where the wheel touching the ground, not the origin of the suspension empty.
            isContactingGround = true; // If ANY force is applied onto the car, the wheel must be touching the ground, so set this value to true, this value is not being used at the moment, but will be used in the vehicle controller class
        }
        else
        {
            isContactingGround = false; // If the Raycast doesnt hit anything, the car cannot be touching the ground, so set isContactingGround to false.
            // This is put outside the If() block as the block is only executed if the raycast "hits" something
        }
    }

    // Suspension Physics Tutorial I used and tweaked for my own purposes
    // https://youtu.be/x0LUiE0dxP0?si=mw7BRjGVYcHJTbz0

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position; // Stores the position of the suspension empty
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + (transform.up * -springTravel)); // Shows the raycast controlling the height of the car
        Gizmos.DrawWireSphere(origin + transform.up * -springLength, wheelRadius); // Shows where the wheels "collisions" are
    }
}
