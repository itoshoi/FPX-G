using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MyController : MonoBehaviour
{
    [SerializeField] private SteamVR_Input_Sources source;
    [SerializeField] private SteamVR_Action_Boolean trigger;
    [SerializeField] private SteamVR_Action_Boolean teleport;
    [SerializeField] private SteamVR_Action_Vector2 direction;

    [SerializeField] private CameraController cameraController;
    
    private bool _prevTriggerInput = false;
    private bool _prevTeleportInput = false;

    private void Update()
    {
        var currentDirectionInput = direction.GetAxis(source);
        var currentTeleportInput = teleport.GetState(source);

        if (currentTeleportInput && !_prevTeleportInput)
        {
            cameraController.MoveToLookingNode();
        }

        _prevTeleportInput = currentTeleportInput;
    }

    private void OnTriggerStay(Collider other)
    {
        var currentTriggerInput = trigger.GetState(SteamVR_Input_Sources.Any);

        // トリガーボタン
        if (other.CompareTag("Node/Resource") && currentTriggerInput && !_prevTriggerInput)
        {
            // 派生を生成
            other.GetComponent<ResourceNode>().InstantiateLinkedNodes(SystemManager.Instance.LinkedNodeLimit);
            // プロパティを生成
            other.GetComponent<ResourceNode>().InstantiateStatsNodes();
        }

        _prevTriggerInput = currentTriggerInput;
    }
}