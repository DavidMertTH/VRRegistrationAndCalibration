Publication: https://www.researchgate.net/publication/400386706_Aligning_Realities_A_Registration_Pipeline_for_Arbitrary_Objects_in_Mixed_Reality_Using_Controller-Based_Point_Selection

# VRRegistrationAndCalibration
Calibrate VR controller tips and register real-world objects in Unity-based VR projects.

## References

The Kabsch implementation was taken from https://github.com/zalo/MathUtilities/tree/master/Assets/Kabst.
The calibration process implements a procedure introduced by Rainer Splechtna https://ieeexplore.ieee.org/document/10108564.

##  Usage

1. Drag the **`Registration` prefab, RegistrationUI` prefab, DemoTarget prefab** into your scene.
2. On the `RegistrationVR` component (attached to the Registration prefab), assign the DemoTarget Object
3. On the `RegistrationUI` Object assign the Registration Component to the RegistrationVR Controller Component.


##  Creating a RegiTarget

1. Add the **`RegiTarget`** component to the GameObject you want to register. Alternatively you can also use the DemoTarget Prefab.
3. In the Inspector, use the slider to select how many **marker points** you want to use for the registration process.
4. In the Scene view, position the markers so they match the real-world reference points of the object.

## Spatial Anchors

After an object is registered, it can be locked in position using a spatial anchor. If the function Registration.SaveRegistration is used, the spatial anchor is saved on the device.
Note: This deletes all previous saved anchors for this specific UUID to prevent incorrect initialisation.
With the function: Registration.RestoreLastPlacedAnchor(), the last saved anchor is bound to the given registration target.

This system allows multiple objects to be stored and re-instantiated.

## Fixed Axis

When selecting the Option: *fixed X/Z*, the Y axis will be the only one corrected. This is advantageous because, frequently, the pitch and yaw of an object are correct, but the rotation is not. This is the default setting when selecting the 'Projection Plane Mapping' option.

## Controller Tip

The Meta Quest 3 Controller Tip can be printed with the **`ControllerTip.stl`** STL File

![WhatsApp Bild 2025-08-01 um 12 52 12_f7fb5ee5](https://github.com/user-attachments/assets/e9ae9166-d62a-4282-8858-34d68c3c2c8b)
