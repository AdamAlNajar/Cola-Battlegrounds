using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordRPCManager : MonoBehaviour
{
    public static DiscordRPCManager Instance;

#if UNITY_STANDALONE_WIN
    private Discord.Discord discord;
    private bool discordInit = false;
#endif

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
#if UNITY_STANDALONE_WIN
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
#endif
    }

    private void OnDisable()
    {
#if UNITY_STANDALONE_WIN
        if (discordInit && discord != null)
        {
            discord.Dispose();
        }
#endif
    }

    private void Update()
    {
#if UNITY_STANDALONE_WIN
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
#endif
    }

    /// <summary>
    /// Updates Discord activity. Safe to call on any platform.
    /// </summary>
    public void ChangeStatus(string state, string details)
    {
#if UNITY_STANDALONE_WIN
        if (!discordInit) return;

        try
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                State = state,
                Details = details,
                Assets = { LargeImage = "main" }
            };
            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res == Discord.Result.Ok)
                    Debug.Log("Discord activity updated successfully.");
                else
                    Debug.LogWarning("Failed to update Discord activity: " + res);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Discord RPC error: " + e.Message);
        }
#else
        // Do nothing on non-Windows platforms
#endif
    }
}
