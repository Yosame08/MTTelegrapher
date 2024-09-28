using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCodeMouse : MonoBehaviour {
    public Transform observers, operators;
    
    public void showObserver() {
        observers.gameObject.SetActive(true);
        operators.gameObject.SetActive(false);
    }
    
    public void showOperator() {
        observers.gameObject.SetActive(false);
        operators.gameObject.SetActive(true);
    }
}
