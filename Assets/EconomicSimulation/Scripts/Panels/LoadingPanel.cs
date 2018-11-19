using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class LoadingPanel : Window
    {
        [SerializeField]
        private Text loadingText;

        [SerializeField]
        private GameObject mapOptionsWindowPrefab;

        // Use this for initialization
        private void Start()
        {
            MainCamera.loadingPanel = this;
            var window = Instantiate(mapOptionsWindowPrefab, transform.parent);
            window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        }

        public void updateStatus(string text)
        {
            loadingText.text = text;
        }

        public override void Refresh()
        {
            
        }
    }
}