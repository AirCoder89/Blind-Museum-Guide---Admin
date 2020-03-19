using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class MLController : MonoBehaviour
{
    public static MLController Instance;
    
    public delegate void LanguageEvent(SystemLanguage language);
    public static event LanguageEvent OnLanguageChanged;
    [SerializeField] private Button currentLanguageBtn;
    [SerializeField] private Button arabicBtn;
    [SerializeField] private Button englishBtn;
    public  SystemLanguage currentLanguage;

    private void Awake()
    {
        if(Instance != null) return;
        Instance = this;
    }

    void Start()
    {
        currentLanguageBtn.onClick.AddListener(ChooseLanguage);
        arabicBtn.onClick.AddListener(SetAsArabic);
        englishBtn.onClick.AddListener(SetAsEnglish);
        SetAsArabic();
    }

    private void SetAsEnglish()
    {
        SetLanguage(SystemLanguage.English);
        currentLanguageBtn.GetComponentInChildren<RtlText>().text = "English";
        currentLanguageBtn.interactable = true;
        arabicBtn.gameObject.SetActive(false);
        englishBtn.gameObject.SetActive(false);
    }

    private void SetAsArabic()
    {
        SetLanguage(SystemLanguage.Arabic);
        currentLanguageBtn.GetComponentInChildren<RtlText>().text = "العربية";
        currentLanguageBtn.interactable = true;
        arabicBtn.gameObject.SetActive(false);
        englishBtn.gameObject.SetActive(false);
    }

    private void ChooseLanguage()
    {
        currentLanguageBtn.interactable = false;
        arabicBtn.gameObject.SetActive(true);
        englishBtn.gameObject.SetActive(true);
    }

    public void SetLanguage(SystemLanguage language)
    {
        currentLanguage = language;
        OnLanguageChanged?.Invoke(language);
    }
}
