using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{
    public class ShowSliderValueInTooltip : ToolTipHandler
    {
        // Use this for initialization
        private void Start()
        {
            base.Start();
            SetTextDynamic(() => "Value: " + GetComponent<Slider>().value);
        }
    }
}