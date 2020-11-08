using System;
using System.Collections.Generic;

using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;


//login class for the players. obtained from info gamer's playfab + unity youtube tutorial.
public class PlayFabManager : SingletonBehaviour<PlayFabManager>
{
    #region Login
    //user variables
    private string userEmail;
    private string userPassword;
    private string userName;

    public GameObject playerLoginPanel;

    //flag that shows it as the active manager of this session, and the one and only singleton
    public bool activeManager { get; private set; }

    //start login process, called from menu manager
    public void StartLogin()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "77ADE";
        }

        //there's registry of a previous entry on the playfab platform
        if (PlayerPrefs.HasKey("userEmail"))
        {
            userEmail = PlayerPrefs.GetString("userEmail");
            userPassword = PlayerPrefs.GetString("userPassword");
            TryLogin();
        }
        else
            playerLoginPanel.SetActive(true);

        activeManager = true;
    }

    //public method to give email from UI
    public void GiveUserEmail(string emailInput)
    {
        userEmail = emailInput;
    }

    //public method to give password from UI
    public void GiveUserPassword(string passwordInput)
    {
        userPassword = passwordInput;
    }

    //public method to give a username from the UI
    public void GiveUserName(string usernameInput)
    {
        userName = usernameInput;
    }

    //login UI request
    public void TryLogin()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    //register and login handling events
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
        //we remember the player data to expedite the login process next time
        PlayerPrefs.SetString("userEmail", userEmail);
        PlayerPrefs.SetString("userPassword", userPassword);

        playerLoginPanel.SetActive(false);
        MenuManager.instance.ShowScreen(0);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = userName };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Succesfully registered new user!");
        PlayerPrefs.SetString("userEmail", userEmail);
        PlayerPrefs.SetString("userPassword", userPassword);

        playerLoginPanel.SetActive(false);
        MenuManager.instance.ShowScreen(0);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.Log("Error in registering new user!");
        Debug.LogError(error.GenerateErrorReport());
    }
    #endregion

    #region PlayerStats

    private int playerHighScore;
    private int playerGold;

    public void SetHighScoreStat(int highScore)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "highScore", Value = highScore },
            }
        },
        result => { Debug.Log("User Statistics Updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    public void SetGoldStat(int goldAmount)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "goldAmount", Value = goldAmount },
            }
        },
        result => { Debug.Log("User Statistics Updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    void GetStatistics()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStatistics,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStatistics(GetPlayerStatisticsResult result)
    {
        Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
        {
            Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
            switch (eachStat.StatisticName)
            {
                case "highScore":
                    playerHighScore = eachStat.Value;
                    break;
                case "goldAmount":
                    playerGold = eachStat.Value;
                    break;
            }
        }
    }

    #endregion
}
