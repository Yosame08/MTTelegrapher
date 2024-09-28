using System;
using TMPro;
using UnityEngine;

public class MonsterInstruction : MonoBehaviour {
    public RectTransform panel;
    public TMP_Text value;
    public CharInfo braver;
    private const int _inf = 11451419;
    private bool _isThis;

    private void OnMouseEnter() {
        _isThis = true;
        panel.position = Input.mousePosition;
        CharProperty charProperty = GetComponent<CharProperty>();
        // 处理获取到的charInfo
        if (charProperty is not null) {
            int pred = Predict(braver.property, charProperty);
            value.text = charProperty.charName + '\n' +
                          charProperty.hp + '\n' +
                          charProperty.atk + '\n' +
                          charProperty.def + '\n' +
                          charProperty.speed + '\n' +
                          charProperty.timer.ToString("F1") + "/" + CharMove.actionTime + "\n" +
                          charProperty.battle + '\n' + 
                          charProperty.coolDown + '\n' +
                          (pred == _inf ? "INF" : pred.ToString());
        }
        else {
            value.text = "Error";
        }
        
        // 获取屏幕高度
        float screenHeight = Screen.height;
        // 计算屏幕底部13%的位置
        float minYPosition = screenHeight * 0.13f;
        // 获取panel的高度
        float panelHeight = panel.rect.height;
        // 确保panel的底部位置不会低于屏幕底部13%的位置
        if (panel.position.y - panelHeight < minYPosition) {
            panel.position = new Vector3(panel.position.x, minYPosition + panelHeight, panel.position.z);
        }
        
        panel.gameObject.SetActive(true);
    }

    private void OnMouseExit() {
        _isThis = false;
        panel.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        if (_isThis) {
            panel.gameObject.SetActive(false);
        }
    }

    private int Predict(CharProperty b, CharProperty m) {
        if (b.atk <= m.def) return _inf;
        int round = Ceil(m.hp, b.atk - m.def);
        double sec = round * CharMove.actionTime / b.speed;
        int mRound = (int)(sec * m.speed / CharMove.actionTime);
        int mbRound = (mRound + m.battle - m.coolDown) / (m.battle + 1);
        int damaged = mRound * Math.Max(m.atk - b.def, 0) + mbRound * m.atk;
        return damaged;
    }

    private int Ceil(int a, int b) {
        return (int)Math.Ceiling(a/(double)b);
    }
    private int Ceil(double a, double b) {
        return (int)Math.Ceiling(a/b);
    }
}
