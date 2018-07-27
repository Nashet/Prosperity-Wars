using UnityEngine;
using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{
    public class TooltipBase : MonoBehaviour
    {
        //manually selectable padding for the background image
        [SerializeField]
        private int horizontalPadding;

        [SerializeField]
        private int verticalPadding;

        //tooltip text
        [SerializeField]
        private Text thisText;

        [SerializeField]
        private int showDelay;

        private int ticksSkipped = 0;

        //horizontal layout of the tooltip
        [SerializeField]
        private HorizontalLayoutGroup hlG;

        [SerializeField]
        private Image bgImageSource;

        [SerializeField]
        private float yOffset = 40f;

        //needed as the layout refreshes only on the first Update() call
        private bool firstUpdate;

        //if the tooltip is inside a UI element
        private bool inside;

        //size of the tooltip, needed to track if out of screen
        // public float width;
        // public float height;

        //detect canvas mode so to apply different behaviors to different canvas modes, currently only RenderMode.ScreenSpaceCamera implemented
        private int canvasMode;

        private RenderMode GUIMode;

        //the scene GUI camera
        private Camera GUICamera;

        //the default tooltip object has the following pivots, so that the offset from the mouse is always proportional to the screen resolution (the y pivot)
        //Pivot(0.5,-0.5)

        //screen viewport corners for out of screen detection
        private Vector3 lowerLeft;

        private Vector3 upperRight;

        //scale factor of proportionality to the reference resolution (1280x720)
        [SerializeField]
        private float currentYScaleFactor;

        [SerializeField]
        private float currentXScaleFactor;

        //standard X and Y offsets of the new tooltip
        private float defaultYOffset;

        private float defaultXOffset;

        //real on screen sizes of the tooltip object
        private float tooltipRealHeight;

        private float tooltipRealWidth;

        private static TooltipBase thatObjectLink;

        //tooltip background image
        [SerializeField]
        private RectTransform bgImage;

        private CanvasGroup canvas;

        private void Start()
        {
            var image = transform.parent.GetComponent<Image>();
            image.color = GUIChanger.DarkestColor;
        }

        // Use this for initialization
        private void Awake()
        {
            //in this line you need to change the string in order to get your Camera //TODO MAYBE DO IT FROM THE INSPECTOR
            //GUICamera = GameObject.Find("GUICamera").GetComponent<Camera>();
            //GUICamera = (Camera)GameObject.FindWithTag("MainCamera");// MainCamera.cameraMy;
            GameObject gameControllerObject = GameObject.FindWithTag("MainCamera");
            if (gameControllerObject != null)
            {
                GUICamera = gameControllerObject.GetComponent<Camera>();
            }

            GUIMode = transform.parent.parent.GetComponent<Canvas>().renderMode;

            bgImageSource = bgImage.GetComponent<Image>();

            //at start the pointer is never to be considered over and UI element
            inside = false;

            //assign the tooltip to the singleton GUI class manager for fast access
            //TacticalGUIManager.tgm.mmttp = this;

            //hide the tooltip
            HideTooltipVisibility();
            canvas = transform.parent.GetComponent<CanvasGroup>();
            transform.parent.gameObject.SetActive(false);
            thatObjectLink = this;


        }

        public bool isInside()
        {
            return inside;
        }

        public void redrawDynamicString(string text)
        {
            //init tooltip string
            thisText.text = text;
        }

        public static TooltipBase get()
        {
            return thatObjectLink;
        }

        //single string input tooltip
        public void SetTooltip(string text)
        {
            NewTooltip();

            //init tooltip string
            thisText.text = text;
            inside = true;
            //call the position function

            OnScreenSpaceCamera();
            LayoutInit();
            firstUpdate = true;
        }

        // Update is called once per frame
        private void Update()
        {
            LayoutInit();
            if (inside)
            {
                if (ticksSkipped >= showDelay)
                    canvas.alpha = 1f;
                OnScreenSpaceCamera();
                ticksSkipped++;
            }
        }

        //this function is used in order to setup the size of the tooltip by cheating on the HorizontalLayoutBehavior. The resize is done in the first update.
        private void LayoutInit()
        {
            if (firstUpdate)
            {
                firstUpdate = false;

                bgImage.sizeDelta = new Vector2(hlG.preferredWidth + horizontalPadding, hlG.preferredHeight + verticalPadding);

                defaultYOffset = (bgImage.sizeDelta.y * currentYScaleFactor * (bgImage.pivot.y));
                defaultXOffset = (bgImage.sizeDelta.x * currentXScaleFactor * (bgImage.pivot.x));

                tooltipRealHeight = bgImage.sizeDelta.y * currentYScaleFactor;
                tooltipRealWidth = bgImage.sizeDelta.x * currentXScaleFactor;

                ActivateTooltipVisibility();
            }
        }

        //init basic variables on a new tooltip set
        private void NewTooltip()
        {
            firstUpdate = true;

            lowerLeft = GUICamera.ViewportToScreenPoint(new Vector3(0.0f, 0.0f, 0.0f));
            upperRight = GUICamera.ViewportToScreenPoint(new Vector3(1.0f, 1.0f, 0.0f));

            //currentYScaleFactor = Screen.height / this.transform.root.GetComponent<CanvasScaler>().referenceResolution.y;
            //currentXScaleFactor = Screen.width / this.transform.root.GetComponent<CanvasScaler>().referenceResolution.x;
        }

        //used to visualize the tooltip one update call after it has been built (to avoid flickers)
        public void ActivateTooltipVisibility()
        {
            Color textColor = thisText.color;
            thisText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
            bgImageSource.color = new Color(bgImageSource.color.r, bgImageSource.color.g, bgImageSource.color.b, 0.8f);
        }

        //used to hide the tooltip so that it can be made visible one update call after it has been built (to avoid flickers)
        public void HideTooltipVisibility()
        {
            Color textColor = thisText.color;
            thisText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            bgImageSource.color = new Color(bgImageSource.color.r, bgImageSource.color.g, bgImageSource.color.b, 0f);
        }

        //position function, currently not working correctly due to the use of pivots and not manual offsets, soon to be fixed
        public void OnScreenSpaceCamera()
        {
            Vector3 newPos = (Input.mousePosition);

            var rect = transform.parent.GetComponent<RectTransform>();
            rect.position = new Vector2(newPos.x - tooltipRealWidth / 2f, newPos.y - tooltipRealHeight - yOffset);
            if (rect.position.x < 0)
            {
                rect.position = new Vector3(0, rect.position.y, rect.position.z);
            }

            if (rect.position.y < 0)
            {
                rect.position = new Vector3(rect.position.x, 0, rect.position.z);
            }

            if (rect.position.x > Screen.width - rect.sizeDelta.x)
            {
                rect.position = new Vector3(Screen.width - rect.sizeDelta.x, rect.position.y, rect.position.z);
            }

            if (rect.position.y > Screen.height - rect.sizeDelta.y)
            {
                rect.position = new Vector3(rect.position.x, Screen.height - rect.sizeDelta.y, rect.position.z);
            }
            transform.parent.gameObject.SetActive(true);
            transform.parent.SetAsLastSibling();
        }

        //call to hide tooltip when hovering out from the object
        public void HideTooltip()
        {
            if (this != null)
            {
                transform.parent.gameObject.SetActive(false);
                canvas.alpha = 0f;
                inside = false;
                HideTooltipVisibility();
                ticksSkipped = 0;
            }

        }
    }
}