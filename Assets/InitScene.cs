using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitScene : MonoBehaviour
{
    // 該檔案是用來做場景控制的腳本
    // 請把它放在同名場景(InitScene)底下的 For Script 物件
    [SerializeField] private int fps;
    public static bool host; // 我把儲存是否為房間主持人的變數寫在這裡
                             // 再搭配 static 型別以及下面的 DontDestroyOnLoad, 讓他可以隨時被存取
    public static string team; // 同上, 用來記錄自己陣營的變數
    public static Dictionary<ulong, string> playerTeam = new Dictionary<ulong, string>(); // 這是房間主持人用來記錄所有人陣營的變數

    public static Transform cam;

    void Awake()
    {
        // 因為這個場景是 InitScene, 會用來初始化/記錄很多東西, 所以要用下面這個 function
        // 讓這個腳本可以不受場景切換影響, 永遠留著
        DontDestroyOnLoad(gameObject);
        // 下方程式碼是用來限制幀數的
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 切換場景到 StartScene (真正的開始畫面)
        SceneManager.LoadScene("StartScene");
    }

    // Update is called once per frame
    void Update()
    {
        // 如果不加這個, 幀數會無法真正限制
        if (Application.targetFrameRate != fps)
        {
            Application.targetFrameRate = fps;
        }
    }
}
