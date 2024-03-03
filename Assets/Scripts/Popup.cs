using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Popup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Color targetColor;
    private float timeLeft;

    // Start is called before the first frame update
    void Start()
    {
        targetColor = new Color(1, 1, 1, 1);
        timeLeft = 0.25f;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft <= Time.deltaTime)
        {
            text.color = targetColor;
            Destroy(gameObject, 0.75f);
        }
        else
        {
            text.color = Color.Lerp(text.color, targetColor, Time.deltaTime / timeLeft);
            timeLeft -= Time.deltaTime;
        }
    }
}
