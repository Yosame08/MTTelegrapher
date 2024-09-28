using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEnterLevel : MonoBehaviour {
    public ScriptScanQR ScriptScanQr;
    public string[] LevelNames;
    private SceneChanging _sceneChanging;

    public void Start() {
        _sceneChanging = GetComponent<SceneChanging>();
    }

    public void EnterLevel(bool experience) {
        ScriptScanQr.PauseScan();
        if (experience) _sceneChanging.sceneName = "TestLevel";
        else {
            // randomly choose one level
            int level = UnityEngine.Random.Range(0, LevelNames.Length);
            _sceneChanging.sceneName = LevelNames[level];
        }
        _sceneChanging.GoToScene();
    }
}
