using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeVisualizer : MonoBehaviour
{
    private void Update()
    {
        foreach (var node in ResourceNode.AllResourceNodes)
        {
            node.Value.RestoreSharedMaterial();
        }
        
        var lookingNode = MyUtility.GetLookingNode(out var hit);
        if (lookingNode)
        {
            lookingNode.SetColor(Color.cyan);
        }
    }
}
