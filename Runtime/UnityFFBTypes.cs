﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityFFB
{
    public enum EffectsType
    {
        ConstantForce = 0,
        RampForce = 1,
        Square = 2,
        Sine = 3,
        Triangle = 4,
        SawtoothUp = 5,
        SawtoothDown = 6,
        Spring = 7,
        Damper = 8,
        Inertia = 9,
        Friction = 10,
        CustomForce = 11
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceInfo
    {
        public uint deviceType;
        [MarshalAs(UnmanagedType.LPStr)]
        public string guidInstance;
        [MarshalAs(UnmanagedType.LPStr)]
        public string guidProduct;
        [MarshalAs(UnmanagedType.LPStr)]
        public string instanceName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string productName;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceAxisInfo
    {
        public uint offset;
        public uint type;
        public uint flags;
        public uint ffMaxForce;
        public uint ffForceResolution;
        public uint collectionNumber;
        public uint designatorIndex;
        public uint usagePage;
        public uint usage;
        public uint dimension;
        public uint exponent;
        public uint reportId;
        [MarshalAs(UnmanagedType.LPStr)]
        public string guidType;
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
    };

    /// <summary>
    /// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416601(v=vs.85)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DICondition
    {
        /// <summary>
        /// Offset for the condition, in the range from - 10,000 through 10,000.
        /// </summary>
        public int offset;
        /// <summary>
        /// Coefficient constant on the positive side of the offset, in the range 
        /// from - 10,000 through 10,000.
        /// </summary>
        public int positiveCoefficient;
        /// <summary>
        /// Coefficient constant on the negative side of the offset, in the range 
        /// from - 10,000 through 10,000. If the device does not support separate
        /// positive and negative coefficients, the value of lNegativeCoefficient 
        /// is ignored, and the value of lPositiveCoefficient is used as both the 
        /// positive and negative coefficients.
        /// </summary>
        public int negativeCoefficient;
        /// <summary>
        /// Maximum force output on the positive side of the offset, in the range
        /// from 0 through 10,000.
        /// 
        /// If the device does not support force saturation, the value of this
        /// member is ignored.
        /// </summary>
        public uint positiveSaturation;
        /// <summary>
        /// Maximum force output on the negative side of the offset, in the range
        /// from 0 through 10,000.
        ///
        /// If the device does not support force saturation, the value of this member
        /// is ignored.
        /// 
        /// If the device does not support separate positive and negative saturation,
        /// the value of dwNegativeSaturation is ignored, and the value of dwPositiveSaturation
        /// is used as both the positive and negative saturation.
        /// </summary>
        public uint negativeSaturation;
        /// <summary>
        /// Region around lOffset in which the condition is not active, in the range
        /// from 0 through 10,000. In other words, the condition is not active between
        /// lOffset minus lDeadBand and lOffset plus lDeadBand.
        /// </summary>
        public int deadband;
    }

    /// <summary>
    /// Describes the state of a joystick device with extended capabilities.
    /// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416628(v=vs.85)
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct DIJOYSTATE2
    {
        public int lX;
        public int lY;
        public int lZ;
        public int lRx;
        public int lRy;
        public int lRz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] rglSlider;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] rgdwPOV;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] rgbButtons;
        public int lVX;
        public int lVY;
        public int lVZ;
        public int lVRx;
        public int lVRy;
        public int lVRz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	    public int[] rglVSlider;
        public int lAX;
        public int lAY;
        public int lAZ;
        public int lARx;
        public int lARy;
        public int lARz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	    public int[] rglASlider;
        public int lFX;
        public int lFY;
        public int lFZ;
        public int lFRx;
        public int lFRy;
        public int lFRz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	    public int[] rglFSlider;
    }
}