using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaterManager))]
public class WaterManagerInspector : Editor
{
    private WaterManager waterManager;
    private int waveCount = 1;
    public override void OnInspectorGUI()
    {
        waterManager = (WaterManager)target;
        base.OnInspectorGUI();
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
            GUILayout.Label("Wave count");
            waveCount = EditorGUILayout.IntSlider(waveCount, 1, 48);
        GUILayout.EndHorizontal();

        if(GUILayout.Button("Generate " + waveCount + " new wave" + (waveCount>1?"s":"")))
        {
            waterManager.RandomizeWaves(waveCount);
        }
        GUILayout.Space(10);

        if(GUILayout.Button("Dispatch Data"))
        {
            waterManager.DispatchData();
        }
        GUILayout.Space(10);
        if(GUILayout.Button("Dispose Buffer"))
        {
            waterManager.DisposeBuffer();
        }
    }
}
