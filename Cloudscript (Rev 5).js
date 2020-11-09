
handlers.ResetPlayerStats = function (args, context) {
    var request = {
        PlayFabId: currentPlayerId, Statistics: [{
                StatisticName: "playerHighScore",
                Value: 0
            },
            {
                StatisticName: "playerGold",
                Value: 0
            },
            {
                StatisticName: "playerGems",
                Value: 0
            }]
    };
    
    var playerStatResult = server.UpdatePlayerStatistics(request);
    return {messageValue : "Player Statistics Reset"};
};

handlers.UpdatePlayerStats = function (args, context) {
    var request = {
        PlayFabId: currentPlayerId, Statistics: [{
                StatisticName: "playerHighScore",
                Value: args.playerHighScore
            },
            {
                StatisticName: "playerGold",
                Value: args.playerGold
            },
            {
                StatisticName: "playerGems",
                Value: args.playerGems
            }]
    };
    
    var playerStatResult = server.UpdatePlayerStatistics(request);
    return {messageValue : "Updated Player Statistics"};
};
