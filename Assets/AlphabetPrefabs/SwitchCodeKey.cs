using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCodeKey : MonoBehaviour
{
    public Transform observers, operators;
    private bool _isObserver = true;
    private bool _pressing = false;

    private void Update() {
        if (Input.GetKey(KeyCode.F) && !_pressing) {
            _pressing = true;
            if (_isObserver) {
                showOperator();
                _isObserver = false;
            }
            else {
                showObserver();
                _isObserver = true;
            }
        }
        else if (!Input.GetKey(KeyCode.G)) {
            _pressing = false;
        }
    }

    private void showObserver() {
        observers.gameObject.SetActive(true);
        operators.gameObject.SetActive(false);
    }
    
    private void showOperator() {
        observers.gameObject.SetActive(false);
        operators.gameObject.SetActive(true);
    }
}
