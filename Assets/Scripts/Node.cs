using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Node : MonoBehaviour
{
    public enum Type
    {
        Default,
        ResourceNode,
        LiteralNode
    }
    
    #region Constant

    protected const string BaseUrl = "http://ja.dbpedia.org/";
    protected const string SparqlUrl = BaseUrl + "sparql";

    // ばね定数
    protected const float SpringConstant = 3;

    // 減衰定数
    protected const float SpringDecay = 3;

    // クーロン力の定数
    protected const float CoulombConstant = 0.5f;
    
    protected readonly Dictionary<(Type type1, Type type2), float> LinkedNodeDistance = new Dictionary<(Type t1, Type t2), float>
    {
        {(Type.ResourceNode, Type.ResourceNode), 2},
        {(Type.ResourceNode, Type.LiteralNode), 0.15f},
        {(Type.LiteralNode, Type.ResourceNode), 0.15f},
        {(Type.LiteralNode, Type.LiteralNode), 0.15f},
    };

    #endregion

    public Type NodeType { get; protected set; }
    public Dictionary<string, Node> linkedNodes = new Dictionary<string, Node>();

    private Camera _camera;
    protected Rigidbody NodeRigidbody;
    private LineRenderer _lineRenderer;

    protected virtual void Awake()
    {
        _camera = Camera.main;
        // _linkedNodes = GameObject.FindGameObjectsWithTag("Node").Select(go => go.GetComponent<Node>()).ToList();
        NodeRigidbody = GetComponent<Rigidbody>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    protected virtual void Start()
    {
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

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Contains("Node"))
            AddCoulombForce(other.GetComponentInParent<Node>());
    }

    protected virtual void Init()
    {
        // UpdateRotation();
    }

    private void UpdateLine()
    {
        _lineRenderer.useWorldSpace = true;
        var positions = linkedNodes.Select(node => node.Value.transform.position).ToList();
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
        var transform1 = transform;
        transform1.forward = transform1.position - _camera.transform.position;
    }

    #region AddForce

    private void AddForce()
    {
        // AddCoulombForce();
        AddSpringForce();
    }

    /// <summary>
    /// このノードと特定のノードとの間にクーロン力を適用する
    /// </summary>
    private void AddCoulombForce(Component node)
    {
        if (node == this) return;

        var vector = transform.position - node.transform.position;
        var distance = vector.magnitude;

        if (distance < 2)
        {
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
            var vector = transform.position - linkedNode.Value.transform.position;
            var distance = vector.magnitude;
            var direction = vector.normalized;

            var force = SpringConstant * (LinkedNodeDistance[(NodeType, linkedNode.Value.NodeType)] - distance) * direction -
                        SpringDecay * NodeRigidbody.velocity;
            NodeRigidbody.AddForce(force);
        }
    }

    #endregion
}