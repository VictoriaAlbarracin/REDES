using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] NetworkRigidbody3D _networkRb;
    [SerializeField] float _initialForce;
    [SerializeField] int _dmg;
    [SerializeField] float _lifeTime;

    TickTimer _lifeTimer;

    public override void Spawned()
    {
        _networkRb.Rigidbody.AddForce(transform.right * _initialForce, ForceMode.VelocityChange);

        _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!_lifeTimer.Expired(Runner)) return;
    
        Runner.Despawn(Object);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        Runner.Despawn(Object);
    }
}
