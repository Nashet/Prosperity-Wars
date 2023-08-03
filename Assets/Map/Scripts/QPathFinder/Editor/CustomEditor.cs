using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace QPathFinder
{
    public class CustomGUI : Editor
    {
        //static List<string> HeaderList = new List<string>();

        public static void Space()
        {
            EditorGUILayout.Space();
        }

        public static void DrawSeparator(Color color, float size = 1.0f)
        {
            EditorGUILayout.Space();
            Texture2D tex = new Texture2D(1, 1);

            GUI.color = color;
            float y = GUILayoutUtility.GetLastRect().yMax;
            GUI.DrawTexture(new Rect(0.0f, y, Screen.width, size), tex);
            tex.hideFlags = HideFlags.DontSave;
            GUI.color = Color.white;

            EditorGUILayout.Space();
        }

        public static GUIStyle GetStyleWithRichText ( GUIStyle style = null )
        {
            style = style != null ? style : new GUIStyle();
            style.richText = true;
            return style;
        }
        public static GUIStyle SetAlignmentForText ( TextAnchor anchor, GUIStyle style = null )
        {
            style = style != null ? style : new GUIStyle();
            style.alignment = anchor;
            return style;
        }
    }
}