using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZXing;

///扫描二维码 -> 识别二维码信息
public class ScriptScanQR : MonoBehaviour {
    private enum TypeQR {
        User,
        Codewords
    }
    
    private TypeQR _typeQR;

    private BarcodeReader _barcodeReader; //库文件的对象（二维码信息保存的位置）

    private bool _isScanning; //扫描开关
    private float _interval; //扫描识别时间间隔
    private WebCamTexture _webCamTexture; //摄像机映射纹理
    private Color32[] _data; //让信息以像素点的形式 按照数据存放

    private WebCamDevice[] _devices; //摄像机硬件设备
    private long _deviceIndex; //摄像机索引

    [FormerlySerializedAs("CameraTexture")]
    public RawImage cameraTexture; //摄像机映射显示区域

    public TMP_Text text, hint, leftInfoText, rightInfoText;
    
    public RectTransform confirmPanel; //确认面板
    public CodewordLayout leftLayout, rightLayout;
    public Button startButton, expButton, noPointButton;

    private UserInfo _tempInfo;
    private List<Codeword> _tempCodewords;
    
    public static UserInfo LeftInfo, RightInfo;
    public static List<Codeword> LeftCodewords, RightCodewords;

    private void Start() {
        DeviceInit(); //开始摄像机
        _isScanning = true; //打开扫描开关
        text.text = null; //清空文本
        hint.text = "请将二维码放入框内"; //提示信息
    }

    private void Update() {
        if (_isScanning) //每秒扫描一次
        {
            _interval += Time.deltaTime;
            if (_interval >= 1) {
                _interval = 0;
                ScanCode(); //开始扫描
            }
        }
    }

    /// <summary>
    /// 开启摄像机 前期准备工作
    /// </summary>
    void DeviceInit() {
        _devices = WebCamTexture.devices; //获取所有摄像机的硬件 比如前置 后置
        _deviceIndex = 0; //默认使用第一个摄像头
        _webCamTexture = new WebCamTexture(_devices[0].name, 384, 384); //创建一个摄像机显示的区域 （device[0]一般是后置摄像头，400,300为大小）
        cameraTexture.texture = _webCamTexture; //显示图片信息
        _webCamTexture.Play(); //打开摄像机进行识别
        _barcodeReader = new BarcodeReader(); //实例化二维码信息，并存储对象
    }

    /// <summary>
    /// 切换到下一个摄像头
    /// </summary>
    public void ChangeCamera() {
        _deviceIndex++; //索引加一
        if (_deviceIndex >= _devices.Length) //如果索引大于摄像头数量
        {
            _deviceIndex = 0; //则返回第一个摄像头
        }

        _webCamTexture.Stop(); //停止摄像头
        _webCamTexture = new WebCamTexture(_devices[_deviceIndex].name, 512, 432); //重新创建一个摄像头
        cameraTexture.texture = _webCamTexture; //显示摄像头信息
        _webCamTexture.Play(); //打开摄像头
    }

    /// <summary>
    /// 识别二维码信息
    /// </summary>
    /// <example>"[WorldTreeII]codewords=111`测试111`234`测试234`114514`这是114514啊`"</example>
    void ScanCode() {
        Debug.Log("Scanning...");
        _data = _webCamTexture.GetPixels32(); //获取摄像机中的像素点数组的信息
        Result result = _barcodeReader.Decode(_data, _webCamTexture.width, _webCamTexture.height); //获取二维码上的信息
        if (result != null) //判断是否有信息 有则识别成功
        {
            Debug.Log("Result: " + result.Text);
            text.text = result.Text; //显示 二维码上的信息
            if (!result.Text.StartsWith("[WorldTreeII]")) return;
            // 截取开头部分后面的字符串
            string str = result.Text.Substring(13);
            if (str.StartsWith("token=")) {
                string token = str.Substring(6);
                hint.text = "正在向服务器请求用户信息...";
                PauseScan();
                _typeQR = TypeQR.User;
                APIClient.GetUserData(this, token, UserDataCallback);
            }
            else if (str.StartsWith("codewords=")) {
                string codewords = str.Substring(10);
                string[] parts = codewords.Split('`');
                if (parts.Length % 2 != 1) {
                    hint.text = "识别到错误的二维码信息";
                    return;
                }
                PauseScan();
                _typeQR = TypeQR.Codewords;
                _tempCodewords = new List<Codeword>();
                for (int i = 0; i < parts.Length - 1; i += 2) { // 末尾为`空，所以减一
                    Codeword codeword = new Codeword();
                    char[] chars = parts[i].ToCharArray();
                    codeword.chars = new int[chars.Length];
                    for (int j = 0; j < chars.Length; j++) {
                        codeword.chars[j] = chars[j] - '0';
                    }
                    string txt = parts[i + 1];
                    codeword.meaning = txt;
                    _tempCodewords.Add(codeword);
                }
                hint.text = "识别成功，请选择码字的方向";
                confirmPanel.gameObject.SetActive(true);
            }
            else hint.text = "识别到错误的二维码信息";
        }
    }

