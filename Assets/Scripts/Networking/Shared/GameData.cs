using System;

public enum Map
{
    Default
}

public enum GameMode
{
    Default
}

public enum GameQueue
{
    Solo,
    Team,
    SoloDeathMatch,
    SoloTimeMatch,
    SoloPointsMatch
}

[Serializable]
public class UserData
{
    public string userName;
    public string userAuthId;
    public int teamIndex = -1;
    
    public GameInfo userGamePreferences = new GameInfo();
}

[Serializable]
public class GameInfo
{
    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        return gameQueue switch
        {
            GameQueue.Solo => "solo-queue",
            GameQueue.Team => "team-queue",
            GameQueue.SoloDeathMatch => "solo-queue-deathmatch",
            GameQueue.SoloTimeMatch => "solo-queue-time-match",
            GameQueue.SoloPointsMatch=>"solo-queue-points-match",
            _ => "solo-queue"
        };
    }
}