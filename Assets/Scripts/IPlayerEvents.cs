using System;
using UnityEngine;

public interface IPlayerEvents
{
    event Action<float> onMovement;
}
