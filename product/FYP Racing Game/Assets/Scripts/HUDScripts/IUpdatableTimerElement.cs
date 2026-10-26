using UnityEngine;

public interface IUpdatableTimerElement
{
    public enum GaugeType
    {
        BestLap1,
        BestLap2,
        BestLap3,
        BestSector1,
        BestSector2,
        BestSector3,
    }

    GaugeType Type { get; }
    public void SetValues(string value1);
    /*
    GaugeType - value1 - value2

    RPM - rpm - engineLimiter
    Speed - speed - N/A
    Gear - gear - N/A
     */
}
