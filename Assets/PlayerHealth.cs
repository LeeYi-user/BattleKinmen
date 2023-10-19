using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour // 這個腳本跟網路有關, 所以要用 NetworkBehavior
{
    // 該檔案是用來控制玩家生命值的, 請把它放在玩家物件之下
    public Image healthBar;
    public float maxHealth = 100f;
    public float currentHealth;

    [SerializeField] private SkinnedMeshRenderer skin;
    [SerializeField] private Material blue;
    [SerializeField] private Material red;

    Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        // 如果當前的玩家物件不是自己, 就直接 return
        if (!IsOwner)
        {
            return;
        }
        // 否則就抓取 HealthBar 物件, 以便正確顯示生命值
        healthBar = GameObject.Find("Health").GetComponent<Image>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // 測試用, 按下 O 鍵便可受傷
        if (Input.GetKeyDown(KeyCode.O))
        {
            TakeDamage(10);
        }
    }

    // 這個 function 會在之後寫攻擊腳本時用到, 所以要先弄成 public
    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // 當前生命值 - 受到的傷害

        if (healthBar) // 因為前面有弄一個 if (!IsOwner) return, 所以非你控制的玩家物件都不會抓取 HealthBar 物件
                       // 所以要加這行才能避免出 Bug (這樣寫好像有點怪, 之後有空再修)
        {
            healthBar.fillAmount = currentHealth / maxHealth; // 控制 UI slide
        }

        StartCoroutine(DamageFlash());
    }

    IEnumerator DamageFlash()
    {
        skin.material = red;
        yield return new WaitForSeconds(0.15f);
        skin.material = blue;
    }
}
