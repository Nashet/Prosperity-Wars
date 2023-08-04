using Nashet.UnityUIUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Warning! This mono object will not work in webGL if instantiated (will not load image file)    
    /// </summary>
    class MapOptions : Hideable
    {
        static public bool MadeChoise;

        [SerializeField]
        private Toggle randomMap, industrialStart;

        [SerializeField]
        private CanvasSampleOpenFileImage imageOpener;

        [SerializeField]
        private RawImage loadedImage;

        [SerializeField]
        private Button generate, loadImage;
        [SerializeField] List<GameObject> hideableElements;

		internal static Texture2D MapImage;
		private RectTransform rectTransform;

		private void Start()
        {
			rectTransform = GetComponent<RectTransform>();
			GUIChanger.Apply(gameObject);
            industrialStart.isOn = Game.IndustrialStart;
            if (Game.devMode)
                Hide();
        }
        //public override void Refresh()
        //{

        //}
        public void OnIndustrialStartChanged(bool value)
        {
            Game.IndustrialStart = value;
        }
        public void OnMapFromFileToggled(bool value)
        {
			if (value == true)
			{
				generate.interactable = false;
				loadImage.interactable = true;
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 354);
				SetHideableObjectsState(true);
			}
			else
            {
                generate.interactable = true;
                loadImage.interactable = false;
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 140);
				SetHideableObjectsState(false);
			}
		}

		private void SetHideableObjectsState(bool state)
		{
			foreach (var item in hideableElements)
			{
				item.gameObject.SetActive(state);
			}
		}

		public void OnLoadImageClicked()
        {

        }
        public void OnGenerate()
        {
            if (randomMap.isOn)
            {
                Game.readMapFormFile = false;
                MadeChoise = true;
                Hide();
            }
            else //take picture
            { 
                if (loadedImage.texture == null) //todo thats never true
                    MessageSystem.Instance.NewMessage("", "Wasn't able to load that image", "Ok", false);
                else
                {
                    Game.readMapFormFile = true;
                    MapImage = loadedImage.texture as Texture2D;
                    MadeChoise = true;
                    Hide();
                }
            }

        }
        private void Update()
        {
            if (!randomMap.isOn)
                generate.interactable = loadedImage.texture!=null;
            else
                generate.interactable = true;
        }
    }
}
