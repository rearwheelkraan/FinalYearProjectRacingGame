using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/*
This Script Must be applied to an empty in thhe scene for the SceneBootstrapper to work
Ensure in Unity Project Settings this script is run first, Setting it to a large negative value (-9999) is adequate
*/

public class MLHeuristicsSceneBootstrapper : MonoBehaviour
{
    public static MLHeuristicsSceneBootstrapper Instance;

    [SerializeField]
    private GameObject uiEmpty;

    [SerializeField]
    private GameObject positionTracker;

    [SerializeField]
    private InGameUIController IGUIcontroller;
    private GameObject startMenuInstance;
    private GameObject pauseMenuInstance;

    [SerializeField]
    private GameObject startMenu;

    [SerializeField]
    private GameObject pauseMenu;
    private ButtonUIIdentifier[] startMenuButtonArray;
    private ButtonUIIdentifier[] pauseMenuButtonArray;

    void Awake()
    {
        if (Instance != null && Instance != this) // Boilerplate Singleton Code, as this is a BootStrapper there should only be 1
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EventSystemCheck(); // Method Ensures there is an EventSystem in the scene, without it, no controls work at all
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputActionController.Instance.controls.Driving.Enable(); // As all track maps will start with a UI, make sure the UI control map is activated and the Driving one is deactivated
        InputActionController.Instance.controls.UIControl.Disable();
    }

    public void EventSystemCheck()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null) // If there is an EventSystem in the scene already this method does nothing
        {
            GameObject eventSystem = new GameObject();
            eventSystem.name = "EventSystem";
            eventSystem.AddComponent<EventSystem>(); // EventSystem and InputSystemUIInputModule are the two components that define an EventSystem Object
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }
}
