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

    int _currentLife;
    Vector2 _moveDir;
    bool _isJumpPressed;
    int _jumpsPerformed;

    public event Action<float> onMovement;
    public override void Spawned()
    {
        if (HasStateAuthority)
            Camera.main.GetComponent<CameraMovement>().SetTarget(transform);

       _currentLife = _maxLife;
    }
    public override void Render() //funciona como un update y chequea si se presiono la tecla y le avisa al fixedupdate
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
           _isJumpPressed = true;
        }
    }
    public override void FixedUpdateNetwork() //usa callback
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, _groundLayer);
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

    public void RPC_TakeDamage(int dmg)
    {
        _currentLife -= dmg;

        if (_currentLife <= 0)
        {
            Dead();
        }
    }

    void Dead()
    {
        Runner.Despawn(Object);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
    }
}
