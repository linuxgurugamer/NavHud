﻿using System;
using KSP;
using UnityEngine;
using System.Collections;

namespace NavHud
{
    public class EdgeMarkers
    {
        private LineRenderer[] _lines;
        private GameObject[] _objects;
        
        private const int Prograde      =  0;
        private const int Retrograde    =  1;
        private const int Normal        =  2;
        private const int Antinormal    =  3;
        private const int Radial        =  4;
        private const int Antiradial    =  5;
        private const int Target        =  6;
        private const int Antitarget    =  7;
        private const int Maneuver      =  8;
        private const int Heading       =  9;
        private const int Antiheading   = 10;
        private const int Alignment     = 11;
        private const int Antialignment = 12;
        private const int Waypoint      = 13;
        
        private float _r;

        public EdgeMarkers()
        {
            _lines = new LineRenderer[14];
            _objects = new GameObject[14];

            for (int i = 0; i < _lines.Length; i++)
            {
                _objects[i] = new GameObject();
                _objects[i].layer = 7;
                // Add line
                _lines[i] = _objects[i].AddComponent< LineRenderer >() as LineRenderer;
                _lines[i].GetComponent<Renderer>().material = new Material(Shader.Find("Particles/Additive"));
                _lines[i].positionCount = 2;
            }
        }

        public void SetValues(Values values)
        {
            _r = (float)values.Distance;
            Util.SetColors(ref _lines[Prograde     ], values.ProgradeColor, values.ProgradeColor);
            Util.SetColors(ref _lines[Retrograde   ], values.ProgradeColor, values.ProgradeColor);
            Util.SetColors(ref _lines[Normal       ], values.NormalColor, values.NormalColor);
            Util.SetColors(ref _lines[Antinormal   ], values.NormalColor, values.NormalColor);
            Util.SetColors(ref _lines[Radial       ], values.RadialColor, values.RadialColor);
            Util.SetColors(ref _lines[Antiradial   ], values.RadialColor, values.RadialColor);
            Util.SetColors(ref _lines[Target       ], values.TargetColor, values.TargetColor);
            Util.SetColors(ref _lines[Antitarget   ], values.TargetColor, values.TargetColor);
            Util.SetColors(ref _lines[Maneuver     ], values.ManeuverColor, values.ManeuverColor);
            Util.SetColors(ref _lines[Heading      ], values.HeadingColor, values.HeadingColor);
            Util.SetColors(ref _lines[Antiheading  ], values.HeadingColor, values.HeadingColor);
            Util.SetColors(ref _lines[Alignment    ], values.AlignmentColor, values.AlignmentColor);
            Util.SetColors(ref _lines[Antialignment], values.AlignmentColor, values.AlignmentColor);
            for (int i = 0; i < _lines.Length; i++)
            {
                Util.SetWidth(ref _lines[i], 0f, 0.01f*_r);
            }
        }
        
        public void SetDirections(Vector3d prograde, Vector3d normal, Vector3d radial, Vector3 screenedge)
        {
            SetPosition(Prograde, prograde, screenedge);
            SetPosition(Retrograde, -prograde, screenedge);
            SetPosition(Normal, normal, screenedge);
            SetPosition(Antinormal, -normal, screenedge);
            SetPosition(Radial, radial, screenedge);
            SetPosition(Antiradial, -radial, screenedge);
        }

        public void SetTarget(Vector3d target, Vector3 screenedge)
        {
            SetPosition(Target, target, screenedge);
            SetPosition(Antitarget, -target, screenedge);
        }

        public void SetManeuver(Vector3d maneuver, Vector3 screenedge)
        {
            SetPosition(Maneuver, maneuver, screenedge);
        }

        public void SetHeading(Vector3d heading, Vector3 screenedge)
        {
            SetPosition(Heading, heading, screenedge);
            SetPosition(Antiheading, -heading, screenedge);
        }

