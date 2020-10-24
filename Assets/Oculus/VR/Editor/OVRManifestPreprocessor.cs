/************************************************************************************

Copyright   :   Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus SDK License Version 3.4.1 (the "License");
you may not use the Oculus SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;

public class OVRManifestPreprocessor
{
    [MenuItem("Oculus/Tools/Create store-compatible AndroidManifest.xml", false, 100000)]
    public static void GenerateManifestForSubmission()
    {
        var so = ScriptableObject.CreateInstance(typeof(OVRPluginUpdaterStub));
        var script = MonoScript.FromScriptableObject(so);
        string assetPath = AssetDatabase.GetAssetPath(script);
        string editorDir = Directory.GetParent(assetPath).FullName;
        string srcFile = editorDir + "/AndroidManifest.OVRSubmission.xml";

        if (!File.Exists(srcFile))
        {
            Debug.LogError("Cannot find Android manifest template for submission." +
                " Please delete the OVR folder and reimport the Oculus Utilities.");
            return;
        }

        string manifestFolder = Application.dataPath + "/Plugins/Android";

        if (!Directory.Exists(manifestFolder))
            Directory.CreateDirectory(manifestFolder);

        string dstFile = manifestFolder + "/AndroidManifest.xml";

        if (File.Exists(dstFile))
        {
            if (!EditorUtility.DisplayDialog("AndroidManifest.xml Already Exists!", "Would you like to replace the existing manifest with a new one? All modifications will be lost.", "Replace", "Cancel"))
            {
                return;
            }
        }

        PatchAndroidManifest(srcFile, dstFile, false);

        AssetDatabase.Refresh();
    }

    [MenuItem("Oculus/Tools/Update AndroidManifest.xml")]
    public static void UpdateAndroidManifest()
    {
        string manifestFile = "Assets/Plugins/Android/AndroidManifest.xml";

        if (!File.Exists(manifestFile))
        {
            Debug.LogError("Unable to update manifest because it does not exist! Run \"Create store-compatible AndroidManifest.xml\" first");
            return;
        }

        if (!EditorUtility.DisplayDialog("Update AndroidManifest.xml", "This will overwrite all Oculus specific AndroidManifest Settings. Continue?", "Overwrite", "Cancel"))
        {
            return;
        }

        PatchAndroidManifest(manifestFile, skipExistingAttributes: false);
        AssetDatabase.Refresh();
    }

    [MenuItem("Oculus/Tools/Remove AndroidManifest.xml")]
    public static void RemoveAndroidManifest()
    {
        AssetDatabase.DeleteAsset("Assets/Plugins/Android/AndroidManifest.xml");
        AssetDatabase.Refresh();
    }

    private static void AddOrRemoveTag(XmlDocument doc, string @namespace, string path, string elementName, string name, bool required, bool modifyIfFound, params string[] attrs) // name, value pairs	
    {
        var nodes = doc.SelectNodes(path + "/" + elementName);
        XmlElement element = null;
        foreach (XmlElement e in nodes)
        {
            if (name == null || name == e.GetAttribute("name", @namespace))
            {
                element = e;
                break;
            }
        }

        if (required)
        {
            if (element == null)
            {
                var parent = doc.SelectSingleNode(path);
                element = doc.CreateElement(elementName);
                element.SetAttribute("name", @namespace, name);
                parent.AppendChild(element);
            }

            for (int i = 0; i < attrs.Length; i += 2)
            {
                if (modifyIfFound || string.IsNullOrEmpty(element.GetAttribute(attrs[i], @namespace)))
                {
                    if (attrs[i + 1] != null)
                    {
                        element.SetAttribute(attrs[i], @namespace, attrs[i + 1]);
                    }
                    else
                    {
                        element.RemoveAttribute(attrs[i], @namespace);
                    }
                }
            }
        }
        else
        {
            if (element != null && modifyIfFound)
            {
                element.ParentNode.RemoveChild(element);
            }
        }
    }

    public static void PatchAndroidManifest(string sourceFile, string destinationFile = null, bool skipExistingAttributes = true, bool enableSecurity = false)
    {
        if (destinationFile == null)
        {
            destinationFile = sourceFile;
        }

        bool modifyIfFound = !skipExistingAttributes;

        try
        {
            OVRProjectConfig projectConfig = OVRProjectConfig.GetProjectConfig();

            // Load android manfiest file
            XmlDocument doc = new XmlDocument();
            doc.Load(sourceFile);

            string androidNamepsaceURI;
            XmlElement element = (XmlElement)doc.SelectSingleNode("/manifest");
            if (element == null)
            {
                UnityEngine.Debug.LogError("Could not find manifest tag in android manifest.");
                return;
            }

            // Get android namespace URI from the manifest
            androidNamepsaceURI = element.GetAttribute("xmlns:android");
            if (string.IsNullOrEmpty(androidNamepsaceURI))
            {
                UnityEngine.Debug.LogError("Could not find Android Namespace in manifest.");
                return;
            }

            // remove launcher and leanback launcher
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest/application/activity/intent-filter",
                "category",
                "android.intent.category.LAUNCHER",
                required: false,
                modifyIfFound: true); // always remove launcher
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest/application/activity/intent-filter",
                "category",
                "android.intent.category.LEANBACK_LAUNCHER",
                required: false,
                modifyIfFound: true); // always remove leanback launcher
            // add info category
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest/application/activity/intent-filter",
                "category",
                "android.intent.category.INFO",
                required: true,
                modifyIfFound: true); // always add info launcher

            // First add or remove headtracking flag if targeting Quest
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest",
                "uses-feature",
                "android.hardware.vr.headtracking",
                OVRDeviceSelector.isTargetDeviceQuestFamily,
                true,
                "version", "1",
                "required", "true");

            // If Quest is the target device, add the handtracking manifest tags if needed
            // Mapping of project setting to manifest setting:
            // OVRProjectConfig.HandTrackingSupport.ControllersOnly => manifest entry not present
            // OVRProjectConfig.HandTrackingSupport.ControllersAndHands => manifest entry present and required=false
            // OVRProjectConfig.HandTrackingSupport.HandsOnly => manifest entry present and required=true
            OVRProjectConfig.HandTrackingSupport targetHandTrackingSupport = OVRProjectConfig.GetProjectConfig().handTrackingSupport;
            bool handTrackingEntryNeeded = OVRDeviceSelector.isTargetDeviceQuestFamily && (targetHandTrackingSupport != OVRProjectConfig.HandTrackingSupport.ControllersOnly);

            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest",
                "uses-feature",
                "oculus.software.handtracking",
                handTrackingEntryNeeded,
                modifyIfFound,
                "required", (targetHandTrackingSupport == OVRProjectConfig.HandTrackingSupport.HandsOnly) ? "true" : "false");
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest",
                "uses-permission",
                "com.oculus.permission.HAND_TRACKING",
                handTrackingEntryNeeded,
                modifyIfFound);

            // Add focus aware tag if this app is targeting Quest Family
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest/application/activity",
                "meta-data",
                "com.oculus.vr.focusaware",
                OVRDeviceSelector.isTargetDeviceQuestFamily,
                modifyIfFound,
                "value", projectConfig.focusAware ? "true" : "false");

            // Add support devices manifest according to the target devices
            if (OVRDeviceSelector.isTargetDeviceQuestFamily)
            {
                string targetDeviceValue = "quest";
                if (OVRDeviceSelector.isTargetDeviceQuest && OVRDeviceSelector.isTargetDeviceQuest2)
                {
                    targetDeviceValue = "quest|delmar";
                }
                else if (OVRDeviceSelector.isTargetDeviceQuest2)
                {
                    targetDeviceValue = "delmar";
                }
                else if (OVRDeviceSelector.isTargetDeviceQuest)
                {
                    targetDeviceValue = "quest";
                }
                else
                {
                    Debug.LogError("Unexpected target devices");
                }
                AddOrRemoveTag(doc,
                    androidNamepsaceURI,
                    "/manifest/application",
                    "meta-data",
                    "com.oculus.supportedDevices",
                    true,
                    modifyIfFound,
                    "value", targetDeviceValue);
            }

            // Add system keyboard tag
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest",
                "uses-feature",
                "oculus.software.overlay_keyboard",
                projectConfig.focusAware && projectConfig.requiresSystemKeyboard,
                modifyIfFound,
                "required", "false");

            // make sure the VR Mode tag is set in the manifest
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest/application",
                "meta-data",
                "com.samsung.android.vr.application.mode",
                true,
                modifyIfFound,
                "value", "vr_only");

            // make sure android label and icon are set in the manifest
            AddOrRemoveTag(doc,
                androidNamepsaceURI,
                "/manifest",
                "application",
                null,
                true,
                modifyIfFound,
                "label", "@string/app_name",
                "icon", "@mipmap/app_icon",
                // Disable allowBackup in manifest and add Android NSC XML file				
                "allowBackup", projectConfig.disableBackups ? "false" : "true",
                "networkSecurityConfig", projectConfig.enableNSCConfig && enableSecurity ? "@xml/network_sec_config" : null
                );

            doc.Save(destinationFile);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }
}
