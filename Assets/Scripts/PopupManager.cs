using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    public GameObject canvas;
    public GameObject popup;

    public List<string> popups;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
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

        popGO.transform.SetParent(canvas.transform, false);
        popGO.GetComponent<TextMeshProUGUI>().text = msg;
    }
}
