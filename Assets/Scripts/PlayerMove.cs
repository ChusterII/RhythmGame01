using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour
{

    public LayerMask whatCanBeClickedOn;
    
    private Camera _camera;
    private NavMeshAgent _agent;

    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, whatCanBeClickedOn))
        {
            _agent.SetDestination(hitInfo.point);
        }
    }
    
    private void InitializeComponents()
    {
        _camera = Camera.main;
        _agent = GetComponent<NavMeshAgent>();
    }


    #region Touch Controls
    /*private void MovePlayer()
    {
        //transform.position = Vector3.MoveTowards(transform.position, _touchPosition, speed * Time.deltaTime);
        _rb.MovePosition(_touchPosition);
    }

    private void HandleFingerTap(Lean.Touch.LeanFinger finger)
    {
        _touchPosition = finger.GetWorldPosition(_camera.orthographicSize, _camera);
    }*/
    

    #endregion
    
}
