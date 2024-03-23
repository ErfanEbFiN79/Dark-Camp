using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float timeBtwShot;
    [SerializeField] private float heatPerShot;

    public float TimeBtwShot => timeBtwShot;
    public float HeatPerShot => heatPerShot;
}