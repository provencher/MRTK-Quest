﻿//------------------------------------------------------------------------------ -
//MRTK - Quest
//https ://github.com/provencher/MRTK-Quest
//------------------------------------------------------------------------------ -
//
//MIT License
//
//Copyright(c) 2020 Eric Provencher
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files(the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions :
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//------------------------------------------------------------------------------ -

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace prvncher.MixedReality.Toolkit.Utils
{
    public static class HandPoseUtils
    {
        /// <summary>
        /// Returns true if index finger tip is closer to wrist than index knuckle joint.
        /// </summary>
        /// <param name="hand">Hand to query joint pose against.</param>
        /// <returns></returns>
        public static bool IsIndexGrabbing(Handedness hand)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, hand, out var wristPose) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, hand, out var indexTipPose) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, hand, out var indexKnucklePose))
            {
                // compare wrist-knuckle to wrist-tip
                Vector3 wristToIndexTip = indexTipPose.Position - wristPose.Position;
                Vector3 wristToIndexKnuckle = indexKnucklePose.Position - wristPose.Position;
                return wristToIndexKnuckle.sqrMagnitude >= wristToIndexTip.sqrMagnitude;
            }
            return false;
        }

        /// <summary>
        /// Returns curl of ranging from 0 to 1. 1 if index finger curled/closer to wrist. 0 if the finger is not curled.
        /// </summary>
        /// <param name="handedness">Handedness to query joint pose against.</param>
        /// <returns> Float ranging from 0 to 1. 0 if index finger is straight/not curled, 1 if index finger is curled</returns>
        public static float IndexCurl(Handedness handedness)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, handedness, out var fingerTipPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, handedness, out var fingerKnucklePose))
            {

                return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
            }
            return 0.0f;
        }

        /// <summary>
        /// Returns true if middle finger tip is closer to wrist than middle knuckle joint.
        /// </summary>
        /// <param name="hand">Hand to query joint pose against.</param>
        /// <returns></returns>
        public static bool IsMiddleGrabbing(Handedness hand)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, hand, out var wristPose) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, hand, out var indexTipPose) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, hand, out var indexKnucklePose))
            {
                // compare wrist-knuckle to wrist-tip
                Vector3 wristToIndexTip = indexTipPose.Position - wristPose.Position;
                Vector3 wristToIndexKnuckle = indexKnucklePose.Position - wristPose.Position;
                return wristToIndexKnuckle.sqrMagnitude >= wristToIndexTip.sqrMagnitude;
            }
            return false;
        }

        /// <summary>
        /// Returns curl of  middle finger ranging from 0 to 1. 1 if index finger curled/closer to wrist. 0 if the finger is not curled.
        /// </summary>
        /// <param name="handedness">Handedness to query joint pose against.</param>
        /// <returns> Float ranging from 0 to 1. 0 if middle finger is straight/not curled, 1 if middle finger is curled</returns>
        public static float MiddleCurl(Handedness handedness)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, handedness, out var fingerTipPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, handedness, out var fingerKnucklePose))
            {

                return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
            }
            return 0.0f;
        }

        /// <summary>
        /// Returns curl of ring finger ranging from 0 to 1. 1 if ring finger curled/closer to wrist. 0 if the finger is not curled.
        /// </summary>
        /// <param name="handedness">Handedness to query joint pose against.</param>
        /// <returns> Float ranging from 0 to 1. 0 if ring finger is straight/not curled, 1 if ring finger is curled</returns>
        public static float RingCurl(Handedness handedness)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, handedness, out var fingerTipPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.RingKnuckle, handedness, out var fingerKnucklePose))
            {

                return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
            }
            return 0.0f;
        }

        /// <summary>
        /// Returns curl of pinky finger ranging from 0 to 1. 1 if pinky finger curled/closer to wrist. 0 if the finger is not curled.
        /// </summary>
        /// <param name="handedness">Handedness to query joint pose against.</param>
        /// <returns> Float ranging from 0 to 1. 0 if pinky finger is straight/not curled, 1 if pinky finger is curled</returns>
        public static float PinkyCurl(Handedness handedness)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, handedness, out var fingerTipPose) &&
                           HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, handedness, out var fingerKnucklePose))
            {

                return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
            }
            return 0.0f;
        }

        /// <summary>
        /// Returns true if middle thumb tip is closer to pinky knuckle than thumb knuckle joint.
        /// </summary>
        /// <param name="hand">Hand to query joint pose against.</param>
        /// <returns></returns>
        public static bool IsThumbGrabbing(Handedness hand)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, hand, out var pinkyKnucklePose) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, hand, out var thumbTipPose) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, hand, out var thumbKnucklePose))
            {
                // compare pinkyKnuckle-ThumbKnuckle to pinkyKnuckle-ThumbTip
                Vector3 pinkyKnuckleToThumbTip = thumbTipPose.Position - pinkyKnucklePose.Position;
                Vector3 pinkyKnuckleToThumbKnuckle = thumbKnucklePose.Position - pinkyKnucklePose.Position;
                return pinkyKnuckleToThumbKnuckle.sqrMagnitude >= pinkyKnuckleToThumbTip.sqrMagnitude;
            }
            return false;
        }



        // Curl calculation of a finger based on the angle made by vectors wristToFingerKuncle and fingerKuckleToFingerTip.
        static private float CalculateCurl(Vector3 wristJoint, Vector3 fingerKnuckleJoint, Vector3 fingerTipJoint)
        {
            var palmToFinger = (fingerKnuckleJoint - wristJoint).normalized;
            var fingerKnuckleToTip = (fingerKnuckleJoint - fingerTipJoint).normalized;

            var curl = Vector3.Dot(fingerKnuckleToTip, palmToFinger);

            //Redefining the range from [-1,1] to [0,1]
            curl = (curl + 1) / 2.0f;
            return curl;

        }
    }
}