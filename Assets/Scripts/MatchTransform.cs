using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MatchTransform : MonoBehaviour
{
    public Transform target;

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            transform.localPosition = target.localPosition;
            transform.localRotation = target.localRotation;
        }
    }
}
