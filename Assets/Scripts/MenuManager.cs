using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class to control de menu events. singleton.
public class MenuManager : SingletonBehaviour<MenuManager>
{
    //UI screens
    public GameObject[] UIScreens;

    //screen that is currently being displayed

    //camera of the menu, for aesthetic purposes
    public Transform menuCamera;
    public float cameraRotationSpeed; //rotation speed of the camera effect

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteAll();

        ShowScreen(0);
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
