using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.AccessControl;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;

public class UserInput : MonoBehaviour
{
    // Related Car Controller Script

    [SerializeField]
    private CarController car;

    // Variables to hold the player's inputs

    private float throttleInput;
    private float brakeInput;
    private float steerInput;
    private bool handbrakeInput;

    void Start()
    {
        car = transform.root.GetComponent<CarController>();
    }

    void OnEnable()
    {
        car = transform.root.GetComponent<CarController>();
        if (car.GetGearboxType() != VehicleConfiguration.GearboxType.SimpleAutomatic)
        {
            InputActionController.Instance.controls.Driving.GearUp.performed += GearUp;
            InputActionController.Instance.controls.Driving.GearDown.performed += GearDown; // Put in OnEnable(), but the opposite on OnDisable()
        }
    }

    void OnDisable()
    {
        InputActionController.Instance.controls.Driving.GearUp.performed -= GearUp;
        InputActionController.Instance.controls.Driving.GearDown.performed -= GearDown;
    }

    void Update()
    {
        throttleInput =
            InputActionController.Instance.controls.Driving.Accelerate.ReadValue<float>();
        brakeInput = InputActionController.Instance.controls.Driving.Brake.ReadValue<float>();
        steerInput = InputActionController.Instance.controls.Driving.Steering.ReadValue<float>();
        handbrakeInput =
            InputActionController.Instance.controls.Driving.Handbrake.ReadValue<float>() > 0f;
        car.SetInput(throttleInput, brakeInput, steerInput, handbrakeInput);
    }

    private void GearDown(InputAction.CallbackContext context)
    {
        car.GearDown();
    }

    private void GearUp(InputAction.CallbackContext context)
    {
        car.GearUp();
    }
}
