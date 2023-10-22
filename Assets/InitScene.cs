using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitScene : MonoBehaviour
{
    // 元件用途: 初始化
    // 元件位置: InitScene 的 For Script 之下

    [SerializeField] private int fps;

    public static bool host;
    public static int team;
    public static Dictionary<ulong, int> playerTeam = new Dictionary<ulong, int>();
    public static Transform cam;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("StartScene");
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.targetFrameRate != fps)
        {
            Application.targetFrameRate = fps;
        }
    }
}
