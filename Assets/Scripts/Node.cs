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
public class Node : MonoBehaviour
{
    private const string BaseUrl = "http://ja.dbpedia.org/";
    private const string SparqlUrl = BaseUrl + "sparql";

    private Camera _camera;
    public List<Node> linkedNodes = new List<Node>();
    private readonly List<string> _linkedNodeNames = new List<string>();
    private LineRenderer _lineRenderer;

    [SerializeField] private TextMeshPro titleText;

    [FormerlySerializedAs("targetDistance")] [SerializeField] private float linkedNodeDistance = 1;
    // ばね定数
    private const float SpringConstant = 1;
    // 減衰定数
    private const float SpringDecay = 1;
    
    // クーロン力の定数
    private const float CoulombConstant = 0.01f;

    public string Name { get; private set; }
    private Rigidbody NodeRigidbody { get; set; }

    public static Node Instantiate(string name, Vector3 pos, Transform parent = null)
    {
        var prefab = Resources.Load<Node>("Node");
        var obj = Instantiate(prefab, pos, Quaternion.identity, parent);
        obj.Name = name;
        obj.name = name;
        return obj;
    }

    private void Start()
    {
        _camera = Camera.main;
        // _linkedNodes = GameObject.FindGameObjectsWithTag("Node").Select(go => go.GetComponent<Node>()).ToList();
        NodeRigidbody = GetComponent<Rigidbody>();
        _lineRenderer = GetComponent<LineRenderer>();

        Init();
    }

    private void Update()
    {
        UpdateLine();
        UpdateRotation();
    }

    private void FixedUpdate()
    {
        AddForce();
    }

    private void UpdateLine()
    {
        _lineRenderer.useWorldSpace = true;
        var positions = linkedNodes.Select(node => node.transform.position).ToList();
        positions.Add(transform.position);

        _lineRenderer.positionCount = positions.Count * 2;
        for (int i = 0; i < positions.Count; i++)
        {
            _lineRenderer.SetPosition(i * 2, transform.position);
            _lineRenderer.SetPosition(i * 2 + 1, positions[i]);
        }
    }

    private void UpdateRotation()
    {
        transform.forward = transform.position - _camera.transform.position;
    }

    private void AddForce()
    {
        AddCoulombForce();
        AddSpringForce();
    }

    /// <summary>
    /// 全ノード間にクーロン力を適用する
    /// </summary>
    private void AddCoulombForce()
    {
        foreach (var node in SystemManager.AllNodes)
        {
            if(node == this) continue;
            
            var vector = transform.position - node.transform.position;
            var distance = vector.magnitude;
            var direction = vector.normalized;

            var force = CoulombConstant * direction / Mathf.Pow(distance, 2);
            NodeRigidbody.AddForce(force);
        }
    }

    /// <summary>
    /// リンクされたノードにばねの力を適用する
    /// </summary>
    private void AddSpringForce()
    {
        // リンクされたノード間にはばねの力が働く
        foreach (var linkedNode in linkedNodes)
        {
            var vector = transform.position - linkedNode.transform.position;
            var distance = vector.magnitude;
            var direction = vector.normalized;

            var force = SpringConstant * (linkedNodeDistance - distance) * direction - SpringDecay * NodeRigidbody.velocity;
            NodeRigidbody.AddForce(force);
        }
    }

    private void Init()
    {
        StartCoroutine(InitCoroutine());
    }

    private IEnumerator InitCoroutine()
    {
        var query = "?query=select distinct * where { <http://ja.dbpedia.org/resource/" + Name + "> ?p ?o .  }";

        Debug.Log(SparqlUrl + query);

        using (var webRequest = UnityWebRequest.Get(SparqlUrl + query))
        {
            webRequest.SetRequestHeader("Accept", "application/sparql-results+json");
            yield return webRequest.SendWebRequest();

            Debug.Log(webRequest.downloadHandler.text);

            titleText.text = Name;

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
            }
        }
    }

    public Node[] InstantiateLinkedNodes()
    {
        foreach (var nodeName in _linkedNodeNames)
        {
            var pos = Random.onUnitSphere;
            
            var obj = Instantiate(nodeName, pos, transform.parent);
            linkedNodes.Add(obj);
            obj.linkedNodes.Add(this);

            if (5 < linkedNodes.Count)
                break;
        }

        return linkedNodes.ToArray();
    }
}