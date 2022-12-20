using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotateItem : MonoBehaviour
{
    public float Speed;

    // Update is called once per frame
    void Update()
    {
        float angle = transform.rotation.eulerAngles.y;

        angle = angle + Speed * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0, angle, 0);
    }
}
