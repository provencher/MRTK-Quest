/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus Master SDK License Version 1.0 (the "License"); you may not use
the Utilities SDK except in compliance with the License, which is provided at the time of installation
or download, or which otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at
https://developer.oculus.com/licenses/oculusmastersdk-1.0/

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(OVRManager))]
public class OVRManagerEditor : Editor
{
	override public void OnInspectorGUI()
	{
#if UNITY_ANDROID
		OVRProjectConfig projectConfig = OVRProjectConfig.GetProjectConfig();
        OVRProjectConfigEditor.DrawTargetDeviceInspector(projectConfig);

        EditorGUILayout.Space();
#endif

		DrawDefaultInspector();

		bool modified = false;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID
		OVRManager manager = (OVRManager)target;

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);
		OVREditorUtil.SetupBoolField(target, new GUIContent("Enable Specific Color Gamut",
			"If checked, the target HMD will perform a color space transformation"), ref manager.enableColorGamut, ref modified);

		if (manager.enableColorGamut)
		{
			OVREditorUtil.SetupEnumField(target, new GUIContent("Color Gamut",
			"The target color gamut when displayed on the HMD"), ref manager.colorGamut, ref modified);
		}
#endif

#if UNITY_ANDROID
		EditorGUILayout.Space();
        OVRProjectConfigEditor.DrawProjectConfigInspector(projectConfig);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Mixed Reality Capture for Quest (experimental)", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		OVREditorUtil.SetupEnumField(target, "ActivationMode", ref manager.mrcActivationMode, ref modified);
		EditorGUI.indentLevel--;
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Mixed Reality Capture", EditorStyles.boldLabel);
		OVREditorUtil.SetupBoolField(target, "Show Properties", ref manager.expandMixedRealityCapturePropertySheet, ref modified);
		if (manager.expandMixedRealityCapturePropertySheet)
		{
			string[] layerMaskOptions = new string[32];
			for (int i=0; i<32; ++i)
			{
				layerMaskOptions[i] = LayerMask.LayerToName(i);
				if (layerMaskOptions[i].Length == 0)
				{
					layerMaskOptions[i] = "<Layer " + i.ToString() + ">";
				}
			}

			EditorGUI.indentLevel++;

			EditorGUILayout.Space();
			OVREditorUtil.SetupBoolField(target, "enableMixedReality", ref manager.enableMixedReality, ref modified);
			OVREditorUtil.SetupEnumField(target, "compositionMethod", ref manager.compositionMethod, ref modified);
			OVREditorUtil.SetupLayerMaskField(target, "extraHiddenLayers", ref manager.extraHiddenLayers, layerMaskOptions, ref modified);

			if (manager.compositionMethod == OVRManager.CompositionMethod.External)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("External Composition", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				OVREditorUtil.SetupColorField(target, "backdropColor (target, Rift)", ref manager.externalCompositionBackdropColorRift, ref modified);
				OVREditorUtil.SetupColorField(target, "backdropColor (target, Quest)", ref manager.externalCompositionBackdropColorQuest, ref modified);
			}

			if (manager.compositionMethod == OVRManager.CompositionMethod.Direct)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Direct Composition", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
				OVREditorUtil.SetupEnumField(target, "capturingCameraDevice", ref manager.capturingCameraDevice, ref modified);
				OVREditorUtil.SetupBoolField(target, "flipCameraFrameHorizontally", ref manager.flipCameraFrameHorizontally, ref modified);
				OVREditorUtil.SetupBoolField(target, "flipCameraFrameVertically", ref manager.flipCameraFrameVertically, ref modified);

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Chroma Key", EditorStyles.boldLabel);
				OVREditorUtil.SetupColorField(target, "chromaKeyColor", ref manager.chromaKeyColor, ref modified);
				OVREditorUtil.SetupFloatField(target, "chromaKeySimilarity", ref manager.chromaKeySimilarity, ref modified);
				OVREditorUtil.SetupFloatField(target, "chromaKeySmoothRange", ref manager.chromaKeySmoothRange, ref modified);
				OVREditorUtil.SetupFloatField(target, "chromaKeySpillRange", ref manager.chromaKeySpillRange, ref modified);

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Dynamic Lighting", EditorStyles.boldLabel);
				OVREditorUtil.SetupBoolField(target, "useDynamicLighting", ref manager.useDynamicLighting, ref modified);
				OVREditorUtil.SetupEnumField(target, "depthQuality", ref manager.depthQuality, ref modified);
				OVREditorUtil.SetupFloatField(target, "dynamicLightingSmoothFactor", ref manager.dynamicLightingSmoothFactor, ref modified);
				OVREditorUtil.SetupFloatField(target, "dynamicLightingDepthVariationClampingValue", ref manager.dynamicLightingDepthVariationClampingValue, ref modified);

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Virtual Green Screen", EditorStyles.boldLabel);
				OVREditorUtil.SetupEnumField(target, "virtualGreenScreenType", ref manager.virtualGreenScreenType, ref modified);
				OVREditorUtil.SetupFloatField(target, "virtualGreenScreenTopY", ref manager.virtualGreenScreenTopY, ref modified);
				OVREditorUtil.SetupFloatField(target, "virtualGreenScreenBottomY", ref manager.virtualGreenScreenBottomY, ref modified);
				OVREditorUtil.SetupBoolField(target, "virtualGreenScreenApplyDepthCulling", ref manager.virtualGreenScreenApplyDepthCulling, ref modified);
				OVREditorUtil.SetupFloatField(target, "virtualGreenScreenDepthTolerance", ref manager.virtualGreenScreenDepthTolerance, ref modified);

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Latency Control", EditorStyles.boldLabel);
				OVREditorUtil.SetupFloatField(target, "handPoseStateLatency", ref manager.handPoseStateLatency, ref modified);
				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;
		}
#endif
        if (modified)
        {
            EditorUtility.SetDirty(target);
        }
	}

}
