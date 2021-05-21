using System;
using System.Runtime.InteropServices;

namespace DirectInputFFB {
    public class Native {
        
        #if UNITY_STANDALONE_WIN
        private const string FFBDLL = "UnityDirectInputFFB";

        [DllImport(FFBDLL)]
        public static extern int StartDirectInput();

        [DllImport(FFBDLL)]
        public static extern IntPtr EnumerateFFBDevices(ref int deviceCount);

        [DllImport(FFBDLL)]
        public static extern IntPtr EnumerateFFBAxes(ref int axisCount);

        [DllImport(FFBDLL)]
        public static extern int CreateFFBDevice(string guidInstance);

        [DllImport(FFBDLL)]
        public static extern int AddFFBEffect(EffectsType effectType);

        [DllImport(FFBDLL)]
        public static extern int UpdateConstantForce(int magnitude, int[] directions);

        [DllImport(FFBDLL)]
        public static extern int UpdateSpring(DICondition[] conditions);

        [DllImport(FFBDLL)]
        public static extern int UpdateEffectGain(EffectsType effectType, float gainPercent);

        [DllImport(FFBDLL)]
        public static extern int SetAutoCenter(bool autoCenter);

        [DllImport(FFBDLL)]
        public static extern void StartAllFFBEffects();

        [DllImport(FFBDLL)]
        public static extern void StopAllFFBEffects();

        [DllImport(FFBDLL)]
        public static extern void StopDirectInput();

        [DllImport(FFBDLL)]
        public static extern int PollDevice();
        
        [DllImport(FFBDLL)]
        public static extern int GetDeviceState(ref DIJOYSTATE2 DeviceStateObj);
    
        #endif

    }
}
