using UnityEngine;

public class HeightAdjust : MonoBehaviour
{
    private RectTransform _rectTransform;
    private float _height;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _height = _rectTransform.rect.height;
    }

    private void Update()
    {
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _height);
    }
}
