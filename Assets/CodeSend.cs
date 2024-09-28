using UnityEngine;
using UnityEngine.Serialization;

public class CodeSend : MonoBehaviour {
    public CodeLayout operatorSend;
    public CodeLayout observerSend;
    private readonly bool[] _keyOperator = new bool[5];
    private readonly bool[] _keyObserver = new bool[5];

    // Update is called once per frame
    void Update() {
        CheckSend(_keyOperator, operatorSend, new[] {KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B});
        CheckSend(_keyObserver, observerSend, new[] {KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5});
        
        if (Input.GetKeyDown(KeyCode.G)) {
            observerSend.RemoveFirstCode();
        }

        if (Input.GetKeyDown(KeyCode.Backspace)) {
            operatorSend.RemoveFirstCode();
        }
    }

    void CheckSend(bool[] pressed, CodeLayout sender, KeyCode[] keys) {
        int send = 0;
        for (int check = 1; check <= 5; ++check) {
            if (Input.GetKeyDown(keys[check-1]) && !pressed[check-1]) {
                send = check;
                pressed[check-1] = true;
            }
            if (Input.GetKeyUp(keys[check-1])) pressed[check-1] = false;
        }
        if (send != 0) sender.AddCode(send);
    }
}