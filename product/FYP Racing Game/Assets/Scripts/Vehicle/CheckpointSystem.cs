using System;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Rendering;

public class CheckpointSystem : MonoBehaviour
{
    /*
    For This Script to work:

    There Must be a GameObject with Tag "CheckpointParent"
    With Checkpoints as children of that GameObject, using tag "Checkpoint" will be needed if interacted with by AI Agents
    There Must be atleast 1 Finish/Start Line
    All Individual Checkpoints must be indexed within their Checkpoint Class
    */
    [SerializeField]
    private int currentCheckpoint;

    [SerializeField]
    private int lap; // Before the car passes the start line, it will have to be on lap 0 (for the car to start on lap 1)

    [SerializeField]
    private int testingID; // ONLY TO BE USED FOR TESTING

    [SerializeField]
    private Transform checkpointsParent;

    [SerializeField]
    private Agent agent;

    [SerializeField]
    private Transform[] checkpoints;
    public bool isAgentUsable;
    public bool isAgentTrainable;
    public int lapsRequired;
    public event Action<int> OnSectorPassed; // Events to reduce Coupling
    public event Action<int> OnLapPassed;
    public event Action<CheckpointSystem> OnCourseCompleted;
    public event Action<int> OnPositionChanged;
    public event Action<int> OnNumDriversChanged;
    public float distanceToNextCheckpoint;

    [SerializeField]
    private int positionInRace;

    [SerializeField]
    private int NoOfDrivers;

    void Start()
    {
        lap = 0;
        agent = transform.root.GetComponent<Agent>();
        if (isAgentTrainable != true)
        {
            if (GameObject.FindWithTag("CheckpointParent") != null)
            {
                checkpointsParent = GameObject.FindWithTag("CheckpointParent").transform; // checkpointsParent is now automatically found with the "CheckpointParent" tag
                checkpoints = new Transform[checkpointsParent.childCount];
                foreach (Transform iteratingCheckpoint in checkpointsParent)
                {
                    checkpoints[iteratingCheckpoint.GetComponent<Checkpoint>().index] =
                        iteratingCheckpoint;
                }
            }
            else
            {
                Debug.LogError(
                    "No Checkpoint Parent Found!, Checkpoint System Will Be Non-Functional!"
                );
            }
        }

        currentCheckpoint = checkpointsParent.childCount - 1; // The car should start on the last checkpoint, so Lap 1 can be started
        PositionTracker.Instance.RegisterCarCheckpointSystem(this);
        PositionTracker.Instance.UpdateNumberOfDrivers += SetNumberOfDrivers;
    }

    void OnEnable()
    {
        PositionTracker.Instance.RegisterCarCheckpointSystem(this);
        PositionTracker.Instance.UpdateNumberOfDrivers += SetNumberOfDrivers;
        if (GameObject.FindWithTag("CheckpointParent") != null)
        {
            checkpointsParent = GameObject.FindWithTag("CheckpointParent").transform; // checkpointsParent is now automatically found with the "CheckpointParent" tag
            checkpoints = new Transform[checkpointsParent.childCount];
            foreach (Transform iteratingCheckpoint in checkpointsParent)
            {
                checkpoints[iteratingCheckpoint.GetComponent<Checkpoint>().index] =
                    iteratingCheckpoint;
            }
        }
        else
        {
            Debug.LogError(
                "No Checkpoint Parent Found!, Checkpoint System Will Be Non-Functional!"
            );
        }
    }

    void OnDisable()
    {
        PositionTracker.Instance.UnregisterCarCheckpointSystem(this);
        PositionTracker.Instance.UpdateNumberOfDrivers -= SetNumberOfDrivers;
    }

    void Update()
    {
        //Debug.Log(getNextCheckpoint());
        distanceToNextCheckpoint = Vector3.Distance(
            transform.position,
            getNextCheckpoint().transform.position
        );
        //Debug.Log("Position To Next Checkpoint: " + distanceToNextCheckpoint);
    }

