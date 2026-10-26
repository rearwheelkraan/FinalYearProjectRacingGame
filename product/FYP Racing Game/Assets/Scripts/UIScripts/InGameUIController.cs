using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InGameUIController : MonoBehaviour
{
    [SerializeField]
    GameObject pauseScreenSpace;

    [SerializeField]
    GameObject startScreenSpace;
    bool hasGameStarted;

    public void Start()
    {
        InputActionController.Instance.controls.UIControl.Enable();
        InputActionController.Instance.controls.Driving.Pause.performed += PauseGame; //Should Be OnEnable() and removed in OnDisable()
        InputActionController.Instance.controls.UIControl.Unpause.performed += UnpauseGame;
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        if (hasGameStarted == true)
        {
            Time.timeScale = 0f;
            pauseScreenSpace.SetActive(true);
            InputActionController.Instance.controls.Driving.Disable();
            InputActionController.Instance.controls.UIControl.Enable();
        }
        else
        {
            Debug.Log("Cannot Pause Game, Game has not started!");
        }
    }

    public void UnpauseGame(InputAction.CallbackContext context)
    {
        if (hasGameStarted == true)
        {
            Time.timeScale = 1f;
            pauseScreenSpace.SetActive(false);
            InputActionController.Instance.controls.UIControl.Disable();
            InputActionController.Instance.controls.Driving.Enable();
        }
        else
        {
            Debug.Log("Cannot UnPause Game, Game has not started!");
        }
    }

    public void ExitGame()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void StartGame()
    {
        startScreenSpace.SetActive(false);
        InputActionController.Instance.controls.UIControl.Disable();
        InputActionController.Instance.controls.Driving.Enable();
        Time.timeScale = 1f; // Start Time Passing, Game has Started
        hasGameStarted = true;
    }

    public void SetPauseMenu(GameObject inputPauseMenu)
    {
        pauseScreenSpace = inputPauseMenu;
    }

    public void SetStartMenu(GameObject inputStartMenu)
    {
        startScreenSpace = inputStartMenu;
    }
}
