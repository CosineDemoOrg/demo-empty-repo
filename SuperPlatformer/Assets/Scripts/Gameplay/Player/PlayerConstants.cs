namespace Gameplay.Player
{
    /// &lt;summary&gt;
    /// Movement constants for the player, as defined in PRD §6.2.
    /// All units are specified as in the PRD (tiles/s, seconds, etc).
    /// &lt;/summary&gt;
    public static class PlayerConstants
    {
        /// &lt;summary&gt;
        /// WALK_SPEED = 4.0 tiles/sec (PRD §6.2)
        /// &lt;/summary&gt;
        public static readonly float WALK_SPEED = 4.0f;

        /// &lt;summary&gt;
        /// RUN_SPEED = 6.5 tiles/sec (PRD §6.2)
        /// &lt;/summary&gt;
        public static readonly float RUN_SPEED = 6.5f;

        /// &lt;summary&gt;
        /// RUN_ACCEL_TIME = 0.18 sec to reach full run speed (PRD §6.2)
        /// &lt;/summary&gt;
        public static readonly float RUN_ACCEL_TIME = 0.18f;

        /// &lt;summary&gt;
        /// GRAVITY = 30.0 tiles/sec² downward (PRD §6.2)
        /// &lt;/summary&gt;
        public static readonly float GRAVITY = 30.0f;

        /// &lt;summary&gt;
        /// JUMP_INITIAL_VELOCITY = 10.0 tiles/sec upward (PRD §6.2)
        /// &lt;/summary&gt;
        public static readonly float JUMP_INITIAL_VELOCITY = 10.0f;

        /// &lt;summary&gt;
        /// COYOTE_TIME = 0.08 sec grace window after leaving ground (PRD §6.2)
        /// &lt;/summary&gt;
        public static readonly float COYOTE_TIME = 0.08f;

        /// &lt;summary&gt;
        /// JUMP_BUFFER_TIME = 0.11 sec input buffer for jump (PRD §6.2)
        /// &lt;/summary&gt;
        public static readonly float JUMP_BUFFER_TIME = 0.11f;

        /// &lt;summary&gt;
        /// TILE_SIZE = 1.0 Unity units per tile (PRD §2.1)
        /// &lt;/summary&gt;
        public static readonly float TILE_SIZE = 1.0f;
    }
}