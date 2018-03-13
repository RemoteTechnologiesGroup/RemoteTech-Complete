using CommNet;
using KSP.UI;
using RemoteTech.Common;
using RemoteTech.Common.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Delay.UI
{
    /// <summary>
    ///     Class used to display and handle the status quadrant (time delay) and the flight computer button.
    ///     This is located right below the time warp UI, hence its name.
    /// </summary>
    public class DelayQuadrant
    {
        /// <summary>
        ///     RemoteTech delay quadrant texture.
        /// </summary>
        private readonly Texture2D _delayQuadrantTexture;

        /// <summary>
        ///     RemoteTech delay quadrant text style.
        /// </summary>
        private readonly GUIStyle _delayTextStyle;

        /// <summary>
        ///     KSP time quadrant image (used for position access).
        /// </summary>
        private readonly Image _timeQuadrantImage;

        public DelayQuadrant()
        {
            // check if the KSP TimeWarp object is present (otherwise, we can't do anything)
            if (TimeWarp.fetch == null)
            {
                Logging.Error("TimeWarp object is null.");
                return;
            }

            // Get the time quadrant KSP image (used for positioning the our delay quadrant)
            var timeQuadrantGameObject = GameObject.Find("TimeQuadrant");
            if (timeQuadrantGameObject)
            {
                _timeQuadrantImage = timeQuadrantGameObject.GetComponent<Image>();
                if (_timeQuadrantImage == null)
                {
                    Logging.Error("Couldn't find the KSP TimeQuadrant image.");
                    return;
                }
            }

            // create a style (for the connection / delay text) from the high logic skin label style.
            var skin = Object.Instantiate(HighLogic.Skin);
            _delayTextStyle = new GUIStyle(skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                font = skin.font
            };

            // Load RemoteTech delay quadrant background image
            _delayQuadrantTexture = UiUtils.LoadTexture("DelayQuadrantStatus");
            if (_delayQuadrantTexture == null)
                Logging.Error("Couldn't load the RemoteTech delay quadrant texture.");
        }

        /// <summary>
        /// </summary>
        private static CommNetVessel CommNetVessel => FlightGlobals.ActiveVessel.Connection;

        private static string DisplayText
        {
            get
            {
                if (CommNetVessel == null)
                    return "N/A";

                //TODO check if vessel has local control
                //if(CommNetVessel.Vessel.)

                if (CommNetVessel.IsConnected)
                    return $"D+ {CommNetVessel.SignalDelay:F5}s";

                //TODO check if delay is enabled (even if this assembly is loaded delay might be disabled in settings)
                // return "Connected";

                return "No Connection";
            }
        }

        /// <summary>
        ///     Draw the RemoteTech delay status quadrant.
        /// </summary>
        public void Draw()
        {
            // don't draw if we haven't been able to get the KSP time quadrant image.
            if (_timeQuadrantImage == null)
                return;

            // get image coordinates in screen coordinates
            Vector2 timeWarpImageScreenCoord =
                UIMainCamera.Camera.WorldToScreenPoint(_timeQuadrantImage.rectTransform.position);

            // calculate proper UI scaling depending on game settings.
            var scale = GameSettings.UI_SCALE_TIME * GameSettings.UI_SCALE;
            var topLeftTotimeQuadrant = Screen.height -
                                        (timeWarpImageScreenCoord.y - _timeQuadrantImage.preferredHeight * scale);
            var texBackgroundHeight = _delayQuadrantTexture.height * 0.7f * scale;
            var texBackgroundWidth = _delayQuadrantTexture.width * 0.8111f * scale;

            // get the delay text position
            var delaytextPosition = new Rect((timeWarpImageScreenCoord.x + 12.0f) * scale,
                topLeftTotimeQuadrant + 2 * scale, 50.0f * scale, 20.0f * scale);

            // calculate the position under the KSP time warp object
            var pos = new Rect(timeWarpImageScreenCoord.x, topLeftTotimeQuadrant, texBackgroundWidth,
                texBackgroundHeight);

            // draw the image
            GUI.DrawTexture(pos, _delayQuadrantTexture);

            // get color for the delay-text
            _delayTextStyle.normal.textColor = new Color(0.56078f, 0.10196f, 0.07450f);
            if (CommNetVessel != null && CommNetVessel.IsConnected)
                _delayTextStyle.normal.textColor = XKCDColors.GreenApple;

            // draw connection / delay text
            _delayTextStyle.fontSize = (int) (_delayTextStyle.font.fontSize * scale);
            GUI.Label(delaytextPosition, DisplayText, _delayTextStyle);
        }
    }
}