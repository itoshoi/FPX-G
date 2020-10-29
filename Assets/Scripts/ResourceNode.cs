using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class ResourceNode : Node
{
    #region Variable

    public static Dictionary<string, ResourceNode> AllResourceNodes { get; } = new Dictionary<string, ResourceNode>();

    [SerializeField] private TextMeshPro titleText;

    // 余計なAPI呼び出しをなくすため、APIで取得時に関連ノードの名前をとっておく
    private readonly List<string> _linkedNodeNames = new List<string>();

    public string Label { get; private set; }
    public string Abstract { get; private set; }

    #endregion

    #region Instantiate

    public static ResourceNode Instantiate(string name, Vector3 pos, Transform parent = null)
    {
        var prefab = Resources.Load<ResourceNode>("ResourceNode");
        var obj = Instantiate(prefab, pos, Quaternion.identity, parent);
        obj.Label = name;
        obj.name = name;
        AllResourceNodes.Add(name, obj);
        return obj;
    }

    public ResourceNode[] InstantiateLinkedNodes(int limitCount)
    {
        var madeNodes = new List<ResourceNode>();
        foreach (var nodeName in _linkedNodeNames)
        {
            if (linkedNodes.ContainsKey(nodeName)) continue;
            if (AllResourceNodes.ContainsKey(nodeName))
            {
                linkedNodes.Add(nodeName, AllResourceNodes[nodeName]);
                AllResourceNodes[nodeName].linkedNodes.Add(Label, this);
                continue;
            }

            var pos = transform.position + Random.onUnitSphere * LinkedNodeDistance[(NodeType, Type.ResourceNode)];
            var obj = ResourceNode.Instantiate(nodeName, pos, transform.parent);
            madeNodes.Add(obj);
            linkedNodes.Add(nodeName, obj);
            obj.linkedNodes.Add(Label, this);

            if (limitCount < linkedNodes.Count)
                break;
        }

        return madeNodes.ToArray();
    }

    public LiteralNode[] InstantiateStatsNodes()
    {
        var madeNodes = new List<LiteralNode>();

        var abstKey = Label + " Abstract";
        if (!linkedNodes.ContainsKey(abstKey))
        {
            // var pos = transform.position + Random.onUnitSphere * LinkedNodeDistance[(NodeType, Type.LiteralNode)];
            var pos = transform.TransformPoint(Vector3.right * LinkedNodeDistance[(NodeType, Type.LiteralNode)]);
            var obj = LiteralNode.Instantiate(Abstract, pos, transform.parent);
            madeNodes.Add(obj);
            linkedNodes.Add(abstKey, obj);
            obj.linkedNodes.Add(Label, this);
        }

        return madeNodes.ToArray();
    }

    #endregion

    #region Initialize

    protected override void Init()
    {
        NodeType = Type.ResourceNode;
        StartCoroutine(InitCoroutine());
    }

    private IEnumerator InitCoroutine()
    {
        var query = "?query=select distinct * where { <http://ja.dbpedia.org/resource/" + Label + "> ?p ?o .  }";

        Debug.Log(SparqlUrl + query);

        using (var webRequest = UnityWebRequest.Get(SparqlUrl + query))
        {
            webRequest.SetRequestHeader("Accept", "application/sparql-results+json");
            yield return webRequest.SendWebRequest();

            Debug.Log(webRequest.downloadHandler.text);

            titleText.text = Label;

            var jo = JObject.Parse(webRequest.downloadHandler.text);

            foreach (var po in jo["results"]["bindings"])
            {
                if (po["p"]["value"].Value<string>().Contains("http://ja.dbpedia.org/property"))
                {
                    if (po["o"]["type"].Value<string>() == "uri" &&
                        !po["o"]["value"].Value<string>().Contains("Template"))
                    {
                        var nodeName = po["o"]["value"].Value<string>().Replace("http://ja.dbpedia.org/resource/", "");
                        _linkedNodeNames.Add(nodeName);
                    }
                }

                if (po["p"]["value"].Value<string>().Contains("http://dbpedia.org/ontology/abstract"))
                {
                    Abstract = po["o"]["value"].Value<string>();
                    Debug.Log("Abstract\n" + Abstract);
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// 全ノード間にクーロン力を適用する
    /// </summary>
    private void AddCoulombForce()
    {
        foreach (var node in AllResourceNodes.Values)
        {
            if (node == this) continue;

            var vector = transform.position - node.transform.position;
            var distance = vector.magnitude;

            var direction = vector.normalized;
            var force = CoulombConstant * direction / Mathf.Pow(distance, 2);

            NodeRigidbody.AddForce(force);
        }
    }
}