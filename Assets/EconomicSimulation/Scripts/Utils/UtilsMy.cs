using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nashet.Utils
{

	public static class UtilsMy
    {
        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static GameObject CreateButton(Transform parent, float x, float y,
                                            float w, float h, string message,
                                            UnityAction eventListner)
        {
            GameObject buttonObject = new GameObject("Button");
            buttonObject.transform.SetParent(parent);

            //buttonObject.layer = LayerUI;

            RectTransform trans = buttonObject.AddComponent<RectTransform>();
            SetSize(trans, new Vector2(w, h));
            trans.anchoredPosition3D = new Vector3(0, 0, 0);
            trans.anchoredPosition = new Vector2(x, y);
            trans.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            trans.localPosition.Set(0, 0, 0);

            CanvasRenderer renderer = buttonObject.AddComponent<CanvasRenderer>();

            Image image = buttonObject.AddComponent<Image>();

            Texture2D tex = Resources.Load<Texture2D>("button_bkg");
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                                      new Vector2(0.5f, 0.5f));

            Button button = buttonObject.AddComponent<Button>();
            button.interactable = true;
            button.onClick.AddListener(eventListner);

            GameObject textObject = CreateText(buttonObject.transform, 0, 0, 0, 0,
                                                       message, 24);

            return buttonObject;
        }

        private static void SetSize(RectTransform trans, Vector2 size)
        {
            Vector2 currSize = trans.rect.size;
            Vector2 sizeDiff = size - currSize;
            trans.offsetMin = trans.offsetMin -
                                      new Vector2(sizeDiff.x * trans.pivot.x,
                                          sizeDiff.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax +
                                      new Vector2(sizeDiff.x * (1.0f - trans.pivot.x),
                                          sizeDiff.y * (1.0f - trans.pivot.y));
        }

        private static GameObject CreateText(Transform parent, float x, float y,
                                         float w, float h, string message, int fontSize)
        {
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(parent);

            //textObject.layer = LayerUI;

            RectTransform trans = textObject.AddComponent<RectTransform>();
            trans.sizeDelta.Set(w, h);
            trans.anchoredPosition3D = new Vector3(0, 0, 0);
            trans.anchoredPosition = new Vector2(x, y);
            trans.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            trans.localPosition.Set(0, 0, 0);

            CanvasRenderer renderer = textObject.AddComponent<CanvasRenderer>();

            Text text = textObject.AddComponent<Text>();
            text.supportRichText = true;
            text.text = message;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.color = new Color(0, 0, 1);

            return textObject;
        }
    }
}