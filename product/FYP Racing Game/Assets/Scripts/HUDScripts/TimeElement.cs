using Google.Protobuf.WellKnownTypes;
using TMPro;
using UnityEngine;

public class TimeElement : MonoBehaviour, IUpdatableTimerElement // To be applied on TMP Pro Text Elements
{
    private TMP_Text uiText;

    [SerializeField]
    private IUpdatableTimerElement.GaugeType gaugeType;
    public IUpdatableTimerElement.GaugeType Type => gaugeType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        uiText = GetComponent<TMP_Text>();
    }

    public void SetValues(string value1)
    {
        uiText.text = value1;
    }
}
