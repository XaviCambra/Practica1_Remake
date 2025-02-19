using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPPlayerController : MonoBehaviour
{
    float m_Yaw;
    float m_Pitch;
    public float m_YawRotationalSpeed;
    public float m_PitchRotationalSpeed;


    public float m_MinPitch;
    public float m_MaxPitch;

    public Transform m_PitchController;
    public bool m_UseYawInverted;
    public bool m_UsePitchInverted;

    public CharacterController m_CharacterController;
    public float m_Speed;
    public float m_FastSpeedMultiplier = 1.5f;

    [Header("Key controls")]
    public KeyCode m_LeftKeyCode;
    public KeyCode m_RightKeyCode;
    public KeyCode m_UpKeyCode;
    public KeyCode m_DownKeyCode;
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    public KeyCode m_RunKeyCode = KeyCode.LeftShift;
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockKeyCode = KeyCode.O;
    public KeyCode m_ReloadKeyCode;
    bool m_AngleLocked = false;
    bool m_AimLocked = true;
    


    [Header("Shoot")]
    public float m_MaxShootDistance = 50.0f;
    public LayerMask m_ShootingLayerMask;
    public GameObject m_DecalPrefab;
    TCObjectPool m_DecalsPool;
    public float m_MaxAmmoPerGun = 35.0f;
    public float m_AmmoInGun;
    public float m_TotalAmmo;

    float m_Life;
    float m_Shield;
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;

    [Header("Camera")]
    public Camera m_Camera;
    public float m_NormalMovementFOV = 60.0f;
    public float m_RunMovementFOV= 80.0f;

    public float m_VerticalSpeed = 0.0f;
    bool m_OnGround = true;

    public float m_JumpSpeed = 10.0f;
    bool m_Shooting = false;
    bool m_Reloading = false;
    
    [Header("Animations")]
    public Animation m_Animation;
    public AnimationClip m_IdleAnimationClip;
    public AnimationClip m_ShootAnimationClip;
    public AnimationClip m_ReloadAnimationClip;
    public AnimationClip m_WalkAnimationClip;
    public AnimationClip m_RunAnimationClip;
    public AnimationClip m_JumpAnimationClip;

    [Header("UI")]
    public Image m_LifeBarImage;
    public TextMeshProUGUI m_LifeText;
    public Image m_ShieldBarImage;
    public TextMeshProUGUI m_ShieldText;
    public GameObject m_AmmoCountInfo;

    void Start()
    {
        m_Life = GameControler.GetGameController().GetPlayerLife();
        m_Shield = GameControler.GetGameController().GetPlayerShield();
        GameControler.GetGameController().SetPlayer(this);
        m_Yaw = transform.rotation.y;
        m_Pitch = m_PitchController.localRotation.x;
        Cursor.lockState = CursorLockMode.Locked;
        m_AimLocked = Cursor.lockState == CursorLockMode.Locked;
        SetIdleWeaponAnimation();
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        m_TotalAmmo = 70.0f;
        Reload();

        GameObject l_Decal = m_DecalPrefab.gameObject;

        m_DecalsPool = new TCObjectPool(20, m_DecalPrefab);

        SetAmmoCounter();
    }

    #if UNITY_EDITOR
    void UpdateInputDebug()
    {
        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;
        if (Input.GetKeyDown(m_DebugLockKeyCode))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
            m_AimLocked = Cursor.lockState == CursorLockMode.Locked;
        }
        
    }
