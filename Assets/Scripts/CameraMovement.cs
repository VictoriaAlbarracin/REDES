using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    Transform _target;
    [SerializeField] private Vector3 offset;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (!_target) return;

        var newPosition = _target.position + offset;
        newPosition.z = transform.position.z;
        transform.position = newPosition;
    }
}
