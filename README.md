# VRRegistrationAndCalibration
Calibrate VR controller tips and register real-world objects in Unity-based VR projects.

##  Usage

1. Drag the **`Registration` prefab, RegistrationUI` prefab, DemoTarget prefab** into your scene.
2. On the `RegistrationVR` component (attached to the Registration prefab), assign the DemoTarget Object
3. On the `RegistrationUI` component assign the RegistrationVR Component of the Registration Object


##  Creating a RegiTarget

1. Add the **`RegiTarget`** component to the GameObject you want to register. Alternatively you can also use the DemoTarget Prefab.
3. In the Inspector, use the slider to select how many **marker points** you want to use for the registration process.
4. In the Scene view, position the markers so they match the real-world reference points of the object.


## Calibration

The Calibration Process implements a procesdure introduced by Rainer Splechtna (https://ieeexplore.ieee.org/document/10108564)

## Controller Tip

The Meta Quest 3 Controller Tip can be printed with the **`ControllerTip.stl`** STL File

![WhatsApp Bild 2025-08-01 um 12 52 12_f7fb5ee5](https://github.com/user-attachments/assets/e9ae9166-d62a-4282-8858-34d68c3c2c8b)
