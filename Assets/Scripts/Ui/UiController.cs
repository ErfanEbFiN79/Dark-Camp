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
    [SerializeField] private Slider gunSlider1;
    
    [Header("Slider Hand1")]
    [SerializeField] private Slider gunSlider1H2;
    
    
    public TMP_Text GT1 => gT1;
    public TMP_Text GT2 => gT2;


    public Slider GunSlider1
    {
        get => gunSlider1;
        set => gunSlider1 = value;
    }


    public Slider GunSlider1H2
    {
        get => gunSlider1H2;
        set => gunSlider1H2 = value;
    }

    private void Awake()
    {
        instance = this;
    }
}
