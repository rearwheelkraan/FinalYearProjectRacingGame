using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.VisualScripting;
using UnityEngine;

public class EngineSoundController : MonoBehaviour
{
    public float currentRPM;
    public VehicleConfiguration carConfig;

    [SerializeField]
    public List<EngineSoundLoop> engineSoundLoops;

    [SerializeField]
    public GameObject engineSoundEmpty;

    void Start()
    {
        carConfig = transform.root.GetComponent<VehicleConfiguration>();
        engineSoundLoops = carConfig.engineSoundLoops;
        foreach (var engineSoundLoop in engineSoundLoops)
        {
            engineSoundLoop.audioSource = engineSoundEmpty.AddComponent<AudioSource>();
            engineSoundLoop.audioSource.clip = engineSoundLoop.engineLoop;
            engineSoundLoop.audioSource.loop = true;
            engineSoundLoop.audioSource.playOnAwake = true;
            engineSoundLoop.audioSource.volume = 0f;
            engineSoundLoop.audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var engineSoundLoop in engineSoundLoops)
        {
            float volume = 0f;
            if (currentRPM >= engineSoundLoop.minRPM && currentRPM <= engineSoundLoop.maxRPM)
            {
                int range = engineSoundLoop.maxRPM - engineSoundLoop.minRPM;
                float proximityToOptimalRPM = Mathf.Abs(currentRPM - engineSoundLoop.optimalRPM);
                volume = 1f - (proximityToOptimalRPM / range);
            }
            engineSoundLoop.audioSource.volume = volume;

            float pitch = currentRPM / engineSoundLoop.optimalRPM;
            engineSoundLoop.audioSource.pitch = pitch;
        }
    }

    public void SetRPM(float inputRPM)
    {
        currentRPM = inputRPM;
    }
}
