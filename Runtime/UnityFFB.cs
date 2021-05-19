using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace UnityFFB
{
    public class UnityFFB : MonoBehaviour
    {
        public static UnityFFB instance;

        /// <summary>
        /// Whether or not to enable Force Feedback when the behavior starts.
        /// </summary>
        public bool enableOnAwake = true;
        /// <summary>
        /// Whether or not to automatically select the first FFB device on start.
        /// </summary>
        public bool autoSelectFirstDevice = true;
        /// <summary>
        /// Whether or not to automatically disable auto-centering on the device.
        /// </summary>
        public bool disableAutoCenter = true;
        /// <summary>
        /// Whether or not to automatically add a constant force effect to the device.
        /// </summary>
        public bool addConstantForce = true;
        /// <summary>
        /// Whether or not to automatically add a spring force to the device.
        /// </summary>
        public bool addSpringForce = false;

        // Constant force properties
        public int force = 0;
        public float sensitivity = 1.0f;
        public int[] axisDirections = new int[0];

        public bool ffbEnabled { get; private set; }
        public bool constantForceEnabled { get; private set; }
        public bool springForceEnabled { get; private set; }

        public DeviceInfo[] devices = new DeviceInfo[0];

        public DeviceInfo? activeDevice = null;

        public DeviceAxisInfo[] axes = new DeviceAxisInfo[0];
        public DICondition[] springConditions = new DICondition[0];

        public DIJOYSTATE2 DeviceState;

        public System.Collections.Generic.List<string> test;

#if UNITY_STANDALONE_WIN
        void Awake(){
            instance = this;

            if (enableOnAwake)
            {
                EnableForceFeedback();
            }
        }

        private void FixedUpdate()
        {
            if (constantForceEnabled)
            {
                UnityFFBNative.UpdateConstantForce((int)(force * sensitivity), axisDirections);
            }
            // DebugFunc1();
        }
#endif

        public void EnableForceFeedback(){
#if UNITY_STANDALONE_WIN
            if (ffbEnabled)
            {
                return;
            }

            if (UnityFFBNative.StartDirectInput() >= 0)
            {
                ffbEnabled = true;
            }
            else
            {
                ffbEnabled = false;
            }

            int deviceCount = 0;

            IntPtr ptrDevices = UnityFFBNative.EnumerateFFBDevices(ref deviceCount);

            Debug.Log($"[UnityFFB] Device count: {devices.Length}");
            if (deviceCount > 0)
            {
                devices = new DeviceInfo[deviceCount];

                int deviceSize = Marshal.SizeOf(typeof(DeviceInfo));
                for (int i = 0; i < deviceCount; i++)
                {
                    IntPtr pCurrent = ptrDevices + i * deviceSize;
                    devices[i] = Marshal.PtrToStructure<DeviceInfo>(pCurrent);
                }

                foreach (DeviceInfo device in devices)
                {
                    string ffbAxis = UnityEngine.JsonUtility.ToJson(device, true);
                    Debug.Log(ffbAxis);
                }

                if (autoSelectFirstDevice)
                {
                    SelectDevice(devices[0].guidInstance);
                }
            }
#endif
        }

        public void DisableForceFeedback(){
#if UNITY_STANDALONE_WIN
            UnityFFBNative.StopDirectInput();
            ffbEnabled = false;
            constantForceEnabled = false;
            devices = new DeviceInfo[0];
            activeDevice = null;
            axes = new DeviceAxisInfo[0];
            springConditions = new DICondition[0];
#endif
        }

        public void SelectDevice(string deviceGuid){
#if UNITY_STANDALONE_WIN
            // For now just initialize the first FFB Device.
            int hresult = UnityFFBNative.CreateFFBDevice(deviceGuid);
            if (hresult == 0)
            {
                activeDevice = devices[0];

                if (disableAutoCenter)
                {
                    hresult = UnityFFBNative.SetAutoCenter(false);
                    if (hresult != 0)
                    {
                        Debug.LogError($"[UnityFFB] SetAutoCenter Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                    }
                }

                int axisCount = 0;
                IntPtr ptrAxes = UnityFFBNative.EnumerateFFBAxes(ref axisCount);
                if (axisCount > 0)
                {
                    axes = new DeviceAxisInfo[axisCount];
                    axisDirections = new int[axisCount];
                    springConditions = new DICondition[axisCount];

                    int axisSize = Marshal.SizeOf(typeof(DeviceAxisInfo));
                    for (int i = 0; i < axisCount; i++)
                    {
                        IntPtr pCurrent = ptrAxes + i * axisSize;
                        axes[i] = Marshal.PtrToStructure<DeviceAxisInfo>(pCurrent);
                        axisDirections[i] = 0;
                        springConditions[i] = new DICondition();
                    }

                    if (addConstantForce)
                    {
                        hresult = UnityFFBNative.AddFFBEffect(EffectsType.ConstantForce);
                        if (hresult == 0)
                        {
                            hresult = UnityFFBNative.UpdateConstantForce(0, axisDirections);
                            if (hresult != 0)
                            {
                                Debug.LogError($"[UnityFFB] UpdateConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                            }
                            constantForceEnabled = true;
                        }
                        else
                        {
                            Debug.LogError($"[UnityFFB] AddConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                        }
                    }

                    if (addSpringForce)
                    {
                        hresult = UnityFFBNative.AddFFBEffect(EffectsType.Spring);
                        if (hresult == 0)
                        {
                            for (int i = 0; i < springConditions.Length; i++)
                            {
                                springConditions[i].deadband = 0;
                                springConditions[i].offset = 0;
                                springConditions[i].negativeCoefficient = 2000;
                                springConditions[i].positiveCoefficient = 2000;
                                springConditions[i].negativeSaturation = 10000;
                                springConditions[i].positiveSaturation = 10000;
                            }
                            hresult = UnityFFBNative.UpdateSpring(springConditions);
                            Debug.LogError($"[UnityFFB] UpdateSpringForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                        }
                        else
                        {
                            Debug.LogError($"[UnityFFB] AddSpringForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                        }
                    }
                }
                Debug.Log($"[UnityFFB] Axis count: {axes.Length}");
                foreach (DeviceAxisInfo axis in axes)
                {
                    string ffbAxis = UnityEngine.JsonUtility.ToJson(axis, true);
                    Debug.Log(ffbAxis);
                }
            }
            else
            {
                activeDevice = null;
                Debug.LogError($"[UnityFFB] 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }
#endif
        }

        public void SetConstantForceGain(float gainPercent){
#if UNITY_STANDALONE_WIN
            if (constantForceEnabled)
            {
                int hresult = UnityFFBNative.UpdateEffectGain(EffectsType.ConstantForce, gainPercent);
                Debug.LogError($"[UnityFFB] UpdateEffectGain Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }
#endif
        }

        public void StartFFBEffects(){
#if UNITY_STANDALONE_WIN
            UnityFFBNative.StartAllFFBEffects();
            constantForceEnabled = true;
#endif
        }

        public void StopFFBEffects(){
#if UNITY_STANDALONE_WIN
            UnityFFBNative.StopAllFFBEffects();
            constantForceEnabled = false;
#endif
        }

#if UNITY_STANDALONE_WIN
        public void OnApplicationQuit(){
            DisableForceFeedback();
        }
#endif
        [ContextMenu("GetState")]
        public void DebugFunc1(){
            DIJOYSTATE2 prevDeviceState = DeviceState;
            int hresult = UnityFFBNative.GetDeviceState(ref DeviceState);
            if(hresult!=0){ Debug.LogError($"[UnityFFB] GetDeviceState : 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); }

            test = Compare(DeviceState, prevDeviceState);

            // FieldInfo[] fields = DeviceState.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            // foreach (FieldInfo fi in fields) {
            //     object value = fi.GetValue(DeviceState);
            //     // Debug.Log(value.ToString());
            //     if(value.ToString().EndsWith("[]")) {
            //         Type t = value.GetType();
            //         Debug.Log($"{fi.Name} = {value}");
            //         // var isEqual = DeviceState[fi.Name].SequenceEqual(prevDeviceState[fi.Name]);
            //         // IStructuralEquatable se1 = DeviceState;
            //         // var isEqual = se1.Equals (prevDeviceState, StructuralComparisons.StructuralEqualityComparer);
            //         // Debug.Log(isEqual);
            //         //System.Diagnostics.Debug.WriteLine(fi.Name);
            //         //System.Diagnostics.Debugger.Break();
            //     } else {
            //         // Debug.Log($"{fi.Name} = {value}");
            //     }
            // }
            // var json = JsonUtility.ToJson(DeviceState);
            // Debug.Log(json);
            // Debug.Log("Test2");
            // Debug.Log(json);

        }

        [ContextMenu("GetState2")]
        public void DebugFunc2(){
            DIJOYSTATE2 prevDeviceState = DeviceState;
            int hresult = UnityFFBNative.GetDeviceState(ref DeviceState);
            if(hresult!=0){ Debug.LogError($"[UnityFFB] GetDeviceState : 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); }

            var DEV = DEVCompare(DeviceState, prevDeviceState);
            Debug.Log( DEV );
        }

        public List<string> DEVCompare(DIJOYSTATE2 x, DIJOYSTATE2 y) {
            Debug.Log( x.GetType().GetFields() );
            return (  
            from l1 in x.GetType().GetFields()  
            join l2 in y.GetType().GetFields() on l1.Name equals l2.Name  
            where !l1.GetValue(x).Equals(l2.GetValue(y))  
            select string.Format("{0} {1} {2}", l1.Name, l1.GetValue(x), l2.GetValue(y))  
            ).ToList(); 
        }

        public List<string> Compare(DIJOYSTATE2 x, DIJOYSTATE2 y) {  
            return (  
            from l1 in x.GetType().GetFields()  
            join l2 in y.GetType().GetFields() on l1.Name equals l2.Name  
            where !l1.GetValue(x).Equals(l2.GetValue(y))  
            select string.Format("{0} {1} {2}", l1.Name, l1.GetValue(x), l2.GetValue(y))  
            ).ToList();
        }


    }
}
