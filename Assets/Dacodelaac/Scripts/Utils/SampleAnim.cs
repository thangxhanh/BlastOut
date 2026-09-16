using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAnim : MonoBehaviour
{
    [SerializeField] private AnimationClip clip;

    /* SampleAnimEditor nằm ở assembly khác nên cần property public để đọc. */
    public AnimationClip Clip => clip;

    void Awake()
    {
        Destroy(this);
    }
}
