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

        // Default bindings as per PRD §7.1:
        // Left/Right: Arrow keys
        // Jump: Z or Space
        // Run: X or Left Shift
        // Crouch: Down Arrow
        // Shoot: C or Mouse0
        // Pause: Escape or P

        // TODO: Replace with SerializedDictionary&lt;KeyCode, ...&gt; for future rebinding
        [Header("Key Bindings (change in Inspector for prototyping)")]
        [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode jumpKey1 = KeyCode.Z;
        [SerializeField] private KeyCode jumpKey2 = KeyCode.Space;
        [SerializeField] private KeyCode runKey1 = KeyCode.X;
        [SerializeField] private KeyCode runKey2 = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouchKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode shootKey1 = KeyCode.C;
        [SerializeField] private int shootMouseButton = 0; // left click
        [SerializeField] private KeyCode pauseKey1 = KeyCode.Escape;
        [SerializeField] private KeyCode pauseKey2 = KeyCode.P;

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
            float left = UnityEngine.Input.GetKey(leftKey) ? -1f : 0f;
            float right = UnityEngine.Input.GetKey(rightKey) ? 1f : 0f;
            state.moveAxis = left + right;
            if (state.moveAxis != 0)
                state.moveAxis = Mathf.Clamp(state.moveAxis, -1f, 1f);

            // Jump
            state.jumpPressed = UnityEngine.Input.GetKeyDown(jumpKey1) || UnityEngine.Input.GetKeyDown(jumpKey2);
            state.jumpHeld = UnityEngine.Input.GetKey(jumpKey1) || UnityEngine.Input.GetKey(jumpKey2);

            // Run
            state.runHeld = UnityEngine.Input.GetKey(runKey1) || UnityEngine.Input.GetKey(runKey2);

            // Crouch
            state.crouch = UnityEngine.Input.GetKey(crouchKey);

            // Shoot
            state.shootPressed = UnityEngine.Input.GetKeyDown(shootKey1) || UnityEngine.Input.GetMouseButtonDown(shootMouseButton);

            // Pause
            state.pausePressed = UnityEngine.Input.GetKeyDown(pauseKey1) || UnityEngine.Input.GetKeyDown(pauseKey2);

            return state;
        }
    }
}
#endif