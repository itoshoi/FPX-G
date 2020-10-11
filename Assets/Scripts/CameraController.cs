using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private const float DefaultSpeed = 2;

    private void Start()
    {
        _camera = Camera.main;
    }

    /// <summary>
    /// targetに向かって動きます。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="autoMode">onだと自動でノードの位置につくまで移動し続けます。 offだと1フレーム分だけ動きます。</param>
    /// <param name="speed"></param>
    public void MoveTowards(Vector3 target, bool autoMode = false, float speed = DefaultSpeed)
    {
        var maxDistance = Time.deltaTime;
        var cameraPosition = _camera.transform.position;
        var direction = Vector3.MoveTowards(cameraPosition, target, maxDistance);

        var degree = direction - cameraPosition;

        transform.position += degree;

        if (autoMode)
            MoveTowardsInstance = StartCoroutine(MoveTowardsCoroutine(target, speed));
    }

    private void MoveTowards(Node target, bool autoMode, float speed = DefaultSpeed)
    {
        if (autoMode)
            MoveTowardsInstance = StartCoroutine(MoveTowardsCoroutine(target, speed));
        else
            MoveTowards(target.transform.position, false, speed);
    }

    private Coroutine _moveTowardsInstance = null;

    private Coroutine MoveTowardsInstance
    {
        get => _moveTowardsInstance;
        set
        {
            if (_moveTowardsInstance != null)
                StopCoroutine(_moveTowardsInstance);

            _moveTowardsInstance = value;
        }
    }

    private IEnumerator MoveTowardsCoroutine(Vector3 target, float speed = DefaultSpeed, float stopRange = 0.05f)
    {
        while ((target - _camera.transform.position).magnitude > stopRange)
        {
            MoveTowards(target, false, speed);
            yield return null;
        }
    }

    private IEnumerator MoveTowardsCoroutine(Node target, float speed = DefaultSpeed, float stopRange = 0.05f)
    {
        while ((target.transform.position - _camera.transform.position).magnitude > stopRange)
        {
            MoveTowards(target, false, speed);
            yield return null;
        }
    }

    /// <summary>
    /// 見ているノードの方向に移動します。
    /// </summary>
    /// <param name="autoMode">onだと自動でノードの位置につくまで移動し続けます。 offだと1フレーム分だけ動きます。</param>
    /// <param name="speed"></param>
    public void MoveToLookingNode(bool autoMode = true, float speed = DefaultSpeed)
    {
        var node = MyUtility.GetLookingNode(out var hit);
        if (node)
        {
            // 最終的なノードとの距離
            const float distance = 0.5f;
            var targetPos = hit.transform.position - _camera.transform.forward * distance;
            // 高さはノードの高さに合わせる
            targetPos.y = node.transform.position.y;
            MoveTowards(targetPos, autoMode, speed);
        }
    }
}