using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitSceneManager : MonoBehaviour
{
    [SerializeField] private int fps;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    private void Start()
    {
        SceneManager.LoadScene("MenuScene");
    }

    private void Update()
    {
        if (Application.targetFrameRate != fps)
        {
            Application.targetFrameRate = fps;
        }
    }
}
