using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;

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
        public static extern int UpdateEffectGain(EffectsType effectType, float gainPercent);
        
        [DllImport(FFBDLL)]
        public static extern int GetDeviceState(ref DIJOYSTATE2 DeviceStateObj);

        [DllImport(FFBDLL)]
        public static extern int UpdateConstantForce(int magnitude, int[] directions);
    
        [DllImport(FFBDLL)]
        public static extern int UpdateSpringRaw(DICondition[] conditions);

        [DllImport(FFBDLL)]
        public static extern int UpdateSpring(int offset, int Coefficient, int Saturation);

        [DllImport(FFBDLL)]
        public static extern int UpdateDamperRaw(DICondition[] conditions);

        [DllImport(FFBDLL)]
        public static extern int UpdateDamper(int magnitude);

        [DllImport(FFBDLL)]
        public static extern int SetAutoCenter(bool autoCenter);

        [DllImport(FFBDLL)]
        public static extern void StartAllFFBEffects();

        [DllImport(FFBDLL)]
        public static extern void StopAllFFBEffects();

        [DllImport(FFBDLL)]
        public static extern void StopDirectInput();

        
        
        
        #endif

    }















    public static class FFBManager{

        private static bool _isInitialized = false;
        public static bool isInitialized { 
            get => _isInitialized; // { return _isInitialized; }
            set{ Debug.Log("[DirectInputFFB]Can't set isInitialized!"); }
        }
        private static bool _ConstantForceEnabled = false;
        private static bool _SpringForceEnabled = false;
        private static bool _DamperForceEnabled = false;

        private static DeviceInfo[] _devices = new DeviceInfo[0];
        private static DeviceInfo? _activeDevice = null;
        private static DeviceAxisInfo[] _axes = new DeviceAxisInfo[0];
        private static DICondition[] _springConditions = new DICondition[0];
        private static DICondition[] _damperConditions = new DICondition[0];
        private static int[] _axisDirections = new int[0];
        private static FlatJoyState2 _activeDeviceState;
        private static int _axesCount = 0;





        public static DeviceInfo[] devices { 
            get => _devices;
            set{ Debug.Log("[DirectInputFFB]Can't set devices!"); }
        }
        public static FlatJoyState2 state {
            get => _activeDeviceState;
            set{ Debug.Log("[DirectInputFFB]Can't set activeDeviceState!"); }
        }
        public static int axesCount{
            get => _axesCount; // No Need to match device GUID and return in devices array
            set{ Debug.Log("[DirectInputFFB]Can't set axesCount!"); }
        }

        public static event EventHandler OnDeviceStateChange = delegate {}; // Handle for events when the device input has changed, e.g. Axis moved or button pressed



/// <summary>
/// Initialize Master FFB
/// Returns True if already Initialized or actually starts DirectInput Capture
/// </summary>

        public static bool Initialize(){
        #if UNITY_STANDALONE_WIN
            if (_isInitialized) { return _isInitialized; }

            if (Native.StartDirectInput() >= 0) {
                _isInitialized = true;
                Debug.Log("[DirectInputFFB]Initialized!");
                EnumerateDirectInputDevices();
                SelectDevice(_devices[0].guidInstance); // Select to first device
                Debug.Log( "[DirectInputFFB]Attached to Device:0, Device: " + devices[0].productName );
            } else {
                _isInitialized = false;
            }
            return _isInitialized;
        #endif
        }

        private static void EnumerateDirectInputDevices(){
            int deviceCount = 0;
            IntPtr ptrDevices = Native.EnumerateFFBDevices(ref deviceCount);
            // Debug.Log($"[DirectInputFFBz] Device count: {deviceCount}");
            if (deviceCount > 0) {
                _devices = new DeviceInfo[deviceCount];

                int deviceSize = Marshal.SizeOf(typeof(DeviceInfo));
                for (int i = 0; i < deviceCount; i++) {
                    IntPtr pCurrent = ptrDevices + i * deviceSize;
                    _devices[i] = Marshal.PtrToStructure<DeviceInfo>(pCurrent);
                }
                // foreach (DeviceInfo device in devices) {
                //     string ffbAxis = UnityEngine.JsonUtility.ToJson(device, true);
                //     // Debug.Log(ffbAxis);
                // }
            }

        }

        public static void DisableForceFeedback(){
        #if UNITY_STANDALONE_WIN
            // Native.StopDirectInput();
            Native.StopAllFFBEffects();
            _isInitialized = false;
            _ConstantForceEnabled = false;
            _SpringForceEnabled = false;
            _DamperForceEnabled = false;

            // _devices = new DeviceInfo[0];
            // _activeDevice = null;
            // _axes = new DeviceAxisInfo[0];
            // _springConditions = new DICondition[0];
        #endif
        }

        public static void SelectDevice(string deviceGuid){ // Sets the device as the primary controller device, Accepts Device.guidInstance
        #if UNITY_STANDALONE_WIN
            // For now just initialize the first FFB Device.
            int hresult = Native.CreateFFBDevice(deviceGuid);
            if (hresult == 0) {
                _activeDevice = _devices[0];

                // if (disableAutoCenter) {
                //     hresult = Native.SetAutoCenter(false);
                //     if (hresult != 0) {
                //         Debug.LogError($"[DirectInputFFB] SetAutoCenter Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                //     }
                // }

                IntPtr ptrAxes = Native.EnumerateFFBAxes(ref _axesCount); // Return the axis and how many it returned
                if (_axesCount > 0) {
                    _axes               = new DeviceAxisInfo[_axesCount]; // Size the _axis array to fit for this device
                    _axisDirections     = new int           [_axesCount]; // ^^
                    _springConditions   = new DICondition   [_axesCount]; // ^^
                    _damperConditions   = new DICondition   [_axesCount]; // ^^

                    int axisSize = Marshal.SizeOf(typeof(DeviceAxisInfo));
                    for (int i = 0; i < _axesCount; i++) {
                        IntPtr pCurrent = ptrAxes + i * axisSize;
                        _axes[i]             = Marshal.PtrToStructure<DeviceAxisInfo>(pCurrent); // Fill with data from the device
                        _axisDirections[i]   = 0; // Default each Axis Direction to 0
                        _springConditions[i] = new DICondition(); // For each axis create an effect
                        _damperConditions[i] = new DICondition(); // For each axis create an effect
                    }
                }
                // Debug.Log($"[DirectInputFFB] Axis count: {_axes.Length}");
                // foreach (DeviceAxisInfo axis in _axes) {
                //     string ffbAxis = UnityEngine.JsonUtility.ToJson(axis, true);
                //     // Debug.Log(ffbAxis);
                // }
            } else {
                _activeDevice = null;
                Debug.LogError($"[DirectInputFFB] 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }
        #endif
        }

        public static void SelectDevice(DeviceInfo Device){ // Sets the device as the primary controller device, Accepts DeviceInfo
            SelectDevice( Device.guidInstance ); 
        }




        public static bool EnableConstantForce(){
            if(_ConstantForceEnabled){ return true; } // Already Enabled 
            
            int hresult = Native.AddFFBEffect(EffectsType.ConstantForce); // Enable the Constant Force Effect
            if (hresult == 0) {
                hresult = Native.UpdateConstantForce(0, _axisDirections);
                if (hresult != 0) { Debug.LogError($"[DirectInputFFB] UpdateConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); }
                _ConstantForceEnabled = true;
            } else {
                _ConstantForceEnabled = false;
                Debug.LogError($"[DirectInputFFB] EnableConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }
            return _ConstantForceEnabled;
        }

        public static bool ConstantForce(int magnitude){ // Range Int32.MinValue - MaxValue theoretically?
            if(!_ConstantForceEnabled){ return false; }// Check if ConstantForce enabled
            int hresult = Native.UpdateConstantForce(magnitude, _axisDirections); // Try to apply the force
            if (hresult != 0) { Debug.LogError($"[DirectInputFFB] UpdateConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
            return true; // Didn't fail so it worked
        }

        public static bool SetConstantForceGain(float gainPercent){ // Range 0 through 10,000? (https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416616(v=vs.85))
            if(!_ConstantForceEnabled){ return false; }// Check if ConstantForce enabled
            int hresult = Native.UpdateEffectGain(EffectsType.ConstantForce, gainPercent);
            if (hresult != 0) { Debug.LogError($"[DirectInputFFB] UpdateEffectGain Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
            return true; // Didn't fail so it worked
        }
        
        public static bool EnableSpringForce(){
            if(_SpringForceEnabled){ return true; } // Already Enabled 
            int hresult = Native.AddFFBEffect(EffectsType.Spring); // Try add the Spring Effect
            if (hresult == 0) { // If worked, try to set it
                for (int i = 0; i < _springConditions.Length; i++) {
                    _springConditions[i].deadband = 0;
                    _springConditions[i].offset = 0;
                    _springConditions[i].negativeCoefficient = 2000;
                    _springConditions[i].positiveCoefficient = 2000;
                    _springConditions[i].negativeSaturation = 10000;
                    _springConditions[i].positiveSaturation = 10000;
                }
                hresult = Native.UpdateSpringRaw(_springConditions);
                if (hresult != 0) { Debug.LogError($"[DirectInputFFB] UpdateSpring Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
                _SpringForceEnabled = true;
            } else {
                Debug.LogError($"[DirectInputFFB] EnableSpringForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }


            return _SpringForceEnabled;
        }

        public static bool SpringForce(DICondition[] conditions){
            if(!_SpringForceEnabled){ return false; }// Check if SpringForce enabled
            int hresult = Native.UpdateSpringRaw(conditions);
            if (hresult != 0) { Debug.LogError($"[DirectInputFFB] SpringForceRaw Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
            return true; // Didn't fail so it worked
        }
        
        public static bool SpringForce(int SpringOffset, int SpringCoefficient, int SpringSaturation){
            if(!_SpringForceEnabled){ return false; }// Check if SpringForce enabled
            int hresult = Native.UpdateSpring(SpringOffset, SpringCoefficient, SpringSaturation);
            if (hresult != 0) { Debug.LogError($"[DirectInputFFB] UpdateSpring Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
            return true; // Didn't fail so it worked
        }

        public static bool EnableDamperForce(){ // Adds the Damper effect to the DirectInput Device
            if(_DamperForceEnabled){ return true; } // Already Enabled 
            int hresult = Native.AddFFBEffect(EffectsType.Damper); // Try add the Damper Effect        
            if (hresult == 0) { // If worked, try to set it
                for (int i = 0; i < _damperConditions.Length; i++) {
                    _damperConditions[i].deadband = 0;
                    _damperConditions[i].offset = 0;
                    _damperConditions[i].negativeCoefficient = 0;
                    _damperConditions[i].positiveCoefficient = 0;
                    _damperConditions[i].negativeSaturation = 0;
                    _damperConditions[i].positiveSaturation = 0;
                }
                hresult = Native.UpdateDamperRaw(_damperConditions);
                if (hresult != 0) { Debug.LogError($"[DirectInputFFB] UpdateDamper Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
                _DamperForceEnabled = true;
            } else {
                Debug.LogError($"[DirectInputFFB] EnableDamperForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
            }

            return _DamperForceEnabled;
        }

        public static bool DamperForce(DICondition[] conditions){
            if(!_DamperForceEnabled){ return false; }// Check if DamperForce enabled
            int hresult = Native.UpdateDamperRaw(conditions);
            if (hresult != 0) { Debug.LogError($"[DirectInputFFB] DamperForceRaw Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
            return true; // Didn't fail so it worked
        }

        public static bool DamperForce(int magnitude){
            if(!_DamperForceEnabled){ return false; }// Check if DamperForce enabled
            int hresult = Native.UpdateDamper(magnitude);
            if (hresult != 0) { Debug.LogError($"[DirectInputFFB] UpdateDamper Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");return false; }
            return true; // Didn't fail so it worked
        }


        // DICondition[] CalculateSpringCondition(int Offset, int Coefficient, int Saturation){ // Transform Offset, Coefficient, Saturation to DICondition
        //     DICondition[] SpringConditions = new DICondition[FFBManager.axesCount];
        //     for (int i = 0; i < SpringConditions.Length; i++) {
        //         SpringConditions[i] = new DICondition();
        //         SpringConditions[i].offset = Offset;
        //         SpringConditions[i].positiveCoefficient = Coefficient;
        //         SpringConditions[i].negativeCoefficient = Coefficient;
        //         SpringConditions[i].positiveSaturation  = (uint)Saturation;
        //         SpringConditions[i].negativeSaturation  = (uint)Saturation;
        //         SpringConditions[i].deadband = 0;
        //         // Debug.Log($"Spring: {SpringConditions[i].positiveCoefficient}, {SpringConditions[i].negativeCoefficient}, {SpringConditions[i].positiveSaturation}, {SpringConditions[i].negativeSaturation} ");
        //     }
        //     return SpringConditions;
        // }

        // DICondition[] CalculateDamperCondition(int magnitude){ // Transform Magnitude to DICondition
        //     DICondition[] DamperConditions = new DICondition[FFBManager.axesCount];
        //     for (int i = 0; i < DamperConditions.Length; i++) {
        //         DamperConditions[i] = new DICondition();
        //         DamperConditions[i].offset = 0;
        //         DamperConditions[i].positiveCoefficient = magnitude;
        //         DamperConditions[i].negativeCoefficient = magnitude;
        //         DamperConditions[i].positiveSaturation  = (uint)0;
        //         DamperConditions[i].negativeSaturation  = (uint)0;
        //         DamperConditions[i].deadband = 0;
        //         // Debug.Log($"Damper: {magnitude} ");
        //     }
        //     return DamperConditions;
        // }


        public static void PollDevice(){
            DirectInputFFB.DIJOYSTATE2 DeviceState = new DirectInputFFB.DIJOYSTATE2(); // Store the raw state of the device
            int hresult = DirectInputFFB.Native.GetDeviceState(ref DeviceState); // Fetch the device state
            if(hresult!=0){ Debug.LogError($"[DirectInputFFB] GetDeviceState : 0x{hresult.ToString("x")} {DirectInputFFB.WinErrors.GetSystemMessage(hresult)}\n[DirectInputFFB] Perhaps the device has not been attached/acquired"); }
            
            FlatJoyState2 state = Utilities.FlattenDIJOYSTATE2(DeviceState); // Flatten the state for comparison
            if( !state.Equals(_activeDeviceState) ){ // Some input has changed
                _activeDeviceState = state;
                OnDeviceStateChange(null, EventArgs.Empty); // Bubble up event
            }
        }


    }
}
