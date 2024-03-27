using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public static UiController instance;

    [Header("Text")]
    [SerializeField] private TMP_Text gT1;
    [SerializeField] private TMP_Text gT2;

    [Header("Slider Hand1")]
    public Slider hpSlider;

    [Header("Dead")] 
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TMP_Text deathText;
    
    public TMP_Text GT1 => gT1;
    public TMP_Text GT2 => gT2;

    public GameObject DeathScreen
    {
        get => deathScreen;
        set => deathScreen = value;
    }

    public TMP_Text DeathText
    {
        get => deathText;
        set => deathText = value;
    }
    

    private void Awake()
    {
        instance = this;
    }
}
