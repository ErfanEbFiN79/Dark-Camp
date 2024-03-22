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

    [Header("Fire effect")]
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private ParticleSystem electEffect;

    [Header("Fire Setting")] 
    [SerializeField] private float timeBtwShoots;
    [SerializeField] private float timeBtwShootsH2;
    private float shotCounterH1;
    private float shotCounterH2;

    [Header("Heat Setting Gun 1")]
    [SerializeField] private float maxHeat;
    [SerializeField] private float heatPerShot; // per shot how mush add
    [SerializeField] private float coolRate;
    [SerializeField] private float overHeatCoolRate;
    private float heatCounter; // for show
    private bool overHeated;
    
    [Header("Heat Setting Gun 2")]
    [SerializeField] private float maxHeat2;
    [SerializeField] private float heatPerShot2; // per shot how mush add
    [SerializeField] private float coolRate2;
    [SerializeField] private float overHeatCoolRate2;
    private float heatCounter2; // for show
    private bool overHeated2;
    
    
    
    

 
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
        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shot();
            }
        
            if (Input.GetMouseButton(0))
            {
                shotCounterH1 -= Time.deltaTime;
                if (shotCounterH1 <= 0)
                {
                    Shot();
                }

            }

            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overHeatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                overHeated = false;
            }
        }

        if (!overHeated2)
        {
            
            if (Input.GetMouseButtonDown(1))
            {
                Shot2();
            }

            if (Input.GetMouseButton(1))
            {
                shotCounterH2 -= Time.deltaTime;
                if (shotCounterH2 <= 0)
                {
                    Shot2();
                }

            }
        }
        else
        {
            heatCounter2 -= overHeatCoolRate2 * Time.deltaTime;
            if (heatCounter2 <= 0)
            {
                overHeated2 = false;
            }
        }
        
        if (heatCounter <= 0)
        {
            heatCounter = 0;
        }
        
        if (heatCounter2 <= 0)
        {
            heatCounter2 = 0;
        }
        
        heatCounter2 -= coolRate2 * Time.deltaTime;
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
                0.6f,
                0.5f,
                0
            )
        );

        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Instantiate(
                fireEffect,
                hit.point,
                Quaternion.identity
            );
        }

        shotCounterH1 = timeBtwShoots;

        heatCounter += heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;

            overHeated = true;
            
        }
    }

    private void Shot2()
    {
        Ray ray = cam.ViewportPointToRay(
            new Vector3(
                0.4f,
                0.5f,
                0
            )
        );

        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Instantiate(
                electEffect,
                hit.point,
                Quaternion.identity
            );
        }
        shotCounterH2 = timeBtwShootsH2;
        
        heatCounter2 += heatPerShot2;
        if (heatCounter2 >= maxHeat2)
        {
            heatCounter2 = maxHeat2;

            overHeated2 = true;
            
        }
    }

    

    #endregion


    
    
    
}
