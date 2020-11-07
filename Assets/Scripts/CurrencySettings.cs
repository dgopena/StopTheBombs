using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CurrencySettings : ConfigResource<CurrencySettings>
{
    [Header("Projectile Prices")] //costs in gold of each type of projectile
    public int fragmentProjectile = 10;
    public int explosiveProjectile = 50;
    public int rollingProjectile = 25;

    [Header("Effect Prices")] //costs in gold for special battle effects
    public int healingPrice = 50;
    public int startingGold = 500;

    [Header("Kill to Gold Ratio")]
    public float killsToGold = 3; //kills to one gold ratio, rounded up
}
