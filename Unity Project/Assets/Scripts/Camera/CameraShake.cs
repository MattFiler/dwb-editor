using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoSingleton<CameraShake>
{
    public enum Type
    { 
        POS = 0,
        ROT = 1
    }

    public enum Axis
    {
        ALL = 0,
        X = 1,
        Y = 2,
        Z = 3,
        XY = 4,
        XZ = 5,
        YZ = 6
    }

    /* Used to determine which type of shake is required, a Positional or Rotational shake */
    Type shakeType = Type.POS;

    /* Used to determine which axis to shake the camera on */
    Axis axis = Axis.ALL;

    /* Used for how much the camera shakes by */
    float magnitude = 0.0f;

    /* Used for how long the camera shakes for */
    float duration = 0.5f;

    /* Used for resetting the camera to original position after teh shaking ends */
    Vector3 startPos = new Vector3();

    /* Used for resetting the camera to original rotation after the shaking ends */
    Quaternion startRot = new Quaternion();

    /* Used for to check if the camera is already shaking incase the 'Shake' function gets called again  */
    bool shaking = false;

    IEnumerator Shake()
    {
        while(duration > 0.0f)
        {
            switch (shakeType)
            {
                case Type.POS:
                    {
                        float x = axis == Axis.X || axis == Axis.ALL || axis == Axis.XY || axis == Axis.XZ ? 
                            startPos.x + Random.Range(-1f, 1f) * magnitude : startPos.x;
                        float y = axis == Axis.Y || axis == Axis.ALL || axis == Axis.XY || axis == Axis.YZ ?
                            startPos.y + Random.Range(-1f, 1f) * magnitude : startPos.y;

                        transform.localPosition = new Vector3(x, y, startPos.z);
                        break;
                    }
                case Type.ROT:
                    {
                        float x = axis == Axis.X || axis == Axis.ALL || axis == Axis.XY || axis == Axis.XZ ?
                            startRot.x + Random.Range(-0.01f, 0.01f) * magnitude : startRot.x;
                        float y = axis == Axis.Y || axis == Axis.ALL || axis == Axis.XY || axis == Axis.YZ ?
                            startRot.y + Random.Range(-0.01f, 0.01f) * magnitude : startRot.y;
                        float z = axis == Axis.Z || axis == Axis.ALL || axis == Axis.XZ || axis == Axis.YZ ?
                            startRot.z + Random.Range(-0.01f, 0.01f) * magnitude : startRot.z;

                        transform.localRotation = new Quaternion(x, y, z, startRot.w);
                        break;
                    }
            }

            duration -= Time.deltaTime;
            yield return null;
        }

        transform.localPosition = startPos;
        transform.localRotation = startRot;
        shaking = false;
    }



    /* The function is used to set the camera to shake. Pass in 3 or 4 parameters. 
     * 
     * ShakeType : Which type of shake is required position or rotation.
     * Axis : Which axis you want to shake on X, Y, Z, X+Y, X+Z, Y+Z or ALL.
     * Mag : Is how much you want it to shake by.
     * Dur : Is how long it should shake for (this is set to default 0.25 if you don't pass anything in).
     * 
     *  Example : 'CameraShake.Instance.Shake(CameraShake.ShakeType.POS, CameraShake.Axis.X, 5.0f, 0.5f);'
     */
    public void Shake(Type type, Axis ax,  float mag, float dur = 0.25f)
    {
        if (shaking)
            return;

        shakeType = type;
        magnitude = mag / 10.0f;
        duration = dur;
        axis = ax;
        shaking = true;

        startPos = transform.localPosition;
        startRot = transform.localRotation;

        StartCoroutine(Shake());
    }


}
