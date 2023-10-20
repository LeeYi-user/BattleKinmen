using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Enter_start_area : MonoBehaviour
{
    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        // 根據隊伍信息選擇出生點
        if (InitScene.team == "Red")
        {
            // 玩家屬於紅隊，將其生成在紅隊出生點
            Debug.Log("分紅");
            float x = Random.Range(-5f, 5f);
            float y = Random.Range(-5f, 5f);
            pos = new Vector3(x, 2, y);
            transform.position = pos;
        }
        else if (InitScene.team == "Blue")
        {
            // 玩家屬於藍隊，將其生成在藍隊出生點
            Debug.Log("分藍");
            float x = Random.Range(15f, 25f);
            float y = Random.Range(-5f, 5f);
            pos = new Vector3(x, 2, y);
            transform.position = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}