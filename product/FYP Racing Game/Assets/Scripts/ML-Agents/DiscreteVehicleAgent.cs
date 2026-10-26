using System;
using System.Data.Common;
using Google.Protobuf.Collections;
using NUnit.Framework.Internal;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class DiscreteVehicleAgent : Agent
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    public int StepsBeforeEpisodeEnd;
    private float steeringOutput;
    private float currentDifficulty;
    private float spawnOpponent;
    private float brakingOutput;
    private float acceleratorOutput;

    void Start()
    {
        car = GetComponentInParent<CarController>();
        startPosition = car.transform.position;
        startRotation = car.transform.rotation;
    }

    [SerializeField]
    private CarController car;

    [SerializeField]
    private CheckpointSystem checkpointSystem;

    [SerializeField]
    private Transform selectedCheckpointParent;
    private Rigidbody vehicleRigidBody;

    [SerializeField]
    private GameObject body;
    public Transform[] checkpointParentsForTraining;
    public Transform[] mapParentsForTraining;
    public bool isTraining;
    public int maxSteps;
    private int wallHitCounter;
    public Vector3 prevDistToCheckpoint;
    public int teamNumber;
    private int vehicleHitCounter;
    private Vector3 distToCheckpoint;

    public bool hasObstacles;
    public bool hasDiffSpawnPositions;

    public Transform[] obstacles;
    public Vector3[] spawnPositions;
    public float minimumSpawnAngle;
    public float maximumSpawnAngle;
    private float throttleInput;
    private float brakeInput;
    private float steerInput;
    public int xVariation;
    public int zVariation;
    public bool ifEpisodeEndOnCollision;

    public override void Initialize()
    {
        base.Initialize();
        //checkpointSystem.SetCheckpointParent(checkpointParentsForTraining[0]);
    }

    public void setPrevDistToCheckpoint(Vector3 input)
    {
        prevDistToCheckpoint = input;
    }

    public override void OnEpisodeBegin()
    {
        car = transform.root.GetComponent<CarController>();
        checkpointSystem = transform.root.GetComponent<CheckpointSystem>();
        vehicleRigidBody = transform.root.GetComponent<Rigidbody>(); // Finds the rigid body (Root Component of the Car)
        if (hasDiffSpawnPositions == true)
        {
            car.transform.position = new Vector3(
                UnityEngine.Random.Range(
                    startPosition.x - xVariation,
                    startPosition.x + xVariation
                ),
                1,
                UnityEngine.Random.Range(startPosition.z - zVariation, startPosition.z + zVariation)
            );
            car.transform.rotation = Quaternion.Euler(
                new Vector3(0, UnityEngine.Random.Range(minimumSpawnAngle, maximumSpawnAngle), 0)
            );
        }
        else
        {
            car.transform.position = startPosition;
            car.transform.rotation = startRotation;
        }
        if (hasObstacles == true)
        {
            foreach (Transform obstacle in obstacles)
            {
                if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                {
                    obstacle.gameObject.SetActive(true);
                }
                else
                {
                    obstacle.gameObject.SetActive(false);
                }
            }
        }

        checkpointSystem.resetProgress();
        car.ResetVehicle();
        steeringOutput = 0f;
        // if (isTraining == true)
        // {
        //     currentDifficulty = Academy.Instance.EnvironmentParameters.GetWithDefault(
        //         "training_difficulty",
        //         0.0f
        //     );
        //     spawnOpponent = Academy.Instance.EnvironmentParameters.GetWithDefault(
        //         "spawn_opponent",
        //         0.0f
        //     );
        //     ApplyTrainingDifficultyChanges(currentDifficulty, spawnOpponent);
        // }
        wallHitCounter = 0;
        Vector3 prevDistToCheckpoint =
            checkpointSystem.getNextCheckpoint().position - transform.position;
    }

    // private void ApplyTrainingDifficultyChanges(float currentDifficulty, float spawnOpponent)
    // {
    //     //Debug.Log("Working");
    //     if (currentDifficulty == 0f)
    //     {
    //         Debug.Log("Difficulty 0, Easiest Track");
    //         checkpointSystem.SetCheckpointParent(checkpointParentsForTraining[0]);
    //         maxSteps = 24000;
    //         for (int i = 0; i < mapParentsForTraining.Length; i++)
    //         {
    //             mapParentsForTraining[i].gameObject.SetActive(i == 0);
    //         }
    //     }
    //     else if (currentDifficulty == 1f)
    //     {
    //         Debug.Log("Difficulty 1, Easy Oval Track");
    //         checkpointSystem.SetCheckpointParent(checkpointParentsForTraining[1]);
    //         for (int i = 0; i < mapParentsForTraining.Length; i++)
    //         {
    //             mapParentsForTraining[i].gameObject.SetActive(i == 1);
    //         }
    //         maxSteps = 50000;
    //     }

    //     if (spawnOpponent == 0f)
    //     {
    //         if (teamNumber != 0)
    //         {
    //             vehicleRigidBody.gameObject.transform.SetPositionAndRotation(
    //                 new Vector3(0f, -1000f, 0f),
    //                 new Quaternion(0, 0, 0, 0)
    //             );
    //             vehicleRigidBody.useGravity = false;
    //         }
    //         else
    //         {
    //             vehicleRigidBody.useGravity = true;
    //             vehicleRigidBody.gameObject.SetActive(true);
    //             car.transform.position = startPosition;
    //             car.transform.rotation = startRotation;
    //         }
    //     }
    //     else
    //     {
    //         vehicleRigidBody.useGravity = true;
    //         vehicleRigidBody.gameObject.SetActive(true);
    //         car.transform.position = startPosition;
    //         car.transform.rotation = startRotation;
    //     }
    // }

    public override void CollectObservations(VectorSensor sensor)
    {
        distToCheckpoint = checkpointSystem.getNextCheckpoint().position - transform.position; // Find the difference between the car's current location and the next checkpoint, so the agent knows where to go
        sensor.AddObservation(
            transform.InverseTransformDirection(distToCheckpoint.normalized / 20f)
        );

        //AddReward(-0.001f); // Penalises the ML Agent to encourage faster driving

        if (prevDistToCheckpoint.magnitude < distToCheckpoint.magnitude + 10f)
        {
            prevDistToCheckpoint = distToCheckpoint;
        }
        float angle = Vector3.SignedAngle(transform.forward, distToCheckpoint, Vector3.up) / 180f; // Angle to the checkpoint
        sensor.AddObservation(angle);
        prevDistToCheckpoint = distToCheckpoint;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float checkpointDistDelta = prevDistToCheckpoint.magnitude - distToCheckpoint.magnitude;
        if (checkpointDistDelta > 0)
        {
            AddReward(checkpointDistDelta * 0.1f);
        }
        else
        {
            // Reversing Penalty
            AddReward(checkpointDistDelta * 0.2f);
        }

        AddReward(
            0.001f * Vector3.Dot(vehicleRigidBody.linearVelocity, distToCheckpoint.normalized)
        );
        float angle = Vector3.Angle(transform.forward, distToCheckpoint);

        if (Mathf.Abs(angle) > 140f)
        {
            Debug.Log("Not Facing Checkpoint, facing at angle: " + angle);
            AddReward(-0.01f * angle);
        }

        var input = actionBuffers.DiscreteActions;
        int agentAccelDecision = input[0];
        switch (agentAccelDecision)
        {
            case 0:
                acceleratorOutput = 1f;
                brakingOutput = 0f;
                break;
            case 1:
                acceleratorOutput = 0f;
                brakingOutput = 1f;
                break;
        }

        int agentSteerDecision = input[1];
        switch (agentSteerDecision)
        {
            case 0:
                steeringOutput = -1f; // steer left
                break;

            case 1:
                steeringOutput = 0f; // steer straight
                break;

            case 2:
                steeringOutput = 1f; // steer right
                break;
        }

        // var input = actionBuffers.ContinuousActions;
        // float agentSpeedDecision = input[0];
        // float agentSteerDecision = input[1];

        // if (agentSpeedDecision > 0)
        // {
        //     acceleratorOutput = agentSpeedDecision;
        //     brakingOutput = 0f;
        // } else if (agentSpeedDecision < 0)
        // {
        //     acceleratorOutput = 0f;
        //     brakingOutput = -agentSpeedDecision;
        // } else
        // {
        //     acceleratorOutput = 0f;
        //     brakingOutput = 0f;
        // }

        // steeringOutput = agentSteerDecision;

        // car.SetDirectInput(acceleratorOutput, brakingOutput, steeringOutput * car.maxSteeringAngle); // 0 = throttle, 1 = brake, 2 = steering

        // The key difference with the above method as it allows you to control the angle of the wheels directly, which will make training the model far easier as it doesnt need the interferance required by users.

        car.SetInput(acceleratorOutput, brakingOutput, steeringOutput, false); // 0 = throttle, 1 = brake, 2 = steer, The Agent CANNOT Use the Handbrake
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        throttleInput =
            InputActionController.Instance.controls.Driving.Accelerate.ReadValue<float>();
        brakeInput = InputActionController.Instance.controls.Driving.Brake.ReadValue<float>();
        steerInput = InputActionController.Instance.controls.Driving.Steering.ReadValue<float>();

        Debug.Log(throttleInput + " " + brakeInput + " " + steerInput);
        var input = actionsOut.DiscreteActions;

        if (throttleInput != 0)
        {
            input[0] = 0;
        }
        else
        {
            input[0] = 1;
        }
        ;

        if (steerInput < 0)
        {
            input[1] = 0;
        }
        else if (steerInput > 0)
        {
            input[1] = 2;
        }
        else
        {
            input[1] = 1;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
        {
            AddReward(-1.3f);
            Debug.Log("Hit wall");
            if (ifEpisodeEndOnCollision == true)
            {
                EndEpisode();
            }
            // wallHitCounter += 1;
            // if (wallHitCounter > 5)
            // {
            //     Debug.Log("Hit wall, Failed.");

            //     EndEpisode();
            // }
        }

        if (collision.gameObject.CompareTag("Vehicle"))
        {
            AddReward(-1.3f);

            Debug.Log("Hit vehicle");
            if (ifEpisodeEndOnCollision == true)
            {
                EndEpisode();
            }
            // vehicleHitCounter += 1;
            // if (vehicleHitCounter > 5)
            // {
            //     Debug.Log("Hit vehicle, Failed.");
            //     EndEpisode();
            // }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
        {
            AddReward(-1.3f);

            Debug.Log("Hit wall");
            if (ifEpisodeEndOnCollision == true)
            {
                EndEpisode();
            }
            //     wallHitCounter += 1;
            //     if (wallHitCounter > 5)
            //     {
            //         Debug.Log("Hit wall, Failed.");
            //         EndEpisode();
            //     }
            // }

            if (collision.gameObject.CompareTag("Vehicle"))
            {
                AddReward(-1.3f);
                Debug.Log("Hit vehicle");
                if (ifEpisodeEndOnCollision == true)
                {
                    EndEpisode();
                }
                // vehicleHitCounter += 1;
                // if (vehicleHitCounter > 5)
                // {
                //     Debug.Log("Hit vehicle, Failed.");
                //     EndEpisode();
                // }
            }
        }
    }
}
