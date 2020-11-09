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

    [Header("Kill to Gold Ratio (Soft Currency)")]
    public float killsToGold = 3; //kills to one gold ratio, rounded up

    [Header("Wait Time Variables")]
    public int maxPlayChances = 5; //max chances to play until you have to wait
    public int playRecoveryTime = 300; //seconds to wait for a play chance to reload
    public int speedTimeGemPrice = 1; //gem price to recover one play chance

    //gold = soft currency. you earn it by playing
    //gems = hard currency
    //here we should put the real money prices of gems, too
    [Header("Gem Prices (Hard Currency)")]
    public int startingGems = 10; //gem count a new player starts with
    public int gemPriceInGold = 100; //how much gold you need to spend to buy one gem
}