        public void SetAlignment(Vector3d alignment, Vector3 screenedge)
        {
            SetPosition(Alignment, alignment, screenedge);
            SetPosition(Antialignment, -alignment, screenedge);
        }

        public void SetWaypoint(Vector3d waypoint, Vector3 screenedge)
        {
            SetPosition(Waypoint, waypoint, screenedge);
        }

        private void SetPosition(int key, Vector3 position, Vector3 screenedge)
        {
            Vector3 pointer = -Vector3.forward;
            if(
               position.z<0 ||
               Math.Abs(position.x*screenedge.z)>Math.Abs(screenedge.x*position.z) ||
               Math.Abs(position.y*screenedge.z)>Math.Abs(screenedge.y*position.z)
              )
            {
                if (Math.Abs(position.x*screenedge.y) > Math.Abs(position.y*screenedge.x))
                {
                    pointer.x = Math.Sign(position.x)*Math.Abs(screenedge.x);
                    pointer.y = position.y*Math.Abs(screenedge.x/position.x);
                } else {
                    pointer.y = Math.Sign(position.y)*Math.Abs(screenedge.y);
                    pointer.x = position.x*Math.Abs(screenedge.y/position.y);
                }
                pointer.z = screenedge.z;
            }
            Vector3 pointerxy = new Vector3(pointer.x, pointer.y, 0f).normalized;
            _lines[key].SetPosition(0,pointer*_r);
            _lines[key].SetPosition(1,(pointer-pointerxy*0.01f)*_r);
        }

        public void SetParent(Transform parent)
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                // Parent the object to the camera -- this is probably not nessesary.
                //objects[j].transform.parent = cam.transform;
                _lines[i].transform.parent = parent;
                _lines[i].useWorldSpace = false;
                _lines[i].transform.localPosition = Vector3.zero;
                _lines[i].transform.localEulerAngles = Vector3.zero;
            }
        }

        private Vector3 ScreenEdgePointer(Vector3 position, Vector3 screenedge)
        {
            Vector3 pointer = Vector3.zero;
            if (Math.Abs(position.x*screenedge.y) > Math.Abs(position.y*screenedge.x))
            {
                pointer.x = Math.Sign(position.x)*Math.Abs(screenedge.x);
                pointer.y = position.y*Math.Abs(screenedge.x/position.x);
            } else {
                pointer.y = Math.Sign(position.y)*Math.Abs(screenedge.y);
                pointer.x = position.x*Math.Abs(screenedge.y/position.y);
            }
            pointer.z = screenedge.z;
            return pointer;
        }

        public void SetDirectionsActive(bool active)
        {
            for (int i = 0; i < 6; i++)
            {
                _objects[i].SetActive(active);
            }
        }

        public void SetTargetActive(bool active)
        {
            _objects[Target    ].SetActive(active);
            _objects[Antitarget].SetActive(active);
        }

        public void SetManeuverActive(bool active)
        {
            _objects[Maneuver].SetActive(active);
        }

        public void SetHeadingActive(bool active)
        {
            _objects[Heading    ].SetActive(active);
            _objects[Antiheading].SetActive(active);
        }

        public void SetAlignmentActive(bool active)
        {
            _objects[Alignment    ].SetActive(active);
            _objects[Antialignment].SetActive(active);
        }

        public void SetWaypointActive(bool active)
        {
            _objects[Waypoint    ].SetActive(active);
        }

        public void LoadWaypointColor()
        {
            if(NavWaypoint.fetch != null && NavWaypoint.fetch.Visual != null)
            {
                GameObject navWaypointIndicator = NavWaypoint.fetch.Visual;
                Material material = navWaypointIndicator.GetComponent<Renderer>().material;
				Color color = material.GetColor("_TintColor");
                _lines[Waypoint].startColor = color;
                _lines[Waypoint].endColor = color;
            } else {
                Debug.LogWarning("Tried to load texture while navWaypoint is not instantiated.");
            }
        }
    }
}
