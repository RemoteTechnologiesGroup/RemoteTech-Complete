using System;
using RemoteTech.Common;
using RemoteTech.Delay.UI;
using UnityEngine;

namespace RemoteTech.Delay
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RemoteTechDelayCoreFlight : RemoteTechDelayCore
    {
        public new void Start()
        {
            base.Start();
        }
    }

    public abstract class RemoteTechDelayCore : MonoBehaviour
    {
        // handle the F2 key GUI show / hide
        private bool _guiVisible = true;

        /// <summary>
        ///     UI overlay used to display and handle the status quadrant (time delay) and the flight computer button.
        /// </summary>
        public DelayQuadrant DelayQuadrant { get; protected set; }


        /// <summary>
        ///     Methods can register to this event to be called during the OnGUI() method of the Unity engine (GUI Rendering engine
        ///     phase).
        /// </summary>
        public event Action OnGuiUpdate = delegate { };

        /// <summary>
        ///     Called by Unity engine during initialization phase.
        ///     Only ever called once.
        /// </summary>
        public void Start()
        {
            Logging.Debug($"RemoteTech-Delay Starting. Scene: {HighLogic.LoadedScene}");

            DelayQuadrant = new DelayQuadrant();
        }

        /// <summary>
        ///     Called by the Unity engine during the GUI rendering phase.
        ///     Note that OnGUI() is called multiple times per frame in response to GUI events.
        ///     The Layout and Repaint events are processed first, followed by a Layout and keyboard/mouse event for each input
        ///     event.
        /// </summary>
        public void OnGUI()
        {
            if (!_guiVisible)
                return;

            DelayQuadrant?.Draw();
        }

        /// <summary>
        ///     F2 GUI Show / Hide functionality: called when the UI must be displayed.
        /// </summary>
        public void UiOn()
        {
            _guiVisible = true;
        }

        /// <summary>
        ///     F2 GUI Show / Hide functionality: called when the UI must be hidden.
        /// </summary>
        public void UiOff()
        {
            _guiVisible = false;
        }
    }
}