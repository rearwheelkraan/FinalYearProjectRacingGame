using UnityEngine;

public interface IUpdatablePositionElement
{
    public enum GaugeType
    {
        Position,
        TotalNumDrivers,
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
