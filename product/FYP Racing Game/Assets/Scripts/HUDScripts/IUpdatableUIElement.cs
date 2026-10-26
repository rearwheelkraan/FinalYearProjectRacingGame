using UnityEngine;

public interface IUpdatableUIElement
{
    public enum GaugeType
    {
        RPM,
        Speed,
        Gear,
    }

    GaugeType Type { get; }
    public void SetValues(float value1, float value2);
    /*
    GaugeType - value1 - value2

    RPM - rpm - engineLimiter
    Speed - speed - N/A
    Gear - gear - N/A
     */
}
