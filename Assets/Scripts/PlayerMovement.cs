using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] InputActionReference _moveAction;
    Vector2 _moveDir;
    public override void FixedUpdateNetwork()
    {
        _moveDir = _moveAction.action.ReadValue<Vector2>();
    }
}
