using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//central class that controls game events, enemy spawning and game phases. singleton class.
public class GameManager : SingletonBehaviour<GameManager>
{
    //game screens UI elements
    [Header("UI Elements")]
    public GameObject[] UIScreens;
    public Text scoreLabel;
    public Text highScoreLabel;
    public Image lifeBar;
    public Text finalScoreLabel;
    public Text finalHighScoreLabel;
    public Text finalGoldLabel;
    public GameObject storePanel;
    public Text goldLabel;
    public Text warningLabel;
    public Transform storeOpenButton;
    public Transform[] storeButtons;
    public GameObject touchControls;

    //wall to defend. used for the explosion effect on defeat
    [Header("Game Elements")]
    public Transform targetWall;
    //wall health points. reaching zero means game over
    private int currentWallHP;

    //enemy spawning elements
    public GameObject enemyPrefab; //prefab of the base enemy
    private float spawnCooldown; //pause between spawn tries
    private float rateUpCooldown; //time between each rate up
    private int currentSpawnRate; //current chance for spawnming enemies each second
    private List<EnemyBehavior> activeEnemies; //list of active enemies

    [HideInInspector]
    public bool gamePaused = true; //pause check

    //score of the round
    private int score;

    //store variables
    private bool storeOpened = false;
    
    //start game settings
    void Start()
    {
        currentWallHP = GameSettings.instance.wallHealthPoints;
        currentSpawnRate = GameSettings.instance.enemyStartRate;
        rateUpCooldown = GameSettings.instance.secondsToRateUp;
        score = 0;
        scoreLabel.text = "Score: " + score;

        int highScore = PlayerPrefs.GetInt("HighestScore", 0);
        if (highScore <= 0)
            highScoreLabel.gameObject.SetActive(false);
        else
        {
            highScoreLabel.gameObject.SetActive(true);
            highScoreLabel.text = "High Score: " + highScore;
        }

        storeButtons[0].GetChild(1).GetComponent<Text>().text = "Fragment Bomb (" + CurrencySettings.instance.fragmentProjectile + ")";
        storeButtons[1].GetChild(1).GetComponent<Text>().text = "Explosive Bomb (" + CurrencySettings.instance.explosiveProjectile + ")";
        storeButtons[2].GetChild(1).GetComponent<Text>().text = "Rolling Bomb (" + CurrencySettings.instance.rollingProjectile + ")";
        storeButtons[3].GetChild(1).GetComponent<Text>().text = "Heal Wall 1HP (" + CurrencySettings.instance.healingPrice + ")";

        lifeBar.fillAmount = 1f;
        SpawnEnemy();
        gamePaused = false;
    }
    
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
            GameOver();

        if (gamePaused)
            return;

        //we try seeing if its time to check the spawning
        if (spawnCooldown < 0f)
        {
            TryEnemySpawn();
            spawnCooldown = 1f;
        }
        else
            spawnCooldown -= Time.deltaTime;

