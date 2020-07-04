using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using prvncher.MixedReality.Toolkit.Utils;
using Microsoft.MixedReality.Toolkit.Utilities;

public class FingerCurlExample : MonoBehaviour
{
    public Handedness handedness;

    public GameObject indexFinger;
    public GameObject middleFinger;
    public GameObject ringFinger;
    public GameObject pinkyFinger;



    // Update is called once per frame
    void Update()
    {
        var fingerCurl = HandPoseUtils.IndexCurl(handedness);
        var scale = new Vector3(1.0f, 1.0f - fingerCurl, 1.0f);
        indexFinger.transform.localScale = scale;

        fingerCurl = HandPoseUtils.MiddleCurl(handedness);
        scale = new Vector3(1.0f, 1.0f - fingerCurl, 1.0f);
        middleFinger.transform.localScale = scale;

        fingerCurl = HandPoseUtils.RingCurl(handedness);
        scale = new Vector3(1.0f, 1.0f - fingerCurl, 1.0f);
        ringFinger.transform.localScale = scale;

        fingerCurl = HandPoseUtils.PinkyCurl(handedness);
        scale = new Vector3(1.0f, 1.0f - fingerCurl, 1.0f);
        pinkyFinger.transform.localScale = scale;
    }
}
