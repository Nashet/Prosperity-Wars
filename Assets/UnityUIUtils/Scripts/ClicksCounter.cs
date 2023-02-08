using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{
    [RequireComponent(typeof(Button))]
    public class ClicksCounter : MonoBehaviour
    {
        [SerializeField] private int requiredClicks = 5;
        [SerializeField] private float coolDownSeconds = 0.2f;
        public UnityEvent onEnacted;

        private Button button;
        private int clicksCounted;

        private void Start()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                throw new ArgumentException($"Component {this} has no point without Button component");
            }
            if (onEnacted == null)
            {
                throw new ArgumentException($"Component {this} needs action");
            }
            button.onClick.AddListener(ClicksHandler);
        }

        private void ClicksHandler()
        {
            clicksCounted++;
            //Debug.LogError($"clicksCounted {clicksCounted}");
            if (clicksCounted >= requiredClicks)
            {
                clicksCounted = 0;
                StopAllCoroutines();
                onEnacted.Invoke();
            }
            else if (clicksCounted == 1)
            {
                StartCoroutine(Df());
            }
        }

        private IEnumerator Df()
        {
            for (int i = 0; i < clicksCounted; i++)
            {
                yield return new WaitForSeconds(coolDownSeconds);
                clicksCounted--;
                if (clicksCounted < 0)
                    clicksCounted = 0;
            }
        }
    }
}