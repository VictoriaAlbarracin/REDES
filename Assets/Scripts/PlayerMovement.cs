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

    [Header("Disparo")]
    [SerializeField] Bullet _bulletPrefab;  //bala
    [SerializeField] Transform _bulletSpawnPoint;

    [Networked, OnChangedRender(nameof(CurrentLifeChanged))]
    int CurrentLife {  get; set; }

    void CurrentLifeChanged() => Debug.Log(CurrentLife); //por ahora solo un debug

    Vector2 _moveDir;
    bool _isJumpPressed;
    int _jumpsPerformed;

    bool _isFirePressed;

    public event Action<float> onMovement;
    public override void Spawned()
    {
        if (HasStateAuthority)
            Camera.main.GetComponent<CameraMovement>().SetTarget(transform);

       CurrentLife = _maxLife;
    }
    public override void Render() //funciona como un update y chequea si se presiono la tecla y le avisa al fixedupdate
    {
        if (!HasStateAuthority) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
           _isJumpPressed = true;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame) _isFirePressed = true;
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] //que cualquiera lo pueda llamar, que solo el que tenga autoridad lo pueda ejecutar
    public void RPC_TakeDamage(int dmg)
    {
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
    }
}
