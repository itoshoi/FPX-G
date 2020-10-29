using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

public class MyUtility
{
    private static Camera _camera;

    private static Node _prevLookingNode = null;

    /// <summary>
    /// HMDが見ている方向にあるノードを返す。
    /// </summary>
    /// <param name="hit">レイキャストの結果を格納する</param>
    /// <param name="allowReturnPrevNode">結果がnullだった際に前回見たノードを返すかどうか</param>
    /// <returns></returns>
    public static Node GetLookingNode(out RaycastHit hit, bool allowReturnPrevNode = true)
    {
        if (!_camera) _camera = Camera.main;

        var transform1 = _camera.transform;
        // var ray = new Ray(transform1.position, transform1.forward);
        SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out var ray);

        // レイキャストごとやってくれそうだけど、layermaskとかが無さそうだったから使ってない
        // var success = SRanipal_Eye_v2.Focus(GazeIndex.COMBINE, out var ray, out var info);

        ray.origin = transform1.position;
        ray.direction = transform1.TransformDirection(ray.direction);
        Debug.DrawRay(ray.origin, ray.direction, Color.green);
        
        var hasHit = Physics.Raycast(ray, out hit, 10, LayerMask.GetMask("Node"));

        if (!hasHit)
            return allowReturnPrevNode ? _prevLookingNode : null;

        var lookingNode = hit.transform.GetComponent<Node>();
        _prevLookingNode = lookingNode;
        return lookingNode;
    }
}