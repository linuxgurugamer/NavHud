using System;
using KSP;
using UnityEngine;
using System.Collections;


namespace NavHud
{
    class Util
    {
        internal static void SetColors(ref LineRenderer lr, Color startColor, Color endColor)
        {
            lr.startColor = startColor;
            lr.endColor = endColor;
        }
        internal static void SetWidth(ref LineRenderer lr, float start, float end)
        {
            lr.startWidth = start;
            lr.endWidth = end;
        }
    }
}
