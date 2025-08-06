#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
using System.Collections.Generic;
using UnityEngine;

namespace Core.Input
{
    /// &lt;summary&gt;
    /// Input abstraction layer. Handles polling legacy UnityEngine.Input and exposes a single InputState per frame.
    /// &lt;/summary&gt;
    public class InputManager : MonoBehaviour
    {
        public struct InputState
        {
            public float moveAxis;      // -1 (left) to 1 (right)
            public bool jumpPressed;
            public bool jumpHeld;
            public bool crouch;
            public bool runHeld;
            public bool shootPressed;
            public bool pausePressed;
        }

        public static InputManager Instance { get; private set; }

        public InputState Current { get; private set; }

        // Input bindings as per PRD §5:
        // | Action        | Primary          | Alt   |
        // |-------------- |------------------|-------|
        // | Move Left     | ←                |  A    |
        // | Move Right    | →                |  D    |
        // | Jump          | ↑                | Space |
        // | Crouch        | ↓                |  S    |
        // | Run / Sprint  | Hold LMB (0)     | Shift |
        // | Shoot Fireball| Left Mouse Click | Ctrl  |
        // | Pause/Menu    | Right Mouse Click| Esc   |

        [Header("Key Bindings (PRD §5, change in Inspector for prototyping)")]
        [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode leftKeyAlt = KeyCode.A;
        [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode rightKeyAlt = KeyCode.D;
        [SerializeField] private KeyCode jumpKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode jumpKeyAlt = KeyCode.Space;
        [SerializeField] private KeyCode crouchKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode crouchKeyAlt = KeyCode.S;
        [SerializeField] private KeyCode runKeyAlt = KeyCode.LeftShift;
        [SerializeField] private int runMouseButton = 0; // left mouse button
        [SerializeField] private KeyCode shootKeyAlt = KeyCode.LeftControl;
        [SerializeField] private int shootMouseButton = 0; // left mouse button
        [SerializeField] private KeyCode pauseKeyAlt = KeyCode.Escape;
        [SerializeField] private int pauseMouseButton = 1; // right mouse button

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            Current = PollInput();
        }

        private InputState PollInput()
        {
            InputState state = new InputState();

            // Horizontal axis: digital
            float left = (UnityEngine.Input.GetKey(leftKey) || UnityEngine.Input.GetKey(leftKeyAlt)) ? -1f : 0f;
            float right = (UnityEngine.Input.GetKey(rightKey) || UnityEngine.Input.GetKey(rightKeyAlt)) ? 1f : 0f;
            state.moveAxis = left + right;
            if (state.moveAxis != 0)
                state.moveAxis = Mathf.Clamp(state.moveAxis, -1f, 1f);

            // Jump
            state.jumpPressed = UnityEngine.Input.GetKeyDown(jumpKey) || UnityEngine.Input.GetKeyDown(jumpKeyAlt);
            state.jumpHeld = UnityEngine.Input.GetKey(jumpKey) || UnityEngine.Input.GetKey(jumpKeyAlt);

            // Crouch
            state.crouch = UnityEngine.Input.GetKey(crouchKey) || UnityEngine.Input.GetKey(crouchKeyAlt);

            // Run: held if MouseButton(0) *or* Shift
            state.runHeld = UnityEngine.Input.GetMouseButton(runMouseButton) || UnityEngine.Input.GetKey(runKeyAlt);

            // Shoot: fires only if MouseButtonDown(0) and not holding run, or Ctrl
            // (PlayerController must check runHeld to gate fireball)
            bool mouse0Down = UnityEngine.Input.GetMouseButtonDown(shootMouseButton);
            bool holdingRun = UnityEngine.Input.GetMouseButton(runMouseButton) || UnityEngine.Input.GetKey(runKeyAlt);
            bool ctrlDown = UnityEngine.Input.GetKeyDown(shootKeyAlt);
            state.shootPressed = (mouse0Down && !holdingRun) || ctrlDown;

            // Pause: fires on MouseButtonDown(1) (right click), or Escape
            state.pausePressed = UnityEngine.Input.GetMouseButtonDown(pauseMouseButton) || UnityEngine.Input.GetKeyDown(pauseKeyAlt);

            return state;
        }
    }
}
#endif