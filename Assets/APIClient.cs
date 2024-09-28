using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public static class APIClient {
    // private const string URLGetPlayed = "http://192.168.144.8:8080/api/subtask/telegram/get_played";
    // private const string URLUserInfo = "http://192.168.144.8:8080/api/user/get_info_by_random_string";
    // private const string URLSubmission = "http://192.168.144.8:8080/api/subtask/telegram/submit";
    private const string URLGetPlayed = "http://10.20.26.32:8080/api/subtask/telegram/get_played";
    private const string URLUserInfo = "http://10.20.26.32:8080/api/user/get_info_by_random_string";
    private const string URLSubmission = "http://10.20.26.32:8080/api/subtask/telegram/submit";
    private const int Timeout = 8;
    private static readonly string FilePath = Path.Combine(Application.dataPath, "submission_log.txt");

    public static void SubmitResult(MonoBehaviour monoBehaviour, string userA, string userB, int mapID, double score,
        double monsterScore, double time, double hp) {
        // 写入信息到本地文件
        string logEntry = $"{userA}, {userB}, {mapID}, {score}, {monsterScore}, {time}, {hp}";
        File.AppendAllText(FilePath, logEntry + "\n");

        monoBehaviour.StartCoroutine(PostRequest(userA, userB, mapID, score, monsterScore, time, hp, logEntry));
    }

    private static IEnumerator PostRequest(string userA, string userB, int mapID, double score, double monsterScore, double time, double hp, string logEntry) {
        string jsonData = "{\"rank\":998244000,\"usernameA\":\"" + userA + "\",\"usernameB\":\"" + userB + "\",\"telegramId\":" + mapID + ",\"score\":" + score + ",\"monsterScore\":" + monsterScore + ",\"time\":" + time + ",\"hp\":" + hp + "}";

        // 创建 UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(URLSubmission, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = Timeout;

        // 发送请求并等待响应
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError("Error: " + request.error);
        } else {
            Debug.Log("Form upload complete! Response: " + request.downloadHandler.text);
            // 解析响应
            string responseText = request.downloadHandler.text;
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(responseText);

            // 检查服务器返回的 code 字段
            if (responseData.code == 0) {
                // 追加 "success" 到本地文件
                File.AppendAllText(FilePath, logEntry + " success\n");
            }
        }
    }
    
    public static void GetUserData(MonoBehaviour monoBehaviour, string token, System.Action<ResponseData, string> callback) {
        monoBehaviour.StartCoroutine(PostRandomString(token, callback));
    }
    
    private static IEnumerator PostRandomString(string token, System.Action<ResponseData, string> callback) {
        // 创建UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(URLUserInfo, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{\"value\":\"" + token + "\"}");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = Timeout;

        // 发送请求并等待响应
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError) {
            callback(null, request.error);
        }
        else {
            // 解析响应
            string responseText = request.downloadHandler.text;
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(responseText);

            if (responseData.code == 0) {
                Debug.Log("通信成功");
                callback(responseData, null);
            }
            else {
                callback(null, responseData.message);
            }
        }
    }
    
    public static void GetRandLevel(MonoBehaviour monoBehaviour, string usernameA, string usernameB, System.Action<int> callback) {
        monoBehaviour.StartCoroutine(GetRandLevelRequest(usernameA, usernameB, callback));
    }
    
    private static IEnumerator GetRandLevelRequest(string usernameA, string usernameB, System.Action<int> callback) {
        // 创建请求数据对象
        var requestData = new {
            usernameA = usernameA,
            usernameB = usernameB
        };

        // 序列化为 JSON
        string jsonData = "{\"usernameA\":\"" + usernameA + "\",\"usernameB\":\"" + usernameB + "\"}";

        // 创建 UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(URLGetPlayed, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = Timeout;

        // 发送请求并等待响应
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError("Error: " + request.error);
            callback(0); // 处理错误情况
        } else {
            // 解析响应
            string responseText = request.downloadHandler.text;
            IntData responseData = JsonUtility.FromJson<IntData>(responseText);

            if (responseData.code == 0) {
                // 调用 callback 并传递数据
                callback(responseData.data);
            } else {
                Debug.LogError("Error: " + responseData.message);
                callback(0); // 处理非正常响应
            }
        }
    }
}

// 定义响应数据结构
[System.Serializable]
public class ResponseData {
    public int code;
    public string message;
    public UserInfo data;
}

[System.Serializable]
public class UserInfo {
    public int id;
    public string username;
    public string password;
    public string nickname;
    public string email;
    public string userPic;
    public string createTime;
    public string updateTime;
    public int coin;
    public int point;
}

[System.Serializable]
public class IntData {
    public int code;
    public string message;
    public int data;
}