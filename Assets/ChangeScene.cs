using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    float timeleft;
    Image background;
    Color targetColor;

    // Start is called before the first frame update
    void Start()
    {
        timeleft = 3;
        background = gameObject.GetComponent<Image>();
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
            StartCoroutine(GameStart());
        }
    }

    IEnumerator GameStart()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("SampleScene");
    }
}
