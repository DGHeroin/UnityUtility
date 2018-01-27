using UnityEngine;
using UnityEditor;
using System;

namespace UnityUtility.Editor.Helper
{
    public class EditorHelper : MonoBehaviour {

        [MenuItem("Tools/Utility/Screenshot/TakeScreenshot")]
        private static void Screenshot() {
            string filename = string.Format ("/Screenshot_{0}.png", Now());
            string path = Application.persistentDataPath + filename;
            Debug.Log ("Screenshot: " + path);
            ScreenCapture.CaptureScreenshot (path);
        }

        static int Now() {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}

