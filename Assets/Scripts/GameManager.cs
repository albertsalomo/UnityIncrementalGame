using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public AudioSource tapSound;
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    // Fungsi [Range (min, max)] ialah menjaga value agar tetap berada di antara min dan max-nya 

    [Range(0f, 1f)]
    public float AutoCollectPercentage = 0.1f;
    public float SaveDelay = 5f;
    public ResourceConfig[] ResourcesConfigs;
    public Sprite[] ResourcesSprites;

    public Transform ResourcesParent;
    public ResourceController ResourcePrefab;
    public TapText TapTextPrefab;
    public Transform background;

    //IMAGE LIST
    public Transform SilverIcon;
    public Transform CoinIcon;
    public Transform DiamondIcon;

    public Text GoldInfo;
    public Text AutoCollectInfo;
    public Text TotalTapInfo;

    private List<ResourceController> _activeResources = new List<ResourceController>();
    private List<TapText> _tapTextPool = new List<TapText>();
    private float _collectSecond;
    private float _saveDelayCounter;

    private void Start()
    {
        AddAllResources();
        GoldInfo.text = $"Gold: { UserDataManager.Progress.Gold.ToString("0") }";

    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        _saveDelayCounter -= deltaTime;

        // Fungsi untuk selalu mengeksekusi CollectPerSecond setiap detik
        _collectSecond += deltaTime;
        if (_collectSecond >= 1f)
        {
            CollectPerSecond();
            _collectSecond = 0f;
        }

        CheckResourceCost();

        int resourceUnlockCount = 0;

        foreach (ResourceController resource in _activeResources)
        {
            if (resource.IsUnlocked)
            {
                resourceUnlockCount += 1;
            }
        }

        //set gambar 
        SilverIcon.gameObject.SetActive(false);
        CoinIcon.gameObject.SetActive(false);
        DiamondIcon.gameObject.SetActive(false);


        if (resourceUnlockCount >= 1 && resourceUnlockCount < 3)
        {
            SilverIcon.gameObject.SetActive(true);
            SilverIcon.transform.localScale = Vector3.LerpUnclamped(SilverIcon.transform.localScale, Vector3.one * .5f, 0.15f);
            SilverIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);
        }
        else if(resourceUnlockCount >= 3 && resourceUnlockCount < 7)
        {
            SilverIcon.gameObject.SetActive(false);
            CoinIcon.gameObject.SetActive(true);
            CoinIcon.transform.localScale = Vector3.LerpUnclamped(CoinIcon.transform.localScale, Vector3.one * 2f, 0.15f);
            CoinIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);
        }
        else if(resourceUnlockCount >= 7)
        {
            CoinIcon.gameObject.SetActive(false);
            DiamondIcon.gameObject.SetActive(true);
            DiamondIcon.transform.localScale = Vector3.LerpUnclamped(DiamondIcon.transform.localScale, Vector3.one * .25f, 0.15f);
            DiamondIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);
        }

        TotalResource();
    }


    private void AddAllResources()
    {
        bool showResources = true;
        int index = 0;
        foreach (ResourceConfig config in ResourcesConfigs)
        {
            GameObject obj = Instantiate(ResourcePrefab.gameObject, ResourcesParent, false);
            ResourceController resource = obj.GetComponent<ResourceController>();

            resource.SetConfig(index,config);
            obj.gameObject.SetActive(showResources);

            if (showResources && !resource.IsUnlocked)
            {
                showResources = false;
            }
            _activeResources.Add(resource);
            index++;
        }
    }

    public void ShowNextResource()
    {
        foreach (ResourceController resource in _activeResources)
        {
            if (!resource.gameObject.activeSelf)
            {
                resource.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void CollectPerSecond()
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        output *= AutoCollectPercentage;
        // Fungsi ToString("F1") ialah membulatkan angka menjadi desimal yang memiliki 1 angka di belakang koma 
        AutoCollectInfo.text = $"Auto Collect: { output.ToString("F1") } / second";
        AddGold(output);
    }

    public void AddGold(double value)
    {
        UserDataManager.Progress.Gold += value;
        GoldInfo.text = $"Point: { UserDataManager.Progress.Gold.ToString("0")}";
        UserDataManager.Save(_saveDelayCounter < 0f);

        if (_saveDelayCounter < 0f)
        {
            _saveDelayCounter = SaveDelay;
        }
    }

    public void TotalResource()
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }
        }
        TotalTapInfo.text = $"Point per tap: { output.ToString("F0") }";
    }

    public void CollectByTap(Vector3 tapPosition, Transform parent)
    {
        double output = 0;

        foreach (ResourceController resource in _activeResources)

        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        TapText tapText = GetOrCreateTapText();
        tapText.transform.SetParent(parent, false);
        tapText.transform.position = tapPosition;
        tapText.Text.text = $"+{ output.ToString("0") }";
        tapText.gameObject.SetActive(true);

        //coin pas di tap

        CoinIcon.transform.localScale = Vector3.one * 2.25f;
        SilverIcon.transform.localScale = Vector3.one * 0.55f;
        DiamondIcon.transform.localScale = Vector3.one * .3f;

        AddGold(output);
        tapSound.Play();
    }

    private TapText GetOrCreateTapText()
    {
        TapText tapText = _tapTextPool.Find(t => !t.gameObject.activeSelf);
        if (tapText == null)
        {
            tapText = Instantiate(TapTextPrefab).GetComponent<TapText>();

            _tapTextPool.Add(tapText);
        }
        return tapText;
    }

    private void CheckResourceCost()
    {
        foreach (ResourceController resource in _activeResources)
        {
            bool isBuyable = false;

            if (resource.IsUnlocked)
            {
                isBuyable = UserDataManager.Progress.Gold >= resource.GetUpgradeCost();
            }

            else
            {
                isBuyable = UserDataManager.Progress.Gold >= resource.GetUnlockCost();
            }

            resource.resourceImage.sprite = ResourcesSprites[isBuyable ? 1 : 0];
        }
    }
}

// Fungsi System.Serializable adalah agar object bisa di-serialize dan
// value dapat di-set dari inspector

[System.Serializable]
public struct ResourceConfig
{
    public string name;
    public double unlockCost;
    public double upgradeCost;
    public double output;
}
