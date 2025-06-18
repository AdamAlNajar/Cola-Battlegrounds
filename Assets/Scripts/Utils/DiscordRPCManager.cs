using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordRPCManager : MonoBehaviour
{
    public static DiscordRPCManager Instance;
    Discord.Discord discord;
    bool discordInit = false;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        try
        {
            discord = new Discord.Discord(1382033954315440249, (ulong)Discord.CreateFlags.NoRequireDiscord);
            discordInit = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Discord not available: " + e.Message);
            discordInit = false;
        }
    }
    private void OnDisable()
    {
        if (discordInit&& discord != null)
        {
            discord.Dispose();
        }
    }
    public void ChangeStatus(string state, string details)
    {
        if (!discordInit)
            return;
        try
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
                if (res == Discord.Result.Ok)
                {
                    Debug.Log("Discord activity updated successfully.");
                }
                else
                {
                    Debug.LogWarning("Failed to update Discord activity: " + res);
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Discord RPC error: " + e.Message);
        }
    }
    private void Update() {
        if (discordInit && discord != null)
        {
            try
            {
                discord.RunCallbacks();
            }
            catch
            {
                Debug.Log("[DiscordRPCManager] RPC FAILED TO RUN");
                discordInit = false;
            }
        }
    }
}
