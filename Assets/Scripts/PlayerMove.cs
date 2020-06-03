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
    [HideInInspector]
    public bool movementEnabled;
    private bool _isMoving;
    
    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
    }

    /// <summary>
    /// This function moves the player around the screen. It only registers one movement per click of the mouse
    /// (no changing direction while moving!)
    /// </summary>
    private void MovePlayer()
    {
        if (Input.GetMouseButtonDown(0) && movementEnabled && !_isMoving)
        {
            _isMoving = true;
            SetAgentDestination();
            
            //TODO: Draw an overlapSphere on destination of the size of the player and check if it will kill a coin
        }

        if (Math.Abs(_agent.destination.x - transform.position.x) < 0.01f &&
            Math.Abs(_agent.destination.z - transform.position.z) < 0.01f && _isMoving)
        {
            _isMoving = false;
        }
    }

    private void SetAgentDestination()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, whatCanBeClickedOn))
        {
            _agent.SetDestination(new Vector3(hitInfo.point.x, 0.59f, hitInfo.point.z));
        }
        
    }
    
    private void InitializeComponents()
    {
        _camera = Camera.main;
        _agent = GetComponent<NavMeshAgent>();
    }

    public Vector3 GetDestination()
    {
        return _agent.destination;
    }
    
    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.51f);
    }*/



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
