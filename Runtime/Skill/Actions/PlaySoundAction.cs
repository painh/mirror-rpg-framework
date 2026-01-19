using UnityEngine;

namespace MirrorRPG.Skill.Actions
{
    /// <summary>
    /// Action that plays a sound at a specific time
    /// </summary>
    [System.Serializable]
    public class PlaySoundAction : SkillAction
    {
        [Header("Sound Settings")]
        [Tooltip("Audio clip to play")]
        public AudioClip audioClip;

        [Tooltip("Volume")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("Pitch")]
        [Range(0.5f, 2f)]
        public float pitch = 1f;

        [Header("Spatial Settings")]
        [Tooltip("Play as 3D sound at owner position")]
        public bool is3DSound = true;

        [Tooltip("Minimum distance for 3D sound")]
        public float minDistance = 1f;

        [Tooltip("Maximum distance for 3D sound")]
        public float maxDistance = 50f;

        public override void Execute(SkillActionContext context)
        {
            if (audioClip == null) return;

            if (is3DSound)
            {
                PlayAt(context.Owner.transform.position);
            }
            else
            {
                Play2D();
            }
        }

        private void PlayAt(Vector3 position)
        {
            // Create temporary audio source
            var audioGO = new GameObject("SkillSound_" + audioClip.name);
            audioGO.transform.position = position;

            var audioSource = audioGO.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.spatialBlend = 1f; // Full 3D
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.Play();

            // Destroy after clip finishes
            Object.Destroy(audioGO, audioClip.length / pitch + 0.1f);
        }

        private void Play2D()
        {
            // Create temporary audio source for 2D sound
            var audioGO = new GameObject("SkillSound2D_" + audioClip.name);

            var audioSource = audioGO.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.spatialBlend = 0f; // Full 2D
            audioSource.Play();

            Object.Destroy(audioGO, audioClip.length / pitch + 0.1f);
        }
    }
}
