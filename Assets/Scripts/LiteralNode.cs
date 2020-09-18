using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LiteralNode : Node
{
    [SerializeField] private TextMeshPro textMeshPro;
    public string Literal { get; private set; }

    public static LiteralNode Instantiate(string literal, Vector3 pos, Transform parent)
    {
        var prefab = Resources.Load<LiteralNode>("LiteralNode");
        var obj = Instantiate(prefab, pos, Quaternion.identity, parent);
        obj.Literal = literal;
        return obj;
    }

    protected override void Init()
    {
        base.Init();
        NodeType = Type.LiteralNode;
        textMeshPro.text = Literal;
    }
}
