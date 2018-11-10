﻿/*
 * Copyright (c) 2014 Ninenium
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using KSP;
using UnityEngine;
using KSP.IO;
using System.Collections;
using System.Collections.Generic;
using KSP.UI.Screens;
using ClickThroughFix;
using ToolbarControl_NS;

namespace NavHud
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class NavHud : MonoBehaviour
    {
        private Camera _hudCam;

        private int _version = 2;

        private bool _enabled = true;
        private bool useBlizzy = false;
        private bool _hud_hidden = false;

        public bool Enabled {
            get { return _enabled; }
            set {
                if (_enabled != value)
                {
                    _enabled = value;
                    _behaviour.Enabled = value;
                }
            }
        }

        private bool _linesEnabled = true;
        public bool LinesEnabled {
            get { return _linesEnabled; }
            set {
                if (_linesEnabled != value)
                {
                    _linesEnabled = value;
                    _behaviour.LinesEnabled = value;
                }
            }
        }

        private bool _markersEnabled = true;
        public bool MarkersEnabled {
            get { return _markersEnabled; }
            set {
                if (_markersEnabled != value)
                {
                    _markersEnabled = value;
                    _behaviour.MarkersEnabled = value;
                }
            }
        }
        /* // Broken 1.1.3 waypoint code
        private bool _waypointEnabled = true;
        public bool WaypointEnabled {
            get { return _waypointEnabled; }
            set {
                if (_waypointEnabled != value)
                {
                    _waypointEnabled = value;
                    _behaviour.WaypointEnabled = value;
                }
            }
        }
        */
        private bool _enableMap = false;
        public bool EnableMap {
            get { return _enableMap; }
            set {
                _enableMap = value;
                _behaviour.EnabledMap = value;
            }
        }

        private bool _enableText = true;
        public bool EnableText {
            get { return _enableText; }
            set {
                if (_enableText != value)
                {
                    _enableText = value;
                }
            }
        }

        private bool _lockText = true;
        public bool LockText {
            get { return _lockText; }
            set {
                if (_lockText != value)
                {
                    _lockText = value;
                }
            }
        }

        private bool _hideWithUI = true;
        public bool HideWithUI {
            get { return _hideWithUI; }
            set {
                if (_hideWithUI != value)
                {
                    _hideWithUI = value;
                }
            }
        }


        private MainBehaviour _behaviour;
        private Values _values = new Values();

        private KeyCode _toggleKey = KeyCode.Y;
        private bool _settingKeyBinding = false;

        //private bool _toolbarAvailable = false;
        //private IButton _button;
        //public ApplicationLauncherButton stockButton;
        ToolbarControl toolbarControl = null;

        private UnityEngine.GUI.WindowFunction onColorWindow;

        private UnityEngine.GUI.WindowFunction OnColorWindow {
            get { return onColorWindow; }
            set {
                if (onColorWindow == value && _colorWindowVisible)
                {
                    _colorWindowVisible = false;
                } else {
                    onColorWindow = value;
                    _colorWindowVisible = true;
                }
            }
        }

        private Rect _mainWindowPosition, _colorWindowPosition;
        private Vector2 _colorWindowScrollPos, _colorScrollPos, _sizeScrollPos;
        private bool _mainWindowVisible = false, _colorWindowVisible = false;
        private Rect _hudTextWindowPosition;

        public NavHud()
        {

        }

        void Start()
        {

            #region Check availability of toolbar
#if false
            if (ToolbarManager.ToolbarAvailable)
            {
                _button = ToolbarManager.Instance.add("NavHud", "NavHudButton");
                _button.TexturePath = "NavHud/ToolbarIcon";
                _button.ToolTip = "NavBall HUD";
                _button.OnClick += (e) => _mainWindowVisible = !_mainWindowVisible;
                _toolbarAvailable = true;
            }
            else{
                if (ApplicationLauncher.Ready)
                {
#endif
                    OnGUIAppLauncherReady();
#if false
        }
                else
                {
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                }

            }
#endif
            #endregion

            // People kept hitting time acceleration by accident, so moved middle-ish.
            _mainWindowPosition = new Rect(Screen.width / 3, Screen.height / 6, 10, 10);
            _colorWindowPosition = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
            _hudTextWindowPosition = new Rect(Screen.width / 2 - 20.0f, Screen.height * 0.7f, 10, 10);

            Load();

#region Setup hud camera
            GameObject hudCameraGameObject = new GameObject("HUDCamera");
            _hudCam = hudCameraGameObject.AddComponent<Camera>();
            _hudCam.transform.position = Vector3.zero;
            _hudCam.transform.rotation = Quaternion.identity;
            _hudCam.name = "HUDCamera";
            _hudCam.clearFlags = CameraClearFlags.Nothing;
            _hudCam.cullingMask = (1 << 7);
            _hudCam.depth = 1;
#endregion

#region Setup MainBehaviour
            _behaviour = _hudCam.gameObject.AddComponent<MainBehaviour>();
            _behaviour.HudCam = _hudCam;
            _behaviour.Values = _values;
            _behaviour.Enabled = _enabled;
            _behaviour.LinesEnabled = _linesEnabled;
            _behaviour.MarkersEnabled = _markersEnabled;
            //_behaviour.WaypointEnabled = _waypointEnabled; // Broken 1.1.3 waypoint code
            _behaviour.EnabledMap = _enableMap;
#endregion

            GameEvents.onShowUI.Add(showUI);
            GameEvents.onHideUI.Add(hideUI);
        }

        void showUI()
        {
            _behaviour.Enabled = _enabled;
            _hud_hidden = false;
        }

        void hideUI()
        {
            if (_hideWithUI)
            {
                _behaviour.Enabled = false;
                _hud_hidden = true;
            }
        }

        void Update()
        {
            // The toggle key toggles the HUD on and off, and holding down LeftAlt while
            // hitting the toggle key will cycle through the speed modes.
            if (Input.GetKeyDown(_toggleKey))
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    FlightGlobals.CycleSpeedModes(); // Changed in 1.1 from FlightUIController.fetch.cycleSpdModes();
                } else {
                    Enabled = !Enabled;
                }
            }
        }

        private void Save()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<NavHud>();
            config.SetValue("version", _version);
            config.SetValue("main window position", _mainWindowPosition);
            config.SetValue("color window position", _colorWindowPosition);
            config.SetValue("hud text position", _hudTextWindowPosition);
            config.SetValue("enable text", _enableText);
            config.SetValue("lock text", _lockText);
            config.SetValue("toggle key", _toggleKey);
            config.SetValue("enabled", _enabled);
            config.SetValue("useBlizzy", useBlizzy);
            config.SetValue("linesEnabled", _linesEnabled);
            config.SetValue("markersEnabled", _markersEnabled);
            //config.SetValue("waypointEnabled", _waypointEnabled); // Broken 1.1.3 waypoint code
            config.SetValue("enabledMap", _enableMap);
            config.SetValue("hideWithUI", _hideWithUI);
            _values.Save(config);
            config.save();
        }

        private void Load()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<NavHud>();
            config.load();
            if (config.GetValue<int>("version") < _version)
            {
                Save();
            } else {
                _mainWindowPosition = config.GetValue<Rect>("main window position");
                _colorWindowPosition = config.GetValue<Rect>("color window position");
                _hudTextWindowPosition = config.GetValue<Rect>("hud text position", new Rect(Screen.width / 2 - 20.0f, Screen.height * 0.7f, 10, 10));
                _enableText = config.GetValue("enable text", true);
                _lockText = config.GetValue("lock text", false);
                _toggleKey = config.GetValue<KeyCode>("toggle key");
                _enabled = config.GetValue<bool>("enabled", true);
                useBlizzy = config.GetValue<bool>("useBlizzy", true);
                _linesEnabled = config.GetValue<bool>("linesEnabled", true);
                _markersEnabled = config.GetValue<bool>("markersEnabled", true);
                // _waypointEnabled = config.GetValue<bool>("waypointEnabled", true);// Broken 1.1.3 waypoint code
                _enableMap = config.GetValue<bool>("enabledMap", false);
                _hideWithUI = config.GetValue<bool>("hideWithUI", true);
                _values.Load(config);
            }
        }

