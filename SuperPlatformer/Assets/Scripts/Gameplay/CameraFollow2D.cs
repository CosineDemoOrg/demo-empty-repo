using UnityEngine;
using Gameplay.Player;

namespace SuperPlatformer.Gameplay
{
    /// <summary>
    /// Smooth camera follow with deadzone and lookahead.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.15f;
        public float deadZoneWidth = 4f, deadZoneHeight = 4f;
        public float lookAheadDist = 2f;
        public float tileSize = 1f;

        private Vector3 _currentVelocity;
        private Vector3 _lookAhead;

        private PlayerController player;

        void Start()
        {
            if (target != null)
                player = target.GetComponent<PlayerController>();
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 camPos = transform.position;
            Vector3 tgtPos = target.position;

            // Dead zone logic
            float dzW = deadZoneWidth * tileSize * 0.5f;
            float dzH = deadZoneHeight * tileSize * 0.5f;

            Vector3 delta = tgtPos - camPos;
            Vector3 offset = Vector3.zero;
            if (Mathf.Abs(delta.x) > dzW)
                offset.x = delta.x - Mathf.Sign(delta.x) * dzW;
            if (Mathf.Abs(delta.y) > dzH)
                offset.y = delta.y - Mathf.Sign(delta.y) * dzH;

            // Lookahead (horizontal velocity only)
            if (player != null && Mathf.Abs(player.Velocity.x) > 0.01f)
                _lookAhead.x = lookAheadDist * tileSize * Mathf.Sign(player.Velocity.x);
            else
                _lookAhead.x = Mathf.MoveTowards(_lookAhead.x, 0, Time.deltaTime * tileSize * 8);

            Vector3 goal = camPos + offset + _lookAhead;
            goal.z = camPos.z; // Don't modify Z

            transform.position = Vector3.SmoothDamp(camPos, goal, ref _currentVelocity, smoothTime);
        }
    }
}