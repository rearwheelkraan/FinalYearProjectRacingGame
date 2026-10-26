using NUnit.Framework.Interfaces;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SliderElement : MonoBehaviour, IUpdatableUIElement // To be applied on Slider UI Elements
{
    private Slider uiSlider;

    [SerializeField]
    private IUpdatableUIElement.GaugeType gaugeType;
    private float inputValue;
    public IUpdatableUIElement.GaugeType Type => gaugeType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiSlider = GetComponent<Slider>();
        inputValue = 0f;
    }

    void OnEnable()
    {
        uiSlider = GetComponent<Slider>();
        inputValue = 0f;
    }

    // Update is called once per frame
    void Update() { }

    public void SetValues(float value1, float value2)
    {
        switch (gaugeType)
        {
            case IUpdatableUIElement.GaugeType.RPM:
                inputValue = value1 / value2;
                break; // value1 is RPM, value2 is engineLimiter
            default:
                break;
        }
        Debug.Log(math.clamp(inputValue, 0, 1));
        Debug.Log(uiSlider.value);
        uiSlider.value = math.clamp(inputValue, 0, 1); // Slider needs a value between 0 - 1
    }
}
