using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    static float s_duration = 0f;

    static float s_amount;

    static Vector3 s_originalPos;

    static Transform s_cameraTransform;

    void OnEnable()
    {
        s_cameraTransform = transform;

        s_originalPos = s_cameraTransform.localPosition;
    }

    void Update()
    {
        if (s_duration > 0)
        {
            //if (Time.frameCount % 1 == 0)
            {
                s_cameraTransform.localPosition = s_originalPos + Random.insideUnitSphere * s_amount;

                s_duration -= Time.deltaTime;
            }
        }
        else
        {
            s_cameraTransform.localPosition = s_originalPos;
        }
    }

    public static void StartCameraShake(float a_duration, float a_amount)
    {
        if (s_duration > 0f && a_amount < s_amount - 0.4f)
            return;

        s_duration       = a_duration;
        s_amount         = a_amount;
    }
}