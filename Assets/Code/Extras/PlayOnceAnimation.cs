using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnceAnimation : MonoBehaviour
{
    public Animation animation;
    public AnimationClip clip;

    public void PlayOnce()
    {
        animation.clip = clip;
        animation.Play();
    }

    public float GetClipDuration()
    {
        return animation.clip.length;
    }
}
