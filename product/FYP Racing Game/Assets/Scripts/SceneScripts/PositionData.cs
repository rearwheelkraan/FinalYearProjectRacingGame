public struct PositionData
{
    int currentLap;
    int currentCheckpoint;
    float distanceToNextCheckpoint;

    public PositionData(
        int inputCurrentLap,
        int inputCurrentCheckpoint,
        float inputDistanceToNextCheckpoint
    )
    {
        currentLap = inputCurrentLap;
        currentCheckpoint = inputCurrentCheckpoint;
        distanceToNextCheckpoint = inputDistanceToNextCheckpoint;
    }
}
