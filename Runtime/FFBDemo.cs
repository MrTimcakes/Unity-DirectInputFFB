using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DirectInputFFB;

public class FFBDemo : MonoBehaviour{
    [Range(-100, 100)]
    public int ConstantForceMagnitude = 0;
    [Range(0, 150)]
    public int ConstantForceMagnitudeMultiplier = 125;
    public float ConstantForceGain = 0.0f;

    public DeviceInfo[] RawDeviceInfo;
    public FlatJoyState2 DeviceState;

    public void Awake(){
        FFBManager.Initialize(); // Initialize if it's not already
        FFBManager.EnableConstantForce(); // Enable Constant Force effect
        FFBManager.EnableSpringForce(); // Enable Spring Effect

        RawDeviceInfo = FFBManager.devices;
        FFBManager.OnDeviceStateChange += OnDeviceStateChange; // Register listner
    }

    public void OnDeviceStateChange(object sender, EventArgs args){
        DeviceState = FFBManager.state; // Raw state, usually use whats exposed by the Input System
    }

    public void FixedUpdate(){
        FFBManager.ConstantForce( ConstantForceMagnitude * ConstantForceMagnitudeMultiplier );
    }

    [ContextMenu("SetConstantForceGain")]
    public void SetConstantForceGain(){
        FFBManager.SetConstantForceGain( ConstantForceGain );
    }

    [ContextMenu("SetConstantForceGain0")]
    public void SetConstantForceGain0(){
        FFBManager.SetConstantForceGain( 0.0f );
    }
 

    [ContextMenu("SetConstantForceMagnitude0")]
    public void SetConstantForceMagnitude0(){
        FFBManager.ConstantForce(0);
    }

    [ContextMenu("DEVSpring")]
    public void DEVSpring(){

        // DICondition[] conditions = new DICondition[0];
        // conditions = new DICondition[_axisCount];

        // for (int i = 0; i < conditions.Length; i++) {
        //     conditions[i] = new DICondition();
        //     conditions[i].deadband = 0;
        //     conditions[i].offset = 0;
        //     conditions[i].negativeCoefficient = 2000;
        //     conditions[i].positiveCoefficient = 2000;
        //     conditions[i].negativeSaturation = 10000;
        //     conditions[i].positiveSaturation = 10000;
        // }
        // FFBManager.SpringForce();
    }

}
