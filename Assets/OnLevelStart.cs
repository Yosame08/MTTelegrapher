using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnLevelStart : MonoBehaviour {
    public Transform[] levels;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var level in levels) {
            level.gameObject.SetActive(false);
        }
        levels[GlobalVar.Level-1].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
