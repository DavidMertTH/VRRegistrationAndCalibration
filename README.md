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

