# VRRegistrationAndCalibration
Calibrate VR controller tips and register real-world objects in Unity-based VR projects.

## Installation

After the installation move the Files in Plugins in your own Plugin folder.

##  Usage

1. Drag the **`Registration` prefab** into your scene.
2. Drag the **`RegistrationUI` prefab** into your scene.
3. On the `RegistrationVR` component (attached to the Registration prefab), assign the following:
   - The **controller** you want to calibrate.
   - The **`RegistrationUI` GameObject**.
   - A **`RegiTarget`** that you want to register.

---

##  Creating a RegiTarget

1. Add the **`RegiTarget`** component to the GameObject you want to register.
2. In the Inspector, use the slider to select how many **marker points** you want to use for the registration process.
3. In the Scene view, position the markers so they match the real-world reference points of the object.

