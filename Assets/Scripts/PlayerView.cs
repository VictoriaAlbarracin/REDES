using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkMecanimAnimator))]

public class PlayerView : MonoBehaviour
{
    NetworkMecanimAnimator _mecanim;

    IPlayerEvents _playerEvents;

    private void Awake()
    {
        _mecanim = GetComponent<NetworkMecanimAnimator>();

        _playerEvents = GetComponentInParent<IPlayerEvents>();

        if (_playerEvents is null) return;
        _playerEvents.onMovement += SetMovementParameter;
    }

    public void SetMovementParameter(float xAxi)
    {
        _mecanim.Animator.SetFloat("Axi", Mathf.Abs(xAxi));    //tipo un modulo para q siempre sea positivo
    }
}
