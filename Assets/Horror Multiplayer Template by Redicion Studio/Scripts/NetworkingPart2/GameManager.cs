using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    public static bool GameBooted = false;

    public delegate void PlayerSpawned(GameObject _player, bool _observe);
    public static PlayerSpawned GameEvent_PlayerSpawned;

    public delegate void SpectatePlayer(GameObject _player);
    public static SpectatePlayer GameEvent_SpectatePlayer;

    public delegate void PlayerDespawned(GameObject _player);
    public static PlayerDespawned GameEvent_PlayerDespawned;

    public static void SpawnPlayer(GameObject _player, bool _observe) 
    {
        GameEvent_PlayerSpawned?.Invoke(_player, _observe);

        if (_observe)
            GameEvent_SpectatePlayer?.Invoke(_player);
    }
}
