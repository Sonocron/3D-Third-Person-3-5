using UnityEngine;

public class PlayerStepSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 0.2f;
    [SerializeField] private Vector3 raycastPosition;

    [SerializeField] private AudioClip[] stoneWalkSounds;
    [SerializeField] private AudioClip[] grassWalkSounds;
    [SerializeField] private AudioClip[] defaultWalkSounds;
    [SerializeField] private AudioClip[] stoneRunSounds;
    [SerializeField] private AudioClip[] grassRunSounds;
    [SerializeField] private AudioClip[] defaultRunSounds;
    [SerializeField] private AudioClip[] stoneLandSounds;
    [SerializeField] private AudioClip[] grassLandSounds;
    [SerializeField] private AudioClip[] defaultLandSounds;
    public void PlayWalkSound()
    {
        Ray ray = new Ray(transform.position + raycastPosition, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            string groundTag = hit.collider.tag;

            switch (groundTag)
            {
                case "Grass":
                    PlayRandomSound(grassWalkSounds);
                    break;
                
                case "Stone":
                    PlayRandomSound(stoneWalkSounds);
                    break;
                
                default:
                    PlayRandomSound(defaultWalkSounds);
                    break;
            }
        }
    }
    public void PlayRunSound()
    {
        Ray ray = new Ray(transform.position + raycastPosition, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            string groundTag = hit.collider.tag;

            switch (groundTag)
            {
                case "Grass":
                    PlayRandomSound(grassRunSounds);
                    break;
                
                case "Stone":
                    PlayRandomSound(stoneRunSounds);
                    break;
                
                default:
                    PlayRandomSound(defaultRunSounds);
                    break;
            }
        }
    }
    public void PlayLandSound()
    {
        Ray ray = new Ray(transform.position + raycastPosition, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            string groundTag = hit.collider.tag;

            switch (groundTag)
            {
                case "Grass":
                    PlayRandomSound(grassLandSounds);
                    break;
                
                case "Stone":
                    PlayRandomSound(stoneLandSounds);
                    break;
                
                default:
                    PlayRandomSound(defaultLandSounds);
                    break;
            }
        }
    }
    void PlayRandomSound(AudioClip[] audioClips)
    {
        int index = Random.Range(0, audioClips.Length);
        audioSource.PlayOneShot(audioClips[index]);
    }
}