    public Transform getNextCheckpoint()
    {
        int nextIndex = (currentCheckpoint + 1) % checkpoints.Length;
        return checkpoints[nextIndex];
    }

    public void resetProgress() // To only be used to reset the agent when training
    {
        lap = 0;
        currentCheckpoint = checkpointsParent.childCount - 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        Checkpoint passingCheckpoint = other.GetComponent<Checkpoint>();
        if (passingCheckpoint.index == currentCheckpoint + 1) // If the checkpoint the car is passing through is the next one, update the checkpoint
        {
            currentCheckpoint = passingCheckpoint.index;
            Debug.Log("Passed Checkpoint!" + currentCheckpoint);
            if (isAgentUsable == true)
            {
                agent.AddReward(0.3f); // Reward for passing the checkpoint
            }
            if (passingCheckpoint.isSectorLine == true)
            {
                OnSectorPassed?.Invoke(passingCheckpoint.sectorIndex);
            }
            if (passingCheckpoint.index != 0 && passingCheckpoint.isStartLine == true) // If the checkpoint youre passing through is a finish line, (start lines must have an index of 0)
            {
                Debug.Log("Passed Finish Line!" + currentCheckpoint);
                Debug.Log("Course Completed!" + currentCheckpoint);
                OnCourseCompleted?.Invoke(this);
                if (isAgentUsable == true)
                {
                    agent.EndEpisode(); // Agent has traversed the course
                }
            }
        }
        else
        {
            if (passingCheckpoint.index == 0 && passingCheckpoint.isStartLine == true) // If the checkpoint isnt the next index up, if its 0 that checkpoint should the start line, so check if the checkpoint has the start line flag
            {
                Debug.Log("Passed Start Line!" + currentCheckpoint);
                if (currentCheckpoint == checkpointsParent.childCount - 1) // If so, youve passed the start line and started another lap
                {
                    if (passingCheckpoint.isSectorLine == true)
                    {
                        OnSectorPassed?.Invoke(passingCheckpoint.sectorIndex);
                    }
                    OnLapPassed?.Invoke(lap);
                    Debug.Log("Lap " + lap + "Complete!");
                    lap = lap + 1;
                    currentCheckpoint = 0; // Resetting this to zero is essential, otherwise the car loses track and wont interact with ANY subsequent checkpoints

                    if (lap > lapsRequired)
                    {
                        Debug.Log("Course Completed!");
                        if (isAgentUsable == true)
                        {
                            agent.AddReward(0.4f);
                        }
                        OnCourseCompleted?.Invoke(this);
                        if (isAgentUsable == true)
                        {
                            agent.EndEpisode(); // Agent has traversed the course
                        }
                    }
                }
                else
                {
                    Debug.Log("Did not pass previous checkpoint, no new lap");
                    return;
                }
            }
        }
    }

    public int GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }

    public int GetCurrentLap()
    {
        return lap;
    }

    public float GetDistanceToNextCheckpoint()
    {
        return distanceToNextCheckpoint;
    }

    public void SetCarPositionInRace(int pos)
    {
        positionInRace = pos;
        OnPositionChanged?.Invoke(positionInRace);
    }

    public int GetPositionInRace()
    {
        return positionInRace;
    }

    public void SetNumberOfDrivers(int inputNumDrivers)
    {
        NoOfDrivers = inputNumDrivers;
        OnNumDriversChanged?.Invoke(NoOfDrivers);
    }

    public int GetNumberOfDrivers()
    {
        return NoOfDrivers;
    }

    public void SetCheckpointParent(Transform inputCheckpointParent)
    {
        checkpointsParent = inputCheckpointParent;
        checkpoints = new Transform[checkpointsParent.childCount];
        foreach (Transform iteratingCheckpoint in checkpointsParent)
        {
            checkpoints[iteratingCheckpoint.GetComponent<Checkpoint>().index] = iteratingCheckpoint;
        }
        //Debug.Log("Method Is Running" + checkpoints);
    }

    //TODO: clean this up, its already hard enough to understand as is.
}
