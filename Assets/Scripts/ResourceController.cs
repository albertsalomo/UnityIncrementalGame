using UnityEngine;
using UnityEngine.UI;
using System;

public class ResourceController : MonoBehaviour
{
    public Button resourceButton;
    public Image resourceImage;
    public Text resourceDescription;
    public Text resourceUpgradeCost;
    public Text resourceUnlockCost;
    public AudioSource upgradeSound;
    public AudioSource unlockedSound;

    private ResourceConfig _config;

    private int _index;
    private int _level
    {
        set
        {
            // Menyimpan value yang di set ke _level pada Progress Data
            UserDataManager.Progress.ResourcesLevels[_index] = value;
            UserDataManager.Save(true);
        }

        get
        {
            // Mengecek apakah index sudah terdapat pada Progress Data
            if (!UserDataManager.HasResources(_index))
            {
                // Jika tidak maka tampilkan level 1
                return 1;
            }
            // Jika iya maka tampilkan berdasarkan Progress Data
            return UserDataManager.Progress.ResourcesLevels[_index];
        }
    }

    public bool IsUnlocked { get; private set; }

    private void Start()
    {
        resourceButton.onClick.AddListener(() =>
        {
            if (IsUnlocked)
            {
                UpgradeLevel();
            }
            else
            {
                UnlockResource();
            }
        });
        resourceButton.onClick.AddListener(UpgradeLevel);
    }

    public void SetConfig(int index, ResourceConfig config)
    {
        _index = index;
        _config = config;

        // ToString("0") berfungsi untuk membuang angka di belakang koma
        resourceDescription.text = $"{ _config.name } Lv. { _level }\n+{ GetOutput().ToString("0") }";
        resourceUnlockCost.text = $"Unlock Cost\n{ _config.unlockCost }";
        resourceUpgradeCost.text = $"Upgrade Cost\n{ GetUpgradeCost() }";
        SetUnlocked(_config.unlockCost == 0 || UserDataManager.HasResources(_index));
    }

    public double GetOutput()
    {
        return _config.output * _level;
    }

    public double GetUpgradeCost()
    {
        return Math.Round((_config.upgradeCost + (_level * _config.upgradeCost * (0.4 + _level * 0.5))),0,MidpointRounding.ToEven);
    }

    public double GetUnlockCost()
    {
        return _config.unlockCost;
    }

    public void UpgradeLevel()
    {
        double upgradeCost = GetUpgradeCost();
        if (UserDataManager.Progress.Gold < upgradeCost)
        {
            return;
        }

        GameManager.Instance.AddGold(-upgradeCost);
        _level++;

        resourceUpgradeCost.text = $"Upgrade Cost\n{ GetUpgradeCost() }";
        resourceDescription.text = $"{ _config.name } Lv. { _level }\n+{ GetOutput().ToString("0") }";
        upgradeSound.Play();
        AnalyticsManager.LogUpgradeEvent(_index, _level);
    }

    public void UnlockResource()
    {
        double unlockCost = GetUnlockCost();
        if (UserDataManager.Progress.Gold < unlockCost)
        {
            return;
        }

        SetUnlocked(true);
        UserDataManager.Progress.Gold -= unlockCost;
        GameManager.Instance.ShowNextResource();
        AchievementController.Instance.UnlockAchievement(AchievementType.UnlockResource, _config.name);
        unlockedSound.Play();
        AnalyticsManager.LogUnlockEvent(_index);
    }
    public void SetUnlocked(bool unlocked)
    {
        IsUnlocked = unlocked;
        if (unlocked)
        {
            // Jika resources baru di unlock dan belum ada di Progress Data, maka tambahkan data
            if (!UserDataManager.HasResources(_index))
            {
                UserDataManager.Progress.ResourcesLevels.Add(_level);
                UserDataManager.Save(true);
            }
        }
        resourceImage.color = IsUnlocked ? Color.white : Color.grey;
        resourceUnlockCost.gameObject.SetActive(!unlocked);
        resourceUpgradeCost.gameObject.SetActive(unlocked);
    }
}