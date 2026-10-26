using UnityEngine;

public class StartDrivingControlsTestingScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputActionController.Instance.controls.Driving.Enable();
    }
}
