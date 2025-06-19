using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hyperlinks : MonoBehaviour
{
    public void OpenGithubPage()
    {
        Application.OpenURL("https://github.com/AdamAlNajar/ColaBattlegrounds");
    }
    public void OpenItchioPage()
    {
        Application.OpenURL("https://adamalnajar.itch.io/");
    }
    public void OpenYoutubePage()
    {
        Application.OpenURL("https://www.youtube.com/@adamoolah");
    }
}