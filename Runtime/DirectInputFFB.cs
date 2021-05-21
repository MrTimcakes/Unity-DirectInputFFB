using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DirectInputFFB {
    public class DirectInputFFB : MonoBehaviour {
        public static DirectInputFFB instance;

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

        public bool DEBUGGING;

        public DIJOYSTATE2 DeviceState;

        public System.Collections.Generic.List<string> test;

        #if UNITY_STANDALONE_WIN
        void Awake(){
            instance = this;

            if (enableOnAwake) { EnableForceFeedback(); }
        }

        private void FixedUpdate() {
            if (constantForceEnabled) { Native.UpdateConstantForce((int)(force * sensitivity), axisDirections); }
            if(DEBUGGING){ DebugFunc1(); }
        }
        #endif

        public void EnableForceFeedback(){
        #if UNITY_STANDALONE_WIN
            if (ffbEnabled) { return; }

            if (Native.StartDirectInput() >= 0) {
                ffbEnabled = true;
            } else {
                ffbEnabled = false;
            }

            int deviceCount = 0;

            IntPtr ptrDevices = Native.EnumerateFFBDevices(ref deviceCount);

            Debug.Log($"[DirectInputFFB] Device count: {devices.Length}");
            if (deviceCount > 0) {
                devices = new DeviceInfo[deviceCount];

                int deviceSize = Marshal.SizeOf(typeof(DeviceInfo));
                for (int i = 0; i < deviceCount; i++) {
                    IntPtr pCurrent = ptrDevices + i * deviceSize;
                    devices[i] = Marshal.PtrToStructure<DeviceInfo>(pCurrent);
                }

                foreach (DeviceInfo device in devices) {
                    string ffbAxis = UnityEngine.JsonUtility.ToJson(device, true);
                    Debug.Log(ffbAxis);
                }

                if (autoSelectFirstDevice) { SelectDevice(devices[0].guidInstance); }
            }
        #endif
        }

        public void DisableForceFeedback(){
        #if UNITY_STANDALONE_WIN
            Native.StopDirectInput();
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
            int hresult = Native.CreateFFBDevice(deviceGuid);
            if (hresult == 0) {
                activeDevice = devices[0];

                if (disableAutoCenter) {
                    hresult = Native.SetAutoCenter(false);
                    if (hresult != 0) {
                        Debug.LogError($"[DirectInputFFB] SetAutoCenter Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                    }
                }

                int axisCount = 0;
                IntPtr ptrAxes = Native.EnumerateFFBAxes(ref axisCount);
                if (axisCount > 0) {
                    axes = new DeviceAxisInfo[axisCount];
                    axisDirections = new int[axisCount];
                    springConditions = new DICondition[axisCount];

                    int axisSize = Marshal.SizeOf(typeof(DeviceAxisInfo));
                    for (int i = 0; i < axisCount; i++) {
                        IntPtr pCurrent = ptrAxes + i * axisSize;
                        axes[i] = Marshal.PtrToStructure<DeviceAxisInfo>(pCurrent);
                        axisDirections[i] = 0;
                        springConditions[i] = new DICondition();
                    }

                    if (addConstantForce) {
                        hresult = Native.AddFFBEffect(EffectsType.ConstantForce);
                        if (hresult == 0) {
                            hresult = Native.UpdateConstantForce(0, axisDirections);
                            if (hresult != 0) {
                                Debug.LogError($"[DirectInputFFB] UpdateConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                            }
                            constantForceEnabled = true;
                        } else {
                            Debug.LogError($"[DirectInputFFB] AddConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                        }
                    }

                    if (addSpringForce) {
                        hresult = Native.AddFFBEffect(EffectsType.Spring);
                        if (hresult == 0) {
                            for (int i = 0; i < springConditions.Length; i++) {
                                springConditions[i].deadband = 0;
                                springConditions[i].offset = 0;
                                springConditions[i].negativeCoefficient = 2000;
                                springConditions[i].positiveCoefficient = 2000;
                                springConditions[i].negativeSaturation = 10000;
                                springConditions[i].positiveSaturation = 10000;
                            }
                            hresult = Native.UpdateSpring(springConditions);
                            Debug.LogError($"[DirectInputFFB] UpdateSpringForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                        } else {
                            Debug.LogError($"[DirectInputFFB] AddSpringForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                        }
                    }
                }
                Debug.Log($"[DirectInputFFB] Axis count: {axes.Length}");
                foreach (DeviceAxisInfo axis in axes) {
                    string ffbAxis = UnityEngine.JsonUtility.ToJson(axis, true);
                    Debug.Log(ffbAxis);
                }
            } else {
                activeDevice = null;
                Debug.LogError($"[DirectInputFFB] 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }
        #endif
        }

        public void SetConstantForceGain(float gainPercent){
        #if UNITY_STANDALONE_WIN
            if (constantForceEnabled) {
                int hresult = Native.UpdateEffectGain(EffectsType.ConstantForce, gainPercent);
                Debug.LogError($"[DirectInputFFB] UpdateEffectGain Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }
        #endif
        }

        public void StartFFBEffects(){
        #if UNITY_STANDALONE_WIN
            Native.StartAllFFBEffects();
            constantForceEnabled = true;
        #endif
        }

        public void StopFFBEffects(){
        #if UNITY_STANDALONE_WIN
            Native.StopAllFFBEffects();
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
            int hresult = Native.GetDeviceState(ref DeviceState);
            if(hresult!=0){ Debug.LogError($"[DirectInputFFB] GetDeviceState : 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); }

            test = Compare(DeviceState, prevDeviceState);
        }

        [ContextMenu("GetState2")]
        public void DebugFunc2(){
            DIJOYSTATE2 prevDeviceState = DeviceState;
            int hresult = Native.GetDeviceState(ref DeviceState);
            if(hresult!=0){ Debug.LogError($"[DirectInputFFB] GetDeviceState : 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); }

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
