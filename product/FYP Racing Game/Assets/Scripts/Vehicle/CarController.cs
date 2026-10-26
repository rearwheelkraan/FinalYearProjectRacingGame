using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float rpm;
    private float torqueMultiplier;
    private float wheelInertia;
    private int numberOfDrivenWheels;
    private float minSpeed;
    private float engineInertia;
    private float engineLimiter;
    private float finalDrive;

    private Rigidbody rigidbody;
    private float[] gears;
    private int currentGear;
    private float wheelRadius;

    // Engine Variables

    private float engineTorque;
    private float engineAngularAcceleration;
    private float wheelRPM;
    private VehicleConfiguration.GearboxType gearboxType;
    private float lowerGearDownshiftTarget;
    private float upperShiftPoint;
    private TorqueCurve lowerCurve;
    private TorqueCurve upperCurve;
    private bool handbrakeEnabled;
    private float throttle;

    // HUD Variables

    public UIHandler uiHandler;

    // Sound Variables

    public EngineSoundController engineSoundController;

    // ML Related Variables

    public bool isAgentUsable;

    void Start()
    {
        uiHandler = transform.root.GetComponentInChildren<UIHandler>();
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
        torqueMultiplier = transform.root.GetComponent<VehicleConfiguration>().torqueMultiplier;
        wheelInertia = transform.root.GetComponent<VehicleConfiguration>().wheelInertia;
        minSpeed = transform.root.GetComponent<VehicleConfiguration>().minSpeed;
        engineInertia = transform.root.GetComponent<VehicleConfiguration>().engineInertia;
        engineLimiter = transform.root.GetComponent<VehicleConfiguration>().engineLimiter;
        gears = transform.root.GetComponent<VehicleConfiguration>().gears;
        finalDrive = transform.root.GetComponent<VehicleConfiguration>().finalDrive;
        wheelRadius = transform.root.GetComponent<VehicleConfiguration>().wheelRadius;
        gearboxType = transform.root.GetComponent<VehicleConfiguration>().gearboxType;
        lowerGearDownshiftTarget = transform
            .root.GetComponent<VehicleConfiguration>()
            .lowerGearDownshiftTarget;
        upperShiftPoint = transform.root.GetComponent<VehicleConfiguration>().upperShiftPoint;
        fl = transform.Find("SpringOriginFL").GetComponent<WheelController>();
        fr = transform.Find("SpringOriginFR").GetComponent<WheelController>();
        rl = transform.Find("SpringOriginRL").GetComponent<WheelController>();
        rr = transform.Find("SpringOriginRR").GetComponent<WheelController>();
        rigidbody = transform.root.GetComponent<Rigidbody>();
        wheels = new WheelController[4];
        wheels[0] = fl;
        wheels[1] = fr;
        wheels[2] = rl;
        wheels[3] = rr;
        isDirectSteerInput = false;
        numberOfDrivenWheels = 0;
        if (
            gearboxType == VehicleConfiguration.GearboxType.Automatic
            || gearboxType == VehicleConfiguration.GearboxType.SimpleAutomatic
        )
        {
            currentGear = 2;
        }
        else
        {
            currentGear = 1;
        }

        foreach (WheelController wheel in wheels)
        {
            if (wheel.drivenWheel == true)
            {
                numberOfDrivenWheels += 1;
            }
        }
        lowerCurve = new TorqueCurve(-1, 10000, 1, 3, -2000, 500);
        upperCurve = new TorqueCurve(-1, 1000, 1, 3, -2250, 500);
    }

    void FixedUpdate()
    {
        throttle = 0f;
        if (currentGear == 1) // Neutral Gear Logic
        {
            wheelThrottleForce = transform.forward * 0;
            if (rpm > 700)
            {
                rpm -= 71; // Have the RPMs Fall With A Constant
            }
            else
            {
                rpm = 700; // TODO: Allow The Car To Rev in Neutral
            }
        }
        else
        {
            float engineToWheelRPM = wheelRPM * gears[currentGear] * finalDrive; // Driveshaft Speed in RPM

            rpm = Mathf.Lerp(rpm, engineToWheelRPM, 10f * Time.deltaTime); // Couple the Engine and Driveshaft Speed, Switched to 10f for more aggressive clutch engagement

            if (gearboxType == VehicleConfiguration.GearboxType.SimpleAutomatic)
            {
                if (
                    Math.Abs(
                        rigidbody.transform.InverseTransformDirection(rigidbody.linearVelocity).z
                    ) < minSpeed
                )
                {
                    // Gear Change
                    if (throttleInput > 0f)
                    {
                        currentGear = 2;
                        throttle = throttleInput;
                    }
                    if (brakeInput > 0f)
                    {
                        currentGear = 0;
                        throttle = brakeInput;
                        wheelBrakeForce = -transform.forward * throttleInput;
                    }
                }
                else
                {
                    // Continue In Gear
                    if (currentGear == 0)
                    {
                        throttle = brakeInput;
                        wheelBrakeForce = -transform.forward * throttleInput;
                    }
                    else
                    {
                        throttle = throttleInput;
                        wheelBrakeForce = -transform.forward * brakeInput;

                        AutomaticGearShift();
                    }
                }
            }
            else // Automatic downshift logic (manual is the same just remove this logic)
            {
                throttle = throttleInput;
                wheelBrakeForce = -transform.forward * brakeInput;
                AutomaticGearShift();
            }
            if ( // If the vehicle is moving super slow, set the rpm to idle (clutch disengaged)
                Math.Abs(rigidbody.transform.InverseTransformDirection(rigidbody.linearVelocity).z)
                < minSpeed
            )
            {
                rpm = 700;
            }

            //Debug.Log(throttle);

            // Engine Power Logic

            wheelRPM = CarCalculations.CalculateWheelRPM(
                rigidbody.transform.InverseTransformDirection(rigidbody.linearVelocity).z,
                wheelRadius
            );

            engineTorque = CarCalculations.CalculateTorqueFromRPM(
                lowerCurve,
                upperCurve,
                rpm,
                7100,
                throttle,
                engineLimiter
            ); // Calculate the Torque the engine produces using a Torque Curve

            rpm += CarCalculations.CalculateRPMIncrease(
                engineTorque,
                engineInertia,
                Time.fixedDeltaTime
            );
            if (rpm < 700) // If the rpm drops too low the car should idle (Clutch Disengaged)
            {
                rpm = 700;
            }

            if (rpm >= engineLimiter) // If the Engine Spins Faster Than the Engine Limiter
            {
                engineTorque = 0f; // Cut Power, No Fuel = No Torque
            }

            wheelThrottleForce = // Calculate the Force to be applied on the wheels
                CarCalculations.CalculateForceToWheels(
                    engineTorque,
                    wheelRadius,
                    gears[currentGear],
                    finalDrive,
                    numberOfDrivenWheels
                ) * transform.forward;
        }

        steeringAngle = CarCalculations.CalculateSteerAngle(
            false,
            steeringAngle,
            steerInput,
            maxSteerSpeed,
            centreingAnglePerFrame,
            maxSteeringAngle
        );

        foreach (WheelController wheel in wheels)
        {
            wheel.SetInput(wheelThrottleForce, wheelBrakeForce, steeringAngle, handbrakeEnabled);
        }
    }

    void Update()
    {
        if (!isAgentUsable)
        {
            uiHandler.setValues(
                Mathf.Abs(
                    rigidbody.transform.InverseTransformDirection(rigidbody.linearVelocity).z
                ),
                rpm,
                currentGear - 1
            ); // As the Array goes, 0 = Reverse, 1 = Neutral, 2 = 1st Gear, send currentGear - 1, this correction is done here and should not be done anywhere else in the code
            engineSoundController.SetRPM(rpm);
        }
    }

    public void GearUp()
    {
        if (currentGear >= gears.Length - 1)
        {
            Debug.Log("No Higher Gear");
        }
        else
        {
            currentGear += 1;
            Debug.Log("Gear Up!");
        }
    }

    public void GearDown()
    {
        if (currentGear <= 0)
        {
            Debug.Log("No Lower Gear");
        }
        else
        {
            if (wheelRPM * gears[currentGear - 1] * finalDrive > engineLimiter) // If the Engine RPM is above the limiter at the given wheel speed but a gear lower, a downshift would go past the limiter and cause engine damage, this if check ensures the user cant go above the engine limiter accidentally when
            {
                Debug.Log("Downshift Protection!!, Downshift RPM Will Exceed Engine Limiter");
            }
            else
            {
                currentGear += -1;
                Debug.Log("Gear Down");
            }
        }
    }

    public void AutomaticGearShift()
    {
        if (
            currentGear > 2
            && wheelRPM * gears[currentGear - 1] * finalDrive <= lowerGearDownshiftTarget
        ) // If the Gear is NOT Neutral Reverse, or 1st Gear, The Car Will Automatically Downshift
        {
            GearDown();
            Debug.Log("Automatic Downshift " + currentGear + " To " + (currentGear - 1));
        }
        if (rpm >= upperShiftPoint && currentGear > 1) // If the Gear is NOT Neutral or Reverse, The Car Will Automatically Upshift
        {
            GearUp();
            Debug.Log("Automatic Upshift " + (currentGear - 2) + " To " + (currentGear - 1));
        }
    }

    // float GetAverageWheelSpeeds()
    // {
    //     float tallyWheelSpeed = 0f;
    //     foreach (WheelController wheel in wheels)
    //     {
    //         if (wheel.drivenWheel == true)
    //         {
    //             tallyWheelSpeed += wheel.GetWheelVelocity();
    //         }
    //     }
    //     return tallyWheelSpeed / numberOfDrivenWheels;
    // }

    // float GetAverageWheelAcceleration()
    // {
    //     float tallyWheelAccel = 0f;
    //     foreach (WheelController wheel in wheels)
    //     {
    //         if (wheel.drivenWheel == true)
    //         {
    //             tallyWheelAccel += wheel.GetWheelAcceleration();
    //         }
    //     }
    //     return tallyWheelAccel / numberOfDrivenWheels;
    // }

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
        float externalSteerInput,
        bool externalHandbrakeEnabled
    ) // used to set the input values in this script from the seperate user / ML input script
    {
        throttleInput = externalThrottleInput;
        brakeInput = externalBrakeInput;
        steerInput = externalSteerInput;
        handbrakeEnabled = externalHandbrakeEnabled;
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

    public VehicleConfiguration.GearboxType GetGearboxType()
    {
        return gearboxType;
    }

    // void Steer()
    // {
    //     if (isDirectSteerInput == false)
    //     {
    //         steeringAngle += steerInput * maxSteerSpeed; // steerInput is taken from the external input script
    //         if (steeringAngle > 0) //+ve is steering right, -ve is steering left
    //         {
    //             steeringScalarValue = -1; // If the Wheels are turned right, the car will want to centre its wheels by turning left
    //         }
    //         else
    //         {
    //             steeringScalarValue = 1; // and vice versa
    //         }
    //         steeringAngle += centreingAnglePerFrame * steeringScalarValue; // Caster / Wheel Centreing force, super basic tho
    //     }
    //     steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);
    // }
}
