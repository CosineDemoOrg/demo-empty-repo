# Super Platformer — Controls & Prototyping Guide

## Default Controls

| Action         | Primary                    | Alt         |
|----------------|---------------------------|-------------|
| Move Left      | ← (Left Arrow)            | A           |
| Move Right     | → (Right Arrow)           | D           |
| Jump           | ↑ (Up Arrow)              | Space       |
| Crouch         | ↓ (Down Arrow)            | S           |
| Run / Sprint   | Hold LMB (Mouse Button 0) | Left Shift  |
| Shoot Fireball | Left Mouse Click          | Left Ctrl   |
| Pause / Menu   | Right Mouse Click         | Escape      |

*All controls can be changed in the InputManager inspector (see TODO for rebinding support).*

---

## Prototyping Instructions

1. **Create a new scene**  
   File → New Scene (or open your test scene).

2. **Add the InputManager prefab**  
   - In your scene hierarchy, create an empty GameObject named `InputManager`.
   - Add the `InputManager` script (`SuperPlatformer/Assets/Scripts/Core/Input/InputManager.cs`).
   - Only one InputManager should exist per scene.

3. **Add the Player prefab**  
   - Create an empty GameObject named `Player`.
   - Add a `SpriteRenderer` and the `PlayerController` script (`SuperPlatformer/Assets/Scripts/Gameplay/Player/PlayerController.cs`).
   - Assign a `Rigidbody2D` (or let PlayerController auto-find it).
   - In the Inspector, you can toggle "isGroundedDebug" to simulate ground contact, or adjust movement values for quick testing.

4. **Playtest Movement**  
   - Enter Play mode and use the controls above (see table).
   - Tweak exposed movement fields if desired.
   - *Note: Physics/level collision is stubbed; player will not collide with tiles yet.*

---

## Notes

- All movement parameters are defined in `PlayerConstants.cs` for easy PRD traceability.
- PhysicsEngine2D will be hooked up in a future milestone.
- #if UNITY_EDITOR guards prevent errors in headless builds.