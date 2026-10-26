using System;
using Unity.Mathematics;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour //To be applied on the canvas object
{
    [SerializeField]
    private CarController car;

    [SerializeField]
    private VehicleConfiguration carConfig;

    [SerializeField]
    private int gaugeStyle;
    private int speed;
    private int rpm;
    private int engineLimiter;
    private float rpmGaugeSweepRange;
    private IUpdatableUIElement[] gaugeUpdatableArray;
    private IUpdatableTimerElement[] gaugeTimerArray;
    private IUpdatablePositionElement[] gaugePositionArray;
    private GaugeStyle[] styleArray;
    private UnitRelatedElement[] unitRelatedElements;
    private int currentGear;

    [SerializeField]
    private bool isMetric;

    [SerializeField]
    private LapTimer lapTimer;

    [SerializeField]
    private CheckpointSystem checkpointSystem;
    private float fastestSector1Time;
    private float fastestSector2Time;
    private float fastestSector3Time;
    private float fastestLapTime;
    private float fastestLapTime2;
    private float fastestLapTime3;
    private float currentPosition;
    private float numberOfDrivers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = transform.root.GetComponent<CarController>();
        carConfig = transform.root.GetComponent<VehicleConfiguration>();
        engineLimiter = transform.root.GetComponent<VehicleConfiguration>().engineLimiter;
        gaugeUpdatableArray = GetComponentsInChildren<IUpdatableUIElement>();
        gaugeTimerArray = GetComponentsInChildren<IUpdatableTimerElement>();
        gaugePositionArray = GetComponentsInChildren<IUpdatablePositionElement>();
        styleArray = GetComponentsInChildren<GaugeStyle>(); // ALL GAUGE STYLES MUST BE ENABLED FOR THEM TO BE REGISTERED
        unitRelatedElements = GetComponentsInChildren<UnitRelatedElement>();
        lapTimer = transform.root.GetComponent<LapTimer>();
        checkpointSystem = transform.root.GetComponent<CheckpointSystem>();
        foreach (GaugeStyle style in styleArray)
        {
            if (style.getStyleID() != gaugeStyle)
            {
                style.disableStyle();
            }
        }
        foreach (UnitRelatedElement elements in unitRelatedElements)
        {
            elements.setMetric(isMetric);
        }
        lapTimer.OnSectorTimesCalculated += setSectorTimes;
        lapTimer.OnLapTimesCalculated += setLapTimes;
        checkpointSystem.OnNumDriversChanged += SetTotalDrivers;
        checkpointSystem.OnPositionChanged += SetPosition;
        numberOfDrivers = checkpointSystem.GetNumberOfDrivers();
    }

    // Update is called once per frame
    void Update()
    {
        rpmGaugeSweepRange = setRPMGaugeSweep(rpm, engineLimiter);

        foreach (IUpdatableUIElement gauge in gaugeUpdatableArray)
        {
            switch (gauge.Type)
            {
                case IUpdatableUIElement.GaugeType.RPM:
                    Debug.Log(gauge);
                    gauge.SetValues(rpm, engineLimiter);
                    break;
                case IUpdatableUIElement.GaugeType.Speed:
                    gauge.SetValues(speed, 0);
                    break;
                case IUpdatableUIElement.GaugeType.Gear:
                    gauge.SetValues(currentGear, 0);
                    break;
                default:
                    break;
            }
        }

        foreach (IUpdatableTimerElement gauge in gaugeTimerArray)
        {
            switch (gauge.Type)
            {
                case IUpdatableTimerElement.GaugeType.BestLap1:
                    if (fastestLapTime == float.MaxValue)
                    {
                        break;
                    }
                    else
                    {
                        gauge.SetValues(
                            TimeSpan.FromSeconds(fastestLapTime).ToString(@"mm\:ss\.ff")
                        );
                        //Debug.Log(TimeSpan.FromSeconds(fastestLapTime).ToString(@"mm\:ss\.ff"));
                    }
                    break;
                case IUpdatableTimerElement.GaugeType.BestLap2:
                    if (fastestLapTime2 == float.MaxValue)
                    {
                        break;
                    }
                    else
                    {
                        gauge.SetValues(
                            TimeSpan.FromSeconds(fastestLapTime2).ToString(@"mm\:ss\.ff")
                        );
                        //Debug.Log(TimeSpan.FromSeconds(fastestLapTime2).ToString(@"mm\:ss\.ff"));
                    }
                    break;
                case IUpdatableTimerElement.GaugeType.BestLap3:
                    if (fastestLapTime3 == float.MaxValue)
                    {
                        break;
                    }
                    else
                    {
                        gauge.SetValues(
                            TimeSpan.FromSeconds(fastestLapTime3).ToString(@"mm\:ss\.ff")
                        );
                        //Debug.Log(TimeSpan.FromSeconds(fastestLapTime3).ToString(@"mm\:ss\.ff"));
                    }
                    break;
                case IUpdatableTimerElement.GaugeType.BestSector1:
                    if (fastestSector1Time == float.MaxValue)
                    {
                        break;
                    }
                    else
                    {
                        gauge.SetValues(
                            TimeSpan.FromSeconds(fastestSector1Time).ToString(@"mm\:ss\.ff")
                        );
                        //Debug.Log(TimeSpan.FromSeconds(fastestSector1Time).ToString(@"mm\:ss\.ff"));
                        break;
                    }
                case IUpdatableTimerElement.GaugeType.BestSector2:
                    if (fastestSector2Time == float.MaxValue)
                    {
                        break;
                    }
                    else
                    {
                        gauge.SetValues(
                            TimeSpan.FromSeconds(fastestSector2Time).ToString(@"mm\:ss\.ff")
                        );
                        //Debug.Log(TimeSpan.FromSeconds(fastestSector2Time).ToString(@"mm\:ss\.ff"));
                    }
                    break;
                case IUpdatableTimerElement.GaugeType.BestSector3:
                    if (fastestSector3Time == float.MaxValue)
                    {
                        break;
                    }
                    else
                    {
                        gauge.SetValues(
                            TimeSpan.FromSeconds(fastestSector3Time).ToString(@"mm\:ss\.ff")
                        );
                        //Debug.Log(TimeSpan.FromSeconds(fastestSector3Time).ToString(@"mm\:ss\.ff"));
                    }
                    break;
            }
        }
        foreach (IUpdatablePositionElement gauge in gaugePositionArray)
        {
            switch (gauge.Type)
            {
                case IUpdatablePositionElement.GaugeType.Position:
                    gauge.SetValues(currentPosition.ToString());
                    break;
                case IUpdatablePositionElement.GaugeType.TotalNumDrivers:
                    gauge.SetValues(numberOfDrivers.ToString());
                    break;
                default:
                    break;
            }
        }
    }

    public void setSectorTimes(float[] inputSectorTimes)
    {
        fastestSector1Time = inputSectorTimes[0];
        fastestSector2Time = inputSectorTimes[1];
        fastestSector3Time = inputSectorTimes[2];
    }

    public void setLapTimes(float[] inputLapTimes)
    {
        fastestLapTime = inputLapTimes[0];
        fastestLapTime2 = inputLapTimes[1];
        fastestLapTime3 = inputLapTimes[2];
    }

    public void SetPosition(int inputCarPosition)
    {
        currentPosition = inputCarPosition;
    }

    public void SetTotalDrivers(int inputNumDrivers)
    {
        Debug.Log("SetTotalNumDrivers: " + numberOfDrivers);
        numberOfDrivers = inputNumDrivers;
    }

    public void setValues(float inputSpeed, float inputRPM, int inputGear) // Used to get data from the CarController, should only be used on a per frame basis as this is NOT used for physics calculations
    {
        if (isMetric == true)
        {
            speed = (int)(inputSpeed * 3.6f); // 1m/s = 3.6km/h
        }
        else
        {
            speed = (int)(inputSpeed * 2.23694f); // 1m/s = 2.23694mph
        }
        rpm = (int)inputRPM;
        currentGear = inputGear;
    }

    private float setRPMGaugeSweep(float rpm, float engineLimiter) // As the VAST Majority of UI Element to be used as gauges operate on a 0-1 Scale, this function normalises the RPM to a scale of 0-1, where 1 is the engine limiter.
    {
        float gaugeSweep = rpm / engineLimiter;
        gaugeSweep = math.clamp(gaugeSweep, 0f, 1f);
        return gaugeSweep;
    }

    void styleChange(int inputStyleID)
    {
        if (gaugeStyle != inputStyleID)
        {
            foreach (GaugeStyle style in styleArray)
            {
                if (style.getStyleID() != inputStyleID)
                {
                    style.disableStyle();
                }
                else
                {
                    style.enableStyle();
                }
            }
        }
    }

    public void setMetric(bool input)
    {
        if (input == true)
        {
            isMetric = true;
        }
        else
        {
            isMetric = false;
        }
    }
}