    /// <summary>
    /// 从服务器获取用户信息的回调函数
    /// </summary>
    void UserDataCallback(ResponseData responseData, string error) {
        if (responseData is not { code: 0 }) {
            hint.text = responseData is null ? error : responseData.message;
            ReStartScan();
        }
        else {
            hint.text = "登录用户" + responseData.data.username + "，请选择身份";
            confirmPanel.gameObject.SetActive(true);
            _tempInfo = responseData.data;
            Debug.Log("用户ID: " + responseData.data.id);
        }
    }

    public void LeftConfirm() {
        switch (_typeQR) {
            case TypeQR.User:
                LeftInfo = _tempInfo;
                hint.text = "左侧用户信息已确认";
                leftInfoText.text = "用户名：\n" + LeftInfo.username + "\n积分：\n" + LeftInfo.point + "\n货币：\n" + LeftInfo.coin;
                break;
            case TypeQR.Codewords:
                LeftCodewords = _tempCodewords;
                hint.text = "左侧码字已确认";
                leftLayout.RefreshImages();
                break;
        }
        ConfirmEnd();
    }

    public void RightConfirm() {
        switch (_typeQR) {
            case TypeQR.User:
                RightInfo = _tempInfo;
                hint.text = "右侧用户信息已确认";
                rightInfoText.text = "用户名：\n" + RightInfo.username + "\n积分：\n" + RightInfo.point + "\n货币：\n" + RightInfo.coin;
                break;
            case TypeQR.Codewords:
                RightCodewords = _tempCodewords;
                hint.text = "右侧码字已确认";
                rightLayout.RefreshImages();
                break;
        }
        ConfirmEnd();
    }

    public void ConfirmEnd() {
        _tempInfo = null;
        confirmPanel.gameObject.SetActive(false);
        if (CheckStart()) hint.text = "点击按钮开始游戏";
        ReStartScan();
    }
    
    private bool CheckStart() {
        bool ok = !(LeftInfo is null || RightInfo is null || LeftCodewords is null || RightCodewords is null);
        startButton.gameObject.SetActive(ok);
        expButton.gameObject.SetActive(ok);
        noPointButton.gameObject.SetActive(ok);
        if (ok) {
            _webCamTexture.Stop();
            _webCamTexture = null;
        }
        else {
            _webCamTexture = new WebCamTexture(_devices[_deviceIndex].name, 512, 432); //重新创建一个摄像头
            cameraTexture.texture = _webCamTexture; //显示摄像头信息
            _webCamTexture.Play(); //打开摄像头
        }
        return ok;
    }

    public void ResetLeftInfo() {
        LeftInfo = null;
        CheckStart();
        hint.text = "观察者用户信息已重置";
        leftInfoText.text = "等待加入...";
    }

    public void ResetRightInfo() {
        RightInfo = null;
        CheckStart();
        hint.text = "操作者用户信息已重置";
        rightInfoText.text = "等待加入...";
    }
    
    public void ResetLeftCode() {
        LeftCodewords = null;
        leftLayout.RefreshImages();
        CheckStart();
        hint.text = "观察者码字已重置";
    }
    
    public void ResetRightCode() {
        RightCodewords = null;
        rightLayout.RefreshImages();
        CheckStart();
        hint.text = "操作者码字已重置";
    }

    public void PauseScan() {
        _isScanning = false;
        _webCamTexture.Stop();
    }

    private void ReStartScan() {
        if (_webCamTexture) {
            _isScanning = true;
            _webCamTexture.Play();
        }
        _interval = 0;
    }
}