using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;

public class UserInputMLTestBed : MonoBehaviour
{
    // Related Car Controller Script

    private CarControllerAgent car;

    // Variables to hold the player's inputs

    private float throttleInput;
    private float brakeInput;
    private float steerInput;

    void Start()
    {
        car = transform.root.GetComponent<CarControllerAgent>();
    }

    void Update()
    {
        throttleInput =
            InputActionController.Instance.controls.Driving.Accelerate.ReadValue<float>();
        brakeInput = InputActionController.Instance.controls.Driving.Brake.ReadValue<float>();
        steerInput = InputActionController.Instance.controls.Driving.Steering.ReadValue<float>();
        car.SetInput(throttleInput, brakeInput, steerInput);
    }
}
