using UnityEditor;
using UnityEngine;

public class CrossSceneData : MonoBehaviour
{
    public static CrossSceneData Instance;
    private bool isRace;
    private int trackConfiguration;
    private int numberOfOpponents;
    public GameObject selectedCar;

    void Awake()
    {
        if (Instance != null && Instance != this) // This MUST be a singleton as all inputs go through this script (and the GameObject associated with it)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetIsRace(bool inputIsRace)
    {
        isRace = inputIsRace;
    }

    public bool GetIsRace()
    {
        return isRace;
    }

    public void SetTrackConfiguration(int inputTrackID)
    {
        trackConfiguration = inputTrackID;
    }

    public int GetTrackConfiguration()
    {
        return trackConfiguration;
    }

    public void SetNumberOfOpponents(int inputNoOfOpponents)
    {
        numberOfOpponents = inputNoOfOpponents;
    }

    public int GetNumberOfOpponents()
    {
        return numberOfOpponents;
    }

    public void SetSelectedCar(GameObject inputCurrentCarPrefab)
    {
        selectedCar = inputCurrentCarPrefab;
    }

    public GameObject GetSelectedCar()
    {
        return selectedCar;
    }
}
