
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BottomPanel : MonoBehaviour
{
    [SerializeField]
    private Text generalText;
    // Use this for initialization
    void Awake()
    {
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
        generalText.text = "Economic Simulation Demo v0.16.0";
    }
    public void onExitClick()
    {
#if UNITY_WEBGL
        Screen.fullScreen = false;
#else
        Application.Quit();
#endif
    }
    public void onStatisticsClick()
    {
        if (MainCamera.StatisticPanel.isActiveAndEnabled)
            MainCamera.StatisticPanel.hide();
        else
            MainCamera.StatisticPanel.show(true);
    }
    public void onddMapModesChange(int newMapMode)
    {
        if (Game.getMapMode() != newMapMode)
            Game.redrawMapAccordingToMapMode(newMapMode);

    }
}
