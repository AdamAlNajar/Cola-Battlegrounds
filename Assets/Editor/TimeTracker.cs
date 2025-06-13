// Simple editor window that autosaves the working Scene
// Make sure to have this window opened to be able to execute the auto save.
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class TimeTracker : EditorWindow
{
	static double nextTimeToTrack;
	static double trackInterval = 60;
	static double totalTimeElapsed;
	static string folderPath;
	static string FolderPath
	{
		get
		{
			if(string.IsNullOrEmpty(folderPath))
			{
				folderPath = Path.Combine(Application.dataPath, "..", "TimeTracker");
			}

			return folderPath;
		}
		set
		{
			folderPath = value;
		}
	}
	static string sessionID;
	static string SessionID
	{
		get
		{
			if(string.IsNullOrEmpty(sessionID))
			{
				sessionID = UnityEngine.Random.Range(0, 999999).ToString();
			}
			return sessionID;
		}
		set
		{
			sessionID = value;
		}
	}

	[MenuItem("Window/Redlabs/Time Tracker")]
	static void Init()
	{
		TimeTracker window = (TimeTracker)GetWindowWithRect(
			typeof(TimeTracker),
			new Rect(0, 0, 300, 120));
		window.Show();

		GetTotalTime();
	}

	static void GetTotalTime()
	{
		FileInfo[] files = new DirectoryInfo(FolderPath).GetFiles();

		totalTimeElapsed = 0;
		string currentFile = $"{GetCurrentDate()}_{SessionID}.txt";
		foreach(FileInfo file in files)
		{
			if(file.Name == currentFile)
				continue;
			totalTimeElapsed += double.Parse(File.ReadAllText(file.FullName));
		}
	}

	void OnGUI()
	{
		TimeSpan currentTime = TimeSpan.FromSeconds(EditorApplication.timeSinceStartup);
		TimeSpan totalTime = TimeSpan.FromSeconds(totalTimeElapsed);

		EditorGUI.LabelField(new Rect(10, 10, 290, 20), $"Current Session: {GetFormattedTime(currentTime)}");
		EditorGUI.LabelField(new Rect(10, 30, 290, 20), $"Total Time: {GetFormattedTime(currentTime + totalTime)}");

		if(EditorApplication.timeSinceStartup >= nextTimeToTrack)
		{
			Track();

			nextTimeToTrack = EditorApplication.timeSinceStartup + trackInterval;
		}

		this.Repaint();
	}

	static string GetFormattedTime(TimeSpan t)
	{
		return string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
						t.Hours,
						t.Minutes,
						t.Seconds);
	}

	static string GetCurrentDate()
	{
		return string.Format("{0}-{1}-{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
	}

	static void Track()
	{
		string path = Path.Combine(FolderPath, $"{GetCurrentDate()}_{SessionID}.txt");

		Directory.CreateDirectory(Path.GetDirectoryName(path));
		if(!File.Exists(path))
		{
			File.Create(path);
		}

		DateTime current = DateTime.Now;
		DateTime yesterday = current.AddDays(-1).Date;
		double secondsSinceMidnight = (current - yesterday).TotalSeconds;
		double secondsElapsed = EditorApplication.timeSinceStartup;

		File.WriteAllText(path, (Math.Min(secondsElapsed, secondsSinceMidnight).ToString()));
	}
}