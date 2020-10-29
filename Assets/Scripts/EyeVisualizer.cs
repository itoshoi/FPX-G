using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeVisualizer : MonoBehaviour
{
    [SerializeField, ColorUsage(true, true)] private Color defaultNodeColor;

    [SerializeField, ColorUsage(true, true)]
    private Color lookingNodeColor;
    
    private void Update()
    {
        foreach (var node in ResourceNode.AllResourceNodes)
        {
            // node.Value.RestoreSharedMaterial();
            node.Value.SetColor(defaultNodeColor);
        }
        
        var lookingNode = MyUtility.GetLookingNode(out var hit);
        if (lookingNode)
        {
            lookingNode.SetColor(lookingNodeColor);
        }
    }
}
