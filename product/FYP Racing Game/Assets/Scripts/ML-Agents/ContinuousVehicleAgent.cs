using System;
using NUnit.Framework.Internal;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContinuousVehicleAgent : Agent
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    public int StepsBeforeEpisodeEnd;
    private float steeringOutput;
    private float brakingOutput;
    private float acceleratorOutput;

    void Start()
    {
        car = GetComponentInParent<CarControllerAgent>();
        startPosition = car.transform.position;
        startRotation = car.transform.rotation;
    }

    [SerializeField]
    private CarControllerAgent car;

    [SerializeField]
    private CheckpointSystem checkpointSystem;
    private Rigidbody vehicleRigidBody;

    [SerializeField]
    private GameObject body;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void OnEpisodeBegin()
    {
        car = transform.root.GetComponent<CarControllerAgent>();
        checkpointSystem = transform.root.GetComponent<CheckpointSystem>();
        vehicleRigidBody = transform.root.GetComponent<Rigidbody>(); // Finds the rigid body (Root Component of the Car)
        car.transform.position = startPosition;
        car.transform.rotation = startRotation;
        checkpointSystem.resetProgress();
        car.ResetVehicle();
        steeringOutput = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 distToCheckpoint =
            checkpointSystem.getNextCheckpoint().position - transform.position; // Find the difference between the car's current location and the next checkpoint, so the agent knows where to go
        sensor.AddObservation(distToCheckpoint / 20);

        AddReward(-0.0001f); // Penalises the ML Agent to encourage faster driving

        // AddReward(0.00001f * vehicleRigidBody.linearVelocity.magnitude); // Reward Faster Driving instead of punishing, average reward was around -7...

        float angle = Vector3.SignedAngle(transform.forward, distToCheckpoint, Vector3.up) / 180f; // Angle to the checkpoint
        sensor.AddObservation(angle);
    }

    void Update()
    {
        if (StepCount >= 5000000)
        {
            EndEpisode();
            Debug.Log("Took too long, resetting");
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var input = actionBuffers.ContinuousActions;
        float agentSpeedDecision = input[0];
        float agentSteerDecision = input[1];

        if (agentSpeedDecision > 0)
        {
            acceleratorOutput = agentSpeedDecision;
            brakingOutput = 0f;
        }
        else if (agentSpeedDecision < 0)
        {
            acceleratorOutput = 0f;
            brakingOutput = -agentSpeedDecision;
        }
        else
        {
            acceleratorOutput = 0f;
            brakingOutput = 0f;
        }

        steeringOutput = agentSteerDecision;

        car.SetDirectInput(acceleratorOutput, brakingOutput, steeringOutput * car.maxSteeringAngle); // 0 = throttle, 1 = brake, 2 = steering

        if (StepCount > 12000)
        {
            AddReward(-1f); // penalty for taking too long, this is a short track.
            EndEpisode(); // reset and try again
        }

        // The key difference with the above method as it allows you to control the angle of the wheels directly, which will make training the model far easier as it doesnt need the interferance required by users.
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // var input = actionsOut.DiscreteActions;

        // if (accelerateAction.ReadValue<float>() != 0) {
        //     input[0] = 1;
        // } else
        // {
        //     input[0] = 0;
        // }
        // if (brakingAction.ReadValue<float>() != 0) {
        //     input[1] = 1;
        // } else
        // {
        //     input[1] = 0;
        // }

        // if (steerAction.ReadValue<float>() != 0) {
        //     input[2] = 1;
        // } else
        // {
        //     input[2] = 0;
        // }

        // input[0] = accelerateAction.ReadValue<float>();
        // input[1] = brakingAction.ReadValue<float>();
        // input[2] = steerAction.ReadValue<float>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
        {
            AddReward(-0.3f);
            EndEpisode();
            Debug.Log("Hit wall, Failed.");
        }
    }
}
