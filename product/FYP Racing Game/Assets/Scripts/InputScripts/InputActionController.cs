using UnityEngine;

public class InputActionController : MonoBehaviour // This Script Must be attached to a gameObject in the MainMenu, Use an Empty and it will work just fine
{
    public static InputActionController Instance;
    public CustomInputActions controls { get; private set; } // InputActions Made with the New Unity Input System

    void Awake()
    {
        if (Instance != null && Instance != this) // This MUST be a singleton as all inputs go through this script (and the GameObject associated with it)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        controls = new CustomInputActions();
    }
}
