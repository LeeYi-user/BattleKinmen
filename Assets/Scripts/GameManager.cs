using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private int fps;
    [SerializeField] private GameObject popup;

    public List<string> popups;

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
        SceneManager.LoadScene("SampleScene");
    }

    private void Update()
    {
        if (Application.targetFrameRate != fps)
        {
            Application.targetFrameRate = fps;
        }
    }

    public void Popup(string msg)
    {
        bool exist = false;

        foreach (string str in popups)
        {
            if (str == msg)
            {
                exist = true;
                break;
            }
        }

        if (!exist)
        {
            popups.Add(msg);
        }

        GameObject popGO = Instantiate(popup, popup.transform.position - popups.IndexOf(msg) * new Vector3(0, 25, 0), Quaternion.identity);

        popGO.transform.SetParent(GameObject.Find("Canvas").transform, false);
        popGO.GetComponent<TextMeshProUGUI>().text = msg;
    }

    public Vector3 RandomPosition(List<Transform> area)
    {
        Transform grid = area[Random.Range(0, area.Count)];

        float x = grid.position.x + Random.Range(-grid.localScale.x, grid.localScale.x) / 2;
        float y = grid.position.y + Random.Range(-grid.localScale.y, grid.localScale.y) / 2;
        float z = grid.position.z + Random.Range(-grid.localScale.z, grid.localScale.z) / 2;

        return new Vector3(x, y, z);
    }

    public bool CheckGrid(Vector3 position, Transform grid)
    {
        if (Mathf.Abs(position.x - grid.position.x) <= grid.localScale.x / 2 &&
            Mathf.Abs(position.y - grid.position.y) <= grid.localScale.y / 2 &&
            Mathf.Abs(position.z - grid.position.z) <= grid.localScale.z / 2)
        {
            return true;
        }

        return false;
    }
}
