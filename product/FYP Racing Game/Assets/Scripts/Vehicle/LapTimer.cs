using System;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    /*
    For This Script to work:

    There Must be a GameObject with Tag "CheckpointParent"
    With Checkpoints as children of that GameObject
    There Must be atleast 1 Finish/Start Line
    All 3 Sector Checkpoints must be indexed within their Checkpoint Class
    Those 3 Sector Checkpoints Must have Indexes 0, 1 and 2.

    Sector Numbers Based on Checkpoint Sector Indexes:

    Sector 1: 0 -> 1
    Sector 2: 1 -> 2
    Sector 3: 2 -> 0
    */

    private int currentCheckpoint;

    [SerializeField]
    private Transform checkpointsParent;
    private Transform[] checkpoints;

    [SerializeField]
    private CheckpointSystem checkpointSystem;
    private float elapsedTimeFromLap;
    private float elapsedTimeFromSector;
    private double elapsedTimeTotal;
    private float[] topThreeFastestLaps;
    private List<LapData> allLaps;
    private double totalRaceTime;
    private int finishedLap;
    private int currentLap;
    private float fastestSector1Time;
    private float fastestSector2Time;
    private float fastestSector3Time;

    // Stores the Data Related to the last lap
    private float lastLapTime;
    private float lastSector1Time;
    private float lastSector2Time;
    private float lastSector3Time;
    private float[] fastestSectors;
    public event Action<float[]> OnSectorTimesCalculated;
    public event Action<float[]> OnLapTimesCalculated;

    void Awake()
    {
        checkpointSystem = transform.root.GetComponent<CheckpointSystem>();
    }

    void Start()
    {
        if (GameObject.FindWithTag("CheckpointParent") != null)
        {
            checkpointsParent = GameObject.FindWithTag("CheckpointParent").transform; // checkpointsParent is now automatically found with the "CheckpointParent" tag
        }
        else
        {
            Debug.LogError(
                "No Checkpoint Parent Found!, Checkpoint System Will Be Non-Functional!"
            );
        }
        checkpoints = new Transform[checkpointsParent.childCount];
        foreach (Transform iteratingCheckpoint in checkpointsParent)
        {
            checkpoints[iteratingCheckpoint.GetComponent<Checkpoint>().index] = iteratingCheckpoint;
        }
        currentCheckpoint = checkpointsParent.childCount - 1; // The car should start on the last checkpoint, so Lap 1 can be started
        topThreeFastestLaps = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
        fastestSector1Time = float.MaxValue;
        fastestSector2Time = float.MaxValue;
        fastestSector3Time = float.MaxValue;
        fastestSectors = new float[3]
        {
            fastestSector1Time,
            fastestSector2Time,
            fastestSector3Time,
        };
        allLaps = new List<LapData>();
    }

    void OnEnable()
    {
        checkpointSystem.OnCourseCompleted += FinalRaceTime; // Switched to Using Events to Cut Down on Code Duplication and Coupling
        checkpointSystem.OnLapPassed += CompareLapTimes;
        checkpointSystem.OnSectorPassed += CompareSectorTimes;
    }

    void OnDisable()
    {
        checkpointSystem.OnCourseCompleted -= FinalRaceTime;
        checkpointSystem.OnLapPassed -= CompareLapTimes;
        checkpointSystem.OnSectorPassed -= CompareSectorTimes;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTimeFromSector += Time.deltaTime;
        elapsedTimeFromLap += Time.deltaTime;
        elapsedTimeTotal += Time.deltaTime;
        TimeSpan timeFromLap = TimeSpan.FromSeconds(elapsedTimeFromLap);
        TimeSpan timeFromSector = TimeSpan.FromSeconds(elapsedTimeFromSector);
        //Debug.Log("Time From Last Lap: " + timeFromLap.ToString(@"mm\:ss\:ff") + " Time From Last Sector: " + timeFromSector.ToString(@"mm\:ss\:ff"));
    }

    private void CompareLapTimes(int inputFinishedLap) // Called After a Lap is Completed, Called After CompareSectorTimes()
    {
        finishedLap = inputFinishedLap;
        currentLap = finishedLap + 1;
        if (inputFinishedLap == 0) // If the 0th lap has passed, the 1st lap is starting, so any timing before this is null
        {
            elapsedTimeFromLap = 0f; // Reset the timer
        }
        else
        {
            SaveLapTime();

            for (int i = 0; i < topThreeFastestLaps.Length; i++)
            { // Iterate Through the Array from the fastest to the slowest lap
                if (elapsedTimeFromLap < topThreeFastestLaps[i])
                { // If the Lap is faster than the lap stored in the current Array Index
                    for (int j = topThreeFastestLaps.Length - 1; j > 1; j--) // Go Through the Array from the end to the front
                    {
                        topThreeFastestLaps[j] = topThreeFastestLaps[j - 1]; // Move all the entries down by 1
                    }
                    topThreeFastestLaps[i] = elapsedTimeFromLap; // Set the position of the new lap time to the lap time that was beaten
                    break;
                }
            }
            elapsedTimeFromLap = 0f;
            AddLapData(lastLapTime, lastSector1Time, lastSector2Time, lastSector3Time);
            Debug.Log(
                topThreeFastestLaps[0] + " " + topThreeFastestLaps[1] + " " + topThreeFastestLaps[2]
            );
            OnLapTimesCalculated.Invoke(topThreeFastestLaps);
        }
        // This function should compare your lap time to the best three taken, if you beat any of the three, that is shown to the user on the UI
    }

    private void FinalRaceTime(CheckpointSystem cs) // Called When the Race is Completed
    {
        Debug.Log("finalRaceTime");
        totalRaceTime = elapsedTimeTotal;
        Debug.Log(totalRaceTime);
        // This function should calculate the total time to finish the race
    }

    private void CompareSectorTimes(int inputSectorIndex) // Called When a Sector Checkpoint is passed
    {
        Debug.Log("compareSectorTimes" + inputSectorIndex + " " + currentLap);
        if (currentLap < 1) // If the Current Lap is not a positive integer, something is wrong, do not save the sector time.
        {
            elapsedTimeFromSector = 0f;
        }
        else
        {
            switch (inputSectorIndex)
            {
                case 0: // When Crossing SectorIndex 0, Sector 3 has been Completed
                    if (elapsedTimeFromSector < fastestSector3Time)
                    {
                        Debug.Log("New Fastest Sector 1");
                        fastestSector3Time = elapsedTimeFromSector;
                        SaveSectorTime(3);
                    }
                    break;
                case 1: // When Crossing SectorIndex 1, Sector 1 has been Completed
                    if (elapsedTimeFromSector < fastestSector1Time)
                    {
                        Debug.Log("New Fastest Sector 2");
                        fastestSector1Time = elapsedTimeFromSector;
                        SaveSectorTime(2);
                    }
                    break;
                case 2: // When Crossing SectorIndex 2, Sector 2 has been Completed
                    if (elapsedTimeFromSector < fastestSector2Time)
                    {
                        Debug.Log("New Fastest Sector 3");
                        fastestSector2Time = elapsedTimeFromSector;
                        SaveSectorTime(1);
                    }
                    break;
                default:
                    break;
            }
            elapsedTimeFromSector = 0f;
        }

        Debug.Log(
            "Fastest Sector 1 Time: "
                + fastestSector1Time
                + " Fastest Sector 2 Time: "
                + fastestSector2Time
                + " Fastest Sector 3 Time: "
                + fastestSector3Time
        );
        // This Function Should Compare the Sector Time to the best time recorded, if you beat it, that becomes the new fastest time
        fastestSectors[0] = fastestSector1Time;
        fastestSectors[1] = fastestSector2Time;
        fastestSectors[2] = fastestSector3Time;
        OnSectorTimesCalculated.Invoke(fastestSectors);
    }

    private void SaveLapTime()
    {
        lastLapTime = elapsedTimeFromLap;
    }

    private void SaveSectorTime(int sector)
    {
        switch (sector)
        {
            case 1:
                lastSector1Time = elapsedTimeFromSector;
                break;
            case 2:
                lastSector2Time = elapsedTimeFromSector;
                break;
            case 3:
                lastSector3Time = elapsedTimeFromSector;
                break;
            default:
                break;
        }
    }

    private void AddLapData(
        float inputLapDuration,
        float inputSector1Duration,
        float inputSector2Duration,
        float inputSector3Duration
    )
    {
        Debug.Log("AddData");
        LapData tempLap = new LapData(
            inputLapDuration,
            inputSector1Duration,
            inputSector2Duration,
            inputSector3Duration
        );
        allLaps.Add(tempLap);
        Debug.Log(allLaps);
        foreach (LapData laps in allLaps)
        {
            Debug.Log(
                "Lap Duration: "
                    + laps.lapDuration
                    + " Sector 1 Time: "
                    + laps.sector1Duration
                    + " Sector 2 Time: "
                    + laps.sector2Duration
                    + " Sector 3 Time: "
                    + laps.sector3Duration
            );
        }
    }
}
