public struct SpringSpecs
{
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float dampingRate;

    public SpringSpecs(
        float inputRestLength,
        float inputSpringTravel,
        float inputSpringStiffness,
        float inputDampingRate
    )
    {
        restLength = inputRestLength;
        springTravel = inputSpringTravel;
        springStiffness = inputSpringStiffness;
        dampingRate = inputDampingRate;
    }
}
