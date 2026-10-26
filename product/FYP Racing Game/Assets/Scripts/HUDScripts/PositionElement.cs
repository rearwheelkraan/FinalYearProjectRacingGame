using Google.Protobuf.WellKnownTypes;
using TMPro;
using UnityEngine;

public class PositionElement : MonoBehaviour, IUpdatablePositionElement // To be applied on TMP Pro Text Elements
{
    private TMP_Text uiText;

    [SerializeField]
    private IUpdatablePositionElement.GaugeType gaugeType;
    private float inputValue;
    public IUpdatablePositionElement.GaugeType Type => gaugeType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiText = GetComponent<TMP_Text>();
        inputValue = 0f;
    }

    void OnEnable()
    {
        uiText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update() { }

    public void SetValues(string value1)
    {
        uiText.text = value1.ToString();
    }
}
