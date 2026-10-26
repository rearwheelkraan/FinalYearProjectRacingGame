using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

public class CarCalculationsTestScript
{
    /*
    Note: When Writing Tests, you should follow the AAA Pattern:
    - Arrange
    - Act
    - Assert

    Note: The Test Method Naming Convention is:

    - TEST_CONDITION_EXPECTED-BEHAVIOUR()

    Note: Required Tests:
    - Normal Value Testing
    - Erroneous Value Testing
    - Error Log Testing
    - Manually Calculated Result Testing
    */

    public static readonly TorqueCurve audiLowCurve = new TorqueCurve(
        -1f,
        10000f,
        1f,
        3f,
        -2000f,
        500f
    );
    public static readonly TorqueCurve audiHighCurve = new TorqueCurve(
        -1f,
        1000f,
        1f,
        3f,
        -2250f,
        500f
    );
    public static readonly TorqueCurve newLowCurve1 = new TorqueCurve(
        -1f,
        10000f,
        1f,
        2f,
        0f,
        800f
    );
    public static readonly TorqueCurve newHighCurve1 = new TorqueCurve(
        -1f,
        1000f,
        1f,
        12f,
        0f,
        500f
    );

    public static readonly SpringSpecs springSpecs1 = new SpringSpecs(0.2f, 0.5f, 30000f, 4000f);
    public static readonly SpringSpecs springSpecs2 = new SpringSpecs(0.5f, 1f, 15000f, 1200f);

