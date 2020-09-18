using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MyController : MonoBehaviour
{
    [SerializeField] private SteamVR_Action_Boolean trigger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Node/Resource") && trigger.GetState(SteamVR_Input_Sources.Any))
        {
            other.GetComponent<ResourceNode>().InstantiateLinkedNodes(SystemManager.Instance.LinkedNodeLimit);
            other.GetComponent<ResourceNode>().InstantiateStatsNodes();
        }
    }
}
