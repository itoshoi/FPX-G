using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SystemManager : SingletonMonoBehaviour<SystemManager>
{
    [SerializeField] private string theme;
    [SerializeField] private int hopCount;
    [SerializeField] private int linkedNodeLimit = 5;
    private GameObject _nodeParent;

    public int HopCount => hopCount;
    public int LinkedNodeLimit => linkedNodeLimit;

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
        Time.timeScale = 3;
        
        _nodeParent = new GameObject("Nodes");
        var node = ResourceNode.Instantiate(theme, Vector3.zero, _nodeParent.transform);
        
        yield return new WaitForSeconds(2);

        var targetNodes = new List<ResourceNode>{node};
        for (var i = 0; i < hopCount; i++)
        {
            var nextNodes = new List<ResourceNode>();
            foreach (var targetNode in targetNodes)
            {
                var newNodes = targetNode.InstantiateLinkedNodes(linkedNodeLimit);
                nextNodes.AddRange(newNodes);
            }

            targetNodes = nextNodes;
            yield return new WaitForSeconds(5);
        }

        // 配置が整うの待ち
        yield return new WaitForSeconds(3f);
        
        Time.timeScale = 1;
    }
}
