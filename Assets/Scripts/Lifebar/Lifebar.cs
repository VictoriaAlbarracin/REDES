using UnityEngine;
using UnityEngine.UI;
using static Fusion.Sockets.NetBitBuffer;

public class Lifebar : MonoBehaviour
{
    Transform _target;
    [SerializeField] Image _lifebarImage;
    [SerializeField] float _yOffset;

    public void Initialize(Transform target)
    {
        _target = target;
    }

    public void UpdateImage(float amount)
    {
        _lifebarImage.fillAmount = amount;
    }

    public void UpdatePosition()
    {
        transform.position = _target.position + Vector3.up * _yOffset; //actualizamos la posicion y el offset de la barra en base al player
    }
}
