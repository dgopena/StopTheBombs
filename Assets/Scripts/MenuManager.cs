using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//class to control de menu events. singleton.
public class MenuManager : SingletonBehaviour<MenuManager>
{
    //UI screens
    public GameObject[] UIScreens;

    //playfab manager
    public PlayFabManager playFabManager;

    //camera of the menu, for aesthetic purposes
    public Transform menuCamera;
    public float cameraRotationSpeed; //rotation speed of the camera effect

    //UI elements to show wait times
    [Header("Wait Time Variables")]
    public Text playsText;
    public Text timeText;
    public Transform playButton;

    //play chances remaining for the player
    private int currentPlayChances;
    private float timeNextPlayChance;

    //bool to control that the update starts after the setup
    private bool menuStarted = false;

    //Shop UI Elements
    [Header("Shop UI Elements")]
    public Text goldLabel;
    public Text gemLabel;
    public Text warningLabel;
    public Transform[] shopButtons;
    public GameObject confirmationPanelSpeed;
    public GameObject confirmationPanelGems;

    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.DeleteAll();

        //we create the playfab manager after checking if there's no other object of its kind already created
        PlayFabManager[] managersActive = FindObjectsOfType<PlayFabManager>();
        if (managersActive.Length > 1) //later menu reach. must erase the new playfabManager
        {
            GameObject logPanel = playFabManager.playerLoginPanel;
            for (int i = 0; i < managersActive.Length; i++)
            {
                if (!managersActive[i].activeManager)
                {
                    logPanel = managersActive[i].gameObject;
                    Destroy(managersActive[i].gameObject);
                }
                else
                {
                    playFabManager = managersActive[i];
                }
            }

            playFabManager.playerLoginPanel = logPanel;
            playFabManager.playerLoginPanel.SetActive(false);
            LoginSuccess();
        }
        else
        {
            DontDestroyOnLoad(playFabManager.gameObject);
            playFabManager.StartLogin();
            ShowScreen(4);
        }

