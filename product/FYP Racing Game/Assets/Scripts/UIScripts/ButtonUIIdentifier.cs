using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonUIIdentifier : MonoBehaviour
{
    [SerializeField]
    private Button button;

    public enum Function
    {
        StartGame,
        EndGame,
    }

    [SerializeField]
    private Function buttonFunction;

    public Function GetButtonFunction()
    {
        return buttonFunction;
    }

    public Button GetButton()
    {
        return button;
    }
}
