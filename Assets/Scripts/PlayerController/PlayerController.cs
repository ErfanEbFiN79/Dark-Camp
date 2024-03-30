using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private ParticleSystem[] effectsGunOne;
    [SerializeField] private ParticleSystem[] effectsGunTwo;
    
    private float shotCounterH1;
    private float shotCounterH2;

    [Header("Heat Setting Gun 1")]
    [SerializeField] private float maxHeat;
    [SerializeField] private float coolRate;
    [SerializeField] private float overHeatCoolRate;
    private float heatCounter; // for show
    private bool overHeated;
    
    [Header("Heat Setting Gun 2")]
    [SerializeField] private float maxHeat2;
    [SerializeField] private float coolRate2;
    [SerializeField] private float overHeatCoolRate2;
    private float heatCounter2; // for show
    private bool overHeated2;

    [Header("Gun Manager")] 
    [SerializeField] private Gun[] gunsHandOne;
    [SerializeField] private Gun[] gunsHandTwo;
    [SerializeField] private GameObject[] gunsSelectShow;
    [SerializeField] private Slider slider1;
    [SerializeField] private Slider slider2;
    private int hand = 0;
    private int selectedGun1;
    private int selectedGun2;

    [Header("Impact")] 
    [SerializeField] private GameObject heatImpact;

    [Header("Hp")] 
    [SerializeField] private float maxHp;
    private float currentHp;
    
    #endregion  

    #region Unity Methods

    private void Start()
    {
        // lock camera
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        cR = GetComponent<CharacterController>();
        slider1.maxValue = maxHeat;
        slider2.maxValue = maxHeat2;
        currentHp = maxHp;

        if (photonView.IsMine)
        {
            UiController.instance.hpSlider.maxValue = maxHp;
            UiController.instance.hpSlider.value = currentHp;
        }


        //SwitchGun();
        
        photonView.RPC("SetGun1",RpcTarget.All, selectedGun1);
        photonView.RPC("SetGun2",RpcTarget.All, selectedGun2);
        
    }

    private void Update()
    {

        if (photonView.IsMine)
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
        
            // Manage guns
            ManageHands();
            ManageGuns();
        }

        
    }
    
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
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
            UiController.instance.GT1.gameObject.SetActive(false);
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
            UiController.instance.GT1.gameObject.SetActive(true);
            heatCounter -= overHeatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                overHeated = false;
            }
        }

        if (!overHeated2)
        {
            UiController.instance.GT2.gameObject.SetActive(false);
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
            UiController.instance.GT2.gameObject.SetActive(true);
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
        
        slider1.value = heatCounter;
        slider2.value = heatCounter2;
    }

    private void ManageHands()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            hand = hand == 0 ? 1 : 0;
        }
    }

    private void ManageGuns()
    {
        foreach (var obj in gunsSelectShow)
        {
            obj.SetActive(false);
        }
        
        gunsSelectShow[hand].SetActive(true);
        
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            if (hand == 0)
            {
                selectedGun1++;

                if (selectedGun1 >= gunsHandOne.Length)
                {
                    selectedGun1 = 0;
                }
            }
            else if(hand == 1)
            {
                selectedGun2++;

                if (selectedGun2 >= gunsHandTwo.Length)
                {
                    selectedGun2 = 0;
                }
            }
            
            photonView.RPC("SetGun1",RpcTarget.All, selectedGun1);
            photonView.RPC("SetGun2",RpcTarget.All, selectedGun2);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            if (hand == 0)
            {
                selectedGun1--;

                if (selectedGun1 < 0)
                {
                    selectedGun1 = gunsHandOne.Length - 1;
                }
            }
            else if(hand == 1)
            {
                selectedGun2--;

                if (selectedGun2 < 0)
                {
                    selectedGun2 = gunsHandTwo.Length - 1;
                }
            }
            photonView.RPC("SetGun1",RpcTarget.All, selectedGun1);
            photonView.RPC("SetGun2",RpcTarget.All, selectedGun2);
            
        }

        if (hand == 0)
        {
            for (int i = 0; i < gunsHandOne.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun1 = i;
                    photonView.RPC("SetGun1",RpcTarget.All, selectedGun1);
                    photonView.RPC("SetGun2",RpcTarget.All, selectedGun2);
                }
            }
        }
        else
        {
            for (int i = 0; i < gunsHandTwo.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun2 = i;
                    photonView.RPC("SetGun1",RpcTarget.All, selectedGun1);
                    photonView.RPC("SetGun2",RpcTarget.All, selectedGun2);
                }
            }
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
                0.6f,
                0.5f,
                0
            )
        );

        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage",RpcTarget.All, photonView.Owner.NickName,
                    gunsHandOne[selectedGun1].Damage, PhotonNetwork.LocalPlayer.ActorNumber);
                PhotonNetwork.Instantiate(
                    heatImpact.name,
                    hit.point,
                    Quaternion.identity
                );
            }
            else
            {
                Instantiate(
                    effectsGunOne[selectedGun1],
                    hit.point,
                    Quaternion.identity
                );
            }


            
        }

        shotCounterH1 = gunsHandOne[selectedGun1].TimeBtwShot;

        heatCounter += gunsHandOne[selectedGun1].HeatPerShot;
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

            if (hit.collider.gameObject.CompareTag("Player"))
            {
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage",RpcTarget.All, photonView.Owner.NickName, gunsHandTwo[selectedGun2].Damage,
                    PhotonNetwork.LocalPlayer.ActorNumber);
                PhotonNetwork.Instantiate(
                    heatImpact.name,
                    hit.point,
                    Quaternion.identity
                );
            }
            else
            {
                
                Instantiate(
                    effectsGunTwo[selectedGun2],
                    hit.point,
                    Quaternion.identity
                );
            }


        }
        shotCounterH2 = gunsHandTwo[selectedGun2].TimeBtwShot;
        heatCounter2 += gunsHandTwo[selectedGun2].HeatPerShot;
        if (heatCounter2 >= maxHeat2)
        {
            heatCounter2 = maxHeat2;

            overHeated2 = true;
            
        }
    }

    private void SwitchGun()
    {
        foreach (Gun gun in gunsHandOne)
        {
            gun.gameObject.SetActive(false);
        }
        
        foreach (Gun gun in gunsHandTwo)
        {
            gun.gameObject.SetActive(false);
        }
        
        gunsHandOne[selectedGun1].gameObject.SetActive(true);
        gunsHandTwo[selectedGun2].gameObject.SetActive(true);
    }
    

    #endregion

    #region RPC

    [PunRPC]
    private void DealDamage(string whoGetDamage, float damageAmount, int actor)
    {
        TakeDamage(whoGetDamage,damageAmount, actor);
    }

    [PunRPC]
    private void SetGun1(int gunToSwitchTo)
    {
        if (gunToSwitchTo < gunsHandOne.Length)
        {
            selectedGun1 = gunToSwitchTo;
            SwitchGun();
        }
    }

    [PunRPC]
    private void SetGun2(int gunToSwitchTo)
    {
        if (gunToSwitchTo < gunsHandTwo.Length)
        {
            selectedGun2 = gunToSwitchTo;
            SwitchGun();
        }
    }

    #endregion

    #region Damage System

    private void TakeDamage(string whoGetDamage, float damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            // Debug.Log(photonView.Owner.NickName + " has been hit by " + whoGetDamage);
            currentHp -= damageAmount;

            if (currentHp <= 0)
            {
                currentHp = 0;
                SpawnPlayerNetwork.instance.Die(whoGetDamage);
                MatchManager.instance.ChangeStatSend(actor,0,1);
            }
            UiController.instance.hpSlider.value = currentHp;
        }

    } 

    #endregion
    
}
