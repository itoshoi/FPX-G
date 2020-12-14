using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class VRInterfaceModel : MonoBehaviour
{
    private List<float> opCountList = new List<float>();


    // 未知ノードを選択する確率
    private const float pSelectUnknown = 0.8f;
    // 既知ノードを選択する確率
    private float pSelectKnown => 1 - pSelectUnknown;

    // 未知ノードに対してnホップ先を選択する確率
    // [1ホップ目を選択する確率, 2ホップ目を.., 3ホ.., ...]
    // private readonly float[] pSelectHopUnknown = new float[] { 0.6f, 0.3f, 0.1f };
    // private readonly float[] pSelectHopKnown = new float[] { 0.6f, 0.3f, 0.1f };
    // private readonly float[] pSelectHopUnknown = new float[] { 0.3f, 0.4f, 0.3f };
    // private readonly float[] pSelectHopKnown = new float[] { 0.3f, 0.4f, 0.3f };
    private readonly float[] pSelectHopUnknown = new float[] { 1f, 0f, 0f };
    private readonly float[] pSelectHopKnown = new float[] { 1f, 0f, 0f };

	private const float Lambda = 10f;
	private const int MaxSelectHop = 3;

    private bool _simulate = false;
    private SimNode _currentNode;

    private int _opCount = 0;

    class SimNode
    {
        public int nodeId = 0;
        public bool Known { get; set; }
        public List<SimNode> linkedNodes = new List<SimNode>();
        public bool IsGoal { get; set; }

        public SimNode[] KnownLinkedNodes => linkedNodes.Where(node => node.Known).ToArray();
        public SimNode[] UnknownLinkedNodes => linkedNodes.Where(node => !node.Known).ToArray();

        public SimNode[] GetLinkedNodes(float n, bool knownNodes)
        {
            var lastNodes = new List<SimNode> { this };

            for (var i = 1; i < n; i++)
            {
                var lastNodesTmp = new List<SimNode>();
                foreach (var node in lastNodes)
                {
                    lastNodesTmp = lastNodesTmp.Concat(node.linkedNodes).ToList();
                }
                lastNodes = lastNodesTmp;
            }

            var result = new List<SimNode>();
            foreach (var node in lastNodes)
            {
                if (knownNodes)
                {
                    result = result.Concat(node.KnownLinkedNodes).ToList();
                }
                else
                {
                    result = result.Concat(node.UnknownLinkedNodes).ToList();
                }
            }

            return result.ToArray();
        }
    }

    private SimNode InitNodes()
    {
		// when tree
		// var allNodes = MakeTreeNodes();
		// var firstNode = allNodes[0];

		// when watts strogatz graph 
		var allNodes = MakeConnectedWattsStrogatzGraph(600, 6, 0.8f);
		var firstNode = allNodes[0];

        _currentNode = firstNode;
        _currentNode.Known = true;
        _opCount = 0;
		allNodes[Random.Range(0, allNodes.Length)].IsGoal = true;
		// firstNode.linkedNodes[1].linkedNodes[1].linkedNodes[1].IsGoal = true;

        return firstNode;
    }

	private SimNode[] MakeTreeNodes(){
		// 全ホップ数
		const int HopCount = 3;
		// 一つのノードにつながっているノードの数
		const int LinkNodeCount = 5;


        var nodeId = 0;
        var firstNode = new SimNode();
        var lastNodes = new List<SimNode> { firstNode };
		var allNodes = new List<SimNode>{ firstNode };
        for (int i = 0; i < HopCount; i++)
        {
            var lastNodesTmp = new List<SimNode>();

            foreach (var lastNode in lastNodes)
            {
                for (int n = 0; n < LinkNodeCount; n++)
                {
                    var newNode = new SimNode();
                    lastNode.linkedNodes.Add(newNode);
                    newNode.linkedNodes.Add(lastNode);
                    nodeId++;
                    newNode.nodeId = nodeId;
                    lastNodesTmp.Add(newNode);
					allNodes.Add(newNode);
                }
            }
            lastNodes = lastNodesTmp;
        }

		return allNodes.ToArray();
	}


	// Make Watts Strogatts Graph
	// nodeCount : the number of nodes
	// ringK : each node is connected to k/2 nearest neighbors in ring topology
	// rewiringP : the probability of rewiring each edge
	private SimNode[] MakeWattsStrogatzGraph(int nodeCount, int ringK, float rewiringP){
		if(ringK >= nodeCount)
			Debug.Log("parameter is invalid");

		var allNodes = new SimNode[nodeCount];
		for(var i = 0; i < allNodes.Length; i++)
			allNodes[i] = new SimNode();

		// 近くのノードと結合させて円環を作る
		for(int i = 1; i < ringK / 2 + 1; i++){
			for(int j = 0; j < allNodes.Length; j++){
				// リンク先のノードはindexをiだけずらしたもの
				var linkNodeIndex = j + i < allNodes.Length ? j + i : j + i - allNodes.Length;
				// j <-> j + i にリンクを貼る
				allNodes[j].linkedNodes.Add(allNodes[linkNodeIndex]);
				allNodes[linkNodeIndex].linkedNodes.Add(allNodes[j]);
			}
		}

		// ランダムに再結線する
		for(int i=1; i < ringK / 2 + 1; i++){
			for(int j = 0; j < allNodes.Length; j++){
				// j <-> j + iのリンクに関して, 再結線する
				if(Random.Range(0f, 1f) < rewiringP){
					// 現在注目しているノード
					var targetNode = allNodes[j];
					// 削除するノード
					var oldLinkNodeIndex = j + i < allNodes.Length ? j + i : j + i - allNodes.Length;
					var oldLinkNode = allNodes[oldLinkNodeIndex];
					// 新しく接続するノード
					var newLinkNode = allNodes[Random.Range(0, allNodes.Length)];
					// 自己ループと重複ループを避ける
					while(targetNode == newLinkNode || targetNode.linkedNodes.Contains(newLinkNode)){
						newLinkNode = allNodes[Random.Range(0, allNodes.Length)];
						// すでに自分以外の全ノードにエッジがあればスキップ
						if(targetNode.linkedNodes.Count >= nodeCount - 1)
							break;
					}

					targetNode.linkedNodes.Remove(oldLinkNode);
					oldLinkNode.linkedNodes.Remove(targetNode);
					targetNode.linkedNodes.Add(newLinkNode);
					newLinkNode.linkedNodes.Add(targetNode);
				}
			}
		}

		return allNodes;
	}

	private SimNode[] MakeConnectedWattsStrogatzGraph(int nodeCount, int ringK, float rewiringP){
		var allNodes = MakeWattsStrogatzGraph(nodeCount, ringK, rewiringP);
		while(!IsConnectedGraph(allNodes)){
			allNodes = MakeWattsStrogatzGraph(nodeCount, ringK, rewiringP);
		}

		return allNodes;
	}

	private bool IsConnectedGraph(SimNode[] allNodes){
		foreach(var node in allNodes){
			if(node.linkedNodes.Count == 0)
				return false;
		}
		return true;
	}

	private int CountLinkSumCount(SimNode[] allNodes){
		int sum = 0;
		foreach(var node in allNodes){
			sum += node.linkedNodes.Count;
		}
		Debug.Log(sum);

		return sum;
	}

    private void Simulate()
    {
		InitNodes();
		// connectedが保証されてないので絶対に使ってはいけません.
		// MakeRandomGraph();

        while (!_currentNode.IsGoal)
        {
            _opCount++;

            var r1 = Random.Range(0f, 1f);
            // 未知ノードに移動
            if (r1 < pSelectUnknown)
            {
                // var selectHop = SelectHopByProbArray(pSelectHopUnknown);
                var selectHop = SelectHopByExpDist();
                var unknownNodes = _currentNode.GetLinkedNodes(selectHop, false);
                if (unknownNodes.Length == 0)
                    continue;
                var r2 = Random.Range(0, unknownNodes.Length);
                _currentNode = unknownNodes[r2];
                _currentNode.Known = true;
            }
            // 既知ノードに移動
            else
            {
                // var selectHop = SelectHopByProbArray(pSelectHopKnown);
                var selectHop = SelectHopByExpDist();
				Debug.Log("Select Hop " + selectHop);
                var knownNodes = _currentNode.GetLinkedNodes(selectHop, true);
                if (knownNodes.Length == 0)
                    continue;
                var r2 = Random.Range(0, knownNodes.Length);
                _currentNode = knownNodes[r2];
                _currentNode.Known = true;
            }
        }

		Debug.Log("操作数" + _opCount);
		opCountList.Add(_opCount);
    }


    private void Update()
    {
		Simulate();
		Debug.Log("平均 " + opCountList.Average());
    }

    private int SelectHopByProbArray(float[] prob)
    {
        int selectIndex = 0;
        var r = Random.Range(0f, 1f);
        float pSum = 0;
        for (int i = 0; i < prob.Length; i++)
        {
            pSum += prob[i];
            if (r <= pSum)
            {
                selectIndex = i + 1;
                break;
            }
        }

        return selectIndex;
    }

	private float ExpDist(float lamda){
		var r = Random.Range(0f, 1f);
		var x = - Mathf.Log(1 - r) / lamda;
		return x;
	}

	private int SelectHopByExpDist(){
		var selectHop = Mathf.Min((int) ExpDist(Lambda) + 1, MaxSelectHop);
		Debug.Log("Select Hop " + selectHop);
		return selectHop;
	}
}
