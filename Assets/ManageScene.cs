using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ManageScene : MonoBehaviour
{
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject teamPanel;

    float timeleft;
    Image background;
    Color targetColor;

    // Start is called before the first frame update
    void Start()
    {
        timeleft = 3;
        background = storyPanel.GetComponent<Image>();
        targetColor = new Color(0, 0, 0, 100);
    }

    // Update is called once per frame
    void Update()
    {
        if (timeleft > Time.deltaTime)
        {
            background.color = Color.Lerp(background.color, targetColor, Time.deltaTime / timeleft);
            timeleft -= Time.deltaTime;
        }
        else
        {
            background.color = targetColor;
            StartCoroutine(ChooseTeam());
        }
    }

    IEnumerator ChooseTeam()
    {
        yield return new WaitForSeconds(2);
        storyPanel.SetActive(false);
        teamPanel.SetActive(true);
    }

    // 加入國軍
    public void BlueTeam()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // 加入共軍
    public void RedTeam()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
