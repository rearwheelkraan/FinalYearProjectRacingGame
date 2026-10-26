using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public static class CarCalculations
{
    public static float CalculateWheelRPM(float inputLinearVelocityZ, float inputWheelRadius)
    {
        //Debug.Log((100 / (0.5 * 2 * Mathf.PI)) * 60f);
        if (inputWheelRadius > 0)
        {
            return inputLinearVelocityZ / (inputWheelRadius * 2f * Mathf.PI) * 60f; // RPM = Linear Speed at Radius / Circumference * 60
        }
        else
        {
            Debug.LogError("inputWheelRadius is less than 0, WheelRPM Calculation Impossible");
            return 0f;
        }
    }

    public static float CalculateTorqueFromRPM( // TODO: add an engine limiter, remove that responsibility from CarController.cs
        TorqueCurve lowCurve,
        TorqueCurve highCurve,
        float rpm,
        float curveCrossoverPoint,
        float inputThrottle,
        float engineLimiter
    ) // Torque Curve Equation: a/b * ((c/d * (rpm)) + e) + f
    {
        float fullThrottleTorque;
        if (rpm < engineLimiter)
        {
            if (rpm < curveCrossoverPoint)
            {
                fullThrottleTorque = lowCurve.GetTorqueAtPoint(rpm);
            }
            else
            {
                fullThrottleTorque = highCurve.GetTorqueAtPoint(rpm);
            }
        }
        else
        {
            if (rpm < curveCrossoverPoint)
            {
                fullThrottleTorque = lowCurve.GetTorqueAtPoint(engineLimiter);
            }
            else
            {
                fullThrottleTorque = highCurve.GetTorqueAtPoint(engineLimiter);
            }
        }
        if (rpm < 0)
        {
            return 0f;
        }
        inputThrottle = Mathf.Clamp(inputThrottle, 0f, 1f);
        return Mathf.Clamp(fullThrottleTorque * inputThrottle, 0f, float.MaxValue);
    }

    public static float CalculateRPMIncrease(
        float inputEngineTorque,
        float inputEngineInertia,
        float inputDeltaTime
    )
    {
        float engineAngularAcceleration = inputEngineTorque / inputEngineInertia; // Angular Acceleration = Torque / Inertia
        if (inputEngineInertia <= 0f)
        {
            Debug.LogError(
                "Inertia is Less than or Equal to 0, RPM Increase Calculation Not Possible"
            );
            return 0f;
        }
        if (inputEngineTorque < 0f)
        {
            Debug.LogError(
                "Torque is Negative, Not Possible During Normal Game Operation, RPM Increase Calculation Not Possible"
            );
            return 0f;
        }
        if (inputDeltaTime < 0f)
        {
            Debug.LogError(
                "deltaTime is Negative, Not Possible During Normal Game Operation, RPM Increase Calculation Not Possible"
            );
            return 0f;
        }
        if (inputEngineTorque > 8000f)
        {
            Debug.LogError(
                "Excessive Engine Torque, Could Point to Error In Engine Code, RPM Increase Calculation Stopped"
            );
            return 0f;
        }

        return engineAngularAcceleration * (60 / (2 * Mathf.PI)) * inputDeltaTime; // Angular Accel is measured in rad/s^2, (60 / 2pi) will convert it to rpm
    }

    public static float CalculateForceToWheels(
        float engineTorque,
        float wheelRadius,
        float currentGearRatio,
        float finalDrive,
        int numberOfDrivenWheels
    )
    {
        if (finalDrive <= 0f)
        {
            Debug.LogError(
                "finalDrive Is Less Than Or Equal To Zero, Wheel Force Calculation Stopped"
            );
            return 0f;
        }
        if (numberOfDrivenWheels <= 0f)
        {
            Debug.LogError(
                "numberOfDrivenWheels Is Less Than Or Equal To Zero, Wheel Force Calculation Stopped"
            );
            return 0f;
        }
        if (wheelRadius <= 0f)
        {
            Debug.LogError(
                "wheelRadius Is Less Than Or Equal To Zero, Wheel Force Calculation Stopped"
            );
            return 0f;
        }
        if (engineTorque >= 8000f)
        {
            Debug.LogError(
                "Excessive Engine Torque, Could Point to Error In Engine Code, RPM Increase Calculation Stopped"
            );
            return 0f;
        }
        float torqueOutput = engineTorque * (currentGearRatio * finalDrive); // Run the engine torque through the Transmission and Final drive Gears
        float wheelForce = torqueOutput / wheelRadius; // Put the Force to the Wheel Controller, transform.forward must be applied after
        wheelForce = wheelForce / numberOfDrivenWheels; // Torque in the engine is put down on the road through x many wheels, so divide the torque by the number of driven wheels
        return wheelForce;
    }

    public static float CalculateSteerAngle(
        bool isDirectSteerInput,
        float steeringAngle,
        float steerInput,
        float maxSteerSpeed,
        float centreingAnglePerFrame,
        float maxSteeringAngle
    )
    {
        if (steerInput > 1f || steerInput < -1f)
        {
            Debug.LogError(
                "steerInput is Out Of Bounds, steerInput must be within: -1f < steerInput < 1f, Steering Angle Calculation Stopped"
            );
            return 0f;
        }
        if (maxSteerSpeed < 0f)
        {
            Debug.LogError("maxSteerSpeed is Less Than Zero, Steering Angle Calculation Stopped");
            return 0f;
        }
        if (centreingAnglePerFrame < 0f)
        {
            Debug.LogError(
                "centreingAnglePerFrame is Less Than Zero, Steering Angle Calculation Stopped"
            );
            return 0f;
        }
        if (maxSteeringAngle < 0f)
        {
            Debug.LogError(
                "maxSteeringAngle is Less Than Zero, Steering Angle Calculation Possible But May Cause Erroneous Results"
            );
        }
        int steeringScalarValue;
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
        return steeringAngle;
    }

    public static float CalculateSuspensionForce(
        float lastFrameLength,
        float currentFrameLength,
        float fixedDeltaTime,
        SpringSpecs springSpecs
    )
    {
        if (fixedDeltaTime < 0f)
        {
            Debug.LogError(
                "fixedDeltaTime is Less Than Zero, Suspension Force Calculation Stopped"
            );
            return 0f;
        }
        float minLength = springSpecs.restLength - springSpecs.springTravel; // These two Variables determine the absolute minimum and maximum length of the "spring".
        float maxLength = springSpecs.restLength + springSpecs.springTravel;
        lastFrameLength = Mathf.Clamp(lastFrameLength, minLength, maxLength); // Clamped for safety
        currentFrameLength = Mathf.Clamp(currentFrameLength, minLength, maxLength); // Ensures the spring doesnt over/under-extend
        float springVelocity = (lastFrameLength - currentFrameLength) / fixedDeltaTime; // Find the speed of the spring's extension
        float springForce =
            springSpecs.springStiffness * (springSpecs.restLength - currentFrameLength); // Varying the force applied on the car by the compression of the spring, following Hooke's Law (F=kx)
        float damperForce = springSpecs.dampingRate * springVelocity; // Damping is dependent on the speed of the springs oscillation
        float suspensionForce = springForce + damperForce; // The Damping and SpringForce Vectors are added, as the Damping and SpringForce Vectors are in opposite directions.
        return suspensionForce; // The output must be multiplied by transform.up
    }

    public static float CalculateSpringForce(float currentFrameLength, SpringSpecs springSpecs)
    {
        if (currentFrameLength > 100f || currentFrameLength < -100f)
        {
            Debug.LogError(
                "Excessive Spring Length, Likely A Bug In Game Logic, Spring Force Calculation Paused"
            );
            return 0f;
        }
        float minLength = springSpecs.restLength - springSpecs.springTravel;
        float maxLength = springSpecs.restLength + springSpecs.springTravel;
        currentFrameLength = Mathf.Clamp(currentFrameLength, minLength, maxLength);
        float springForce =
            springSpecs.springStiffness * (springSpecs.restLength - currentFrameLength); // Varying the force applied on the car by the compression of the spring, following Hooke's Law (F=kx)
        return springForce; // The output must be multiplied by transform.up
    }
}
