using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class InitManager : MonoBehaviour
{
    public static InitManager Instance;

    [SerializeField] private int fps;

    private void Awake()
    {
        Instance = this;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SwitchProfile(Random.Range(int.MinValue, int.MaxValue).ToString());
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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
