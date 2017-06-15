using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanel : DragPanel
{
    public Text loadingText;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (MainCamera.Game == null)
        {
            SceneManager.LoadScene("ES-base", LoadSceneMode.Additive);
            MainCamera.Game = new Game();
            MainCamera.Game.initialize();

            // MainCamera.Game.Start();
        }

        //if (MainCamera.Game.Update())
        //{
        //    // Alternative to the OnFinished callback
        //    //MainCamera.Game = null;
        //    // close scene here
        //}

    }


}
