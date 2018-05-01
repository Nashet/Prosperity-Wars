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
        private Text generalText;

        // Use this for initialization
        private void Awake() // used to position other windows
        {
            MainCamera.bottomPanel = this;
            generalText.text = "Prosperity Wars v0.20.1";
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
            cameraScript.Move(0f, 0f, 1f);
        }
        public void OnScrollDown()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(0f, 0f, -1f);
        }
        public void OnScaleIn()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(0f, 0.2f, 0f);
        }
        public void OnScaleOut()
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            cameraScript.Move(0f, -0.2f, 0f);
        }
        public void OnTest()
        {
            //gameObject = new GameObject(string.Format("{0}", getID()),);

            var unitObject = GameObject.Instantiate(World.Get.UnitPrefab, World.Get.transform);
            var temp = Game.selectedProvince.getPosition();
            Vector3 position;
            position.x = temp.x;
            position.y = temp.z;
            position.z = temp.y;
            unitObject.GetComponent<Unit>().SetPosition(position);
            unitObject.name = (World.GetAllProvinces().Count() + Random.Range(0, 2000)).ToString();

            MeshCollider meshCollider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
            meshCollider.sharedMesh = unitObject.GetComponent<Mesh>();
            // in case you want the new gameobject to be a child
            // of the gameobject that your script is attached to
            //gameObject.transform.parent = World.Get.transform;
        }
    }
}