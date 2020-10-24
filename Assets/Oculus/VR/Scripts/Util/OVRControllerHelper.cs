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
using System.Collections;

/// <summary>
/// Simple helper script that conditionally enables rendering of a controller if it is connected.
/// </summary>
public class OVRControllerHelper : MonoBehaviour
{
	/// <summary>
	/// The root GameObject that represents the Oculus Touch for Quest And RiftS Controller model (Left).
	/// </summary>
	public GameObject m_modelOculusTouchQuestAndRiftSLeftController;

	/// <summary>
	/// The root GameObject that represents the Oculus Touch for Quest And RiftS Controller model (Right).
	/// </summary>
	public GameObject m_modelOculusTouchQuestAndRiftSRightController;

	/// <summary>
	/// The root GameObject that represents the Oculus Touch for Rift Controller model (Left).
	/// </summary>
	public GameObject m_modelOculusTouchRiftLeftController;

	/// <summary>
	/// The root GameObject that represents the Oculus Touch for Rift Controller model (Right).
	/// </summary>
	public GameObject m_modelOculusTouchRiftRightController;

	/// <summary>
	/// The root GameObject that represents the Oculus Touch for Quest 2 Controller model (Left).
	/// </summary>
	public GameObject m_modelOculusTouchQuest2LeftController;

	/// <summary>
	/// The root GameObject that represents the Oculus Touch for Quest 2 Controller model (Right).
	/// </summary>
	public GameObject m_modelOculusTouchQuest2RightController;

	/// <summary>
	/// The controller that determines whether or not to enable rendering of the controller model.
	/// </summary>
	public OVRInput.Controller m_controller;

	private enum ControllerType
	{
		QuestAndRiftS = 1,
		Rift = 2,
		Quest2 = 3,
	}

	private ControllerType activeControllerType = ControllerType.Rift;

	private bool m_prevControllerConnected = false;
	private bool m_prevControllerConnectedCached = false;

	void Start()
	{
		OVRPlugin.SystemHeadset headset = OVRPlugin.GetSystemHeadsetType();
		switch (headset)
		{
			case OVRPlugin.SystemHeadset.Rift_CV1:
				activeControllerType = ControllerType.Rift;
				break;
			case OVRPlugin.SystemHeadset.Oculus_Quest_2:
				activeControllerType = ControllerType.Quest2;
				break;
			default:
				activeControllerType = ControllerType.QuestAndRiftS;
				break;
		}

		Debug.LogFormat("OVRControllerHelp: Active controller type: {0} for product {1}", activeControllerType, OVRPlugin.productName);

		// Hide all controller models until controller get connected
		m_modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
		m_modelOculusTouchQuestAndRiftSRightController.SetActive(false);
		m_modelOculusTouchRiftLeftController.SetActive(false);
		m_modelOculusTouchRiftRightController.SetActive(false);
		m_modelOculusTouchQuest2LeftController.SetActive(false);
		m_modelOculusTouchQuest2RightController.SetActive(false);
	}

	void Update()
	{
		bool controllerConnected = OVRInput.IsControllerConnected(m_controller);

		if ((controllerConnected != m_prevControllerConnected) || !m_prevControllerConnectedCached)
		{
			if (activeControllerType == ControllerType.Rift)
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(false);
				m_modelOculusTouchRiftLeftController.SetActive(controllerConnected && (m_controller == OVRInput.Controller.LTouch));
				m_modelOculusTouchRiftRightController.SetActive(controllerConnected && (m_controller == OVRInput.Controller.RTouch));
				m_modelOculusTouchQuest2LeftController.SetActive(false);
				m_modelOculusTouchQuest2RightController.SetActive(false);
			}
			else if (activeControllerType == ControllerType.Quest2)
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(false);
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(false);
				m_modelOculusTouchRiftLeftController.SetActive(false);
				m_modelOculusTouchRiftRightController.SetActive(false);
				m_modelOculusTouchQuest2LeftController.SetActive(controllerConnected && (m_controller == OVRInput.Controller.LTouch));
				m_modelOculusTouchQuest2RightController.SetActive(controllerConnected && (m_controller == OVRInput.Controller.RTouch));
			}
			else /*if (activeControllerType == ControllerType.QuestAndRiftS)*/
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(controllerConnected && (m_controller == OVRInput.Controller.LTouch));
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(controllerConnected && (m_controller == OVRInput.Controller.RTouch));
				m_modelOculusTouchRiftLeftController.SetActive(false);
				m_modelOculusTouchRiftRightController.SetActive(false);
				m_modelOculusTouchQuest2LeftController.SetActive(false);
				m_modelOculusTouchQuest2RightController.SetActive(false);
			}

			m_prevControllerConnected = controllerConnected;
			m_prevControllerConnectedCached = true;
		}
	}
}
