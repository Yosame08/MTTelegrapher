using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CodewordLayout : MonoBehaviour
{
    public enum CodewordType
    {
        Left,
        Right
    }
    
    public CodewordType codewordType;
    public RectTransform[] codeImages;
    private TMP_Text _codeText;
    private List<RectTransform> _created = new List<RectTransform>();
    private float _lineHeight;
    
    // Start is called before the first frame update
    void Start()
    {
        _codeText = GetComponentInChildren<TMP_Text>();
        _lineHeight = codeImages[0].rect.height;
        switch (codewordType) {
            case CodewordType.Left:
                if (ScriptScanQR.LeftCodewords != null) RefreshImages();
                break;
            case CodewordType.Right:
                if (ScriptScanQR.RightCodewords != null) RefreshImages();
                break;
        }
    }

    public void RefreshImages() {
        List<Codeword> codewords = codewordType == CodewordType.Left ? ScriptScanQR.LeftCodewords : ScriptScanQR.RightCodewords;
        foreach (RectTransform rectTransform in _created) {
            Destroy(rectTransform.gameObject);
        }
        _created.Clear();
        if (codewords is null) {
            _codeText.text = codewordType == CodewordType.Left ? "观察者的码字" : "操作者的码字";
            return;
        }
        string text = "";
        for (int i = 0; i < codewords.Count; i++) {
            float height = i * _lineHeight;
            for (int j = 0; j < codewords[i].chars.Length; j++) {
                RectTransform rectTransform = Instantiate(codeImages[codewords[i].chars[j] - 1], transform);
                rectTransform.anchoredPosition = new Vector2(j * _lineHeight, -height);
                _created.Add(rectTransform);
            }
            text += codewords[i].meaning + '\n';
        }
        _codeText.text = text;
    }
}
