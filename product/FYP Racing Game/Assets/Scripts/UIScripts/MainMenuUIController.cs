using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    [SerializeField]
    GameObject screenSpace1;

    [SerializeField]
    GameObject screenSpace2;

    void Start()
    {
        InputActionController.Instance.controls.UIControl.Enable();
        InputActionController.Instance.controls.UIControl.Confirm.performed += LoadSceneKeybind; // Should be OnEnable(), and removed in OnDisable()
    }

    void OnDisable()
    {
        InputActionController.Instance.controls.UIControl.Confirm.performed -= LoadSceneKeybind; // When the object is destroyed the eventListener is removed, caused ghost calls
    }

    public void Button1() // Example Function for opening and closing menus
    {
        screenSpace1.SetActive(false);
        screenSpace2.SetActive(true);
    }

    public void LoadSceneKeybind(InputAction.CallbackContext context) // This Method is callable by Keybinds using the New Unity Input System
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void LoadScene() // This Method is able to be called by Buttons on the Canvas
    {
        SceneManager.LoadSceneAsync(1);
    }

    // As a rule, always append 'Keybind' to any future methods that are made to work with the Input System just to make the functions very easy to distinguish
}
