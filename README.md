# 🌀 Unity URP Datamosh Shader

A real-time datamoshing/glitch shader effect for Unity's Universal Render Pipeline (URP). This system simulates motion artifacting, block-based tearing, and compression noise by leveraging camera motion vectors and frame blending logic. Ideal for interactive sequences, glitch transitions, and stylized visuals.

## ✨ Features

- Custom `ScriptableRendererFeature` and `RenderPass` for URP
- Frame-to-frame feedback with working/displacement buffers
- Motion vector-based block shifting
- Parameterized entropy, contrast, diffusion, and velocity controls
- Multi-pass HLSL shader
- Modular and expandable

All parameters are exposed and runtime-adjustable through the `MoshController`.

## 📦 Requirements

- Unity 2022+ with URP
- URP Forward Renderer asset
- Depth and Motion Vectors enabled
- A compatible camera with `MoshController` attached

## 🧪 Parameters

| Parameter           | Description                                      |
|---------------------|--------------------------------------------------|
| `Block Size`        | Macroblock size for distortion artifacts         |
| `Velocity Vector Scale` | Scales motion vector displacement          |
| `Entropy`           | Increases glitch randomness and compression noise |
| `Contrast`          | Controls stripe-like noise intensity             |
| `Diffusion`         | Adds random displacement jittering               |

## How It Works

1. The first shader pass initializes the displacement buffer.
2. The second pass updates this buffer with motion vectors and entropy noise.
3. The third pass blends the current frame with previous ones using the displaced data, simulating a "mosh".

Command Buffers and `ScriptableRenderPass` instances are used to apply this effect non-destructively to the camera's render target.

## Integration

1. Add the `MoshRendererFeature` to your URP Forward Renderer.
2. Attach the `MoshController` to your main camera.
3. Configure your material and runtime parameters.
4. Trigger glitch effects via calls to `Glitch()` or `Reset()`.

---

## Customization Notes

- Shader is fully written in HLSL with motion vector support.
- Supports runtime triggering via `IntVariable` or `PlayerAction`-based systems.
- Designed for narrative or experiential games with intentional aesthetic distortion.

---

## 🧪 TODOs / Notes

- Could be extended with audio-reactive input or scene-based mosh variations.
- Further optimization possible via async GPU readbacks or lower-res buffers.

---

**License**: MIT  
**Made with chaos by** [Oyku Yamakoglu](https://oykuyamakov.github.io/)
📸 [@madayten](https://www.instagram.com/madayten/) on Instagram

## 

Built on the brilliant ideas from [Keijiro](https://github.com/keijiro)'s SRP Datamosh.  
This version pushes it further with a custom URP pipeline and full runtime control.


