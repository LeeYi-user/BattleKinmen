using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Shop : NetworkBehaviour
{
    public static Shop Instance;

    [HideInInspector] public NetworkVariable<int> teamCash = new NetworkVariable<int>(1000, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [HideInInspector] public int cashSpent = 0;
    [HideInInspector] public float cashBonus = 1f;

    public GameObject shopMenu;
    public List<GameObject> categories;
    public List<GameObject> areas;
    public List<ShopItem> shopItems;
    public TextMeshProUGUI cashCounter;
    public Button backButton;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Instance = null;
    }

    private void Start()
    {
        int i = 0;

        foreach (GameObject category in categories)
        {
            int index = i;
            category.GetComponent<Button>().onClick.AddListener(() => { CategoryButtonClick(index); });
            i++;
        }

        int j = 0;

        foreach (ShopItem shopItem in shopItems)
        {
            int index = j;

            shopItem.upgradeButton.onClick.AddListener(() => { UpgradeButtonClick(index, shopItem); });
            shopItem.levelSlider.onValueChanged.AddListener(delegate { LevelUpgraded(shopItem); });
            j++;
        }

        categories[2].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = SampleSceneManager.classes[SampleSceneManager.playerClass].Substring(0, 2);

        for (int k = 0; k < 3; k++)
        {
            if (k != SampleSceneManager.playerClass)
            {
                areas.RemoveAt(k + 2 - (6 - areas.Count));
            }
        }
    }

    private void Update()
    {
        if (PlayerManager.Instance.gameStart < 2 || UnityRelay.disconnecting)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!shopMenu.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                PlayerManager.Instance.gamingScreen.SetActive(false);
                shopMenu.SetActive(true);
            }
            else
            {
                BackButtonClick();
            }
        }

        cashCounter.text = "$ " + (teamCash.Value - cashSpent).ToString();

        if (!IsHost)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            teamCash.Value += 50000;
        }
    }

    public void CategoryButtonClick(int index)
    {
        int i = 0;

        foreach (GameObject category in categories)
        {
            if (i == index)
            {
                category.GetComponent<Image>().color = new Color(166f / 255f, 112f / 255f, 78f / 255f, 1f);
            }
            else
            {
                category.GetComponent<Image>().color = new Color(166f / 255f, 112f / 255f, 78f / 255f, 0f);
            }

            i++;
        }

        int j = 0;

        foreach (GameObject area in areas)
        {
            if (j == index)
            {
                area.SetActive(true);
            }
            else
            {
                area.SetActive(false);
            }

            j++;
        }
    }

    public void UpgradeButtonClick(int index, ShopItem shopItem)
    {
        if (shopItem.levelSlider.value == shopItem.levelSlider.maxValue || shopItem.price > teamCash.Value - cashSpent)
        {
            return;
        }

        cashSpent += shopItem.price;
        shopItem.levelSlider.value++;

        if (shopItem.levelSlider.value < shopItem.levelSlider.maxValue)
        {
            if (index < 20)
            {
                shopItem.price += 200;
            }
            else
            {
                shopItem.price += 2000;
            }

            shopItem.priceText.text = shopItem.price.ToString() + " $";
        }
        else
        {
            shopItem.priceText.text = "MAX";
        }
    }

    public void LevelUpgraded(ShopItem shopItem)
    {
        LevelUpgraded_ServerRpc(NetworkManager.LocalClient.PlayerObject.NetworkObjectId, shopItem.name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LevelUpgraded_ServerRpc(ulong objectId, string name)
    {
        Player player = NetworkManager.SpawnManager.SpawnedObjects[objectId].GetComponent<Player>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        PlayerGun playerGun = player.playerWeapon.transform.GetChild(0).GetComponent<PlayerGun>();
        PlayerKnife playerKnife = player.playerWeapon.transform.GetChild(1).GetComponent<PlayerKnife>();

        switch (name)
        {
            case "health": // 需要更改 maxHealth 和 currentHealth
                player.maxHealth.Value += 25f;
                player.currentHealth.Value += 25f;
                break;
            case "movingSpeed": // 需要更改 moveSpeed
                playerMovement.moveSpeed.Value += 5f / 4f;
                break;
            case "jumpHeight": // 需要更改 jumpForce
                playerMovement.jumpForce.Value += 2f / 4f;
                break;
            case "bulletproof": // 需要更改 bulletproof
                player.bulletproof.Value += 0.1f;
                break;
            case "damage": // 需要更改 damage
                playerGun.damage.Value += 15f;
                playerKnife.damage.Value += 25f;
                break;
            case "ammo": // 需要更改 maxAmmo
                playerGun.maxAmmo.Value += 2;
                break;
            case "attackSpeed": // 需要更改 fireRate 和 attackRate
                playerGun.fireRate.Value += 0.5f / 2.5f;
                playerKnife.attackRate.Value += 1.25f / 5f;
                break;
            case "reloadSpeed": // 需要更改 reloadTime
                playerGun.reloadTime.Value -= 0.1f;
                break;
            case "grenadeCooldown": // 需要額外做技能
                player.playerWeapon.grenadeCooldown.Value -= 2f;
                break;
            case "grenadeRange": // 需要額外做技能
                player.playerWeapon.grenadeRange.Value += 0.25f;
                break;
            case "grenadeDamage": // 需要額外做技能
                player.playerWeapon.grenadeDamage.Value += 15f;
                break;
            case "grenadeDistance": // 需要額外做技能
                player.playerWeapon.grenadeDistance.Value += 100f;
                break;
            case "landmineCooldown": // 需要額外做技能
                player.playerWeapon.landmineCooldown.Value -= 2f;
                break;
            case "landmineRange": // 需要額外做技能
                player.playerWeapon.landmineRange.Value += 0.25f;
                break;
            case "landmineDamage": // 需要額外做技能
                player.playerWeapon.landmineDamage.Value += 15f;
                break;
            case "landmineLimit": // 需要額外做技能
                player.playerWeapon.landmineLimit.Value += 1f;
                break;
            case "healCooldown": // 需要額外做技能
                player.playerWeapon.healCooldown.Value -= 5f;
                break;
            case "healRange": // 需要額外做技能
                player.playerWeapon.healRange.Value += 1.5f;
                break;
            case "healAmount": // 需要額外做技能
                player.playerWeapon.healAmount.Value += 25f;
                break;
            case "healInv": // 需要額外做技能
                player.playerWeapon.healInv.Value += 0.5f;
                break;
            case "respawnSpeed": // 需要更改 respawnCooldown
                PlayerManager.Instance.respawnCooldown *= 1f / Mathf.Pow(2f, 1f / 3f);
                break;
            case "enemyDelay": // 需要更改 enemyDelay
                EnemySpawn.Instance.enemyDelay += 0.1f;
                break;
            case "cashBonus": // 需要更改 cashBonus
                cashBonus += 0.1f;
                break;
            case "mapDefense": // 需要更改 maxDefense 和 currentDefense
                MainSceneManager.Instance.currentDefense += 1;

                if (MainSceneManager.Instance.maxDefense < MainSceneManager.Instance.currentDefense)
                {
                    MainSceneManager.Instance.maxDefense = MainSceneManager.Instance.currentDefense;
                }

                break;
            default:
                break;
        }
    }

    public void BackButtonClick()
    {
        Cursor.lockState = CursorLockMode.Locked;
        PlayerManager.Instance.gamingScreen.SetActive(true);
        shopMenu.SetActive(false);
    }
}
