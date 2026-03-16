using UnityEngine;

public class RoleAnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private AudioClip landingAudioClip;
    [SerializeField] private AudioClip[] footstepAudioClips;
    [SerializeField][Range(0f, 1f)] private float footstepAudioVolume = 0.5f;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponentInParent<CharacterController>();
    }

    public void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f)
            return;

        if (footstepAudioClips == null || footstepAudioClips.Length == 0)
            return;

        int index = Random.Range(0, footstepAudioClips.Length);
        Vector3 playPos = transform.position;

        if (characterController != null)
        {
            playPos = transform.TransformPoint(characterController.center);
        }

        AudioSource.PlayClipAtPoint(footstepAudioClips[index], playPos, footstepAudioVolume);
    }

    public void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f)
            return;

        if (landingAudioClip == null)
            return;

        Vector3 playPos = transform.position;

        if (characterController != null)
        {
            playPos = transform.TransformPoint(characterController.center);
        }

        AudioSource.PlayClipAtPoint(landingAudioClip, playPos, footstepAudioVolume);
    }
}
