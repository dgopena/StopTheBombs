using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ConfigResource<GameSettings>
{
    [Header("Cannon Variables")]
    public float baseRotationSpeed; //speed at which the cannon rotates on its base
    public Vector2 baseRotationLimits; //angle limits the cannon can spin on (degrees). x is min, y is max.
    public float headRotationSpeed; //speed at which the cannons tilts its barrel vertically
    public Vector2 headRotationLimits; //angle limits for the cannon's barrel (degrees) x is min, y is max.
    public float shootCooldown; //cooldown between shots to prevent spamming

    [Header("Projectile Variables")]
    public float projectileStartSpeed; //starting speed value for the projectiles
    public float trajectoryVertexTimeStep; //amount of detail the trajectory shows. the less, the cruder the results are but the better the performance.
    public float projectileMaxLifeTime; //control variable to destroy projectiles that remain to long on play
    public float projectileFlightTimeFactor; //factor to show the projectile moving slower while it flies, just for design purposes

    [Header("Bouncing Projectile Variables")]
    public float bouncerWidth; //width of the projectile
    public int maxBounces; //amount of bounces until destroy
    [Range(0.05f,0.95f)]
    public float bounceForceLoss; //loss of energy on each bounce

    [Header("Fragment Projectile Variables")]
    public float fragmentWidth;
    public float minimumTimeBeforeFragmentation; //minimal flight time before fragmenting
    public int fragmentAmount; //amount of fragments the projectile turns into
    public float fragmentsDownscale; //downscale the fragments get upon fragmentation
    [Range(30f,175f)]
    public float fragmentationAngle; //angle on which the fragments spread

    [Header("Explosive Projectile Variables")]
    public float explosiveWidth;
    public float explosionRadius; //area of damage of the explosion

    [Header("Roller Projectile Variables")]
    public float rollerWidth;
    public float rollingTime; //time it rolls until destroy

    [Header("Enemy Spawn Settings")]
    public float enemyStartZ; //z-coordinate for the enemies to spawn in
    public float enemyStartY; //height for the enemies to spawn in
    [Range(0.5f, 10f)]
    public float enemySpawnRange; //range of x for the enemies to spawn in
    public int maxEnemies; //count limit for enemies in play
    public int enemyStartRate; //chance of spawning an enemy each second
    public float secondsToRateUp; //amount of second to make a rate up on the enemu spawn chance
    public float rateUpFunctionSlowness; //how quickly the rate up function approaches "one". The bigger the value, the slower it grows
    [Range(0.1f, 0.45f)]
    public float rateUp; //amount the rate ups itself

    [Header("Enemy Behavior Settings")]
    public float enemySpeed;
    public float enemyWallDamage;
    public int scorePerKill;

    [Header("Wall Settings")]
    public int wallHealthPoints; //how many impacts does the wall take
    public float wallExplosionForce; //how much force is applied to the wall components upon breaking it

    [Header("Leaderboard Settings")]
    public int displayEntriesNumber;
}

