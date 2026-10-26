using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitRelatedElement : MonoBehaviour
{
    private bool isMetric;
    private TMP_Text uiText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private enum DisplayedUnit // Extendable, just add rule to setDisplayed Unit
    {
        Temperature,
        Speed,
        Force,
        Torque,
    }

    [SerializeField]
    private DisplayedUnit displayedUnit;

    void Start()
    {
        uiText = GetComponent<TMP_Text>();
        setDisplayedUnit(displayedUnit, isMetric);
    }

    void OnEnable()
    {
        uiText = GetComponent<TMP_Text>();
    }

    public void setMetric(bool CCisMetric) // Called to change the Units, will also change the text on the UI when called
    {
        isMetric = CCisMetric;
        setDisplayedUnit(displayedUnit, isMetric);
    }

    void setDisplayedUnit(DisplayedUnit inputDisplayedUnit, bool inputIsMetric) // Takes the Unit Required and if its Metric and produces a string with the desired unit.
    {
        string switchString;
        switch (displayedUnit) // Works by first finding the unit desired, then checking to see if it should be metric or not
        {
            case DisplayedUnit.Temperature:
                if (isMetric == true)
                {
                    switchString = "°C";
                }
                else
                {
                    switchString = "°F";
                }
                break;
            case DisplayedUnit.Speed:
                if (isMetric == true)
                {
                    switchString = "km/h";
                }
                else
                {
                    switchString = "mph";
                }
                break;
            case DisplayedUnit.Force:
                if (isMetric == true)
                {
                    switchString = "N";
                }
                else
                {
                    switchString = "lbf";
                }
                break;
            case DisplayedUnit.Torque:
                if (isMetric == true)
                {
                    switchString = "Nm";
                }
                else
                {
                    switchString = "lb-ft";
                }
                break;
            default:
                switchString = ""; // If no unit is provided, nothing is printed.
                break;
        }
        uiText.text = switchString;
    }
}
