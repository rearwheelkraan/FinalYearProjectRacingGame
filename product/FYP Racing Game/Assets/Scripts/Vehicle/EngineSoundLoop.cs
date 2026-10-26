using UnityEngine;

[System.Serializable]
public class EngineSoundLoop
{
    public AudioClip engineLoop;
    public int minRPM;
    public int maxRPM;
    public int optimalRPM;
    public AudioSource audioSource;
}
