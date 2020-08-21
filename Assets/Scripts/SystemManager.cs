using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    [SerializeField] private string theme;
    private GameObject _nodeParent;
    
    public static List<Node> AllNodes { get; } = new List<Node>();

    private void Start()
    {
        StartCoroutine(MakeNodesCoroutine());
        Init();
    }

    private void Init()
    {
        
    }

    private IEnumerator MakeNodesCoroutine()
    {
        _nodeParent = new GameObject("Nodes");
        var node = Node.Instantiate(theme, Vector3.zero, _nodeParent.transform);
        
        AllNodes.Add(node);
        
        yield return new WaitForSeconds(2);
        var link1 = node.InstantiateLinkedNodes();
        
        AllNodes.AddRange(link1);
        
        yield return new WaitForSeconds(3);
        foreach (var link in link1)
        {
            var link2 = link.InstantiateLinkedNodes();
            AllNodes.AddRange(link2);
        }
    }

    private void Update()
    {
        
    }
}
