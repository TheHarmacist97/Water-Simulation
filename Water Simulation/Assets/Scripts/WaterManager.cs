using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
public class WaterManager : MonoBehaviour
{
    public WaveData[] waves;
    [SerializeField] private Material mat;
    [SerializeField] private ComputeBuffer buffer;

    private int stride;
    void Start()
    {
        DispatchData();
    }

    public void DispatchData()
    {
        stride = Marshal.SizeOf(typeof(WaveData));
        buffer = new ComputeBuffer(waves.Length, stride, ComputeBufferType.Default);
        buffer.SetData(waves);
        mat.SetInt("_NumberOfWaves", waves.Length);
        mat.SetBuffer("_Waves", buffer);
    }

    private void OnDisable()
    {
        buffer.Release();
    }

    public void DisposeBuffer()
    {
        buffer?.Dispose();
    }

    public void RandomizeWaves(int count)
    {
        waves = new WaveData[count];

        for (int i = 0; i < waves.Length; i++)
        {
            WaveData wave = waves[i];
            wave.direction = Random.insideUnitCircle.normalized;
            wave.frequency = Random.Range(0.1f, 5f);
            wave.amplitude = Random.Range(0.01f, 0.5f);
            wave.speed = Random.Range(0.4f, 3f);
            waves[i] = wave;
        }
    }
}
[Serializable]
public struct WaveData
{
    public Vector2 direction;
    [Range(0.1f, 5f)] public float frequency;
    public float amplitude;
    public float speed;
    //public float sharpness;
}

