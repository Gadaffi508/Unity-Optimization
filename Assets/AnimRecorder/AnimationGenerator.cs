using UnityEngine;
using System.Collections.Generic;

public static class AnimationGenerator
{
    public static AnimationClip CreateClip(List<Vector3> posList, List<Quaternion> rotList, float frameRate = 60f)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;

        AnimationCurve curvePX = new AnimationCurve();
        AnimationCurve curvePY = new AnimationCurve();
        AnimationCurve curvePZ = new AnimationCurve();

        for (int i = 0; i < posList.Count; i++)
        {
            float time = i / frameRate;
            curvePX.AddKey(time, posList[i].x);
            curvePY.AddKey(time, posList[i].y);
            curvePZ.AddKey(time, posList[i].z);
        }

        clip.SetCurve("", typeof(Transform), "localPosition.x", curvePX);
        clip.SetCurve("", typeof(Transform), "localPosition.y", curvePY);
        clip.SetCurve("", typeof(Transform), "localPosition.z", curvePZ);

        return clip;
    }
}