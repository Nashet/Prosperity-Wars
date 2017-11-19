
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BottomPanel : MonoBehaviour
{    
    public Text generalText;
    // Use this for initialization
    void Start()
    {
        //btnPlay.onClick.AddListener(() => onbtnPlayClick(btnPlay));
        //btnStep.onClick.AddListener(() => onbtnStepClick(btnPlay));
        
        MainCamera.bottomPanel = this;
        hide();
    }
    public void hide()
    {
        gameObject.SetActive(false);
    }
    public void show()
    {
        gameObject.SetActive(true);
        //panelRectTransform.SetAsLastSibling();
        refresh();
    }
    public void refresh()
    {
        generalText.text = "Economic Simulation v0.16.0";
    }
    
    public void onExitClick()
    {
        Application.Quit();
    }
    public void onddMapModesChange(int newMapMode)
    {
        if (Game.getMapMode() != newMapMode)
            Game.redrawMapAccordingToMapMode(newMapMode);

    }
}