    [TestCase(100, 0.5f, 1909.859f)]
    [TestCase(50, 0.5f, 954.929f)]
    [TestCase(50, 1f, 477.464f)]
    [TestCase(1000, 1f, 9549.296f)]
    public void CalculateWheelRPM_NormalRangeValues_ReturnRPM(
        float linearVelocity,
        float wheelRadius,
        float expectedResult
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f); // To Account for tiny differences due to floating point operations
        float calculatedWheelRPM = CarCalculations.CalculateWheelRPM(linearVelocity, wheelRadius);
        Assert.That(calculatedWheelRPM, Is.EqualTo(expectedResult).Using(floatEC));
    }

    [TestCase(0.1f, 1f, 0.955f)]
    [TestCase(1f, 0.1f, 95.492f)]
    [TestCase(999999f, 1f, 9549287.036f)]
    [TestCase(1f, 999999f, 9.549E-06f)]
    public void CalculateWheelRPM_ErroneousValues_ReturnRPM(
        float linearVelocity,
        float wheelRadius,
        float expectedResult
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f); // To Account for tiny differences due to floating point operations
        float calculatedWheelRPM = CarCalculations.CalculateWheelRPM(linearVelocity, wheelRadius);
        Assert.That(calculatedWheelRPM, Is.EqualTo(expectedResult).Using(floatEC));
    }

    [TestCase(100, 0)]
    [TestCase(100, -99999)]
    [TestCase(100, -99.999f)]
    [TestCase(-999, -1f)]
    [TestCase(999999, 0f)]
    public void CalculateWheelRPM_ErroneousValues_ReturnError(
        float linearVelocity,
        float wheelRadius
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "inputWheelRadius is less than 0, WheelRPM Calculation Impossible"
        );
        float calculatedWheelRPM = CarCalculations.CalculateWheelRPM(linearVelocity, wheelRadius);
    }

    [TestCase(3500, 7100, 1f, 8500f, 430.555f)]
    [TestCase(100, 7100, 1f, 8500f, 113.222f)]
    [TestCase(700, 7100, 1f, 8500f, 187.888f)]
    [TestCase(7100, 7100, 1f, 8500f, 486.388f)]
    [TestCase(3500, 7100, 0.5f, 8500f, 430.555f / 2f)]
    [TestCase(100, 7100, 0.5f, 8500f, 113.222f / 2f)]
    [TestCase(9000, 7100, 1f, 8500f, 159.722f)]
    [TestCase(9000, 7100, 0f, 8500f, 0f)]
    [TestCase(14000, 18000, 1f, 8500f, 430.555f)]
    [TestCase(3500, 50, 1f, 8500f, 0f)]
    public void CalculateTorqueFromRPM_NormalValuesExistingAudiCurve_ReturnTorque(
        float rpm,
        float curveCrossoverPoint,
        float inputThrottle,
        float engineLimiter,
        float expectedResult
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        float calculatedTorque = CarCalculations.CalculateTorqueFromRPM(
            audiLowCurve,
            audiHighCurve,
            rpm,
            curveCrossoverPoint,
            inputThrottle,
            engineLimiter
        );
        Assert.That(calculatedTorque, Is.EqualTo(expectedResult).Using(floatEC));
    }

    [TestCase(3500, 7100, 1f, 8500f, 493.75f)]
    [TestCase(100, 7100, 1f, 8500f, 799.75f)]
    [TestCase(700, 7100, 1f, 8500f, 787.75f)]
    [TestCase(7100, 7100, 1f, 8500f, 149.931f)]
    [TestCase(3500, 7100, 0.5f, 8500f, 493.75f / 2f)]
    [TestCase(100, 7100, 0.5f, 8500f, 799.75f / 2f)]
    [TestCase(9000, 7100, 1f, 8500f, 0f)]
    [TestCase(9000, 7100, 0f, 8500f, 0f)]
    [TestCase(14000f, 18000f, 1f, 8500f, 0f)]
    [TestCase(3500, 50, 1f, 8500f, 414.931f)]
    public void CalculateTorqueFromRPM_NormalValuesNewTorqueCurve_ReturnTorque(
        float rpm,
        float curveCrossoverPoint,
        float inputThrottle,
        float engineLimiter,
        float expectedResult
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        float calculatedTorque = CarCalculations.CalculateTorqueFromRPM(
            newLowCurve1,
            newHighCurve1,
            rpm,
            curveCrossoverPoint,
            inputThrottle,
            engineLimiter
        );
        Assert.That(calculatedTorque, Is.EqualTo(expectedResult).Using(floatEC));
    }

    [TestCase(3500, 7100, float.MaxValue, 8500f, 430.555f)]
    [TestCase(100, 7100, float.MaxValue, 8500f, 113.222f)]
    [TestCase(700, 7100, float.MaxValue, 8500f, 187.888f)]
    [TestCase(7100, 7100, float.MaxValue, 8500f, 486.388f)]
    [TestCase(3500, 7100, float.MinValue, 8500f, 0f)]
    [TestCase(100, 7100, float.MinValue, 8500f, 0f)]
    [TestCase(9000, 7100, float.MinValue, 8500f, 0f)]
    [TestCase(9000, 7100, float.MinValue, 8500f, 0f)]
    [TestCase(float.MinValue, 18000f, 1f, 8500f, 0f)]
    [TestCase(float.MaxValue, 18000f, 1f, 8500f, 159.722f)]
    [TestCase(float.MinValue, 50, 1f, 8500f, 0f)]
    [TestCase(float.MaxValue, 50, 1f, 8500f, 159.722f)]
    public void CalculateTorqueFromRPM_ErroneousValuesExistingAudiCurve_ReturnTorque(
        float rpm,
        float curveCrossoverPoint,
        float inputThrottle,
        float engineLimiter,
        float expectedResult
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        float calculatedTorque = CarCalculations.CalculateTorqueFromRPM(
            audiLowCurve,
            audiHighCurve,
            rpm,
            curveCrossoverPoint,
            inputThrottle,
            engineLimiter
        );
        Assert.That(calculatedTorque, Is.EqualTo(expectedResult).Using(floatEC));
    }

    [TestCase(3500, 7100, float.MaxValue, 8500f, 493.75f)]
    [TestCase(100, 7100, float.MaxValue, 8500f, 799.75f)]
    [TestCase(700, 7100, float.MaxValue, 8500f, 787.75f)]
    [TestCase(7100, 7100, float.MaxValue, 8500f, 149.931f)]
    [TestCase(3500, 7100, float.MinValue, 8500f, 0f)]
    [TestCase(100, 7100, float.MinValue, 8500f, 0f)]
    [TestCase(9000, 7100, float.MinValue, 8500f, 0f)]
    [TestCase(9000, 7100, 0f, 8500f, 0f)]
    [TestCase(float.MinValue, 18000f, 1f, 8500f, 0f)]
    [TestCase(float.MaxValue, 18000f, 1f, 8500f, 0f)]
    [TestCase(float.MinValue, 50, 1f, 8500f, 0f)]
    [TestCase(float.MaxValue, 50, 1f, 8500f, 0f)]
    public void CalculateTorqueFromRPM_ErroneousValuesNewTorqueCurve_ReturnTorque(
        float rpm,
        float curveCrossoverPoint,
        float inputThrottle,
        float engineLimiter,
        float expectedResult
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        float calculatedTorque = CarCalculations.CalculateTorqueFromRPM(
            newLowCurve1,
            newHighCurve1,
            rpm,
            curveCrossoverPoint,
            inputThrottle,
            engineLimiter
        );
        Assert.That(calculatedTorque, Is.EqualTo(expectedResult).Using(floatEC));
    }

    [Test]
    public void CalculateRPMIncrease_LogicTests_ReturnRPMIncrease(
        [Values(0f, 25f, 50f, 100f, 500f)] float engineTorque,
        [Values(1f, 1.5f, 2f, 5f)] float engineInertia,
        [Values(1f / 60f, 1f / 30f, 1f, 2f, 5f)] float deltaTime
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            CarCalculations.CalculateRPMIncrease(engineTorque, engineInertia, deltaTime),
            Is.EqualTo((engineTorque * (60 / (2 * Mathf.PI)) * deltaTime) / engineInertia)
                .Using(floatEC)
        );
    }

    [Test]
    public void CalculateRPMIncrease_LogicTestsNegativeOrZeroEngineInertia_ReturnError(
        [Values(0f, 25f, 50f, 100f, 500f)] float engineTorque,
        [Values(0f, -1f, -2f)] float engineInertia,
        [Values(1f / 60f, 1f / 30f, 1f, 2f, 5f)] float deltaTime
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "Inertia is Less than or Equal to 0, RPM Increase Calculation Not Possible"
        );
        CarCalculations.CalculateRPMIncrease(engineTorque, engineInertia, deltaTime);
    }

    [Test]
    public void CalculateRPMIncrease_LogicTestsNegativeDeltaTime_ReturnError(
        [Values(0f, 25f, 50f, 100f, 500f)] float engineTorque,
        [Values(1f, 2f)] float engineInertia,
        [Values(-1f / 60f, -1f / 30f, -1f, -2f)] float deltaTime
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "deltaTime is Negative, Not Possible During Normal Game Operation, RPM Increase Calculation Not Possible"
        );
        CarCalculations.CalculateRPMIncrease(engineTorque, engineInertia, deltaTime);
    }

    [Test]
    public void CalculateRPMIncrease_LogicTestsNegativeEngineTorque_ReturnError(
        [Values(-25f, -50f, -100f, -500f, float.MinValue)] float engineTorque,
        [Values(1f, 2f)] float engineInertia,
        [Values(1f / 60f, 1f / 30f, 1f, 2f)] float deltaTime
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "Torque is Negative, Not Possible During Normal Game Operation, RPM Increase Calculation Not Possible"
        );
        CarCalculations.CalculateRPMIncrease(engineTorque, engineInertia, deltaTime);
    }

    public void CalculateRPMIncrease_LogicTestsExcessiveEngineTorque_ReturnError(
        [Values(9999f, 99999f, float.MaxValue)] float engineTorque,
        [Values(1f, 2f)] float engineInertia,
        [Values(1f / 60f, 1f / 30f, 1f, 2f)] float deltaTime
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "Excessive Engine Torque, Could Point to Error In Engine Code, RPM Increase Calculation Stopped"
        );
        CarCalculations.CalculateRPMIncrease(engineTorque, engineInertia, deltaTime);
    }

    [TestCase(100f, 1f, 1f, 954.929f)]
    [TestCase(100f, 5f, 1f, 190.986f)]
    [TestCase(500f, 5f, 1f, 954.929f)]
    [TestCase(100f, 1f, 1f / 60f, 15.915f)]
    public void CalculateRPMIncrease_LogicTestsSpecificValues_ReturnSpecificValue(
        float engineTorque,
        float engineInertia,
        float deltaTime,
        float expectedValue
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            CarCalculations.CalculateRPMIncrease(engineTorque, engineInertia, deltaTime),
            Is.EqualTo(expectedValue).Using(floatEC)
        );
    }

    [Test]
    public void CalculateForceToWheels_LogicTests_ReturnWheelForce(
        [Values(0f, 200f, 1000f)] float engineTorque,
        [Values(0.1f, 0.5f, 1f)] float wheelRadius,
        [Values(-3.2f, -1.5f, 0f, 0.681f)] float currentGearRatio,
        [Values(3.8f, 2.81f)] float finalDrive,
        [Values(2, 4)] int numberOfDrivenWheels
    )
    {
        Assert.That(
            CarCalculations.CalculateForceToWheels(
                engineTorque,
                wheelRadius,
                currentGearRatio,
                finalDrive,
                numberOfDrivenWheels
            ),
            Is.EqualTo(
                (engineTorque * (currentGearRatio * finalDrive) / wheelRadius)
                    / numberOfDrivenWheels
            )
        );
    }

    [Test]
    public void CalculateForceToWheels_LogicTestsNegativeOrZeroFinalDriveRatio_ReturnError(
        [Values(0f, 50f, 1000f)] float engineTorque,
        [Values(0.1f, 0.5f, 1f)] float wheelRadius,
        [Values(-3.2f, 0f, 0.681f)] float currentGearRatio,
        [Values(-3.8f, -2.81f, 0f, float.MinValue)] float finalDrive,
        [Values(4)] int numberOfDrivenWheels
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "finalDrive Is Less Than Or Equal To Zero, Wheel Force Calculation Stopped"
        );
        CarCalculations.CalculateForceToWheels(
            engineTorque,
            wheelRadius,
            currentGearRatio,
            finalDrive,
            numberOfDrivenWheels
        );
    }

    [Test]
    public void CalculateForceToWheels_LogicTestsNegativeOrZeroNumberOfDrivenWheels_ReturnError(
        [Values(0f, 50f, 1000f)] float engineTorque,
        [Values(0.5f, 1f)] float wheelRadius,
        [Values(-3.2f, 0f, 0.681f)] float currentGearRatio,
        [Values(3.8f, 2.81f)] float finalDrive,
        [Values(-2, -4, 0, int.MinValue)] int numberOfDrivenWheels
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "numberOfDrivenWheels Is Less Than Or Equal To Zero, Wheel Force Calculation Stopped"
        );
        CarCalculations.CalculateForceToWheels(
            engineTorque,
            wheelRadius,
            currentGearRatio,
            finalDrive,
            numberOfDrivenWheels
        );
    }

    [Test]
    public void CalculateForceToWheels_LogicTestsNegativeOrZeroWheelRadius_ReturnError(
        [Values(0f, 50f, 1000f)] float engineTorque,
        [Values(-0.5f, -1f, 0f, float.MinValue)] float wheelRadius,
        [Values(-3.2f, 0f, 0.681f)] float currentGearRatio,
        [Values(3.8f, 2.81f)] float finalDrive,
        [Values(4)] int numberOfDrivenWheels
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "wheelRadius Is Less Than Or Equal To Zero, Wheel Force Calculation Stopped"
        );
        CarCalculations.CalculateForceToWheels(
            engineTorque,
            wheelRadius,
            currentGearRatio,
            finalDrive,
            numberOfDrivenWheels
        );
    }

    [Test]
    public void CalculateForceToWheels_LogicTestsExcessiveEngineTorque_ReturnError(
        [Values(9999f, 99999f, float.MaxValue)] float engineTorque,
        [Values(0.5f, 1f)] float wheelRadius,
        [Values(-3.2f, 0f, 0.681f)] float currentGearRatio,
        [Values(3.8f, 2.81f)] float finalDrive,
        [Values(4)] int numberOfDrivenWheels
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "Excessive Engine Torque, Could Point to Error In Engine Code, RPM Increase Calculation Stopped"
        );
        CarCalculations.CalculateForceToWheels(
            engineTorque,
            wheelRadius,
            currentGearRatio,
            finalDrive,
            numberOfDrivenWheels
        );
    }

    [TestCase(100f, 0.25f, 3.5f, 3.2f, 4, 1120f)]
    [TestCase(250f, 0.333f, 4f, 3.77f, 4, 2830.330f)]
    [TestCase(450f, 0.666f, 2.33f, 2.5f, 4, 983.953f)]
    [TestCase(800f, 2f, 10f, 15f, 4, 15000f)]
    public void CalculateForceToWheels_LogicTestsSpecificValue_ReturnSpecificValue(
        float engineTorque,
        float wheelRadius,
        float currentGearRatio,
        float finalDrive,
        int numberOfDrivenWheels,
        float expectedResult
    )
    {
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        float calculatedForceToWheels = CarCalculations.CalculateForceToWheels(
            engineTorque,
            wheelRadius,
            currentGearRatio,
            finalDrive,
            numberOfDrivenWheels
        );
        Assert.That(calculatedForceToWheels, Is.EqualTo(expectedResult).Using(floatEC));
    }

    [Test]
    public void CalculateSteerAngle_NormalRangeValuesNonDirectSteerInput_ReturnSteerAngle(
        [Values(false)] bool isDirectSteerInput,
        [Values(0f, 5f, 20f, 30f)] float steeringAngle,
        [Values(0.25f, 1f)] float steerInput,
        [Values(1f, 2f)] float maxSteerSpeed,
        [Values(0.1f, 1f)] float centreingAnglePerFrame,
        [Values(20f, 30f, 40f)] float maxSteeringAngle
    )
    {
        float testingSteeringAngle = steeringAngle;
        float calculatedSteeringAngle = steeringAngle;
        int steeringScalarValue;
        if (isDirectSteerInput == false)
        {
            testingSteeringAngle += steerInput * maxSteerSpeed;
            if (testingSteeringAngle > 0)
            {
                steeringScalarValue = -1;
            }
            else
            {
                steeringScalarValue = 1;
            }
            testingSteeringAngle += centreingAnglePerFrame * steeringScalarValue;
        }
        testingSteeringAngle = Mathf.Clamp(
            testingSteeringAngle,
            -maxSteeringAngle,
            maxSteeringAngle
        );
        Assert.That(
            testingSteeringAngle,
            Is.EqualTo(
                CarCalculations.CalculateSteerAngle(
                    isDirectSteerInput,
                    calculatedSteeringAngle,
                    steerInput,
                    maxSteerSpeed,
                    centreingAnglePerFrame,
                    maxSteeringAngle
                )
            )
        );
    }

    [Test]
    public void CalculateSteerAngle_ErroneousValuesLimitTestingPositiveSteeringAngle_ReturnMaxSteerAngle(
        [Values(false)] bool isDirectSteerInput,
        [Values(30f, 40f, 50f, 60f, float.MaxValue)] float steeringAngle,
        [Values(1f)] float steerInput,
        [Values(1f)] float maxSteerSpeed,
        [Values(1f)] float centreingAnglePerFrame,
        [Values(30f)] float maxSteeringAngle
    )
    {
        Assert.That(
            maxSteeringAngle,
            Is.EqualTo(
                CarCalculations.CalculateSteerAngle(
                    false,
                    steeringAngle,
                    steerInput,
                    maxSteerSpeed,
                    centreingAnglePerFrame,
                    maxSteeringAngle
                )
            )
        );
    }

    [Test]
    public void CalculateSteerAngle_ErroneousValuesLimitTestingNegativeSteeringAngle_ReturnMaxSteerAngle(
        [Values(false)] bool isDirectSteerInput,
        [Values(-30f, -40f, -50f, -60f, float.MinValue)] float steeringAngle,
        [Values(-1f)] float steerInput,
        [Values(1f)] float maxSteerSpeed,
        [Values(1f)] float centreingAnglePerFrame,
        [Values(30f)] float maxSteeringAngle
    )
    {
        Assert.That(
            -maxSteeringAngle,
            Is.EqualTo(
                CarCalculations.CalculateSteerAngle(
                    false,
                    steeringAngle,
                    steerInput,
                    maxSteerSpeed,
                    centreingAnglePerFrame,
                    maxSteeringAngle
                )
            )
        );
    }

    [Test]
    public void CalculateSteerAngle_NormalRangeValuesDirectSteerInput_ReturnSteerAngle(
        [Values(true)] bool isDirectSteerInput,
        [Values(0f, 5f, 20f, 30f)] float steeringAngle,
        [Values(0.25f, 1f)] float steerInput,
        [Values(1f, 2f)] float maxSteerSpeed,
        [Values(0.1f, 1f)] float centreingAnglePerFrame,
        [Values(20f, 30f, 40f)] float maxSteeringAngle
    )
    {
        float testingSteeringAngle = steeringAngle;
        float calculatedSteeringAngle = steeringAngle;
        testingSteeringAngle = Mathf.Clamp(
            testingSteeringAngle,
            -maxSteeringAngle,
            maxSteeringAngle
        );
        Assert.That(
            testingSteeringAngle,
            Is.EqualTo(
                CarCalculations.CalculateSteerAngle(
                    isDirectSteerInput,
                    calculatedSteeringAngle,
                    steerInput,
                    maxSteerSpeed,
                    centreingAnglePerFrame,
                    maxSteeringAngle
                )
            )
        );
    }

    [Test]
    public void CalculateSteerAngle_ErroneousSteerInputValues_ReturnError(
        [Values(-2f, 2f, -10f, 10f, float.MinValue, float.MaxValue)] float steerInput
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "steerInput is Out Of Bounds, steerInput must be within: -1f < steerInput < 1f, Steering Angle Calculation Stopped"
        );
        float calculatedWheelRPM = CarCalculations.CalculateSteerAngle(
            false,
            0f,
            steerInput,
            1f,
            1f,
            30f
        );
    }

    [Test]
    public void CalculateSteerAngle_ErroneousMaxSteerSpeedValues_ReturnError(
        [Values(-2f, -10f, -20f, float.MinValue)] float maxSteeringSpeed
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "maxSteerSpeed is Less Than Zero, Steering Angle Calculation Stopped"
        );
        float calculatedWheelRPM = CarCalculations.CalculateSteerAngle(
            false,
            0f,
            0f,
            maxSteeringSpeed,
            1f,
            30f
        );
    }

    [Test]
    public void CalculateSteerAngle_ErroneousCentreingAnglePerFrameValues_ReturnError(
        [Values(-2f, -10f, -20f, float.MinValue)] float centreingAnglePerFrame
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "centreingAnglePerFrame is Less Than Zero, Steering Angle Calculation Stopped"
        );
        float calculatedWheelRPM = CarCalculations.CalculateSteerAngle(
            false,
            0f,
            0f,
            0f,
            centreingAnglePerFrame,
            30f
        );
    }

    [Test]
    public void CalculateSteerAngle_ErroneousMaxSteeringAngleValues_ReturnError(
        [Values(-2f, -10f, -20f, float.MinValue)] float maxSteeringAngle
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "maxSteeringAngle is Less Than Zero, Steering Angle Calculation Possible But May Cause Erroneous Results"
        );
        float calculatedWheelRPM = CarCalculations.CalculateSteerAngle(
            false,
            0f,
            0f,
            0f,
            0f,
            maxSteeringAngle
        );
    }

    [TestCase(0f, 1f, 1f, 1f, 30f, 0f)]
    [TestCase(13f, 1f, 0.5f, 5f, 30f, 8.5f)]
    [TestCase(27f, 1f, 5f, 0.5f, 30f, 30f)]
    [TestCase(27f, -1f, 5f, 0.5f, 30f, 21.5f)]
    [TestCase(-6f, -1f, 5f, 0.5f, 30f, -10.5f)]
    public void CalculateSteerAngle_NormalRangeValuesNonDirectSteerInput_ReturnHandCalculatedValue(
        float steeringAngle,
        float steerInput,
        float maxSteerSpeed,
        float centreingAnglePerFrame,
        float maxSteeringAngle,
        float calculatedResult
    )
    {
        bool isDirectSteerInput = false;
        float testingSteeringAngle = steeringAngle;
        float calculatedSteeringAngle = steeringAngle;
        int steeringScalarValue;
        if (isDirectSteerInput == false)
        {
            testingSteeringAngle += steerInput * maxSteerSpeed;
            if (testingSteeringAngle > 0)
            {
                steeringScalarValue = -1;
            }
            else
            {
                steeringScalarValue = 1;
            }
            testingSteeringAngle += centreingAnglePerFrame * steeringScalarValue;
        }
        testingSteeringAngle = Mathf.Clamp(
            testingSteeringAngle,
            -maxSteeringAngle,
            maxSteeringAngle
        );
        Assert.That(testingSteeringAngle, Is.EqualTo(calculatedResult));
    }

    [Test]
    public void CalculateSuspensionForce_NormalRangeValuesSpringSpecs1_ReturnSuspensionForce(
        [Values(0.5f, 0.25f, 1f)] float lastFrameLength,
        [Values(0.5f, 0.25f, 1f)] float currentFrameLength,
        [Values(1f, 1f / 60f, 1f / 30f, 2f)] float fixedDeltaTime
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs1;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            calculatedValue,
            Is.EqualTo(
                    CarCalculations.CalculateSuspensionForce(
                        lastFrameLength,
                        currentFrameLength,
                        fixedDeltaTime,
                        tempSpringSpecs
                    )
                )
                .Using(floatEC)
        );
    }

    [Test]
    public void CalculateSuspensionForce_NormalRangeValuesSpringSpecs2_ReturnSuspensionForce(
        [Values(0.5f, 0.25f, 1f)] float lastFrameLength,
        [Values(0.5f, 0.25f, 1f)] float currentFrameLength,
        [Values(1f, 1f / 60f, 1f / 30f, 2f)] float fixedDeltaTime
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs2;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            calculatedValue,
            Is.EqualTo(
                    CarCalculations.CalculateSuspensionForce(
                        lastFrameLength,
                        currentFrameLength,
                        fixedDeltaTime,
                        tempSpringSpecs
                    )
                )
                .Using(floatEC)
        );
    }

    [Test]
    public void CalculateSuspensionForce_ErroneousValuesNegativeFixedDeltaTime_ReturnError(
        [Values(0.5f, 0.25f, 1f)] float lastFrameLength,
        [Values(0.5f, 0.25f, 1f)] float currentFrameLength,
        [Values(-1f, -1f / 60f, -1f / 30f, -2f)] float fixedDeltaTime
    )
    {
        LogAssert.Expect(
            LogType.Error,
            "fixedDeltaTime is Less Than Zero, Suspension Force Calculation Stopped"
        );
        CarCalculations.CalculateSuspensionForce(
            lastFrameLength,
            currentFrameLength,
            fixedDeltaTime,
            springSpecs1
        );
    }

    [Test]
    public void CalculateSuspensionForce_ErroneousValuesSpringSpecs1TestingUpperBoundClamping_ReturnSuspensionForce(
        [Values(2f, 5f, 10f, float.MaxValue)] float lastFrameLength,
        [Values(2f, 5f, 10f, float.MaxValue)] float currentFrameLength,
        [Values(1f, 1f / 60f, 1f / 30f, 2f)] float fixedDeltaTime
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs1;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            calculatedValue,
            Is.EqualTo(
                    CarCalculations.CalculateSuspensionForce(
                        0.7f,
                        0.7f,
                        fixedDeltaTime,
                        tempSpringSpecs
                    )
                )
                .Using(floatEC)
        );
    }

    [Test]
    public void CalculateSuspensionForce_ErroneousValuesSpringSpecs2TestingUpperBoundClamping_ReturnSuspensionForce(
        [Values(2f, 5f, 10f, float.MaxValue)] float lastFrameLength,
        [Values(2f, 5f, 10f, float.MaxValue)] float currentFrameLength,
        [Values(1f, 1f / 60f, 1f / 30f, 2f)] float fixedDeltaTime
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs2;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            calculatedValue,
            Is.EqualTo(
                    CarCalculations.CalculateSuspensionForce(
                        1.5f,
                        1.5f,
                        fixedDeltaTime,
                        tempSpringSpecs
                    )
                )
                .Using(floatEC)
        );
    }

    [Test]
    public void CalculateSuspensionForce_ErroneousValuesSpringSpecs1TestingLowerBoundClamping_ReturnSuspensionForce(
        [Values(-2f, -5f, -10f, float.MinValue)] float lastFrameLength,
        [Values(-2f, -5f, -10f, float.MinValue)] float currentFrameLength,
        [Values(1f, 1f / 60f, 1f / 30f, 2f)] float fixedDeltaTime
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs1;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            calculatedValue,
            Is.EqualTo(
                    CarCalculations.CalculateSuspensionForce(
                        -0.3f,
                        -0.3f,
                        fixedDeltaTime,
                        tempSpringSpecs
                    )
                )
                .Using(floatEC)
        );
    }

    [Test]
    public void CalculateSuspensionForce_ErroneousValuesSpringSpecs2TestingLowerBoundClamping_ReturnSuspensionForce(
        [Values(-2f, -5f, -10f, float.MinValue)] float lastFrameLength,
        [Values(-2f, -5f, -10f, float.MinValue)] float currentFrameLength,
        [Values(1f, 1f / 60f, 1f / 30f, 2f)] float fixedDeltaTime
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs2;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            calculatedValue,
            Is.EqualTo(
                    CarCalculations.CalculateSuspensionForce(
                        -0.5f,
                        -0.5f,
                        fixedDeltaTime,
                        tempSpringSpecs
                    )
                )
                .Using(floatEC)
        );
    }

    [TestCase(0.7f, 0.5f, 1f, -8200f)]
    [TestCase(-0.2f, 0.2f, 2f, -800f)]
    [TestCase(0f, 0f, 1f / 60f, 6000f)]
    [TestCase(-0.2f, 0.7f, 10f, -15360f)]
    [TestCase(0.1f, 0.2f, 1f / 30f, -12000f)]
    public void CalculateSuspensionForce_ErroneousValuesSpringSpecs1SpecificValueTesting_ReturnSpecificValue(
        float lastFrameLength,
        float currentFrameLength,
        float fixedDeltaTime,
        float expectedValue
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs1;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(calculatedValue, Is.EqualTo(expectedValue).Using(floatEC));
    }

    [TestCase(0.7f, 0.5f, 1f, 240f)]
    [TestCase(-0.2f, 0.2f, 2f, 4260f)]
    [TestCase(0f, 0f, 1f / 60f, 7500f)]
    [TestCase(-0.2f, 0.7f, 10f, -3108f)]
    [TestCase(0.1f, 0.2f, 1f / 30f, 900f)]
    public void CalculateSuspensionForce_ErroneousValuesSpringSpecs2SpecificValueTesting_ReturnSpecificValue(
        float lastFrameLength,
        float currentFrameLength,
        float fixedDeltaTime,
        float expectedValue
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs2;
        lastFrameLength = Mathf.Clamp(
            lastFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        currentFrameLength = Mathf.Clamp(
            currentFrameLength,
            tempSpringSpecs.restLength - tempSpringSpecs.springTravel,
            tempSpringSpecs.restLength + tempSpringSpecs.springTravel
        );
        float springForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);
        float damperForce =
            tempSpringSpecs.dampingRate * (lastFrameLength - currentFrameLength) / fixedDeltaTime;
        float calculatedValue = springForce + damperForce;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(calculatedValue, Is.EqualTo(expectedValue).Using(floatEC));
    }

    [Test]
    public void CalculateSpringForce_NormalValuesSpringSpec1_ReturnSpringForce(
        [Values(-0.5f, -0.25f, 0f, 0.25f, 0.5f, 1f)] float currentFrameLength
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs1;
        float minLength = tempSpringSpecs.restLength - tempSpringSpecs.springTravel;
        float maxLength = tempSpringSpecs.restLength + tempSpringSpecs.springTravel;
        currentFrameLength = Mathf.Clamp(currentFrameLength, minLength, maxLength);
        float calculatedSpringForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);

        Assert.That(
            calculatedSpringForce,
            Is.EqualTo(CarCalculations.CalculateSpringForce(currentFrameLength, tempSpringSpecs))
        );
    }

    [Test]
    public void CalculateSpringForce_ErroneousValuesSpringSpec1_ReturnError(
        [Values(float.MinValue, float.MaxValue)] float currentFrameLength
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs1;
        LogAssert.Expect(
            LogType.Error,
            "Excessive Spring Length, Likely A Bug In Game Logic, Spring Force Calculation Paused"
        );
        float temp = CarCalculations.CalculateSpringForce(currentFrameLength, tempSpringSpecs);
    }

    [Test]
    public void CalculateSpringForce_NormalValuesSpringSpec2_ReturnSpringForce(
        [Values(-0.5f, -0.25f, 0f, 0.25f, 0.5f, 1f)] float currentFrameLength
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs2;
        float minLength = tempSpringSpecs.restLength - tempSpringSpecs.springTravel;
        float maxLength = tempSpringSpecs.restLength + tempSpringSpecs.springTravel;
        currentFrameLength = Mathf.Clamp(currentFrameLength, minLength, maxLength);
        float calculatedSpringForce =
            tempSpringSpecs.springStiffness * (tempSpringSpecs.restLength - currentFrameLength);

        Assert.That(
            calculatedSpringForce,
            Is.EqualTo(CarCalculations.CalculateSpringForce(currentFrameLength, tempSpringSpecs))
        );
    }

    [Test]
    public void CalculateSpringForce_ErroneousValuesSpringSpec2_ReturnError(
        [Values(float.MinValue, float.MaxValue)] float currentFrameLength
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs2;
        LogAssert.Expect(
            LogType.Error,
            "Excessive Spring Length, Likely A Bug In Game Logic, Spring Force Calculation Paused"
        );
        float temp = CarCalculations.CalculateSpringForce(currentFrameLength, tempSpringSpecs);
    }

    [TestCase(0.7f, -15000f)]
    [TestCase(15f, -15000f)]
    [TestCase(0.4f, -6000f)]
    [TestCase(-0.1f, 9000f)]
    public void CalculateSpringForce_CalculatedValuesSpringSpec1_ReturnExpectedValue(
        float currentFrameLength,
        float expectedValue
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs1;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            CarCalculations.CalculateSpringForce(currentFrameLength, tempSpringSpecs),
            Is.EqualTo(expectedValue).Using(floatEC)
        );
    }

    [TestCase(0.7f, -3000f)]
    [TestCase(15f, -15000f)]
    [TestCase(0.4f, 1500f)]
    [TestCase(-0.1f, 9000f)]
    public void CalculateSpringForce_CalculatedValuesSpringSpec2_ReturnExpectedValue(
        float currentFrameLength,
        float expectedValue
    )
    {
        SpringSpecs tempSpringSpecs = springSpecs2;
        FloatEqualityComparer floatEC = new FloatEqualityComparer(0.01f);
        Assert.That(
            CarCalculations.CalculateSpringForce(currentFrameLength, tempSpringSpecs),
            Is.EqualTo(expectedValue).Using(floatEC)
        );
    }
}
