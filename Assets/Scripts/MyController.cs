using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Attach Each Hand Controller
/// OnTrigger系の関数を使う為に各コントローラーのオブジェクトにアタッチしているが、
/// これだと２回呼ばれるので本当は良くないし改善したいけどあんまり良いのが思いつかない
/// </summary>
public class MyController : MonoBehaviour
{
    [SerializeField] private GameObject leftControllerObj;
    [SerializeField] private GameObject rightControllerObj;

    [SerializeField] private SteamVR_Action_Boolean trigger;
    [SerializeField] private SteamVR_Action_Boolean teleport;
    [SerializeField] private SteamVR_Action_Vector2 direction;

    [SerializeField] private CameraController cameraController;

    private bool _prevTriggerInput = false;
    private bool _prevTeleportInput = false;

    private bool _isScaling;
    private Vector3 _scalingStartScale;
    private Vector3 _scalingStartPosition;
    private float _scalingStartControllerDistance;
    private Vector3 _scalingStartControllerPosition;

    private void Update()
    {
        var currentDirectionInput = direction.GetAxis(SteamVR_Input_Sources.Any);
        var currentTeleportInput = teleport.GetState(SteamVR_Input_Sources.Any);
        var currentLeftTriggerInput = trigger.GetState(SteamVR_Input_Sources.LeftHand);
        var currentRightTriggerInput = trigger.GetState(SteamVR_Input_Sources.RightHand);

        if (currentTeleportInput && !_prevTeleportInput)
        {
            cameraController.MoveToLookingNode();
        }

        // 両手でトリガーを引いてるときは拡大縮小
        if (currentLeftTriggerInput && currentRightTriggerInput)
        {
            var rightPos = rightControllerObj.transform.position;
            var leftPos = leftControllerObj.transform.position;
            var controllerDistance = (leftPos - rightPos)
                .magnitude;
            var controllerPosition = (leftPos + rightPos) / 2;

            // スケーリングの開始時
            if (!_isScaling)
            {
                _scalingStartScale = SystemManager.NodeParent.transform.lossyScale;
                _scalingStartPosition = SystemManager.NodeParent.transform.position;
                _scalingStartControllerDistance = controllerDistance;
                _scalingStartControllerPosition = controllerPosition;

                _isScaling = true;
            }

            // スケールの変更
            var setScale =
                _scalingStartScale + Vector3.one * (controllerDistance - _scalingStartControllerDistance);
            if (setScale.x > 0 && setScale.y > 0 && setScale.z > 0)
            {
                SystemManager.NodeParent.transform.localScale = setScale;
            }

            // 位置の移動
            var setPosition =
                _scalingStartPosition + (controllerPosition - _scalingStartControllerPosition);
            SystemManager.NodeParent.transform.position = setPosition;
        }
        else
        {
            _isScaling = false;
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