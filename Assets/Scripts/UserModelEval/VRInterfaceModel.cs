using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class VRInterfaceModel : MonoBehaviour
{
    private List<SimRecord> records = new List<SimRecord>();

    public SimRecord[] Records => records.ToArray();

    // 未知ノードを選択する確率
    // probability of selecting unknown node
    private const float pSelectUnknown = 0.8f;
    // 既知ノードを選択する確率
    // probability of selecting known node
    private float pSelectKnown => 1 - pSelectUnknown;

    // 未知ノードに対してnホップ先を選択する確率
    // [1ホップ目を選択する確率, 2ホップ目を.., 3ホ.., ...]
    // private readonly float[] pSelectHopUnknown = new float[] { 0.6f, 0.3f, 0.1f };
    // private readonly float[] pSelectHopKnown = new float[] { 0.6f, 0.3f, 0.1f };
    // private readonly float[] pSelectHopUnknown = new float[] { 0.3f, 0.4f, 0.3f };
    // private readonly float[] pSelectHopKnown = new float[] { 0.3f, 0.4f, 0.3f };
    // private readonly float[] pSelectHopUnknown = new float[] { 1f, 0f, 0f };
    // private readonly float[] pSelectHopKnown = new float[] { 1f, 0f, 0f };

    // simulation dataset
    public SimRecord.DataSet dataSet = SimRecord.DataSet.WattsStrogatz;
    // all node count when watts strogatz model
    public int allNodeCount = 200;
    // mean degree when watts strogatz model (K)
    public int meanDegree = 6;

    // 指数分布のlambda
    // lambda of exp distribution
    public float Lambda { get; set; } = 1f;
    // 始点ノードに戻る確率
    // probablity of returning to first node
    public float pReturnFirst = 0f;
    private const int MaxSelectHop = 3;

	// 視界に目的ノードが入った場合に選択する確率
	public float pSelectVisibleGoal = 0.5f;
	// 視界に入り認識できるノードの最大数
	const int MaxVisibleCount = 20;

    // distanceFtoG between first node and goal node
    public float distanceFtoG = 3;
	public int goalCount = 1;

    private SimNode _currentNode;
    private SimNode _firstNode;
	private List<SimNode> _goalNodes;
    private float _graphDensity;

    // operation count
    private int _opCount = 0;

	// system rand instance
	private System.Random rand = new System.Random();

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
            // 既に探索済みのノードにはアクセスしないようにするため
            var accessedNodes = new List<SimNode> { this };

            for (var i = 1; i < n; i++)
            {
                var lastNodesTmp = new List<SimNode>();
                foreach (var node in lastNodes)
                {
                    lastNodesTmp = lastNodesTmp.Concat(node.linkedNodes).ToList();
                }
                lastNodes = lastNodesTmp;
                accessedNodes = accessedNodes.Concat(lastNodes).ToList();
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

            result.RemoveAll(node => accessedNodes.Contains(node));

            return result.ToArray();
        }

		// hop先までの全接続ノードを取得する
		// result[0]はこのノード
		// result[1]は1ホップ先のノード配列(SimNode[])
		public List<SimNode>[] GetAllLinkedNodes(int hop){
            var lastNodes = new List<SimNode> { this };
            // 既に探索済みのノードにはアクセスしないようにするため
            var accessedNodes = new List<SimNode> { this };
			var result = new List<SimNode>[hop+1];
			result[0] = new List<SimNode>{ this };

            for (var i = 1; i < hop + 1; i++)
            {
                var lastNodesTmp = new List<SimNode>();
                foreach (var node in lastNodes)
                {
					foreach(var linkNode in node.linkedNodes){
						if(!accessedNodes.Contains(linkNode)){
							lastNodesTmp.Add(linkNode);
							accessedNodes.Add(linkNode);
						}
					}
                }
                lastNodes = lastNodesTmp;
				result[i] = lastNodes;
            }

			return result;
		}
    }

    private SimNode InitNodes()
    {
        // when tree
        // var allNodes = MakeTreeNodes();
        // var firstNode = allNodes[0];
		
		_goalNodes = new List<SimNode>();

        SimNode[] allNodes = new SimNode[0];
        switch (dataSet)
        {
            case SimRecord.DataSet.WattsStrogatz:
                allNodes = MakeConnectedWattsStrogatzGraph(allNodeCount, meanDegree, 0.4f);
                break;
            case SimRecord.DataSet.Tree:
                allNodes = MakeTreeNodes();
                break;
            case SimRecord.DataSet.BarabasiAlbert:
                allNodes = MakeScaleFreeNetwork(allNodeCount, meanDegree / 2);
                break;
        }
        // when watts strogatz graph 

        SimNode firstNode = allNodes[0];
        // SimNode goalNode = allNodes[0];
        // var firstNode = allNodes[Random.Range(0, allNodes.Length)];
        // var goalNode = allNodes[Random.Range(0, allNodes.Length)];

        // set firstNode
        // while (firstNode == goalNode)
        // {
			// var goalList = new SimNode[0];
			// GoalNodeが手に入らない場合があるので、ちゃんと定まるまでループ
            // while (goalList.Length == 0)
            // {
		
		switch (dataSet)
		{
			case SimRecord.DataSet.WattsStrogatz:
				firstNode = allNodes[rand.Next(0, allNodes.Length)];
				break;
			case SimRecord.DataSet.Tree:
				firstNode = allNodes[0];
				break;
			case SimRecord.DataSet.BarabasiAlbert:
				firstNode = allNodes[rand.Next(0, allNodes.Length)];
				break;
		}
				// goalNode = allNodes[Random.Range(0, allNodes.Length)];

				// goalList = firstNode.GetLinkedNodes(distanceFtoG, false);
				// goalList = allNodes;
				// if(goalList.Length == 0)
				// 	Debug.Log("retry because goal not found.");
            // }

		while(_goalNodes.Count < goalCount){
			var goalNode = allNodes[rand.Next(0, allNodes.Length)];
			// if(0 < Distance(firstNode, goalNode) && !goalNode.IsGoal){
				goalNode.IsGoal = true;
				_goalNodes.Add(goalNode);
			// }
		}

        // }

		// for dist goal
        // goalNode.IsGoal = true;
		// _goalNodes.Add(goalNode);
		
        _currentNode = firstNode;
        _currentNode.Known = true;
        _opCount = 0;
        _firstNode = firstNode;
        _graphDensity = GraphDensity(allNodes);

        // 視点ノードと終点ノードとの距離を記録
		distanceFtoG = (float) _goalNodes.Sum(g => Distance(firstNode, g)) / (float) _goalNodes.Count;
		// distanceFtoG = 0;

        return firstNode;
    }

    private SimNode[] MakeTreeNodes()
    {
        // 全ホップ数
        const int HopCount = 3;
        // 一つのノードにつながっているノードの数
        const int LinkNodeCount = 5;


        var nodeId = 0;
        var firstNode = new SimNode();
        var lastNodes = new List<SimNode> { firstNode };
        var allNodes = new List<SimNode> { firstNode };
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
    private SimNode[] MakeWattsStrogatzGraph(int nodeCount, int ringK, float rewiringP)
    {
        if (ringK >= nodeCount)
            Debug.Log("parameter is invalid");

        var allNodes = new SimNode[nodeCount];
        for (var i = 0; i < allNodes.Length; i++)
            allNodes[i] = new SimNode();

        // 近くのノードと結合させて円環を作る
        for (int i = 1; i < ringK / 2 + 1; i++)
        {
            for (int j = 0; j < allNodes.Length; j++)
            {
                // リンク先のノードはindexをiだけずらしたもの
                var linkNodeIndex = j + i < allNodes.Length ? j + i : j + i - allNodes.Length;
                // j <-> j + i にリンクを貼る
                allNodes[j].linkedNodes.Add(allNodes[linkNodeIndex]);
                allNodes[linkNodeIndex].linkedNodes.Add(allNodes[j]);
            }
        }

        // ランダムに再結線する
        for (int i = 1; i < ringK / 2 + 1; i++)
        {
            for (int j = 0; j < allNodes.Length; j++)
            {
                // j <-> j + iのリンクに関して, 再結線する
                if (rand.NextDouble() < rewiringP)
                {
                    // 現在注目しているノード
                    var targetNode = allNodes[j];
                    // 削除するノード
                    var oldLinkNodeIndex = j + i < allNodes.Length ? j + i : j + i - allNodes.Length;
                    var oldLinkNode = allNodes[oldLinkNodeIndex];
                    // 新しく接続するノード
                    var newLinkNode = allNodes[rand.Next(0, allNodes.Length)];
                    // 自己ループと重複ループを避ける
                    while (targetNode == newLinkNode || targetNode.linkedNodes.Contains(newLinkNode))
                    {
                        newLinkNode = allNodes[rand.Next(0, allNodes.Length)];
                        // すでに自分以外の全ノードにエッジがあればスキップ
                        if (targetNode.linkedNodes.Count >= nodeCount - 1)
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

    private SimNode[] MakeConnectedWattsStrogatzGraph(int nodeCount, int ringK, float rewiringP)
    {
        var allNodes = MakeWattsStrogatzGraph(nodeCount, ringK, rewiringP);
        while (!IsConnectedGraph(allNodes))
        {
            allNodes = MakeWattsStrogatzGraph(nodeCount, ringK, rewiringP);
        }

        return allNodes;
    }

    // m is link count at once
    private SimNode[] MakeScaleFreeNetwork(int nodeCount, int m)
    {
        var nodes = new List<SimNode>();

        // make first m node
        for (int i = 0; i < m; i++)
            nodes.Add(new SimNode());

        // connect first m node
        foreach (var node1 in nodes)
        {
            foreach (var node2 in nodes)
            {
                if (node1 != node2)
                {
                    node1.linkedNodes.Add(node2);
                }
            }
        }

        // add nodes
        for (int i = 0; i < nodeCount - m; i++)
        {
            // probability distribution of connecting
            var linkSum = nodes.Sum(n => n.linkedNodes.Count);
            var dist = nodes.Select(n => (float)n.linkedNodes.Count / (float)linkSum).ToArray();

            // make a new node
            var newNode = new SimNode();
            // connect m nodes
            var selectIndices = SelectByProbDist(dist, m);
            foreach (var selectIndex in selectIndices)
            {
                newNode.linkedNodes.Add(nodes[selectIndex]);
                nodes[selectIndex].linkedNodes.Add(newNode);
            }
            nodes.Add(newNode);
        }

        return nodes.ToArray();
    }

    private bool IsConnectedGraph(SimNode[] allNodes)
    {
        foreach (var node in allNodes)
        {
            if (node.linkedNodes.Count == 0)
                return false;
        }
        return true;
    }

    private int CountLinkSumCount(SimNode[] allNodes)
    {
        int sum = 0;
        foreach (var node in allNodes)
        {
            sum += node.linkedNodes.Count;
        }
        Debug.Log(sum);

        return sum;
    }

	// return: シミュレーションが成功したか
    public bool Simulate()
    {
		// distanceFtoG = -1;
		// while(distanceFtoG == -1){
		// 	InitNodes();
		// }
        // connectedが保証されてないので絶対に使ってはいけません.
        // MakeRandomGraph();
		InitNodes();

		var tryCount = 0;
		const int maxTryCount = 1000;
        while (!IsAllGoalKnown())
        {
			tryCount++;
			if(maxTryCount < tryCount)
				return false;

			// 選択可能な全ノード
			var selectableNodes = _currentNode.GetAllLinkedNodes(MaxSelectHop);

			// ユーザーの視界を再現した, 見える範囲のノード
			var visibleNodes = new List<SimNode>();
			for(int i = 0; i < MaxVisibleCount; i++){
				var selectHop = SelectHopByExpDist();

				// try{
				// 	// みたいノードが0個になったら, 手前のノードで代替する
				// 	while(0 < selectHop && selectableNodes[selectHop].Count == 0){
				// 		selectHop--;
				// 		continue;
				// 	}
				// } catch(IndexOutOfRangeException){
				// 	Debug.Log("out of range");
				// 	Debug.Log("SelectHop:" + selectHop);
				// 	Debug.Log("length:" + selectableNodes.Length);
				// }

				if(selectableNodes[selectHop].Count == 0)
					continue;

				if(selectHop == 0){
					break;
				}

				var r = rand.Next(0, selectableNodes[selectHop].Count);
				visibleNodes.Add(selectableNodes[selectHop][r]);
				selectableNodes[selectHop].RemoveAt(r);

				var selectableNodesCount = selectableNodes.Sum(ns => ns.Count);
				if(selectableNodesCount == 1){
					break;
				}
			}

			// 目的ノードが視界に入っていたら高確率で選択
			var moved = false;
			foreach(var goalNode in _goalNodes){
				if(visibleNodes.Contains(goalNode)){
					var r = rand.NextDouble();
					if(r < pSelectVisibleGoal && !goalNode.Known){
						_currentNode = goalNode;
						_currentNode.Known = true;
						_opCount++;
						moved = true;
					}
				}
			}

			if(moved)
				continue;

			// 一定確率で始点ノードに戻る
            if ((float) rand.NextDouble() < pReturnFirst && _currentNode != _firstNode)
            {
                _currentNode = _firstNode;
				_opCount++;
                continue;
			}

			if(IsAllGoalKnown())
				continue;

            var r1 = rand.NextDouble();
            // 未知ノードに移動
            if (r1 < pSelectUnknown)
            {
                // var selectHop = SelectHopByProbArray(pSelectHopUnknown);
                var selectHop = SelectHopByExpDist();
                // var unknownNodes = _currentNode.GetLinkedNodes(selectHop, false).ToList();
				var unknownNodes = visibleNodes.Where(n => n.Known == false).ToArray();
                if (unknownNodes.Length == 0)
                    continue;
                var r2 = rand.Next(0, unknownNodes.Length);
                _currentNode = unknownNodes[r2];
                _currentNode.Known = true;
				_opCount++;
            }
            // 既知ノードに移動
            else
            {
                // var selectHop = SelectHopByProbArray(pSelectHopKnown);
                var selectHop = SelectHopByExpDist();
                // Debug.Log("Select Hop " + selectHop);
                // var knownNodes = _currentNode.GetLinkedNodes(selectHop, true);
				var knownNodes = visibleNodes.Where(n => n.Known == true).ToArray();
                if (knownNodes.Length == 0)
                    continue;
                var r2 = rand.Next(0, knownNodes.Length);
                _currentNode = knownNodes[r2];
                _currentNode.Known = true;
				_opCount++;
            }
        }

        // Debug.Log("操作数" + _opCount);
        var record = new SimRecord
        {
            dataSet = dataSet,
			nodeCount = allNodeCount,
			goalCount = goalCount,
            graphDensity = _graphDensity,
            lambda = Lambda,
            distance = distanceFtoG,
            opCount = _opCount,
            pSelectUnknown = pSelectUnknown,
            pReturnFirst = pReturnFirst,
			pSelectVisibleGoal = pSelectVisibleGoal
        };

        records.Add(record);

		return true;
    }

	private bool IsAllGoalKnown(){
		var result = true;
		foreach(var g in _goalNodes)
			if(!g.Known)
				result = false;

		return result;
	}

    private int SelectHopByProbArray(float[] prob)
    {
        int selectIndex = 0;
		var r = rand.NextDouble();
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

    private float ExpDist(float lamda)
    {
		if(Math.Abs(lamda) < 0.01f)
			return rand.Next(0, MaxSelectHop);

		float x = float.PositiveInfinity;
		int tryc = 0;
		// floatにすることでr=1になり, xがinfinityになることがあるのでそれを回避する
		while(x == float.PositiveInfinity || MaxSelectHop < x){
			var r = (float) rand.NextDouble();
			x = -Mathf.Log(1 - r) / Math.Abs(lamda);
			tryc++;
			if(100 < tryc){
				Debug.Log("value is invalid : " + x);
				break;
			}
		}

		if(lamda < 0)
			x = MaxSelectHop - x;

		// Debug.Log("lambda:" + lamda + " hop:" + (int)(x+1));
		
        return x;
    }

    private int SelectHopByExpDist()
    {
        var selectHop = Mathf.Min((int)ExpDist(Lambda) + 1, MaxSelectHop);
        // Debug.Log("Select Hop " + selectHop);
        return selectHop;
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

    private float GraphDensity(SimNode[] nodes)
    {
        var possibleEdges = nodes.Length * (nodes.Length - 1) / 2f;
        var graphEdges = nodes.Sum(n => n.linkedNodes.Count) / 2f;
        return graphEdges / possibleEdges;
    }

	private int Distance(SimNode node1, SimNode node2){
		const int maxHop = 4;
		var lastNodes = new List<SimNode> { node1 };
		// 既に探索済みのノードにはアクセスしないようにするため
		var accessedNodes = new List<SimNode> { node1 };

		for (var i = 1; i < maxHop + 1; i++)
		{
			var lastNodesTmp = new List<SimNode>();
			foreach (var node in lastNodes)
			{
				foreach(var linkNode in node.linkedNodes){
					if(!accessedNodes.Contains(linkNode)){
						lastNodesTmp.Add(linkNode);
						accessedNodes.Add(linkNode);

						if(linkNode == node2)
							return i;
					}
				}
			}
			lastNodes = lastNodesTmp;
		}

		return -1;
// 		if(node1 == node2)
// 			return 0;
	
// 		var dist = 0;
//         for(int i=1; i<6; i++){
//         	if(node1.GetLinkedNodes(i, false).Contains(node2)){
// 				dist = i;
//         		break;
//         	}else if(i == 5){
// 				dist = -1;
//         	}
//         }

// 		return dist;
	}
}
