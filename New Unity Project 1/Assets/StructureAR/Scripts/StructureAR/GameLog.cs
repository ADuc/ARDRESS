/*
  This file is part of the Structure SDK.
  Copyright © 2015 Occipital, Inc. All rights reserved.
  http://structure.io
*/

using System;
using UnityEngine;

namespace StructureAR
{
    /// <summary>
    /// Convenient way to log events to the screen on device since debugging
    /// simple things on the iOS device requires a few extra steps.
    /// </summary>
    public class GameLog : MonoBehaviour
    {
        #region PUBLIC_FIELDS
        public GUIText LogLabel;
        public int LogLines = 15;
        public static int _LogLines;
        #endregion

        #region PRIVATE_FIELDS
        private static string[] _LogText;
        private static GUIText _LogLabel;
        private static int line;
        private static bool _ShowGameLog;
        #endregion

        #region PUBLIC_PROPERTIES
        public static bool ShowGameLog
        {
            get
            {
                return _ShowGameLog;
            }
            set
            {
                GameLog._LogLabel.enabled = value;
                GameLog._ShowGameLog = value;
            }
        }
        #endregion

        #region UNITY_METHODS
        protected void Awake()
        {
            GameLog._LogText = new string[this.LogLines];
            GameLog._LogLines = this.LogLines;
            if (this.LogLabel == null)
            {
                this.LogLabel = this.gameObject.GetComponent<GUIText>();
                if (this.LogLabel == null)
                {
                    this.LogLabel = this.gameObject.AddComponent<GUIText>();
                    GameLog._LogLabel = this.LogLabel;
                }
            }
            else
            {
                GameLog._LogLabel = this.LogLabel;
            }
            GameLog._LogLabel.anchor = TextAnchor.LowerLeft;
            GameLog._LogLabel.alignment = TextAlignment.Left;
            GameLog._LogLabel.fontSize = 32;
            GameLog._LogLabel = this.LogLabel;
        }
        #endregion

        #region GAMELOG_STATIC_METHODS
        public static void Log(object sender, string log)
        {
            if (GameLog._LogLabel == null)
            {
                return;
            }
            GameLog.Log(sender.ToString() + " : " + log);
        }

        public static void Log(string log)
        {
            if (GameLog._LogLabel == null)
            {
                return;
            }

            GameLog._LogLabel.text = String.Empty;

            //make an empty string to copy over the old array
            string[] tempString = new String[GameLog._LogLines];
            for (int i = 1; i < GameLog._LogLines; ++i)
            {
                //copy second line to first
                tempString [i - 1] = GameLog._LogText [i];
            }

            tempString [GameLog._LogLines - 1] = line++.ToString() + ") " + log + "\n";
            tempString.CopyTo(GameLog._LogText, 0);
            String temp = String.Empty;
            for (int i = 0; i < GameLog._LogLines; ++i)
            {
                temp += tempString [i];
            }

            GameLog._LogLabel.text = temp;
        }
        #endregion
    }
}