#region GUI stuff


        void OnGUIAppLauncherReady()
        {
#if false
            {
                this.stockButton = ApplicationLauncher.Instance.AddModApplication(
                    delegate() {
                        _mainWindowVisible = true;
                    },
                    delegate() {
                        _mainWindowVisible = false;
                    }
                    ,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                    (Texture)GameDatabase.Instance.GetTexture("NavHud/ToolbarIcon", false));
            }
#endif
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(ToggleWindow, ToggleWindow,
                 ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                "NavHud_NS",
                "navhudButton",
                "NavHud/ToolbarIcon",
                "NavHud/ToolbarIcon",
                "NavBall HUD"
            );
            toolbarControl.UseBlizzy(useBlizzy);
        }
        void ToggleWindow()
        {
            _mainWindowVisible = !_mainWindowVisible;
        }

        void OnGUI()
        {
            if (toolbarControl != null)
                toolbarControl.UseBlizzy(useBlizzy);

            bool showOverlay = !_hud_hidden && _enabled && _enableText && (!MapView.MapIsEnabled || _enableMap);
            bool showMain = _mainWindowVisible;
            bool showColour = _colorWindowVisible;
            if (!showMain && !showColour && !showOverlay) { return; } // No GUI is enabled

            /* // Old button
            if (!_toolbarAvailable)
            {
                GUILayout.BeginArea(new Rect(200f, 0f, 230f, 30f));
                _mainWindowVisible ^= GUILayout.Button("NH", GUILayout.Width(30f));
                GUILayout.EndArea();
            }
            */
            GUIStyle mainWindowStyle = new GUIStyle(HighLogic.Skin.window);
            mainWindowStyle.fixedWidth = 470f;
            if (showMain)
            {
                _mainWindowPosition = ClickThruBlocker.GUILayoutWindow(99241, _mainWindowPosition, OnMainWindow, "NavHud", mainWindowStyle);
            }
            GUIStyle colorWindowStyle = new GUIStyle(HighLogic.Skin.window);
            colorWindowStyle.fixedWidth = 300f;
            if (showColour)
            {
                _colorWindowPosition = ClickThruBlocker.GUILayoutWindow(99242, _colorWindowPosition, OnColorWindow, "Color Picker", colorWindowStyle);
            }

            if (showOverlay)
            {
				if (!_lockText) {
					_hudTextWindowPosition = ClickThruBlocker.GUILayoutWindow(99243, _hudTextWindowPosition, OnHudTextWindow, "", GUIStyle.none);
				} else {
					_hudTextWindowPosition = GUILayout.Window(99243, _hudTextWindowPosition, OnHudTextWindow, "", GUIStyle.none);
				}
            }

            GUIStyle hudTextSettingsWindowStyle = new GUIStyle(HighLogic.Skin.window);
            hudTextSettingsWindowStyle.fixedWidth = 300f;

            if (_settingKeyBinding)
            {
                if (Event.current.isKey)
                {
                    _toggleKey = Event.current.keyCode;
                    _settingKeyBinding = false;
                }
            }
        }

        private void OnMainWindow(int windowID)
        {
            GUILayout.BeginHorizontal(GUILayout.Width(450f));
            GUILayout.BeginVertical(GUILayout.Width(200f));

            GUILayout.BeginHorizontal();
            useBlizzy = GUILayout.Toggle(useBlizzy, "Use Blizzy toolbar if available", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Enabled = GUILayout.Toggle(Enabled, "Show HUD", GUILayout.ExpandWidth(true));
            _settingKeyBinding ^= GUILayout.Button("[" + (_settingKeyBinding ? "???" : _toggleKey.ToString()) + "]", GUILayout.Width(40f));
            GUILayout.EndHorizontal();
            MarkersEnabled = GUILayout.Toggle(MarkersEnabled, "Show markers");
            LinesEnabled = GUILayout.Toggle(LinesEnabled, "Show lines");
            //WaypointEnabled = GUILayout.Toggle(WaypointEnabled, "Show waypoint"); // Broken 1.1.3 waypoint code
            EnableMap = GUILayout.Toggle(EnableMap, "Show in map");
            EnableText = GUILayout.Toggle(EnableText, "Show HUD text");
            HideWithUI = GUILayout.Toggle(HideWithUI, "Hide with UI");
            LockText = GUILayout.Toggle(LockText, "Lock HUD text");

            if (GUILayout.Button("Reset"))
            {
                _values = new Values();
                _behaviour.Values = _values;
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(250f));

            _sizeScrollPos = GUILayout.BeginScrollView(_sizeScrollPos, false, false, GUILayout.Height(220f));

            GUIStyle labelStyle = new GUIStyle(HighLogic.Skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("Vector size", labelStyle);
            _values.VectorSize = GUILayout.HorizontalSlider(_values.VectorSize, 0.001f, 0.1f);
            GUILayout.Label("Line width", labelStyle);
            _values.LineWidth = GUILayout.HorizontalSlider(_values.LineWidth, 0.0001f, 0.01f);


            GUILayout.Label("# of Altitude lines", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(40f)) & _values.NumberZenithLinesHalf > 0)
            {
                _values.NumberZenithLinesHalf--;
            }
            _values.NumberZenithLinesHalf = (int.Parse(GUILayout.TextField((_values.NumberZenithLinesHalf * 2 + 1).ToString())) - 1) / 2;
            if (GUILayout.Button("+", GUILayout.Width(40f)))
            {
                _values.NumberZenithLinesHalf++;
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("# of Azimuth lines", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(40f)) & _values.NumberAzimuthLinesQuarter > 1)
            {
                _values.NumberAzimuthLinesQuarter--;
            }
            _values.NumberAzimuthLinesQuarter = int.Parse(GUILayout.TextField((_values.NumberAzimuthLinesQuarter * 4).ToString())) / 4;
            if (GUILayout.Button("+", GUILayout.Width(40f)))
            {
                _values.NumberAzimuthLinesQuarter++;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            /*
            if(GUILayout.Button("Cameras?")){
                for(int i=0; i<Camera.allCameras.Length; i++){
                    Debug.Log(i+": "+Camera.allCameras[i].ToString()+"  "+Camera.allCameras[i].depth+"  "+Camera.allCameras[i].cullingMask);
                }
            }
            */

            GUILayout.Label("Colors", labelStyle, GUILayout.ExpandWidth(true));
            _colorScrollPos = GUILayout.BeginScrollView(_colorScrollPos, false, false, GUILayout.Height(250f));
            if (ColorButton(_values.HudTextColor, "HUD Text Color")) OnColorWindow = OnColorWindowHudText;
            GUILayout.Label("Markers", labelStyle, GUILayout.ExpandWidth(true));
            if (ColorButton(_values.HeadingColor, "Heading")) OnColorWindow = OnColorWindowHeading;
            if (ColorButton(_values.ProgradeColor, "Velocity")) OnColorWindow = OnColorWindowPrograde;
            if (ColorButton(_values.NormalColor, "Normal")) OnColorWindow = OnColorWindowNormal;
            if (ColorButton(_values.RadialColor, "Radial")) OnColorWindow = OnColorWindowRadial;
            if (ColorButton(_values.TargetColor, "Target")) OnColorWindow = OnColorWindowTarget;
            if (ColorButton(_values.AlignmentColor, "Alignment")) OnColorWindow = OnColorWindowAlignment;
            if (ColorButton(_values.ManeuverColor, "Maneuver")) OnColorWindow = OnColorWindowManeuver;
            GUILayout.Label("Lines", labelStyle, GUILayout.ExpandWidth(true));
            if (ColorButton(_values.HorizonColor, "Horizon")) OnColorWindow = OnColorWindowHorizon;
            if (ColorButton(_values.NorthColor, "North")) OnColorWindow = OnColorWindowNorth;
            if (ColorButton(_values.EastColor, "East")) OnColorWindow = OnColorWindowEast;
            if (ColorButton(_values.SouthColor, "South")) OnColorWindow = OnColorWindowSouth;
            if (ColorButton(_values.WestColor, "West")) OnColorWindow = OnColorWindowWest;
            if (ColorButton(_values.UpperHalfColor, "Upper half")) OnColorWindow = OnColorWindowUpperHalf;
            if (ColorButton(_values.LowerHalfColor, "Lower half")) OnColorWindow = OnColorWindowLowerHalf;
            if (ColorButton(_values.AzimuthColor, "Vertical")) OnColorWindow = OnColorWindowAzimuth;
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (_values.IChanged)
            {
                _behaviour.Values = _values;
                Debug.Log("NavHUD settings changed.");
            }
            GUI.DragWindow();
        }

        private void OnHudTextWindow(int windowID)
        {
            Vessel vessel = FlightGlobals.fetch.activeVessel;
            double vel = 0.0d;
            string speedLabel;

            GUIStyle hudTextStyle = new GUIStyle();
            hudTextStyle.normal.textColor = _values.HudTextColor;

            GUILayout.BeginVertical();

            bool isRCSOn = vessel.ActionGroups[KSPActionGroup.RCS];
            bool isSASOn = vessel.ActionGroups[KSPActionGroup.SAS];

            GUILayout.BeginHorizontal();
            if (isSASOn)
            {
                GUILayout.Label("SAS", hudTextStyle);
            }
            if (isRCSOn)
            {
                GUILayout.Label("RCS", hudTextStyle);
            }
            GUILayout.EndHorizontal();

            switch (FlightGlobals.speedDisplayMode) // Changed in 1.1 from FlightUIController.speedDisplayMode
            {
            case FlightGlobals.SpeedDisplayModes.Surface:
                vel = FlightGlobals.ship_srfSpeed;
                speedLabel = "Surface: " + vel.ToString("F2") + "m/s";
                break;

            case FlightGlobals.SpeedDisplayModes.Orbit:
                vel = FlightGlobals.ship_obtSpeed;
                speedLabel = "Orbit: " + vel.ToString("F2") + "m/s";
                break;

            case FlightGlobals.SpeedDisplayModes.Target:
                vel = FlightGlobals.ship_tgtSpeed;
                speedLabel = "Target: " + vel.ToString("F2") + "m/s";
                break;

            default:
                throw new ArgumentOutOfRangeException();
            }

            GUILayout.Label(speedLabel, hudTextStyle);
            GUILayout.Label("Throttle: " + vessel.ctrlState.mainThrottle.ToString("P1"), hudTextStyle);

            GUILayout.Label("Heading: " + FlightGlobals.ship_heading.ToString("F1"), hudTextStyle);
            GUILayout.Label("G Force: " + FlightGlobals.ship_geeForce.ToString("F1"), hudTextStyle);

            if (vessel != null && vessel.patchedConicSolver != null &&
                vessel.patchedConicSolver.maneuverNodes != null &&
                vessel.patchedConicSolver.maneuverNodes.Count > 0)
            {
                ManeuverNode node = vessel.patchedConicSolver.maneuverNodes[0];
                double burnDV = node.DeltaV.magnitude;
                double burnRem = node.GetBurnVector(vessel.orbit).magnitude;
                GUILayout.Label("Burn ΔV: " + burnRem.ToString("F2") + "m/s / " + burnDV.ToString("F2") + "m/s", hudTextStyle);

                if (burnRem != double.NaN)
                {
                    double totalThrust = 0.0;
                    double totalIsp = 0.0;
                    calcThrust(ref totalThrust, ref totalIsp);
                    if (vessel.ctrlState.mainThrottle > 0.0)
                    {
                        totalThrust *= vessel.ctrlState.mainThrottle;
                    }

                    double burnTimeRem = calcBurnTime(burnRem, vessel.GetTotalMass(), totalThrust, totalIsp);
                    double burnTimeTotal = calcBurnTime(burnDV, vessel.GetTotalMass(), totalThrust, totalIsp);
                    GUILayout.Label("Burn time: " + GetTimeString(burnTimeRem) + " / " + GetTimeString(burnTimeTotal),
                        hudTextStyle);
                }

                double timeToNode = node.UT - Planetarium.GetUniversalTime();
                if (Math.Sign(timeToNode) >= 0)
                {
                    GUILayout.Label("Node in T - " + GetTimeString(Math.Abs(timeToNode)), hudTextStyle);
                } else {
                    GUILayout.Label("Node in T + " + GetTimeString(Math.Abs(timeToNode)), hudTextStyle);
                }
            }

            GUILayout.EndVertical();
            if (!_lockText)
            {
                GUI.DragWindow();
            }
        }

        double calcBurnTime(double deltaV, double initialMass, double thrust, double isp)
        {
            var exhaustVelocity = isp * 9.82;

            // t = (m0 * ve / T) (1 - e^(-dv/ve))
            return (initialMass * exhaustVelocity / thrust) * (1 - Math.Exp(-deltaV / exhaustVelocity));
        }

        void calcThrust(ref double totalThrust, ref double totalIsp)
        {
            totalThrust = 0.0;
            totalIsp = 0.0;
            double thrustByIsp = 0.0;
            Vessel vessel = FlightGlobals.fetch.activeVessel;

            foreach (Part part in vessel.parts)
            {
                if ((part != null) && (part.Modules != null))
                {
                    if (part.Modules.Contains("ModuleEngines"))
                    {
                        foreach (PartModule module in part.Modules)
                        {
                            if (module != null && module is ModuleEngines && module.isActiveAndEnabled)
                            {
                                ModuleEngines engine = (ModuleEngines)module;

                                if (engine.isOperational && !engine.getFlameoutState)
                                {
                                    double isp = engine.atmosphereCurve.Evaluate((float)(vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres));
                                    //double thrust = isp * engine.maxFuelFlow * engine.g;
                                    double thrust = engine.maxThrust;
                                    totalThrust += thrust;
                                    thrustByIsp += thrust / isp;

                                }
                            }
                        }
                    }
                    else if (part.Modules.Contains("ModuleEnginesFX"))
                    {
                        foreach (PartModule module in part.Modules)
                        {
                            if (module != null && module is ModuleEnginesFX && module.isActiveAndEnabled)
                            {
                                ModuleEnginesFX engine = (ModuleEnginesFX)module;

                                if (engine.isOperational && !engine.getFlameoutState)
                                {
                                    double isp = engine.atmosphereCurve.Evaluate((float)(vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres));
                                    //double thrust = isp * engine.maxFuelFlow * engine.g;
                                    double thrust = engine.maxThrust;
                                    totalThrust += thrust;
                                    thrustByIsp += thrust / isp;

                                }
                            }
                        }
                    }
                }
            }

            totalIsp = totalThrust / thrustByIsp;
        }

        public static int HOURS_PER_DAY
        {
            get { return GameSettings.KERBIN_TIME ? 6 : 24; }
        }
        public static int DAYS_PER_YEAR
        {
            get { return GameSettings.KERBIN_TIME ? 426 : 365; }
        }
        private string GetTimeString(double seconds)
        {
            double s = seconds;
            string timestr = "";
            int[] factors = new int[] {60*60*HOURS_PER_DAY*DAYS_PER_YEAR,
                60*60*HOURS_PER_DAY, 60*60, 60, 1};
            string[] postfixes = new string[] { "y ", "d ", "h:", "m:", "s" };

            bool sawNonZero = false;
            for (int i = 0; i < postfixes.Length; ++i)
            {
                long val = 0;
                if (s >= (double)factors[i])
                {
                    val = (int)(s / (double)factors[i]);
                    s -= (double)(val * factors[i]);
                }

                if (!sawNonZero)
                {
                    if (val > 0)
                    {
                        sawNonZero = true;
                    } else {
                        continue;
                    }
                    timestr += val.ToString();
                } else {
                    timestr += val.ToString("00");
                }
                timestr += postfixes[i];
            }

            if (timestr.Length == 0)
                timestr = "0s";
            return timestr;
        }

        private void OnColorWindowHeading(int windowID)
        {
            _values.HeadingColor = ColorChanger(_values.HeadingColor, "Heading");
        }

        private void OnColorWindowPrograde(int windowID)
        {
            _values.ProgradeColor = ColorChanger(_values.ProgradeColor, "Velocity");
        }

        private void OnColorWindowNormal(int windowID)
        {
            _values.NormalColor = ColorChanger(_values.NormalColor, "Normal");
        }

        private void OnColorWindowRadial(int windowID)
        {
            _values.RadialColor = ColorChanger(_values.RadialColor, "Radial");
        }

        private void OnColorWindowTarget(int windowID)
        {
            _values.TargetColor = ColorChanger(_values.TargetColor, "Target");
        }

        private void OnColorWindowAlignment(int windowID)
        {
            _values.AlignmentColor = ColorChanger(_values.AlignmentColor, "Alignment");
        }

        private void OnColorWindowManeuver(int windowID)
        {
            _values.ManeuverColor = ColorChanger(_values.ManeuverColor, "Maneuver");
        }

        private void OnColorWindowHorizon(int windowID)
        {
            _values.HorizonColor = ColorChanger(_values.HorizonColor, "Horizon");
        }

        private void OnColorWindowNorth(int windowID)
        {
            _values.NorthColor = ColorChanger(_values.NorthColor, "North");
        }

        private void OnColorWindowEast(int windowID)
        {
            _values.EastColor = ColorChanger(_values.EastColor, "East");
        }

        private void OnColorWindowSouth(int windowID)
        {
            _values.SouthColor = ColorChanger(_values.SouthColor, "South");
        }

        private void OnColorWindowWest(int windowID)
        {
            _values.WestColor = ColorChanger(_values.WestColor, "West");
        }

        private void OnColorWindowUpperHalf(int windowID)
        {
            _values.UpperHalfColor = ColorChanger(_values.UpperHalfColor, "Upper half");
        }

        private void OnColorWindowLowerHalf(int windowID)
        {
            _values.LowerHalfColor = ColorChanger(_values.LowerHalfColor, "Lower half");
        }

        private void OnColorWindowAzimuth(int windowID)
        {
            _values.AzimuthColor = ColorChanger(_values.AzimuthColor, "Vertical");
        }

        private void OnColorWindowHudText(int windowID)
        {
            _values.HudTextColor = ColorChanger(_values.HudTextColor, "HUD Text Color");
        }

        private bool ColorButton(Color color, string text)
        {
            GUIStyle buttonStyle = new GUIStyle(HighLogic.Skin.button);
            buttonStyle.normal.textColor = color;
            return GUILayout.Button(text, buttonStyle, GUILayout.ExpandWidth(true));
        }

        private Color ColorChanger(Color color, string text)
        {
            GUIStyle labelStyle = new GUIStyle(HighLogic.Skin.label);

            _colorWindowScrollPos = GUILayout.BeginScrollView(_colorWindowScrollPos, false, false, GUILayout.Height(150f));

            labelStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(text, labelStyle, GUILayout.ExpandWidth(true));

            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.fontStyle = FontStyle.Bold;

            GUILayout.BeginHorizontal();
            labelStyle.normal.textColor = Color.red;
            GUILayout.Label("r", labelStyle, GUILayout.Width(10));
            color.r = GUILayout.HorizontalSlider(color.r, 0f, 1f);
            color.r = float.Parse(GUILayout.TextField((color.r).ToString("0.000"), GUILayout.Width(45)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            labelStyle.normal.textColor = Color.green;
            GUILayout.Label("g", labelStyle, GUILayout.Width(10));
            color.g = GUILayout.HorizontalSlider(color.g, 0f, 1f);
            color.g = float.Parse(GUILayout.TextField((color.g).ToString("0.000"), GUILayout.Width(45)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            labelStyle.normal.textColor = Color.blue;
            GUILayout.Label("b", labelStyle, GUILayout.Width(10));
            color.b = GUILayout.HorizontalSlider(color.b, 0f, 1f);
            color.b = float.Parse(GUILayout.TextField((color.b).ToString("0.000"), GUILayout.Width(45)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            labelStyle.normal.textColor = Color.white;
            GUILayout.Label("a", labelStyle, GUILayout.Width(10));
            color.a = GUILayout.HorizontalSlider(color.a, 0f, 1f);
            color.a = float.Parse(GUILayout.TextField((color.a).ToString("0.000"), GUILayout.Width(45)));
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _colorWindowVisible = !GUILayout.Button("Close");
            GUILayout.EndHorizontal();
            GUI.DragWindow();
            return color;
        }

#endregion

        void OnDestroy()
        {
            Save();

            GameEvents.onShowUI.Remove(showUI);
            GameEvents.onHideUI.Remove(hideUI);

            if (_behaviour != null)
            {
                Destroy(_behaviour);
            }
#if false
        if (!_toolbarAvailable)
            {
                GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                ApplicationLauncher.Instance.RemoveModApplication(stockButton);
            }
            if (_button != null)
            {
                _button.Destroy();
            }
#endif
        toolbarControl.OnDestroy();
        Destroy(toolbarControl);

    }
    }
}

