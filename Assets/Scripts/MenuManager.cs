using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class to control de menu events. singleton.
public class MenuManager : SingletonBehaviour<MenuManager>
{
    //UI screens
    public GameObject[] UIScreens;

    //playfab manager
    public PlayFabManager playFabManager;

    //camera of the menu, for aesthetic purposes
    public Transform menuCamera;
    public float cameraRotationSpeed; //rotation speed of the camera effect

    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.DeleteAll();

        //we create the playfab manager after checking if there's no other object of its kind already created
        PlayFabManager[] managersActive = FindObjectsOfType<PlayFabManager>();
        if(managersActive.Length > 1) //later menu reach. must erase the new playfabManager
        {
            for (int i = 0; i < managersActive.Length; i++)
            {
                if (!managersActive[i].activeManager)
                {
                    Destroy(managersActive[i].gameObject);
                }
                else
                    playFabManager = managersActive[i];
            }

            playFabManager.playerLoginPanel.SetActive(false);
            ShowScreen(0);
        }
        else
        {
            DontDestroyOnLoad(playFabManager.gameObject);
            playFabManager.StartLogin();
            ShowScreen(4);
        }
    }

    void LateUpdate()
    {
        menuCamera.forward = Quaternion.AngleAxis(cameraRotationSpeed * Time.deltaTime, Vector3.up) * menuCamera.forward;
    }

    //changes the screen to display
    public void ShowScreen(int screenIndex)
    {
        for(int i = 0; i < UIScreens.Length; i++)
        {
            UIScreens[i].SetActive(i == screenIndex);
        }
    }

    //takes you to the game scene
    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
