using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour
{
	private System.Random rand = new System.Random();

	private void Start(){
		var parent = new GameObject();
		SystemManager.NodeParent = parent;
		StartCoroutine(MakeScaleFreeNetwork(100, 1));
		// StartCoroutine(StopCoroutine());
	}

	private IEnumerator StopCoroutine(){
		yield return new WaitForSeconds(5);
		Time.timeScale = 0;
	}

    private IEnumerator MakeScaleFreeNetwork(int nodeCount, int m)
    {
        var nodes = new List<ResourceNode>();

        // make first m node
        for (int i = 0; i < m; i++)
            nodes.Add(ResourceNode.Instantiate("first node" + i, Vector3.zero));

        // connect first m node
        foreach (var node1 in nodes)
        {
            foreach (var node2 in nodes)
            {
                if (node1 != node2)
                {
                    node1.linkedNodes.Add(node2.name, node2);
                }
            }
        }

        // add nodes
        for (int i = 0; i < nodeCount - m; i++)
        {
			yield return new WaitForSeconds(1);
            // probability distribution of connecting
            var linkSum = nodes.Sum(n => n.linkedNodes.Count);
            var dist = nodes.Select(n => (float)n.linkedNodes.Count / (float)linkSum).ToArray();

            // make a new node
            var newNode = ResourceNode.Instantiate("node" + i, Random.onUnitSphere * i * 0.3f);
            // connect m nodes
            var selectIndices = SelectByProbDist(dist, m);
            foreach (var selectIndex in selectIndices)
            {
                newNode.linkedNodes.Add(nodes[selectIndex].name, nodes[selectIndex]);
                nodes[selectIndex].linkedNodes.Add(newNode.name, newNode);
            }
            nodes.Add(newNode);
        }

		yield break;
    }

    private int SelectByProbDist(float[] dist)
    {
        // var r = Random.Range(0f, 1f);
		var r = rand.NextDouble();
        var sum = 0f;
        for (int i = 0; i < dist.Length; i++)
        {
            sum += dist[i];
            if (r <= sum)
            {
                return i;
            }
        }
        Debug.Log(sum);
        return dist.Length - 1;
    }

    private int[] SelectByProbDist(float[] dist, int selectCount)
    {
        var indexList = new List<int>();
        while (indexList.Count < selectCount)
        {
            var index = SelectByProbDist(dist);
            if (!indexList.Contains(index))
            {
                indexList.Add(index);
            }
        }
        return indexList.ToArray();
    }

}
