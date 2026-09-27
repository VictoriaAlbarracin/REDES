using UnityEngine;

[RequireComponent(typeof(Animator))]

public class PlayerView : MonoBehaviour
{
    Animator _animator;

    IPlayerEvents _playerEvents;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _playerEvents = GetComponentInParent<IPlayerEvents>();

        if (_playerEvents is null) return;
        _playerEvents.onMovement += SetMovementParameter;
    }

    public void SetMovementParameter(float xAxi)
    {
        _animator.SetFloat("axi", xAxi);    
    }
}
