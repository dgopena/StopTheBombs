using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    //different types of projectile to shoot
    [System.Serializable]
    public enum ProjectileType
    {
        Bouncer,
        Fragment,
        Explosive,
        Roller
    }

    public ProjectileType projectileType = ProjectileType.Bouncer;

    //starting state of the projectile. locks the update if false
    private bool active = false;

    //width of the projectile, which affects how many enemys can take down
    private float projectileWidth;

    //values that define the trajectory the projectile will take
    private Vector3 currentSpeed;
    private Transform fieldObj;
    private int projectilePhase;

    //bouncer projectile variables
    private int maxBounces;
    private int currentBounces;

    //fragment projectile variables
    private float currentLifeTime;
    private float lastRegisteredHeight;

    //roller projectile variables
    private float rollingLifeTime;

    //counter of deaths this projectile has made
    public int enemiesKilled { get; private set; }

    // Update is called once per frame
    void LateUpdate()
    {
        //locks updating on inactive projectiles
        if (!active)
            return;

        //moment it lands on the ground. different effects trigger depending on the projectile type
        if (transform.position.y - fieldObj.transform.position.y < (0.5f * projectileWidth))
        {
            //effects upon landing of the bouncer projectile. it'll keep reshooting himself with less force until an amount of bounces is reached
            if (projectileType == ProjectileType.Bouncer)
            {
                if (currentBounces > maxBounces)
                {
                    active = false;
                    Destroy(gameObject);
                    return;
                }

                currentBounces++;
                Vector3 rePos = transform.position; //we reposition the projectile over the field
                rePos.y = (0.5f * projectileWidth) + fieldObj.transform.position.y;
                transform.position = rePos;

                Vector3 bounceVelocity = GameSettings.instance.bounceForceLoss * currentSpeed;
                bounceVelocity.y *= -1;

                Shoot(transform.position, bounceVelocity, ProjectileType.Bouncer, 1); //we reshoot the missile on the new direction. the different phase avoids the bounce reset
                return;
            }
            //effects upon landing of the roller type projectile, which upon hitting ground starts to roll
            else if(projectileType == ProjectileType.Roller)
            {
                Vector3 rePos = transform.position; //we reposition the projectile over the field
                rePos.y = (0.5f * projectileWidth) + fieldObj.transform.position.y;
                transform.position = rePos;

                projectilePhase += 1; //on this phase it rolls out
                rollingLifeTime = GameSettings.instance.rollingTime; //amount of time that it will roll
                currentSpeed.y = 0f; //we flatten the speed it had
                return;
            }
            //explosive damage effect on a radius upon landing for explosive projectiles
            else if(projectileType == ProjectileType.Explosive)
            {
                enemiesKilled++;
                GameManager.instance.KillEnemiesInRadius(this);
                active = false;
                return;
            }

            active = false;

            Destroy(gameObject);
        }
        else if (projectileType == ProjectileType.Fragment && projectilePhase == 0)
        {
            //we see if its time for the fragmentation, be it after cusp or after minimum flight time
            if (currentLifeTime > 0f)
                currentLifeTime -= Time.deltaTime;
            else if (lastRegisteredHeight > transform.position.y) //started to descend. we destroy de projectile and create the fragments
            {
                int fragmentAmount = GameSettings.instance.fragmentAmount;
                if (fragmentAmount < 2)
                    fragmentAmount = 2; //at least it has to turn in two

                float startCorrectionAngle = -0.5f * GameSettings.instance.fragmentationAngle;
                float angleStep = GameSettings.instance.fragmentationAngle / (float)(fragmentAmount - 1);
                for (int i = 0; i < fragmentAmount; i++)
                {
                    Vector3 startFragmentSpeed = Quaternion.AngleAxis(startCorrectionAngle + (i * angleStep), transform.up) * currentSpeed;

                    //we spawn the new projectiles
                    GameObject fragment = Instantiate<GameObject>(CannonScript.instance.projectilePrefab);
                    //fragment.GetComponent<ProjectileScript>().Shoot(transform.position, startFragmentSpeed, ProjectileType.Fragment, 1); //old working of the fragment projectile
                    fragment.GetComponent<ProjectileScript>().Shoot(transform.position, startFragmentSpeed, ProjectileType.Bouncer, 0);
                }

                //we destroy the parent projectile
                active = false;
                Destroy(gameObject);
                return;
            }

            lastRegisteredHeight = transform.position.y;
        }

        float fact = GameSettings.instance.projectileFlightTimeFactor;

        //after landing, rolling projectiles act different
        if (projectileType == ProjectileType.Roller && projectilePhase > 0)
        {
            transform.position += (currentSpeed * fact * Time.deltaTime);

            //the projectile is destroyed upon it runs out of life
            rollingLifeTime -= Time.deltaTime;
            if (rollingLifeTime < 0f)
            {
                active = false;
                Destroy(gameObject);
            }

            return;
        }

        currentSpeed += (Physics.gravity * fact * Time.deltaTime);
        transform.position += (currentSpeed * fact * Time.deltaTime);
    }

    //method that activates the projectile. from here on, the state of the projectile is in "first flight" mode until reaching ground for the first time
    public void Shoot(Vector3 startPos, Vector3 startVelocity, ProjectileType projType, int projectilePhase = 0)
    {
        projectileType = projType;

        if (projectileType == ProjectileType.Bouncer && projectilePhase == 0)
        {
            currentBounces = 0;
            maxBounces = GameSettings.instance.maxBounces;
            projectileWidth = GameSettings.instance.bouncerWidth;
        }
        else if (projectileType == ProjectileType.Fragment)
        {
            projectileWidth = GameSettings.instance.fragmentWidth;
            lastRegisteredHeight = startPos.y;
            if (projectilePhase == 0)
                currentLifeTime = GameSettings.instance.minimumTimeBeforeFragmentation;
            else
                projectileWidth *= GameSettings.instance.fragmentsDownscale;

        }
        else if (projectileType == ProjectileType.Explosive)
            projectileWidth = GameSettings.instance.explosiveWidth;
        else if (projectileType == ProjectileType.Roller)
        {
            projectileWidth = GameSettings.instance.rollerWidth;
            rollingLifeTime = GameSettings.instance.rollingTime;
        }

        this.projectilePhase = projectilePhase;
        fieldObj = CannonScript.instance.field;
        transform.position = startPos;
        transform.localScale = projectileWidth * Vector3.one;
        currentSpeed = startVelocity;

        active = true;
    }

    //if projectile bounds out of limits is destroyed
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            enemiesKilled++;
            GameManager.instance.KillEnemy(this, other.GetComponent<EnemyBehavior>());
        }

        //rolling projectiles in ground can roll out of the picture since they're bound with a life time
        if (projectileType == ProjectileType.Roller && projectilePhase == 1)
            return;

        if (other.tag == "Limit")
            Destroy(gameObject);
    }
}
