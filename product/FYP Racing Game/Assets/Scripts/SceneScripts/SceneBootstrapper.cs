using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework.Constraints;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/*
This Script Must be applied to an empty in thhe scene for the SceneBootstrapper to work
Ensure in Unity Project Settings this script is run first, Setting it to a large negative value (-9999) is adequate
*/

public class SceneBootstrapper : MonoBehaviour
{
    public static SceneBootstrapper Instance;

    [SerializeField]
    private GameObject uiEmpty;

    [SerializeField]
    private GameObject positionTracker;

    [SerializeField]
    private InGameUIController IGUIcontroller;
    private GameObject startMenuInstance;
    private GameObject pauseMenuInstance;
    public GameObject playerHUD;
    public GameObject mlVehicle;

    [SerializeField]
    private GameObject startMenu;

    [SerializeField]
    private GameObject pauseMenu;
    private ButtonUIIdentifier[] startMenuButtonArray;
    private ButtonUIIdentifier[] pauseMenuButtonArray;
    public Transform[] vehicleSpawnPositions;

    void Awake()
    {
        Time.timeScale = 0f; // Stop Time Passing
        if (Instance != null && Instance != this) // Boilerplate Singleton Code, as this is a BootStrapper there should only be 1
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        positionTracker = new GameObject(); // Add the PositionTracker to a positionTracker Object
        positionTracker.name = "PositionTracker";
        positionTracker.AddComponent<PositionTracker>();

        uiEmpty = new GameObject();
        uiEmpty.name = "UIController"; // Creating an empty with the InGameUIController class, this class handles the UI of the scene while in Game
        uiEmpty.AddComponent<InGameUIController>();
        IGUIcontroller = uiEmpty.GetComponent<InGameUIController>();

        startMenuInstance = Instantiate(startMenu); // Pull the Prefabs from the Script's Variables and put them into the Scene
        pauseMenuInstance = Instantiate(pauseMenu);
        IGUIcontroller.SetStartMenu(startMenuInstance); // Set the menus in InGameUIController
        IGUIcontroller.SetPauseMenu(pauseMenuInstance);

        EventSystemCheck(); // Method Ensures there is an EventSystem in the scene, without it, no controls work at all
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < CrossSceneData.Instance.GetNumberOfOpponents() + 1; i++) // +1 is the players car
        {
            if (i >= vehicleSpawnPositions.Length)
            {
                break;
            }
            else
            {
                Transform currentVehicleSpawnLocation = vehicleSpawnPositions[i];
                Debug.Log(
                    "Car: " + 0 + "Transform: " + vehicleSpawnPositions[i].transform.gameObject.name
                );

                if (i == 0) // players car will be i = 0
                {
                    GameObject currentVehicle = Instantiate(
                        CrossSceneData.Instance.GetSelectedCar()
                    );
                    currentVehicle.transform.position = currentVehicleSpawnLocation.position;
                    currentVehicle.transform.rotation = currentVehicleSpawnLocation.rotation;
                    currentVehicle.AddComponent<UserInput>();
                    CarController currentCarController =
                        currentVehicle.GetComponent<CarController>();
                    GameObject currentVehiclePlayerHUD = Instantiate(
                        playerHUD,
                        currentVehicle.transform
                    );
                    currentCarController.uiHandler =
                        currentVehiclePlayerHUD.GetComponent<UIHandler>();
                }
                else
                {
                    Instantiate(mlVehicle);
                }
            }
        }
        startMenuButtonArray = startMenuInstance.GetComponentsInChildren<ButtonUIIdentifier>(); // Form an Array of all the buttons in a given menu
        pauseMenuButtonArray = pauseMenuInstance.GetComponentsInChildren<ButtonUIIdentifier>();

        ButtonUIIdentifier[] allMenuButtons = startMenuButtonArray
            .Concat(pauseMenuButtonArray)
            .ToArray(); // Put both arrays into one

        foreach (ButtonUIIdentifier uIIdentifier in allMenuButtons) // For each button in the concatenated array, find the function of the button, and match its function to the method it should run when pressed
        {
            switch (uIIdentifier.GetButtonFunction())
            {
                case ButtonUIIdentifier.Function.StartGame:
                    uIIdentifier.GetButton().onClick.AddListener(() => IGUIcontroller.StartGame());
                    Debug.Log(IGUIcontroller);
                    Debug.Log("Added StartGame");
                    break;
                case ButtonUIIdentifier.Function.EndGame:
                    uIIdentifier.GetButton().onClick.AddListener(() => IGUIcontroller.ExitGame());
                    break;
                default:
                    break;
            }
        }

        InputActionController.Instance.controls.Driving.Disable(); // As all track maps will start with a UI, make sure the UI control map is activated and the Driving one is deactivated
        InputActionController.Instance.controls.UIControl.Enable();
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
