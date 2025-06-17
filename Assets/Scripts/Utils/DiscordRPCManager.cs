using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordRPCManager : MonoBehaviour
{
    public static DiscordRPCManager Instance;
    Discord.Discord discord;
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        discord = new Discord.Discord(1382033954315440249, (ulong)Discord.CreateFlags.NoRequireDiscord);
    }
    private void OnDisable()
    {
        discord.Dispose();
    }
    public void ChangeStatus(string state, string details)
    {
        var activityManager = discord.GetActivityManager();
        var activity = new Discord.Activity
        {
            State = state,
            Details = details,
            Assets =
            {
                LargeImage = "main"
            }
        };
        activityManager.UpdateActivity(activity, (res) =>
        {
            Debug.Log("Activity Updated Successfully");
        });
    }
    private void Update() {
        discord.RunCallbacks();
    }
}
