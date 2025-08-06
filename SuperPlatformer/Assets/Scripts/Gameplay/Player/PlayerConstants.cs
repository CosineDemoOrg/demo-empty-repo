namespace Gameplay.Player
{
    /// &lt;summary&gt;
    /// Movement constants for the player, as defined in PRD §6.2.
    /// All units are specified as in the PRD (tiles/s, seconds, etc).
    /// &lt;/summary&gt;
    public static class PlayerConstants
    {
        /// <summary>
        /// WALK_SPEED = 2.5 tiles/sec (PRD §6.2)
        /// </summary>
        public static readonly float WALK_SPEED = 2.5f;

        /// <summary>
        /// RUN_SPEED = 4.0 tiles/sec (PRD §6.2)
        /// </summary>
        public static readonly float RUN_SPEED = 4.0f;

        /// <summary>
        /// RUN_ACCEL_TIME = 0.5 sec to reach full run speed (PRD §6.2, derived)
        /// </summary>
        public static readonly float RUN_ACCEL_TIME = 0.5f;

        /// <summary>
        /// GRAVITY = 30.0 tiles/sec² downward (PRD §6.2)
        /// </summary>
        public static readonly float GRAVITY = 30.0f;

        /// <summary>
        /// JUMP_INITIAL_VELOCITY = 8.5 tiles/sec upward (PRD §6.2)
        /// </summary>
        public static readonly float JUMP_INITIAL_VELOCITY = 8.5f;

        /// <summary>
        /// COYOTE_TIME = 0.10 sec grace window after leaving ground (PRD §6.2)
        /// </summary>
        public static readonly float COYOTE_TIME = 0.10f;

        /// <summary>
        /// JUMP_BUFFER_TIME = 0.08 sec input buffer for jump (PRD §6.2)
        /// </summary>
        public static readonly float JUMP_BUFFER_TIME = 0.08f;

        /// <summary>
        /// TILE_SIZE = 1.0 Unity units per tile (16 × 16 px tile size, PRD §2.1)
        /// </summary>
        public static readonly float TILE_SIZE = 1.0f;
    }
}