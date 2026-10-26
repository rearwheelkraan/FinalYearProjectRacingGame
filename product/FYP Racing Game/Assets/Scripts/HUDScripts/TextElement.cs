using Google.Protobuf.WellKnownTypes;
using TMPro;
using UnityEngine;

public class TextElement : MonoBehaviour, IUpdatableUIElement // To be applied on TMP Pro Text Elements
{
    private TMP_Text uiText;

    [SerializeField]
    private IUpdatableUIElement.GaugeType gaugeType;
    private float inputValue;
    public IUpdatableUIElement.GaugeType Type => gaugeType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiText = GetComponent<TMP_Text>();
        inputValue = 0f;
    }

    void OnEnable()
    {
        uiText = GetComponent<TMP_Text>();
        inputValue = 0f;
    }

    // Update is called once per frame
    void Update() { }

    public void SetValues(float value1, float value2)
    {
        value1 = (int)value1;
        switch (gaugeType)
        {
            case IUpdatableUIElement.GaugeType.Gear:
                if (value1 == -1)
                {
                    uiText.text = "R";
                }
                else if (value1 == 0)
                {
                    uiText.text = "N";
                }
                else
                {
                    uiText.text = value1.ToString();
                }
                break;
            default:
                uiText.text = value1.ToString();
                break;
        }
    }
}
