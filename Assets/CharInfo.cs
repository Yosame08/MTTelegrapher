using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharInfo : MonoBehaviour {
    
    public enum Property {
        Hp,
        Atk,
        Def,
        Speed,
        Regen
    }
    public TMP_Text charInfo, logText;
    public TMP_Text gameOverText;
    public Transform gameOverPanel;
    public CharMove charMove;
    public CharProperty property;
    private CircularArray<string> _logArray = new(10);

    private void Start() {
        logText.text = "";
    }

    void RefreshCharacter() {
        string txt = property.hp.ToString() + '\n' +
                     property.atk + '\n' +
                     property.def + '\n' +
                     property.speed + '\n' +
                     property.regen + " hp/s";
        charInfo.text = txt;
    }
    
    public void Regenerate() {
        ModifyProperty(Property.Hp, property.regen);
    }
    
    public bool ModifyProperty(Property type, int value) {
        switch (type) {
            case Property.Hp:
                if (value < 0 && property.hp <= 0) return false;
                property.hp += value;
                if (property.hp <= 0) {
                    // property.hp = 1;
                    RefreshCharacter();
                    return false;
                    // GameOver("生命值归零，游戏结束。", 0);
                }
                break;
            case Property.Atk:
                property.atk += value;
                break;
            case Property.Def:
                property.def += value;
                break;
            case Property.Speed:
                property.speed += value;
                break;
            case Property.Regen:
                property.regen += value;
                break;
        }
        RefreshCharacter();
        return true;
    }

    public void GameOver(string reason, float score, int leftTime) {
        charMove.enabled = false;
        gameOverPanel.gameObject.SetActive(true);
        int point = (int)Math.Ceiling(200 * Math.Tanh((double)score / 1250));
        gameOverText.text = reason + "\n\n您的得分：" + score + "\n\n换算积分：" + point;
        if (GlobalVar.HasPoint && GlobalVar.Level != 4) {
            APIClient.SubmitResult(this, ScriptScanQR.LeftInfo.username, ScriptScanQR.RightInfo.username, GlobalVar.Level,
                score, 0, leftTime, 0);
        }
        ScriptScanQR.LeftInfo = ScriptScanQR.RightInfo = null;
        ScriptScanQR.LeftCodewords = ScriptScanQR.RightCodewords = null;
    }

    public void AddLog(string log) {
        _logArray.Add(log);
        UpdateLog();
    }

    void UpdateLog() {
        string[] logs = _logArray.GetItems();
        List<string> formattedLogs = new List<string>();

        foreach (string log in logs) {
            if (log?.Length > 12) {
                for (int i = 0; i < log.Length; i += 12) {
                    formattedLogs.Add(log.Substring(i, Mathf.Min(12, log.Length - i)));
                }
            }
            else {
                formattedLogs.Add(log);
            }
        }

        logText.text = string.Join("\n", formattedLogs.ToArray());
    }
}

