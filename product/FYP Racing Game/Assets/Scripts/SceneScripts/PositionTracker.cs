using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    public static PositionTracker Instance;

    [SerializeField]
    private List<CheckpointSystem> carCheckpointSystems = new List<CheckpointSystem>();
    public event Action<CheckpointSystem> UpdatePosition;
    public event Action<int> UpdateNumberOfDrivers;
    private int numberOfFinishedCars;

    void Awake()
    {
        if (Instance != null && Instance != this) // Boilerplate Singleton Code, as this is a BootStrapper there should only be 1
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        UpdateCarPositionList();
        SendPositionDataToCars();
    }

    public void RegisterCarCheckpointSystem(CheckpointSystem cs)
    {
        if (!carCheckpointSystems.Contains(cs))
        {
            carCheckpointSystems.Add(cs);
            Debug.Log(cs + " Added Event");
            cs.OnCourseCompleted += CarFinishedCourse;
            UpdateNumberOfDrivers?.Invoke(carCheckpointSystems.Count);
        }
    }

    public void UnregisterCarCheckpointSystem(CheckpointSystem cs)
    {
        if (carCheckpointSystems.Contains(cs))
        {
            carCheckpointSystems.Remove(cs);
        }
    }

    public void UpdateCarPositionList()
    {
        carCheckpointSystems.Sort(CompareCarPositions);
    }

    public int CompareCarPositions(CheckpointSystem carA, CheckpointSystem carB)
    {
        if (carA.GetCurrentLap() != carB.GetCurrentLap()) // Lap is the Biggest Determiner In Car Position In a race, So Check That First
        {
            return carB.GetCurrentLap().CompareTo(carA.GetCurrentLap());
        }
        if (carA.GetCurrentCheckpoint() != carB.GetCurrentCheckpoint()) // If they're completing the same lap, Compare which checkpoint they're on
        {
            return carB.GetCurrentCheckpoint().CompareTo(carA.GetCurrentCheckpoint());
        }
        return carA.GetDistanceToNextCheckpoint().CompareTo(carB.GetDistanceToNextCheckpoint()); // If they're on the same checkpoint, check how close they are to the next checkpoint, flip carA and carB as Lower Distance To Next Checkpoint Is Better
    }

    public void SendPositionDataToCars()
    {
        int position = 1;
        foreach (CheckpointSystem car in carCheckpointSystems)
        {
            car.SetCarPositionInRace(position + numberOfFinishedCars); // If 1 Car Finishes, Even if the car is at the "Front", it cannot be 1st, its highest position is 2nd
            position++;
        }
    }

    public void CarFinishedCourse(CheckpointSystem cs)
    {
        Debug.Log("Car Finished: " + cs);
        numberOfFinishedCars += 1;
        carCheckpointSystems.Remove(cs);
    }
}
