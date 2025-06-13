using UnityEngine;
public class ClearPlayerPrefs : MonoBehaviour
{
    public void clearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
