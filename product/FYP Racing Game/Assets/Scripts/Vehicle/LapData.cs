public struct LapData
{
    public float lapDuration;
    public float sector1Duration;
    public float sector2Duration;
    public float sector3Duration;

    public LapData(
        float inputLapDuration,
        float inputSector1Duration,
        float inputSector2Duration,
        float inputSector3Duration
    )
    {
        lapDuration = inputLapDuration;
        sector1Duration = inputSector1Duration;
        sector2Duration = inputSector2Duration;
        sector3Duration = inputSector3Duration;
    }
}
