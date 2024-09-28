using TMPro;
using UnityEngine;

public class JumpToLevel : MonoBehaviour {
    public TMP_Text hint;

    public void QueryLevel() {
        GlobalVar.HasPoint = true;
        APIClient.GetRandLevel(this, ScriptScanQR.LeftInfo.username, ScriptScanQR.RightInfo.username, ToLevel);
    }

    public void ToLevel(int level) {
        if (level == 0) {
            hint.text = "没有双方都未玩过的地图";
        }
        else {
            GlobalVar.Level = level;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TestLevel");
        }
    }

    public void RandLevel() {
        GlobalVar.HasPoint = false;
        ToLevel(Random.Range(1, 4));
    }
    
}
