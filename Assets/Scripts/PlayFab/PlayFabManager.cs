using System;
using System.Collections.Generic;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
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
        Debug.Log("Succesful Login");
        //we remember the player data to expedite the login process next time
        PlayerPrefs.SetString("userEmail", userEmail);
        PlayerPrefs.SetString("userPassword", userPassword);

        playerLoginPanel.SetActive(false);
        MenuManager.instance.LoginSuccess();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        if(userName == null || userName.Length == 0)
        {
            Debug.LogError("user name can't be blank!");
            return;
        }

        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = userName };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Succesfully registered new user!");
        PlayerPrefs.SetString("userEmail", userEmail);
        PlayerPrefs.SetString("userPassword", userPassword);

        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = userName }, OnDisplayName, OnLoginFailure);
        playerLoginPanel.SetActive(false);
        MenuManager.instance.LoginSuccess();
    }

    void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " is your new display name");
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.Log("Error in registering new user!");
        Debug.LogError(error.GenerateErrorReport());
    }
    #endregion

    #region PlayerStats

    private int highScoreStat;
    private int goldStat;
    private int gemStat;

    //statistic update without cloudscript. i left them here just for future reference
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

    public void SetGemStat(int gemAmount)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "gemAmount", Value = gemAmount },
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
                    highScoreStat = eachStat.Value;
                    break;
                case "goldAmount":
                    goldStat = eachStat.Value;
                    break;
                case "gemAmount":
                    gemStat = eachStat.Value;
                    break;
            }
        }
    }

    //update variables before cloud update
    public void UpdateHighScoreStat(int highScore)
    {
        highScoreStat = highScore;
    }

    public void UpdateGoldStat(int goldAmount)
    {
        goldStat = goldAmount;
    }

    public void UpdateGemStat(int gemAmount)
    {
        gemStat = gemAmount;
    }

    //stat updating but now with cloudscripts
    public void StartCloudStatUpdate()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStats",
            FunctionParameter = new { playerHighScore = highScoreStat, playerGold = goldStat, playerGems = gemStat },
            GeneratePlayStreamEvent = true,
        }, OnCloudStatUpdate, OnErrorShared);
    }

    private void OnCloudStatUpdate(ExecuteCloudScriptResult result)
    {
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue);
        Debug.Log((string)messageValue);
    }

    //for player reset stats. just for dev purposes for now
    public void StartCloudStatReset()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "ResetPlayerStats",
            FunctionParameter = new { playerHighScore = 0, playerGold = 0, playerGems = 0 },
            GeneratePlayStreamEvent = true,
        }, OnCloudStatReset, OnErrorShared);
    }

    private void OnCloudStatReset(ExecuteCloudScriptResult result)
    {
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue);
        Debug.Log((string)messageValue);
    }

    private void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    #endregion

    #region Time Calls

    public void GetCurrentTime()
    {
        var timeRequest = new GetTimeRequest();
        PlayFabClientAPI.GetTime(timeRequest, OnGetTimeSuccess, OnGetTimeFailure);
    }

    //we send the server time to the menu manager to compare with the stored time stamp and get the time passed since the first "full play" (play with 5 charges)
    private void OnGetTimeSuccess(GetTimeResult result)
    {
        MenuManager.instance.UpdateWaitTimes(result.Time);
    }

    private void OnGetTimeFailure(PlayFabError error)
    {
        Debug.Log("There was a problem getting the time. Error: " + error.GenerateErrorReport());
        MenuManager.instance.UpdateTimeError();
    }

    //gets the timestamp to start a new streak of games. Upon getting the server time a game starts
    public void GetTimeStampedGame()
    {
        var timeRequest = new GetTimeRequest();
        PlayFabClientAPI.GetTime(timeRequest, OnStartTimeStampSuccess, OnGetTimeFailure);
    }

    private void OnStartTimeStampSuccess(GetTimeResult result)
    {
        MenuManager.instance.TimeStampSuccess(result.Time);
    }

    #endregion

    #region Leaderboard

    public void GetLeaderboard()
    {
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "playerHighScore", MaxResultsCount = GameSettings.instance.displayEntriesNumber };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboard, OnGetLeaderboardError);
    }

    void OnGetLeaderboard(GetLeaderboardResult result)
    {
        List<MenuManager.LeaderboardEntry> entries = new List<MenuManager.LeaderboardEntry>();
        foreach(PlayerLeaderboardEntry entry in result.Leaderboard)
        {
            Debug.Log(entry.DisplayName + ": " + entry.StatValue);
            MenuManager.LeaderboardEntry nuEntry = new MenuManager.LeaderboardEntry { name = entry.DisplayName, score = entry.StatValue };
            entries.Add(nuEntry);
        }

        MenuManager.instance.BuildLeaderboardSuccess(entries.ToArray());
    }

    void OnGetLeaderboardError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
        MenuManager.instance.BuildLeaderboardError();
    }

    #endregion
}
