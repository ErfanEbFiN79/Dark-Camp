using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float timeBtwShot;
    [SerializeField] private float heatPerShot;
    [SerializeField] private float damage;
    [SerializeField] private AudioSource shotSound;
    public float TimeBtwShot => timeBtwShot;
    public float HeatPerShot => heatPerShot;

    public float Damage => damage;

    public AudioSource ShotSound => shotSound;
}