        //we try seeing if it's time for a rate up
        if (rateUpCooldown < 0f)
        {
            currentSpawnRate++;
            rateUpCooldown = GameSettings.instance.secondsToRateUp;
        }
        else
            rateUpCooldown -= Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        //we draw a line of the range to where the enemies will spawn, to aid the developer
        Vector3 v1 = new Vector3(-1f * GameSettings.instance.enemySpawnRange, GameSettings.instance.enemyStartY, GameSettings.instance.enemyStartZ);
        Vector3 v2 = new Vector3(GameSettings.instance.enemySpawnRange, GameSettings.instance.enemyStartY, GameSettings.instance.enemyStartZ);

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(v1, v1 + Vector3.forward);
        Gizmos.DrawLine(v2, v2 + Vector3.forward);
        Gizmos.DrawLine(v1, v2);
    }

    //rolls the chances to spawn an enemy
    private void TryEnemySpawn()
    {
        float spawnChance = GetCurrentSpawnProbability();
        if (Random.value < spawnChance)
            SpawnEnemy();
    }

    //spawn a new enemy on the field's starting point
    private void SpawnEnemy()
    {
        if (activeEnemies == null)
            activeEnemies = new List<EnemyBehavior>();

        GameObject nuEnemy = Instantiate<GameObject>(enemyPrefab);
        nuEnemy.transform.position = new Vector3(Random.Range(-GameSettings.instance.enemySpawnRange, GameSettings.instance.enemySpawnRange), GameSettings.instance.enemyStartY, GameSettings.instance.enemyStartZ);
        EnemyBehavior eb = nuEnemy.GetComponent<EnemyBehavior>();
        eb.StartEnemy();
        activeEnemies.Add(eb);
    }

    //simple asymptotic function to get a spawn function that grows but never reaches one
    private float GetCurrentSpawnProbability()
    {
        return currentSpawnRate / (currentSpawnRate + GameSettings.instance.rateUpFunctionSlowness);
    }

    //method to kill the enemy and erasing him out of the active enemies listed
    public void KillEnemy(ProjectileScript projectile, EnemyBehavior victim)
    {
        int enemyIndex = activeEnemies.IndexOf(victim);
        activeEnemies.RemoveAt(enemyIndex);

        victim.KillEnemy(projectile);
    }

    //method for the explosive projectile kind. kills enemies in a radius surrounding the projectile
    public void KillEnemiesInRadius(ProjectileScript proj)
    {
        Vector3 landPosition = proj.transform.position;

        for(int i = 0; i < activeEnemies.Count; i++)
        {
            Vector3 v1 = activeEnemies[i].transform.position;
            v1.y = landPosition.y = 0f;

            if (Vector3.Distance(v1, landPosition) < GameSettings.instance.explosionRadius)
            {
                EnemyBehavior eb = activeEnemies[i];
                activeEnemies.RemoveAt(i);
                i--;
                
                eb.KillEnemy(proj);
            }
        }

        Destroy(proj.gameObject);
    }

    //the wall takes damage. called by an enemy impacting the wall
    public void DamageWall()
    {
        currentWallHP--;

        lifeBar.fillAmount = (float)currentWallHP / (float)GameSettings.instance.wallHealthPoints;

        if (currentWallHP <= 0)
            GameOver();
    }

    //game over call upong wall HP hits zero. Explodes the wall and stops enemies. Also calls UI
    private void GameOver()
    {
        gamePaused = true;
        for(int i = 0; i < activeEnemies.Count; i++)
        {
            activeEnemies[i].StopEnemy();
        }

        Rigidbody cannonRB = CannonScript.instance.GetComponent<Rigidbody>();
        cannonRB.isKinematic = false;
        Vector3 forceDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(0, 1f));
        cannonRB.AddForce(GameSettings.instance.wallExplosionForce * forceDir.normalized);
        CannonScript.instance.GetComponent<BoxCollider>().enabled = true;

        for (int i = 0; i < targetWall.childCount; i++)
        {
            if(targetWall.GetChild(i).GetComponent<Rigidbody>() != null)
            {
                Rigidbody rb = targetWall.GetChild(i).GetComponent<Rigidbody>();
                rb.isKinematic = false;
                forceDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(0, 1f));
                rb.AddForce(GameSettings.instance.wallExplosionForce * forceDir.normalized);
                targetWall.GetChild(i).GetComponent<BoxCollider>().enabled = true;
            }
        }

        //score update. highest score is stored in a player pref
        int highScore = PlayerPrefs.GetInt("HighestScore", 0);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighestScore", score);
        }

        UIScreens[0].SetActive(false);
        UIScreens[1].SetActive(true);

        finalScoreLabel.text = "SCORE: " + score;
        finalHighScoreLabel.text = "high score: " + highScore;

        //we transform the kills to gold
        int goldEarned = Mathf.CeilToInt((float)score / CurrencySettings.instance.killsToGold);
        int goldStored = PlayerPrefs.GetInt("Wallet", CurrencySettings.instance.startingGold);
        PlayerPrefs.SetInt("Wallet", goldEarned + goldStored);

        PlayFabManager.instance.UpdateHighScoreStat(highScore);
        PlayFabManager.instance.UpdateGoldStat(goldEarned + goldStored);
        PlayFabManager.instance.StartCloudStatUpdate();

        //PlayFabManager.instance.SetHighScoreStat(highScore); //previous version of stat updating
        //PlayFabManager.instance.SetGoldStat(goldEarned + goldStored);

        finalGoldLabel.text = "---You earned " + goldEarned + " Gold!---";
    }

    //ui call method from open button on screen
    public void OpenButtonCall()
    {
        storeOpened = !storeOpened;
        storeOpenButton.rotation = Quaternion.Euler(0f, 0f, storeOpened ? -90f : 90f);
        DisplayStore(storeOpened);
    }

    //method to prepare and display the in-game store
    private void DisplayStore(bool open)
    {
        Time.timeScale = open ? 0f : 1f;

        storePanel.GetComponent<Animator>().SetTrigger(open ? "Open" : "Close");
        touchControls.SetActive(!open);

        goldLabel.gameObject.SetActive(open);
        if (open)
        {
            goldLabel.text = "Gold: " + PlayerPrefs.GetInt("Wallet", CurrencySettings.instance.startingGold);
            warningLabel.gameObject.SetActive(false);
        }
    }

    //method to buy a special projectile from the in game store
    public void BuyBullet(int typeID)
    {
        ProjectileScript.ProjectileType type = (ProjectileScript.ProjectileType)typeID;

        if (CannonScript.instance.loadedProjectile)
        {
            warningLabel.text = "! Warning: Can't buy a new bullet with one already on the chamber !";
            warningLabel.gameObject.SetActive(true);
            return;
        }

        int gold = PlayerPrefs.GetInt("Wallet", CurrencySettings.instance.startingGold);
        int price = CurrencySettings.instance.fragmentProjectile;
        if (type == ProjectileScript.ProjectileType.Explosive)
            price = CurrencySettings.instance.explosiveProjectile;
        else if (type == ProjectileScript.ProjectileType.Roller)
            price = CurrencySettings.instance.rollingProjectile;

        if (gold < price)
        {
            warningLabel.text = "! Warning: Not enough gold to buy a " + type.ToString() + " projectile !";
            warningLabel.gameObject.SetActive(true);
            return;
        }

        gold -= price;
        PlayerPrefs.SetInt("Wallet", gold);
        goldLabel.text = "Gold: " + gold;

        PlayFabManager.instance.UpdateGoldStat(gold);

        PlayFabManager.instance.StartCloudStatUpdate();

        //PlayFabManager.instance.SetGoldStat(gold); //previous non-cloudscript version

        CannonScript.instance.LoadSpecialProjectile(type);
    }

    //method buy 1 HP of recovery
    public void BuyHealth()
    {
        if(currentWallHP == GameSettings.instance.wallHealthPoints)
        {
            warningLabel.text = "! Warning: Wall already at max health !";
            warningLabel.gameObject.SetActive(true);
            return;
        }

        int gold = PlayerPrefs.GetInt("Wallet", CurrencySettings.instance.startingGold);
        int price = CurrencySettings.instance.healingPrice;

        if (gold < price)
        {
            warningLabel.text = "! Warning: Not enough gold to buy healing !";
            warningLabel.gameObject.SetActive(true);
            return;
        }

        gold -= price;
        PlayerPrefs.SetInt("Wallet", gold);
        goldLabel.text = "Gold: " + gold;

        PlayFabManager.instance.UpdateGoldStat(gold);

        PlayFabManager.instance.StartCloudStatUpdate();

        //PlayFabManager.instance.SetGoldStat(gold); //previous non-cloudscript version

        currentWallHP++;
        lifeBar.fillAmount = (float)currentWallHP / (float)GameSettings.instance.wallHealthPoints;
    }

    //method to add score from a kill
    public void AddScore(int score)
    {
        this.score += score;
        scoreLabel.text = "Score: " + this.score;
    }

    //method to load the scene again, prompting a new session of the game. made for debug purposes and deactivated on the current version
    public void Retry()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    //method to load the menu scene
    public void GoToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    
}
