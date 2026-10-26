using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleConfiguration : MonoBehaviour
{
    public enum GearboxType
    {
        Manual,
        Automatic,
        SimpleAutomatic,
    }

    [Header("Suspension")] // Values for Configuring any Given Vehicle
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float dampingRate;
    public float wheelRadius;
    public SpringSpecs springSpecs;

    [Header("Steering and Driving Config")]
    public float maxSteeringAngle;
    public float maxSteerSpeed;
    public float centreingAnglePerFrame;

    [Header("Testing Values for Car Controller")]
    public float testingDrivingForceMultiplier;
    public float testingBrakingForceMultiplier;

    public float shearFrictionCo;
    public float rollingFrictionCo;

    public float wheelInertia;
    public float torqueMultiplier;
    public float minSpeed;
    public float engineInertia;
    public int engineLimiter;
    public float finalDrive;
    public float[] gears;
    public float brakeStrength;
    public float frontBrakeMultiplier;
    public float rearBrakeMultiplier;
    public GearboxType gearboxType;
    public float lowerGearDownshiftTarget;
    public float upperShiftPoint;
    public Vector3 cameraLocation;
    public Vector3 cameraRotation;
    public List<EngineSoundLoop> engineSoundLoops;
    public GameObject exhaustSoundEmpty;
}
