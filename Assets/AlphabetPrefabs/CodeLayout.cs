using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CodeLayout : MonoBehaviour {
    public RectTransform[] codeImages;

    // 需要一个链表
    private List<RectTransform> _codeList = new();

    private float _height;

    // Start is called before the first frame update
    private void Start() {
        _height = codeImages[0].GetComponent<RectTransform>().rect.height;
    }
    
    public void AddCode(int id) {
        int index = id - 1;
        if (_codeList.Count > 26) {
            // List 长度大于 26 时，删除第一个元素
            Destroy(_codeList[0].gameObject);
            _codeList.RemoveAt(0);
        }
        _codeList.Add(Instantiate(codeImages[index], transform));
        _codeList.Last().gameObject.SetActive(true);
        Rearrange();
    }
    
    public void RemoveFirstCode() {
        if (_codeList.Count > 0) {
            Destroy(_codeList[0].gameObject);
            _codeList.RemoveAt(0);
            Rearrange();
        }
    }

    /// <summary>
    /// 重新排列下面显示的所有字符
    /// </summary>
    private void Rearrange() {
        for (int i = 0; i < _codeList.Count; i++) {
            RectTransform rectTransform = _codeList[i];
            rectTransform.anchoredPosition = new Vector3(i * (_height-3), 0, 0);
        }
    }
}