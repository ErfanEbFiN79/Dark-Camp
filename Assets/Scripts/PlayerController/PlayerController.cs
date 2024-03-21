using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks
{
    #region Variables

    [Header("See and Rotate")] 
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private bool invertLook;
    [SerializeField] private Vector3 offsetCam;
    [SerializeField] private float minLock;
    [SerializeField] private float maxLock;
    private Camera cam;
    private float verticalRotStore;
    private Vector2 mouseInput;
   
    
    [Header("Move")] 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private MoveType moveType;
    [SerializeField] private CharacterController cR;
    
    private Vector3 moveDir, movment;
    private float activeMoveSpeed;
    private enum MoveType
    {
        Transform,
        CharacterController
    }
    
    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravityMod;

    

 
    #endregion  

    #region Unity Methods

    private void Start()
    {
        // lock camera
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        cR = GetComponent<CharacterController>();
    }

    private void Update()
    {
  
        // Get data
        GetData();
            
        // Rotate
        RotatePlayer();
        RotateGun();
        
        // Move
        ManageMove();
        
        //Gravity
        ManageGravity();
        
        // Mouse
        FreeMouse();
        
        // Shot
        ManageShot();
        
    }
    
    private void LateUpdate()
    {   
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    #endregion

    #region Data

    private void GetData()
    {
        // Rotate left and right
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        
        // Rotate up and down
        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, minLock,  maxLock);
        
        //Move
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        float yVel = movment.y;
        movment = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized;
        movment.y = yVel;
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }
    }

    #endregion
    
    #region Manage

    private void ManageMove()
    {
        switch (moveType)
        {
            case MoveType.Transform:
                Move();
                break;

            case MoveType.CharacterController:
                MoveCharacterController();
                break;
        }
        
    }

    private void ManageGravity()
    {
        if (cR.isGrounded)
        {
            Jump();
        }
        
        movment.y += Physics.gravity.y * Time.deltaTime * gravityMod;
    }
    
    private void FreeMouse()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void ManageShot()
    {
        if (Input.GetMouseButton(0))
        {
            Shot();
        }
    }


    #endregion

    #region Actions

    
    private void RotatePlayer()
    {
        var rotation = transform.rotation;
        rotation = Quaternion.Euler(
            rotation.eulerAngles.x,
            rotation.eulerAngles.y + mouseInput.x,
            rotation.eulerAngles.z
        );
        transform.rotation = rotation;
        
    }

    private void RotateGun()
    {
        if (!invertLook)
        {
            viewPoint.rotation = Quaternion.Euler(
                verticalRotStore,
                viewPoint.rotation.eulerAngles.y,
                viewPoint.rotation.eulerAngles.z
                );
        }
        else
        {
            viewPoint.rotation = Quaternion.Euler(
                -verticalRotStore,
                viewPoint.rotation.eulerAngles.y,
                viewPoint.rotation.eulerAngles.z
                );
        }
        
    }
    
    private void Move()
    {
        transform.position += movment * activeMoveSpeed * Time.deltaTime;
    }

    private void MoveCharacterController()
    {
        cR.Move(movment * activeMoveSpeed * Time.deltaTime);
    }
    
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        { 
            movment.y = jumpForce;
        }
        
    }

    private void Shot()
    {
        Ray ray = cam.ViewportPointToRay(
            new Vector3(
                0.5f,
                0.5f,
                0
            )
        );

        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"we hit" + hit.collider.gameObject.name);
        }
    }

    

    #endregion


    
    
    
}
