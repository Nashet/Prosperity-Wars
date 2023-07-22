using Nashet.GameplayControllers;
using Nashet.Map.GameplayControllers;
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
        protected Text generalText;

        [SerializeField]
        protected GameObject debugWindowPrefab;
		[SerializeField]
		private CameraController cameraController;
        [SerializeField]
        private ProvinceSelectionHelper provinceSelectionHelper;

        // Use this for initialization
        new private void Awake() // used to position other windows
        {
            base.Awake();
            MainCamera.bottomPanel = this;
            generalText.text = "Prosperity Wars v0.20.11";           

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
            var  _newMapMod =( Game.MapModes)newMapMode;
            if (Game.MapMode != _newMapMod)
            {
                Game.MapMode = _newMapMod;
                MainCamera.Get.redrawMapAccordingToMapMode(provinceSelectionHelper.selectedProvince);
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
            cameraController.Move(-1f, 0f);
        }

        public void OnScrollRight()
        {
            cameraController.Move(1f, 0f);
        }

        public void OnScrollUp()
        {
            cameraController.Move(0f, 1f);
        }

        public void OnScrollDown()
        {
            cameraController.Move(0f, -1f);
        }

        public void OnScaleIn()
        {
            cameraController.Zoom(-0.1f);
        }

        public void OnScaleOut()
        {
            cameraController.Zoom(0.1f);
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