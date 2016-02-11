using UnityEngine;
using UnityEngine.SceneManagement;
public class StartScript : MonoBehaviour
{
    static bool play = false;
    // Use this for initialization
    void Start()
    {
        play = false;
    }

    float maxpoints = 5;

    void OnGUI()
    {
        if (!play)
        {
            //GUI.Box(new Rect(Screen.width / 8, Screen.height / 4, Screen.w­idth / 1.3f, Screen.height / 2), "IITG DIHING KRITI");
            if (GUI.Button(new Rect(Screen.width / 2.6f, Screen.height / 2.8f, Screen.width / 4, Screen.height / 10), "Play"))
            { play = true; SceneManager.LoadScene("Core");}
            if (GUI.Button(new Rect(Screen.width / 2.6f, Screen.height / 1.7f, Screen.width / 4, Screen.height / 10), "Quit"))
            { Application.Quit(); }
        }

    }

}
