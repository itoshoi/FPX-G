using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerManager : MonoBehaviour
{
    private SteamVR_Input_Sources _handType = SteamVR_Input_Sources.Any;
    [SerializeField] private SteamVR_Action_Vector2 direction;
    [SerializeField] private SteamVR_Action_Skeleton skeleton;
    [SerializeField] private GameObject cameraRig;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        var dir = direction.GetAxis(_handType);
        var moveDir = new Vector3(dir.x, 0, dir.y);
        cameraRig.transform.position += _camera.transform.rotation * moveDir * Time.deltaTime;
    }
}