        int currentChances = PlayerPrefs.GetInt("playChances", CurrencySettings.instance.maxPlayChances);
        currentPlayChances = currentChances;
    }

    void LateUpdate()
    {
        //a simple camera spin so the menu looks more dynamic
        menuCamera.forward = Quaternion.AngleAxis(cameraRotationSpeed * Time.deltaTime, Vector3.up) * menuCamera.forward;

        if (!menuStarted)
            return;

        if (currentPlayChances == CurrencySettings.instance.maxPlayChances)
            return;

        //we substract from the timer
        if (timeNextPlayChance > 0)
        {
            timeNextPlayChance -= Time.deltaTime;
            UpdateTimer(Mathf.FloorToInt(timeNextPlayChance));
        }
        else
        {
            /*
            //timer reaches zero, we add a new playchance
            int currentChances = PlayerPrefs.GetInt("playChances", CurrencySettings.instance.maxPlayChances);
            currentChances += 1;
            currentPlayChances = currentChances;
            PlayerPrefs.SetInt("playChances", currentChances);
            AddToTimestamp(CurrencySettings.instance.playRecoveryTime); //we move the base of the timestamp to match the addition
            playsText.text = "Play (" + CurrencySettings.instance.maxPlayChances + ")";
            if (currentChances == CurrencySettings.instance.maxPlayChances) //maximum chances reached
                timeText.gameObject.SetActive(false); //no need to show the timer
            else
            {
                timeNextPlayChance = CurrencySettings.instance.playRecoveryTime;
                UpdateTimer(Mathf.FloorToInt(timeNextPlayChance));
            }
            */

            menuStarted = false;
            PlayButtonEnabled(false);
            UpdateTimesCall();

            //PlayButtonEnabled(currentPlayChances != 0);
        }
    }

    //called from playfabmanager upon a successful login
    public void LoginSuccess()
    {
        UpdateTimesCall();
        ShowScreen(0);
    }

    //changes the screen to display
    public void ShowScreen(int screenIndex)
    {
        for (int i = 0; i < UIScreens.Length; i++)
        {
            UIScreens[i].SetActive(i == screenIndex);
        }
    }

    //takes you to the game scene
    public void PlayGame()
    {
        if (currentPlayChances == 0)
            return;

        //we timestamp the new recover chance countdown
        if (currentPlayChances == CurrencySettings.instance.maxPlayChances)
        {
            StartTimeStampedGame();
            return;
        }

        currentPlayChances--;
        PlayerPrefs.SetInt("playChances", currentPlayChances);

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    //correct control of returning to the main screen
    public void BackToMenuScreen()
    {
        PlayButtonEnabled(false);
        LoginSuccess();
    }

    #region Waiting Time Functions
    //called upon entering the menu, to update the wait time of the play chances through a rest call
    private void UpdateTimesCall()
    {
        timeText.gameObject.SetActive(false);

        //we check if player is already cleared to play
        if (PlayerPrefs.GetInt("playChances", CurrencySettings.instance.maxPlayChances) == CurrencySettings.instance.maxPlayChances)
        {
            playsText.text = "Play (" + CurrencySettings.instance.maxPlayChances + ")";
            PlayButtonEnabled(true);
            return;
        }

        //else, we need to check time passed. if successful, it will return the server time
        playFabManager.GetCurrentTime();
    }

    //called from the playfabmanager upong a succesful time update
    public void UpdateWaitTimes(DateTime serverTime)
    {
        //we get the stored
        DateTime timeStamp = LoadLastTimestamp();
        TimeSpan difference = serverTime.Subtract(timeStamp);

        bool putToTheMax = (difference.Days >= 1) || (difference.Hours >= 12);

        int currentPlayChances = PlayerPrefs.GetInt("playChances", CurrencySettings.instance.maxPlayChances);

        if (currentPlayChances < CurrencySettings.instance.maxPlayChances && putToTheMax) //we just put to the max the play chances
            PlayerPrefs.SetInt("playChances", CurrencySettings.instance.maxPlayChances);

        int totalSeconds = (difference.Hours * 3600) + (difference.Minutes * 60) + (difference.Seconds);

        //Debug.Log("time difference: " + totalSeconds);

        //we check how many chances we get from the difference
        int chancesGot = 0;
        int secondsRest = 0;

        if (totalSeconds > (CurrencySettings.instance.maxPlayChances * CurrencySettings.instance.playRecoveryTime))
            PlayerPrefs.SetInt("playChances", CurrencySettings.instance.maxPlayChances);
        else if(currentPlayChances < CurrencySettings.instance.maxPlayChances)
        {
            //we check how many chances we get from the difference
            chancesGot = totalSeconds / CurrencySettings.instance.playRecoveryTime;
            secondsRest = totalSeconds % CurrencySettings.instance.playRecoveryTime;
        }

        int currentChances = PlayerPrefs.GetInt("playChances", CurrencySettings.instance.maxPlayChances);
        currentChances += chancesGot;

        AddToTimestamp(chancesGot * CurrencySettings.instance.playRecoveryTime);

        if (currentChances == CurrencySettings.instance.maxPlayChances) //maximum chances reached
            timeText.gameObject.SetActive(false); //no need to show the timer
        else
        {
            timeNextPlayChance = CurrencySettings.instance.playRecoveryTime - secondsRest;
            timeText.gameObject.SetActive(true);
            UpdateTimer(Mathf.FloorToInt(timeNextPlayChance));
        }

        PlayerPrefs.SetInt("playChances", currentChances); //we update the pref value
        currentPlayChances = currentChances;

        PlayButtonEnabled(currentPlayChances != 0); //we disable or enable the playbutton

        menuStarted = true;
    }

    //if a retrieval of time fails, we block the possibility of playing
    public void UpdateTimeError()
    {
        timeText.gameObject.SetActive(true);
        timeText.text = "Error trying to reach the server. Please try again later.";
        PlayButtonEnabled(false);

        menuStarted = true;
    }

    //alternative for the start game that timestamps the first play for later time taking purposes
    private void StartTimeStampedGame()
    {
        playFabManager.GetTimeStampedGame();
    }

    //succesfully get the timestamp
    public void TimeStampSuccess(DateTime serverTime)
    {
        SaveTimeStamp(serverTime);

        currentPlayChances--;
        PlayerPrefs.SetInt("playChances", currentPlayChances);

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    //method to store the current time stamp as a string on a player pref
    private void SaveTimeStamp(DateTime timeStamp)
    {
        PlayerPrefs.SetString("FullPlaysTimeStamp", timeStamp.ToBinary().ToString());
    }

    //gets the last timestamp saved on the prefs
    private DateTime LoadLastTimestamp()
    {
        long temp = Convert.ToInt64(PlayerPrefs.GetString("FullPlaysTimeStamp"));
        DateTime lastTimeStamp = DateTime.FromBinary(temp);

        return lastTimeStamp;
    }

    //method to keep consistency with de dateTime if the timer adds a playchance while on the menu screen
    private void AddToTimestamp(int seconds)
    {
        DateTime timeStamp = LoadLastTimestamp();
        DateTime nuStamp = timeStamp.AddSeconds(seconds);
        SaveTimeStamp(nuStamp);
    }

    //method to disable or enable the play Button
    private void PlayButtonEnabled(bool enabled)
    {
        Color bc = playButton.GetComponent<Image>().color;
        bc.a = enabled ? 1f : 0.35f;
        playButton.GetComponent<Image>().color = bc;

        playButton.GetComponent<HoldButton>().enabled = enabled;

        playsText.text = enabled ? "Play (" + PlayerPrefs.GetInt("playChances", CurrencySettings.instance.maxPlayChances) + ")" : "Please Wait...";
    }

    //update the time label on screen
    private void UpdateTimer(int secondsRemaining)
    {
        string text = "Next Play Chance in: ";

        int hours = secondsRemaining / 3600; //highly unlikely, but i put it here in case more chances were wanted or the time scales too much
        secondsRemaining = secondsRemaining % 3600;
        int minutes = secondsRemaining / 60;
        secondsRemaining = secondsRemaining % 60;

        if (hours > 0)
            text += hours + ":";
        if (minutes > 0)
            text += minutes + ":";
        if (secondsRemaining < 10)
            text += "0";

        text += secondsRemaining;

        timeText.text = text;
    }
    #endregion

    #region Shop Functions
    //call method to update gem and gold values on the shop
    public void UpdateShop()
    {
        goldLabel.text = "Gold: " + PlayerPrefs.GetInt("Wallet", CurrencySettings.instance.startingGold);
        gemLabel.text = "Gems: " + PlayerPrefs.GetInt("GemPouch", CurrencySettings.instance.startingGems);

        shopButtons[0].GetChild(0).GetComponent<Text>().text = "Skip wait (" + CurrencySettings.instance.speedTimeGemPrice + (CurrencySettings.instance.speedTimeGemPrice > 1 ? "gems)" : "gem)");
        shopButtons[1].GetChild(0).GetComponent<Text>().text = "Buy a Gem (" + CurrencySettings.instance.gemPriceInGold + " gold)";

        warningLabel.gameObject.SetActive(false);

        confirmationPanelGems.SetActive(false);
        confirmationPanelSpeed.SetActive(false);
    }

    //UI method to request the purchase of a gem
    public void BuyGemRequest()
    {
        int goldAmount = PlayerPrefs.GetInt("Wallet", CurrencySettings.instance.startingGold);

        if (goldAmount < CurrencySettings.instance.gemPriceInGold)
        {
            warningLabel.gameObject.SetActive(true);
            warningLabel.text = "! Warning: Not enough gold to buy a gem !";
        }
        else
            confirmationPanelGems.SetActive(true);
    }

    //UI method to confirm gem purchase
    public void BuyGemConfirm()
    {
        //we update the gold
        int goldAmount = PlayerPrefs.GetInt("Wallet", CurrencySettings.instance.startingGold);
        goldAmount -= CurrencySettings.instance.gemPriceInGold;
        PlayerPrefs.SetInt("Wallet", goldAmount);
        PlayFabManager.instance.SetGoldStat(goldAmount);

        //we update the gem count
        int gemCount = PlayerPrefs.GetInt("GemPouch", CurrencySettings.instance.startingGems);
        gemCount++;
        PlayerPrefs.SetInt("GemPouch", gemCount);
        PlayFabManager.instance.SetGemStat(gemCount);

        UpdateShop();
    }

    //UI method to request the purchase of a speed up
    public void BuySpeedRequest()
    {
        int gemAmount = PlayerPrefs.GetInt("GemPouch", CurrencySettings.instance.startingGems);

        if (gemAmount < CurrencySettings.instance.speedTimeGemPrice)
        {
            warningLabel.gameObject.SetActive(true);
            warningLabel.text = "! Warning: Not enough gems to buy a speed up !";
        }
        else if(PlayerPrefs.GetInt("playChances", CurrencySettings.instance.maxPlayChances) == CurrencySettings.instance.maxPlayChances)
        {
            warningLabel.gameObject.SetActive(true);
            warningLabel.text = "! Warning: You are already in your max amount of playing chances. No need to speed up !";
        }
        else
            confirmationPanelSpeed.SetActive(true);
    }

    //UI method to confirm speed up purchase
    public void BuySpeedConfirm()
    {
        //we update the gem count
        int gemCount = PlayerPrefs.GetInt("GemPouch", CurrencySettings.instance.startingGems);
        gemCount -= CurrencySettings.instance.speedTimeGemPrice;
        PlayerPrefs.SetInt("GemPouch", gemCount);
        PlayFabManager.instance.SetGemStat(gemCount);

        //we skip the waiting time
        PlayerPrefs.SetInt("playChances", CurrencySettings.instance.maxPlayChances);
        timeText.gameObject.SetActive(false);
        currentPlayChances = CurrencySettings.instance.maxPlayChances;

        UpdateShop();
    }
    #endregion
}
