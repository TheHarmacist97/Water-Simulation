using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;
public class WaterManager : MonoBehaviour
{
    [Range(1f, 1.3f)] public float lacunarity;
    [Range(0.5f, 1f)] public float gain;
    [Range(-0.1f, 1.5f)]public float sharpness;
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
        SetSharpness();
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
        float originalFrequency = Random.Range(1.2f, 4.5f);
        float originalAmplitude = Random.Range(0.1f, 0.34f);
        //float originalSharpness = Random.Range(-0.75f, 0.75f);
        for (int i = 0; i < waves.Length; i++)
        {
            WaveData wave = waves[i];
            wave.direction = Random.insideUnitCircle.normalized;
            wave.frequency = originalFrequency * Mathf.Pow(lacunarity, i);
            wave.amplitude = originalAmplitude * Mathf.Pow(gain, i);
            wave.speed = Random.Range(0.4f, 3f);
            wave.sharpness = sharpness;
            waves[i] = wave;
        }
    }

    public void SetSharpness()
    {
        for (int i = 0;i < waves.Length;i++)
        {
            WaveData wave = waves[i];
            wave.sharpness = sharpness;
            waves[i] = wave;
        }
    }
        
}
[Serializable]
public struct WaveData
{
    public Vector2 direction;
    [Range(0.2f, 10f)] public float frequency;
    public float amplitude;
    public float speed;
    [Range(-1f, 1f)] public float sharpness;
}

