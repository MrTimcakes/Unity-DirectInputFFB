# Unity DirectInput Force Feedback
<!-- [![Version](https://img.shields.io/VERSIONPLATFORM/MrTimcakes/XXXXXXX.svg?style=flat-square)](LINK TO UNITY HERE) -->

[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=for-the-badge&logo=unity)](https://unity3d.com)
![GitHub issues](https://img.shields.io/github/issues/MrTimcakes/Monch-Native?style=for-the-badge)

This package allows you to easily integrate both the input and ForceFeedback features of DirectX from within Unity. This allows you to interface with HID peripherals with ForceFeedback capabilities. This can be used to build vivid simulated experiences.

The package will create a virtual device inside Unity's Input System. This device can then be used like any other device inside the Input System, allowing for easiy rebinding. 

## Quick Start

### Installation

This package requires use of Unity's new Input System, ensure [com.unity.inputsystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/QuickStartGuide.html) is installed in the project. Install it via the package manager via: 

`Window -> Package Manager => Input System`

Next, install this package:

`Package Manager => + => "Add package from git URL..." => ` `https://github.com/MrTimcakes/Unity-DirectInputFFB.git` 


## Compatible Devices

| Peripheral                         | Test Status    |
|------------------------------------|----------------|
| Fanatec CSL Elite                  | ✅ Verified    |
| Fanatec WRC Wheel Rim              | ✅ Verified    |
| Fanatec CSL LC Pedals              | ✅ Verified    |
| Fanatec ClubSport Shifter SQ V 1.5 | ✅ Verified    |
| Logitech G29                       | 🔲 Untested    |

## Current limitations

1. Architected in a way to only support 1 controller.
2. Currently only supports 1 Effect of each type per device.
3. Only supports Constant Force and Spring Condition.

## Environment

This plugin only works on Windows 64 bit.

Unity Version: 2021.1.7f1

# Support

If you're having any problem, please [raise an issue](https://github.com/MrTimcakes/Unity-DirectInputFFB/issues/new) on GitHub.

## License

This repo is a Fork of [Unity-FFB](https://github.com/skaughtx0r/unity-ffb) and thus is released under the MIT License, further information can be found under the terms specified in the [license](https://github.com/MrTimcakes/Unity-DirectInputFFB/blob/master/LICENSE).