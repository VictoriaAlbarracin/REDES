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
        _animator.SetFloat("Axi", Mathf.Abs(xAxi));    //tipo un modulo para q siempre sea positivo
    }
}
