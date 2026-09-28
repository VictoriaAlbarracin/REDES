using Fusion;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance;

    [SerializeField] GameObject _victoryCanvas;
    [SerializeField] GameObject _defeatCanvas;

    bool _gameFinished;

    public override void Spawned()
    {
        Instance = this;
        _victoryCanvas.SetActive(false);
        _defeatCanvas.SetActive(false);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerDied(PlayerRef loser)
    {
        if (_gameFinished) return;

        _gameFinished = true;

        PlayerRef winner = PlayerRef.None;

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (player != loser)
            {
                winner = player;
                break;
            }
        }

        if (winner == PlayerRef.None)
            return;

        RPC_ShowResult(winner, loser);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ShowResult(PlayerRef winner, PlayerRef loser)
    {
        if (Runner.LocalPlayer == winner) _victoryCanvas.SetActive(true);
        else if (Runner.LocalPlayer == loser) _defeatCanvas.SetActive(true);
    }
}
