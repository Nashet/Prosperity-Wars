using Nashet.UnityUIUtils;
using System.Linq;
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
        protected Text generalText;

        [SerializeField]
        protected GameObject debugWindowPrefab;
        private MainCamera cameraScript;

        // Use this for initialization
        new private void Awake() // used to position other windows
        {
            base.Awake();
            MainCamera.bottomPanel = this;
            generalText.text = "Prosperity Wars v0.20.11";      
            Hide();
            cameraScript = Camera.main.GetComponent<MainCamera>(); ;
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
            var  _newMapMod =( Game.MapModes)newMapMode;
            if (Game.MapMode != _newMapMod)
            {
                Game.MapMode = _newMapMod;
                Game.redrawMapAccordingToMapMode();
            }
        }

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
            cameraScript.Move(-1f, 0f);
        }

        public void OnScrollRight()
        {
            cameraScript.Move(1f, 0f);
        }

        public void OnScrollUp()
        {
            cameraScript.Move(0f, 1f);
        }

        public void OnScrollDown()
        {
            cameraScript.Move(0f, -1f);
        }

        public void OnScaleIn()
        {
            cameraScript.Zoom(-0.1f);
        }

        public void OnScaleOut()
        {
            cameraScript.Zoom(0.1f);
        }

        public void OnTest()
        {
            //gameObject = new GameObject(string.Format("{0}", getID()),);

            //var unitObject = Instantiate(LinksManager.Get.UnitPrefab, World.Get.transform);

            //unitObject.GetComponent<Unit>().SetPosition(Game.selectedProvince);
            //unitObject.name = (World.GetAllProvinces().Count() + Random.Range(0, 2000)).ToString();
        }
    }
}