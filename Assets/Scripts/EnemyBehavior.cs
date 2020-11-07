using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private bool enemyActive = false;

    private void LateUpdate()
    {
        if (!enemyActive)
            return;

        transform.position += GameSettings.instance.enemySpeed * Time.deltaTime * transform.forward;
    }

    //method to activate the enemy
    public void StartEnemy()
    {
        enemyActive = true;
    }

    //method to deactivate enemy
    public void StopEnemy()
    {
        enemyActive = false;
    } 

    //destroys the enemy. adds the score to the total depending on the data the killer projectile carried
    public void KillEnemy(ProjectileScript proj)
    {
        //score calculating. a bonus is given for multiple kills with one projectile
        GameManager.instance.AddScore(GameSettings.instance.scorePerKill * proj.enemiesKilled);

        //particles
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Wall")
        {
            GameManager.instance.DamageWall();

            //particles
            Destroy(gameObject);
        }
    }
}
