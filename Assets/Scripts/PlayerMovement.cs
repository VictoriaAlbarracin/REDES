using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : NetworkBehaviour
{
    [Header ("Configuraciones del Player")]
    [SerializeField] InputActionReference _moveAction;
    [SerializeField] NetworkRigidbody3D _networkRigidbody;
    [SerializeField] float _speed;
    [SerializeField] int _maxLife;

    [Header("Configuraciones de Salto")]
    [SerializeField] int _maxJumps = 2;
    [SerializeField] float _jumpForce;
    [SerializeField] LayerMask _groundLayer;

    [Header("Dash")]
    [SerializeField] float _dashForce;
    [SerializeField] float _dashDuration;
    [SerializeField] float _dashCooldown;
    float _dashTimer;
    float _dashDir;
    float _currentDashCooldown;
    bool _isDashing;

    [Header("Escudo")]
    [SerializeField] float _shieldDuration = 2f;
    [SerializeField] float _shieldCooldown = 6f;
    float _shieldTimer;
    float _currentShieldCooldown;

    [Header("Disparo")]
    [SerializeField] Bullet _bulletPrefab;  //bala
    [SerializeField] Transform _bulletSpawnPoint;

    // sincroniza la vida en la red
    [Networked, OnChangedRender(nameof(CurrentLifeChanged))]
    int CurrentLife {  get; set; }

    // Sincroniza el estado del escudo en la red (para que todos sepan si tenes el escudo o no)
    [Networked] public NetworkBool IsShielded { get; set; }

    void CurrentLifeChanged()
    {
        onLifeUpdated?.Invoke(CurrentLife/(float)_maxLife);
       Debug.Log(CurrentLife); //por ahora solo un debug
    }

    Vector2 _moveDir;
    bool _isJumpPressed;
    int _jumpsPerformed;

    bool _isFirePressed;
    bool _isDashPresed;
    bool _isShieldPressed;

    //EVENTOS
    public event Action<float> onMovement;
    public event Action<float> onLifeUpdated;
    public event Action onDead;
    public override void Spawned()
    {
        LifebarManager.Instance.CreateLifebar(this);

        if (HasStateAuthority)
        {
            CurrentLife = _maxLife;
            Camera.main.GetComponent<CameraMovement>().SetTarget(transform);
        }

        CurrentLifeChanged();
    }
    public override void Render() //funciona como un update y chequea si se presiono la tecla, avisandole al fixedupdate
    {
        if (!HasStateAuthority) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
           _isJumpPressed = true;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame) _isFirePressed = true;

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame) _isDashPresed = true;

        if (Keyboard.current.eKey.wasPressedThisFrame) _isShieldPressed = true;
    }

    void Update() //miau
    {
       // _horizontalAxi = Input.GetAxis("Horizontal"); //no se por q tira error estoy siguiendo los bloques ....
    }

    private void FixedUpdate()
    {
       // transform.position += Vector3.forward * (_horizontalAxi * Time.fixedDeltaTime); //tira error... no c 
    }

    public override void FixedUpdateNetwork() //usa callback
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f, _groundLayer); //si el float es muy grande va a hacer doble saltos
        if (isGrounded)
        {
            _jumpsPerformed = 0;
        }
        _moveDir = _moveAction.action.ReadValue<Vector2>();

        float correctedX = -_moveDir.x;
        Movement(correctedX);

        if (_currentDashCooldown > 0) _currentDashCooldown -= Runner.DeltaTime;
        if (_currentShieldCooldown > 0) _currentShieldCooldown -= Runner.DeltaTime;


        if (_isJumpPressed)
        {
            if (_jumpsPerformed < _maxJumps)
            {
                Jump();
                _jumpsPerformed++;
            }
            _isJumpPressed = false;
        }

        if (_isFirePressed)
        {
            Shoot();
            _isFirePressed = false;
        }

        if (_isDashPresed)
        {
            if (_currentDashCooldown <= 0 && !_isDashing)
            {
                Dash();
            }
            _isDashPresed = false;
        }

        if (_isDashing)
        {
            _dashTimer -= Runner.DeltaTime;

            var velocity = _networkRigidbody.Rigidbody.linearVelocity;
            velocity.x = _dashDir * _dashForce;
            _networkRigidbody.Rigidbody.linearVelocity = velocity;

            if (_dashTimer <= 0)
            {
                _networkRigidbody.Rigidbody.linearVelocity = Vector3.zero;
                _isDashing = false;
            } 
        }

        if (_isShieldPressed)
        {
            if (_currentShieldCooldown <= 0 && !IsShielded)
            {
                ActivateShield();
            }
            _isShieldPressed = false;
        }

        if (IsShielded)
        {
            _shieldTimer -= Runner.DeltaTime;

            if (_shieldTimer <= 0)
            {
                IsShielded = false;
            }
        }

        void Movement(float moveX)
        {
            onMovement?.Invoke(moveX);

            if (moveX != 0)
                transform.right = Vector3.right * Mathf.Sign(moveX);

            _networkRigidbody.Rigidbody.linearVelocity += Vector3.right * (moveX * 10 * Runner.DeltaTime * _speed);

            if (Math.Abs(_networkRigidbody.Rigidbody.linearVelocity.x) <= _speed)
                return;

            var newVelocity = _networkRigidbody.Rigidbody.linearVelocity;
            newVelocity.x = _speed * moveX;
            _networkRigidbody.Rigidbody.linearVelocity = newVelocity;
        }

        void Jump()
        {
            var currentVelocity = _networkRigidbody.Rigidbody.linearVelocity;
            currentVelocity.y = 0;
            _networkRigidbody.Rigidbody.linearVelocity = currentVelocity;

            _networkRigidbody.Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        }
    }
    void Shoot()
    {
        Runner.Spawn(_bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
    }
    void Dash()
    {
        _isDashing = true;
        _dashTimer = _dashDuration;
        _currentDashCooldown = _dashCooldown;
        _dashDir = transform.right.x;
    }

    void ActivateShield()
    {
        IsShielded = true;
        _shieldTimer = _shieldDuration;
        _currentShieldCooldown = _shieldCooldown;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] //que cualquiera lo pueda llamar(source), que solo el que tenga autoridad lo pueda ejecutar (target)
    public void RPC_TakeDamage(int dmg)
    {
        if (IsShielded)
            return;

        CurrentLife -= dmg;

        if (CurrentLife <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Runner.Despawn(Object);
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        onDead?.Invoke();
    }
}
