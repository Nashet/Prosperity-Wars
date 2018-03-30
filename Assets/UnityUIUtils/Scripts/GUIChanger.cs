using Nashet.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{
    public class GUIChanger : MonoBehaviour
    {
        [SerializeField]
        private Color background;

        private static Color _background;

        public static Color BackgroundColor
        {
            get { return _background; }
        }

        [SerializeField]
        private Color buttons;

        private static Color _buttons;

        public static Color ButtonsColor
        {
            get { return _buttons; }
        }

        [SerializeField]
        private Color darkestColor;

        private static Color _darkestColor;

        public static Color DarkestColor
        {
            get { return _darkestColor; }
        }

        [SerializeField]
        private Color disabledButtonColor;

        private static Color _disabledButtonColor;

        public static Color DisabledButtonColor
        {
            get { return _disabledButtonColor; }
        }

        private void Awake()
        {
            _buttons = buttons;

            _background = background;
            _darkestColor = darkestColor;
            _disabledButtonColor = disabledButtonColor;
            Apply(gameObject);
        }

        public static void Apply(GameObject target)
        {
            foreach (var item in target.GetComponentsInChildren<Image>())
            {
                if (item.HasComponent<Button>()
                    || item.HasComponent<Dropdown>()
                    || item.HasComponentInParentParent<Scrollbar>()
                    //|| item.HasComponent<ScrollRect>()
                    || item.HasComponentInParent<Toggle>()
                    || item.HasComponentInParentParent<Slider>()
                    || item.HasComponentInParentParent<SliderExponential>())
                {
                    item.color = ButtonsColor;
                    var button = item.GetComponent<Button>();
                    if (button != null)
                    {
                        ColorBlock cb = button.colors;
                        cb.disabledColor = DisabledButtonColor;
                        button.colors = cb;
                    }
                }
                else
                {
                    item.material = null;
                    item.color = BackgroundColor;
                    //if (item.sprite != sprite)
                    //    item.sprite = sprite;
                }
            }
        }
    }
}