using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CarControllerAgent : MonoBehaviour
{
    [SerializeField]
    private WheelController fl;

    [SerializeField]
    private WheelController fr;

    [SerializeField]
    private WheelController rl;

    [SerializeField]
    private WheelController rr;

    private WheelController[] wheels;

    private float throttleInput;
    private float brakeInput;
    private float steerInput;

    private Vector3 wheelThrottleForce;
    private Vector3 wheelBrakeForce;

    public float testingDrivingForceMultiplier;
    public float testingBrakingForceMultiplier;
    public float maxSteerSpeed;
    private float steeringAngle;
    private float centreingAnglePerFrame;
    private int steeringScalarValue;
    public float maxSteeringAngle;
    private bool isDirectSteerInput;
    private Rigidbody rigidbody;
    private float wheelRadius;
    private float engineLimiter;
    private float lowerGearDownshiftTarget;
    private float upperShiftPoint;
    private float finalDrive;
    private float[] gears;
    private int currentGear;
    private float engineRPM;
    private float minSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        testingBrakingForceMultiplier = transform
            .root.GetComponent<VehicleConfiguration>()
            .testingBrakingForceMultiplier; // Easier To Tweak Values in the config file
        testingDrivingForceMultiplier = transform
            .root.GetComponent<VehicleConfiguration>()
            .testingDrivingForceMultiplier; // Easier To Tweak Values in the config file
        maxSteeringAngle = transform.root.GetComponent<VehicleConfiguration>().maxSteeringAngle;
        maxSteerSpeed = transform.root.GetComponent<VehicleConfiguration>().maxSteerSpeed;
        centreingAnglePerFrame = transform
            .root.GetComponent<VehicleConfiguration>()
            .centreingAnglePerFrame;
        fl = transform.Find("SpringOriginFL").GetComponent<WheelController>();
        fr = transform.Find("SpringOriginFR").GetComponent<WheelController>();
        rl = transform.Find("SpringOriginRL").GetComponent<WheelController>();
        rr = transform.Find("SpringOriginRR").GetComponent<WheelController>();
        wheels = new WheelController[4];
        wheels[0] = fl;
        wheels[1] = fr;
        wheels[2] = rl;
        wheels[3] = rr;
        isDirectSteerInput = false;
        rigidbody = transform.root.GetComponent<Rigidbody>();
        gears = transform.root.GetComponent<VehicleConfiguration>().gears;
        finalDrive = transform.root.GetComponent<VehicleConfiguration>().finalDrive;
        wheelRadius = transform.root.GetComponent<VehicleConfiguration>().wheelRadius;
        engineLimiter = transform.root.GetComponent<VehicleConfiguration>().engineLimiter;
        lowerGearDownshiftTarget = transform
            .root.GetComponent<VehicleConfiguration>()
            .lowerGearDownshiftTarget;
        upperShiftPoint = transform.root.GetComponent<VehicleConfiguration>().upperShiftPoint;
        minSpeed = transform.root.GetComponent<VehicleConfiguration>().minSpeed;
        currentGear = 2;
    }

    void FixedUpdate()
    {
        float wheelRPM =
            math.abs(rigidbody.transform.InverseTransformDirection(rigidbody.linearVelocity).z)
            / (wheelRadius * 2f * math.PI)
            * 60f; // RPM = Linear Speed at Radius / Circumference * 60
        float engineToWheelRPM = wheelRPM * gears[currentGear] * finalDrive; // Driveshaft Speed in RPM
        engineRPM = engineToWheelRPM; // engineRPM has no effect on the speed of the vehicle, it is purely used for engine sound
        if (engineToWheelRPM < 700) // If the vehicle is moving super slow, set the rpm to idle (clutch disengaged)
        {
            engineRPM = 700;
        }
        else
        {
            if (engineToWheelRPM >= upperShiftPoint) // Upshift Logic
            {
                if (currentGear < gears.Length - 1)
                { // Current Gear is a pointer, for regular gears it is 1 gear higher (0 = Reverse, 1 = Neutral, 2 = 1st etc)
                    currentGear += 1;
                    engineRPM = engineToWheelRPM;
                }
                else // If you cannot shift, you are in the highest gear, so now calculate for the engine reaching the limiter
                {
                    if (engineRPM >= engineLimiter)
                    { // If the engine has reached the limiter, cut the throttle and set the RPM to be stuck at the Limiter
                        engineRPM = engineLimiter;
                        throttleInput = 0f;
                    }
                }
            }
            else
            {
                if (wheelRPM * gears[currentGear - 1] * finalDrive <= lowerGearDownshiftTarget) // Downshift Logic
                {
                    if (currentGear >= 3)
                    {
                        currentGear -= 1;
                        engineRPM = engineToWheelRPM;
                    }
                }
            }
        }

        Debug.Log(
            "Throttle: "
                + throttleInput
                + " Speed: "
                + rigidbody.linearVelocity.magnitude * 2.23694f
                + " RPM: "
                + engineRPM
                + " Gear: "
                + (currentGear - 1)
                + " Wheel Engine RPM "
                + engineToWheelRPM
        );

        wheelThrottleForce = transform.forward * testingDrivingForceMultiplier * throttleInput; // Testing Driving Force Multiplier is completely linear, RPM has NO effect on the torque exerted on the tyres
        wheelBrakeForce = -transform.forward * brakeInput;

        Steer();

        foreach (WheelController wheel in wheels)
        {
            wheel.SetInput(wheelThrottleForce, wheelBrakeForce, steeringAngle, false);
        }
    }

    public void ResetVehicle()
    {
        foreach (WheelController wheel in wheels)
        {
            wheel.SetInput(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0, false); // Iterates through every wheel and resets everything, caused issue with AI training where the wheels were already turned when EndEpisode() was called.
        }
    }

    public void SetInput(
        float externalThrottleInput,
        float externalBrakeInput,
        float externalSteerInput
    ) // used to set the input values in this script from the seperate user / ML input script
    {
        throttleInput = externalThrottleInput;
        brakeInput = externalBrakeInput;
        steerInput = externalSteerInput;
        isDirectSteerInput = false;
    }

    public void SetDirectInput(
        float externalThrottleInput,
        float externalBrakeInput,
        float externalSteerAngle
    ) // used to set the input values in this script from the seperate user / ML input script
    {
        throttleInput = externalThrottleInput;
        brakeInput = externalBrakeInput;
        steerInput = 0f;
        steeringAngle = externalSteerAngle;
        isDirectSteerInput = true;
    }

    void Steer()
    {
        if (isDirectSteerInput == false)
        {
            steeringAngle += steerInput * maxSteerSpeed; // steerInput is taken from the external input script
            if (steeringAngle > 0) //+ve is steering right, -ve is steering left
            {
                steeringScalarValue = -1; // If the Wheels are turned right, the car will want to centre its wheels by turning left
            }
            else
            {
                steeringScalarValue = 1; // and vice versa
            }
            steeringAngle += centreingAnglePerFrame * steeringScalarValue; // Caster / Wheel Centreing force, super basic tho
        }
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);
    }
}
