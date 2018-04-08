using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Just bottom panel with several buttons
    /// </summary>
    public class BottomPanel : Window
    {
        [SerializeField]
        private Text generalText;

        // Use this for initialization
        private void Awake() // used to position other windows
        {
            MainCamera.bottomPanel = this;
            generalText.text = "Prosperity Wars v0.20.0";
            Hide();
        }

        public override void Refresh()
        {
        }

        public void onStatisticsClick()
        {
            if (MainCamera.StatisticPanel.isActiveAndEnabled)
                MainCamera.StatisticPanel.Hide();
            else
                MainCamera.StatisticPanel.Show();
        }

        public void onddMapModesChange(int newMapMode)
        {
            if (Game.getMapMode() != newMapMode)
                Game.redrawMapAccordingToMapMode(newMapMode);
        }
        [SerializeField]
        private GameObject debugWindowPrefab;
        public void OnDebugWindowOpen()
        {
            if (!DebugWindow.Exist)
            {
                var window = Instantiate(debugWindowPrefab, transform.parent);
                window.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, 150f);
            }
        }
        
        
        public void OnScrollLeft()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(-1f, 0f, 0f);
        }
        public void OnScrollRight()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(1f, 0f, 0f);
        }
        public void OnScrollUp()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(0f, 1f, 0f);
        }
        public void OnScrollDown()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(0f, -1f, 0f);
        }
        public void OnScaleIn()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(0f, 0f, 0.2f);
        }
        public void OnScaleOut()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(0f, 0f, -0.2f);
        }
    }
}