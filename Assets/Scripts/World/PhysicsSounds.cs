using UnityEngine;

namespace Major.World {
    public class PhysicsSounds : MonoBehaviour {
        [SerializeField] private AudioClip[] audioClips;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Vector2 linearDeltaRange = new(5.0f, 10.0f);
        [SerializeField] private Vector2 angularDeltaRange = new(5.0f, 10.0f);
        [SerializeField] private float minVolume = 0.01f;
        [SerializeField] private float maxVolume = 1.0f;
        private Vector3 prevFrameLinVel;
        private Vector3 prevFrameAngVel;

        private void Awake() {
            if (audioClips.Length == 0 || !rb) {
                Log2.Warning("Physics sounds requires a rigidbody and clips", "PhysicsSounds");
                Destroy(this);
            }
        }

        private void FixedUpdate() {
            var linDelta = Mathf.Abs(prevFrameLinVel.magnitude - rb.linearVelocity.magnitude);
            if (linDelta > linearDeltaRange.x) {
                AudioSource.PlayClipAtPoint(
                    audioClips[Random.Range(0, audioClips.Length)],
                    rb.position,
                    Mathf.Lerp(minVolume, maxVolume, Mathf.InverseLerp(linearDeltaRange.x, linearDeltaRange.y, linDelta))
                );
            }

            var angDelta = Mathf.Abs(prevFrameAngVel.magnitude - rb.angularVelocity.magnitude);
            if (angDelta > angularDeltaRange.x) {
                AudioSource.PlayClipAtPoint(
                    audioClips[Random.Range(0, audioClips.Length)],
                    rb.position,
                    Mathf.Lerp(minVolume, maxVolume, Mathf.InverseLerp(angularDeltaRange.x, angularDeltaRange.y, angDelta))
                );
            }

            prevFrameLinVel = rb.linearVelocity;
            prevFrameAngVel = rb.angularVelocity;
        }
    }
}