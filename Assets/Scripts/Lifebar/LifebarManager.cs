using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LifebarManager : MonoBehaviour
{
    public static LifebarManager Instance { get; private set;}
    [SerializeField] Lifebar _lifebarPrefab;
    List<Lifebar> _lifebarInUse;

    private void Awake()
    {
        Instance = this;
        _lifebarInUse = new List<Lifebar>();
    }
    public void CreateLifebar(PlayerMovement player)
    {
        var newLifebar = Instantiate(_lifebarPrefab, transform);
        newLifebar.Initialize(player.transform);
        player.onLifeUpdated += newLifebar.UpdateImage;
        _lifebarInUse.Add(newLifebar);
        player.onDead += () =>
        {
            _lifebarInUse.Remove(newLifebar);
            Destroy(newLifebar.gameObject);
        };
    }
    private void LateUpdate()
    {
        foreach (var lifebar in _lifebarInUse) //por cada barra de vida actualizamos su posicion
        {
            lifebar.UpdatePosition();
        }
    }
}
