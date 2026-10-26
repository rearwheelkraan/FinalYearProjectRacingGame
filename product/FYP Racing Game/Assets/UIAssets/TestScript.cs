using UnityEngine;
using UnityEngine.UI; // YOU MUST HAVE UnityEngine.UI, not UIElements as that is newer

public class TestScript : MonoBehaviour
{
    Button button;
    public InGameUIController igUIc;

    // This Script Was Added to the Canvas of the start screen
    void Start()
    {
        Button button = GetComponentInChildren<Button>(); // This is unnecessary you can manually add the button and have it still encapsulated in the prefab
        button.onClick.AddListener(igUIc.ExitGame); // This Can Add Listeners to Buttons, Allowing you to form prefabs for UI Elements
    }
}