#endif

    void Update()
    {

#if UNITY_EDITOR
    UpdateInputDebug();
#endif
        Vector3 l_RightDirection = transform.right;
        Vector3 l_ForwardDirection = transform.forward;
        Vector3 l_Direction = Vector3.zero;
        float l_Speed = m_Speed;
        SetIdleWeaponAnimation();
        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");
#if UNITY_EDITOR
        if (m_AngleLocked)
        {
            l_MouseX = 0.0f;
            l_MouseY = 0.0f;
        }
#endif

        if (Input.GetKey(m_UpKeyCode))
        {
            l_Direction = l_ForwardDirection;
            SetWalkAnimation();
        }
        if (Input.GetKey(m_DownKeyCode))
        {
            l_Direction = -l_ForwardDirection;
            SetWalkAnimation();
        }
        if (Input.GetKey(m_RightKeyCode))
        {
            l_Direction += l_RightDirection;
            SetWalkAnimation();
        }
        if (Input.GetKey(m_LeftKeyCode))
        {
            l_Direction -= l_RightDirection;
            SetWalkAnimation();
        }
        if (Input.GetKey(m_JumpKeyCode) && m_OnGround)
        {
            SetJumpAnimation();
            m_VerticalSpeed = m_JumpSpeed;

        }
        float l_FOV = m_NormalMovementFOV;
        if (Input.GetKey(m_RunKeyCode))
        {
            SetRunAnimation();
            l_Speed = m_Speed*m_FastSpeedMultiplier;
            l_FOV = m_RunMovementFOV;

        }
        m_Camera.fieldOfView = l_FOV;

        if (Input.GetKeyDown(m_ReloadKeyCode))
        {
            Reload();
        }
        
        l_Direction.Normalize();
        Vector3 l_Movement = l_Direction * m_Speed * Time.deltaTime;

        m_Yaw = m_Yaw + l_MouseX * m_YawRotationalSpeed*Time.deltaTime*(m_UseYawInverted ? -1.0f : 1.0f);
        m_Pitch = m_Pitch + l_MouseY * m_PitchRotationalSpeed * Time.deltaTime * (m_UsePitchInverted ? -1.0f : 1.0f);
        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(0.0f, 0.0f, m_Pitch);
        

        m_VerticalSpeed = m_VerticalSpeed + Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;


        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);

        if ((l_CollisionFlags & CollisionFlags.Above) != 0 && m_VerticalSpeed>0.0f)
        {
            m_VerticalSpeed = 0.0f;
        }
        if ((l_CollisionFlags & CollisionFlags.Below)!=0)
        {
            m_VerticalSpeed = 0.0f;
            m_OnGround = true;
        }
        else
        {
            m_OnGround = false;
        }

        if (Input.GetMouseButtonDown(0) && CanShoot())
            Shoot();

    }

    
    bool CanShoot()
    {
        return !m_Shooting;
    }
    void Shoot()
    {
        if(m_AmmoInGun != 0)
        {
            Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit l_RaycastHit;
            if (Physics.Raycast(l_Ray, out l_RaycastHit, m_MaxShootDistance, m_ShootingLayerMask.value) && !m_Shooting)
            {
                if (l_RaycastHit.collider.tag == "DroneCollider")
                {
                    l_RaycastHit.collider.GetComponent<DronHP>().Hit(5);
                }
                else if (l_RaycastHit.collider.tag == "GalleryDummy")
                {
                    l_RaycastHit.collider.GetComponent<DummyHP>().Hit(50);
                }
                CreateShootHitParticles(l_RaycastHit.collider, l_RaycastHit.point, l_RaycastHit.normal);
                SetShootWeaponAnimation();
            }
            UpdateAmmoCounter();
            if (m_AmmoInGun == 0)
            {
                m_Reloading = true;
                Reload();
            }
        }
    }

    void CreateShootHitParticles(Collider _Collider, Vector3 Position, Vector3 Normal)
    {
        Debug.DrawRay(Position, Normal * 5.0f, Color.red, 2.0f);
        //GameObject.Instantiate(m_DecalPrefab, Position, Quaternion.LookRotation(Normal));
        
        GameObject l_Decal = m_DecalsPool.GetNextElement();
        l_Decal.SetActive(true);
        l_Decal.transform.position = Position;
        l_Decal.transform.rotation = Quaternion.LookRotation(Normal);
        
    }

    void SetIdleWeaponAnimation()
    {
        m_Animation.CrossFade(m_IdleAnimationClip.name);
    }


    void SetShootWeaponAnimation()
    {
        m_Animation.CrossFade(m_ShootAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.1f);
        StartCoroutine(EndShoot());
    }

    void SetWalkAnimation()
    {
        m_Animation.CrossFade(m_WalkAnimationClip.name);
    }

    void SetRunAnimation()
    {
        m_Animation.CrossFade(m_RunAnimationClip.name);
    }

    void SetJumpAnimation()
    {
        m_Animation.CrossFade(m_JumpAnimationClip.name);
    }

    void SetReloadAnimation()
    {
        m_Animation.CrossFade(m_ReloadAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.1f);
        StartCoroutine(EndReload());
    }

    public IEnumerator EndShoot()
    {
        yield return new WaitForSeconds(m_ShootAnimationClip.length);
        m_Shooting = false;
    }

    public IEnumerator EndReload()
    {
        yield return new WaitForSeconds(m_ReloadAnimationClip.length);
        m_Reloading = false;
    }

    public float GetLife()
    {
        return m_Life;
    }

    public float GetShield()
    {
        return m_Shield;
    }

    public float GetAmmo()
    {
        return m_TotalAmmo;
    }

    public void AddLife(float Life)
    {
        m_Life = Mathf.Clamp(m_Life + Life, 0.0f, 100.0f);
        LifeShieldBarUpdate();
    }

    public void AddShield(float shield)
    {
        m_Shield = Mathf.Clamp(m_Shield + shield, 0.0f, 100.0f);
        LifeShieldBarUpdate();
    }

    public void AddAmmo(float ammo)
    {
        m_TotalAmmo = m_TotalAmmo + ammo;
        AmmoCounter();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "healthItem")
        {
            Debug.Log("Health recogido");
            other.GetComponent<Item>().Pick(this);
        }
        if (other.tag == "shieldItem")
        {
            Debug.Log("Shield recogido");
            other.GetComponent<Item>().Pick(this);
        }
        if (other.tag == "ammoItem")
        {
            Debug.Log("Ammo recogido");
            other.GetComponent<Item>().Pick(this);
        }
        else if(other.tag == "Deadzone")
        {
            Kill();
        }
    }

    void Kill()
    {
        m_Life = 0.0f;
        GameControler.GetGameController().RestartGame();
    }

    public void RestartGame()
    {
        m_Life = 100;
        m_Shield = 100;
        m_LifeBarImage.fillAmount = m_Life;
        m_ShieldBarImage.fillAmount = m_Shield;
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }

    public void Hit(float life)
    {
        Debug.Log(life * 0.7f);

        if (life * 0.7f <= m_Shield)
        {
            m_Shield = m_Shield - life * 0.7f;
            m_Life = m_Life - life * 0.3f;
        }
        else
        {
            m_Life -= (life - m_Shield);
            m_Shield = 0;
        }

        LifeShieldBarUpdate();

        if (m_Life <= 0)
        {
            Kill();
        }
    }

    void LifeShieldBarUpdate()
    {
        m_LifeBarImage.fillAmount = m_Life / 100;
        m_LifeText.text = m_Life.ToString();
        m_ShieldBarImage.fillAmount = m_Shield / 100;
        m_ShieldText.text = m_Shield.ToString();
    }

    void SetAmmoCounter()
    {
        AmmoCounter();
    }

    public void AmmoCounter()
    {
        m_AmmoCountInfo.GetComponent<TextMeshProUGUI>().text = m_AmmoInGun + " / " + m_TotalAmmo;
    }

    void UpdateAmmoCounter()
    {
        m_AmmoInGun--;
        AmmoCounter();
    }

    void Reload()
    {
        float rest = m_MaxAmmoPerGun - m_AmmoInGun;

        if(m_TotalAmmo > rest)
        {
            m_TotalAmmo -= rest;
            m_AmmoInGun = m_MaxAmmoPerGun;
        }
        else
        {
            m_AmmoInGun += m_TotalAmmo;
            m_TotalAmmo = 0;
        }

        AmmoCounter();
    }
}
