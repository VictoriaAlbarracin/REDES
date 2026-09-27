using System;
using UnityEngine;

public interface IPlayerEvents
{
    event Action<float> onMovement;
    event Action onShot;
    event Action<float> onLifeUpdated;
    event Action onDead;
}
