<div align="center">

# Aligning Realities
### VR Registration & Calibration for Unity

Calibrate VR controller tips and register real-world objects in Unity-based VR/MR projects.

[![Paper](https://img.shields.io/badge/Paper-00CCBB?style=for-the-badge)](https://www.researchgate.net/publication/400386706_Aligning_Realities_A_Registration_Pipeline_for_Arbitrary_Objects_in_Mixed_Reality_Using_Controller-Based_Point_Selection)
[![CGT Cologne](https://img.shields.io/badge/CGT%20Cologne-blue?style=for-the-badge)](https://github.com/cgthkoeln)
[![IEEE VR](https://img.shields.io/badge/IEEE%20VR-orange?style=for-the-badge)](https://ieeevr.org/)

<table>
  <tr>
    <td align="center"><img src="https://github.com/user-attachments/assets/74c9796d-4ba2-4445-8e45-8698593f795e" width="250"/></td>
    <td align="center"><img src="https://github.com/user-attachments/assets/8b76c373-0f19-497c-8e39-4bed570a8f11" width="250"/></td>
    <td align="center"><img src="https://github.com/user-attachments/assets/6ca38d91-e3ac-4092-994e-3babeab77850" width="250"/></td>
  </tr>
  <tr>
    <td align="center"><b>Calibration</b></td>
    <td align="center"><b>Marking</b></td>
    <td align="center"><b>Alignment</b></td>
  </tr>
</table>

</div>

---

## 📦 Quick Setup

1. Drag the `Registration` prefab, `RegistrationUI` prefab, and `DemoTarget` prefab into your scene.
2. On the **`RegistrationVR`** component (attached to the `Registration` prefab), assign the `DemoTarget` object.
3. On the **`RegistrationUI`** object, assign the `Registration` component to the `RegistrationVR Controller` component.

---

## 🎯 Creating a RegiTarget

1. Add the **`RegiTarget`** component to the GameObject you want to register — or use the `DemoTarget` prefab directly.
2. In the **Inspector**, use the slider to set the number of **marker points** for the registration process.
3. In the **Scene view**, position the markers to match the real-world reference points of the object.

---

## ⚓ Spatial Anchors

After an object is registered, it can be locked in place using a spatial anchor.

| Function | Description |
|---|---|
| `Registration.SaveRegistration()` | Saves the spatial anchor on the device. Deletes all previous anchors for this UUID to prevent incorrect initialisation. |
| `Registration.RestoreLastPlacedAnchor()` | Binds the last saved anchor to the given registration target. |

> This system supports multiple objects being stored and re-instantiated independently.

---

## 📐 Fixed Axis Mode

When selecting **Fixed X/Z**, only the **Y axis** is corrected. This is useful when pitch and yaw are already accurate but rotation is off.

> ℹ️ This is the default setting when using the **Projection Plane Mapping** option.

---

## 🖨️ Controller Tip

The **Meta Quest 3** controller tip attachment can be 3D-printed using the included STL file:
![WhatsApp Bild 2025-08-01 um 12 52 12_f7fb5ee5](https://github.com/user-attachments/assets/e9ae9166-d62a-4282-8858-34d68c3c2c8b)

```
ControllerTip.stl
```

---

## 📖 References

- **Publication:** [Aligning Realities: A Registration Pipeline for Arbitrary Objects in Mixed Reality Using Controller-Based Point Selection](https://www.researchgate.net/publication/400386706_Aligning_Realities_A_Registration_Pipeline_for_Arbitrary_Objects_in_Mixed_Reality_Using_Controller-Based_Point_Selection)
- **Kabsch Algorithm:** [CGT Cologne](https://github.com/cgthkoeln)
- **Calibration Procedure:** [IEEE VR](https://ieeevr.org/)
