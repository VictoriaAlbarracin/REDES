using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] NetworkPrefabRef _playerPrefab;
    [SerializeField] List<Transform> _spawnPoint;

    int index;
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            var playerIndex = Runner.SessionInfo.PlayerCount-1;
            var spawnPoint = _spawnPoint[playerIndex];

            Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
