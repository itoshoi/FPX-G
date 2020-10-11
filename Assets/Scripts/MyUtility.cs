using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUtility
{
    private static Camera _camera;
    
    /// <summary>
    /// HMDが見ている方向にあるノードを返す。
    /// 見てなければnull。
    /// </summary>
    /// <returns></returns>
    public static Node GetLookingNode(out RaycastHit hit)
    {
        if(!_camera) _camera = Camera.main;
        
        var transform1 = _camera.transform;
        var ray = new Ray(transform1.position, transform1.forward);
        var hasHit = Physics.Raycast(ray, out hit, 10, LayerMask.GetMask("Node"));

        if (!hasHit) return null;

        return hit.transform.GetComponent<Node>();
    }
}