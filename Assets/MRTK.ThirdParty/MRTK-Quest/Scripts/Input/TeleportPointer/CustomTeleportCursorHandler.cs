//------------------------------------------------------------------------------ -
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


using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Teleport;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace prvncher.MixedReality.Toolkit.Input.Teleport
{
    /// <summary>
    /// Custom teleport cursor built for MRTK-Quest
    /// </summary>
    public class CustomTeleportCursorHandler : MonoBehaviour, IMixedRealityTeleportHandler
    {
        private Vector3 cursorOrientation = Vector3.zero;

        [SerializeField]
        [Header("Animated Cursor State Data")]
        [Tooltip("Cursor state data to use for its various states.")]
        private AnimatedCursorStateData[] cursorStateData = null;

        [SerializeField]
        [Tooltip("Animator for the cursor")]
        private Animator cursorAnimator = null;

        [SerializeField]
        private CustomTeleportPointer pointer;

        [Header("Transform References")]
        [SerializeField]
        [Tooltip("Visual that is displayed when cursor is active normally")]
        protected Transform PrimaryCursorVisual = null;

        [SerializeField]
        [Tooltip("Arrow Transform to point in the Teleporting direction.")]
        private Transform arrowTransform = null;

        private List<Renderer> renderers;

        #region IMixedRealityCursor Implementation

        /// <inheritdoc />
        public Vector3 Position => PrimaryCursorVisual.position;

        /// <inheritdoc />
        public Quaternion Rotation => arrowTransform.rotation;

        /// <inheritdoc />
        public Vector3 LocalScale => PrimaryCursorVisual.localScale;

        public CursorStateEnum CursorState { get; private set; } = CursorStateEnum.None;

        /// <inheritdoc />
        public CursorStateEnum CheckCursorState()
        {
            if (CursorState != CursorStateEnum.Contextual)
            {
                if (pointer.IsInteractionEnabled)
                {
                    switch (pointer.TeleportSurfaceResult)
                    {
                        case TeleportSurfaceResult.None:
                            return CursorStateEnum.Release;
                        case TeleportSurfaceResult.Invalid:
                            return CursorStateEnum.ObserveHover;
                        case TeleportSurfaceResult.HotSpot:
                        case TeleportSurfaceResult.Valid:
                            return CursorStateEnum.ObserveHover;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return CursorStateEnum.Release;
            }
            return CursorStateEnum.Contextual;
        }

        private bool CanUpdateCursor => pointer.IsActive && pointer.IsInteractionEnabled && pointer.Result != null;

        private void Start()
        {
            renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        }

        private void Update()
        {
            if (!CanUpdateCursor)
            {
                SetRenderersActive(false);
                return;
            }
            SetRenderersActive(true);
            UpdateCursorState();
            UpdateCursorTransform();
        }

        private void SetRenderersActive(bool isActive)
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = isActive;
            }
        }

        /// <summary>
        /// Internal update to check for cursor state changes
        /// </summary>
        private void UpdateCursorState()
        {
            CursorStateEnum newState = CheckCursorState();
            if (CursorState != newState)
            {
                OnCursorStateChange(newState);
            }
        }

        /// <inheritdoc />
        protected void UpdateCursorTransform()
        {
            transform.position = pointer.Result.Details.Point;

            Vector3 forward = CameraCache.Main.transform.forward;
            forward.y = 0f;

            // Smooth out rotation just a tad to prevent jarring transitions
            PrimaryCursorVisual.rotation = Quaternion.Lerp(PrimaryCursorVisual.rotation, Quaternion.LookRotation(forward.normalized, Vector3.up), 0.5f);

            // Point the arrow towards the target orientation
            cursorOrientation.y = pointer.PointerOrientation;
            arrowTransform.eulerAngles = cursorOrientation;
        }

        /// <summary>
        /// Override OnCursorState change to set the correct animation state for the cursor.
        /// </summary>
        public void OnCursorStateChange(CursorStateEnum state)
        {
            CursorState = state;
            for (int i = 0; i < cursorStateData.Length; i++)
            {
                if (cursorStateData[i].CursorState == state)
                {
                    SetAnimatorParameter(cursorStateData[i].Parameter);
                }
            }
        }

        /// <summary>
        /// Based on the type of animator state info pass it through to the animator
        /// </summary>
        private void SetAnimatorParameter(AnimatorParameter animationParameter)
        {
            // Return if we do not have an animator
            if (cursorAnimator == null || !cursorAnimator.isInitialized)
            {
                return;
            }

            switch (animationParameter.ParameterType)
            {
                case AnimatorControllerParameterType.Bool:
                    cursorAnimator.SetBool(animationParameter.NameHash, animationParameter.DefaultBool);
                    break;
                case AnimatorControllerParameterType.Float:
                    cursorAnimator.SetFloat(animationParameter.NameHash, animationParameter.DefaultFloat);
                    break;
                case AnimatorControllerParameterType.Int:
                    cursorAnimator.SetInteger(animationParameter.NameHash, animationParameter.DefaultInt);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    cursorAnimator.SetTrigger(animationParameter.NameHash);
                    break;
            }
        }

        #endregion IMixedRealityCursor Implementation

        #region IMixedRealityTeleportHandler Implementation

        /// <inheritdoc />
        public void OnTeleportRequest(TeleportEventData eventData)
        {
            OnCursorStateChange(CursorStateEnum.Observe);
        }

        /// <inheritdoc />
        public void OnTeleportStarted(TeleportEventData eventData)
        {
            OnCursorStateChange(CursorStateEnum.Release);
        }

        /// <inheritdoc />
        public void OnTeleportCompleted(TeleportEventData eventData) { }

        /// <inheritdoc />
        public void OnTeleportCanceled(TeleportEventData eventData)
        {
            OnCursorStateChange(CursorStateEnum.Release);
        }
        #endregion IMixedRealityTeleportHandler Implementation
    }
}
