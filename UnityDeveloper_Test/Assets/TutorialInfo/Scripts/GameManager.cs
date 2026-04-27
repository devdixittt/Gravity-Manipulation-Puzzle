using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Central manager that handles game state, win/lose conditions, and coordinates other systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float timeLimitSeconds = 120f; // 2 minutes

    [Header("Fall Detection")]
    [SerializeField] private float fallDeathThreshold = -20f; // Y position below which player dies

    // Events
    public event Action OnGameOver;
    public event Action OnGameWin;
    public event Action<float> OnTimerUpdate;

    private float _timeRemaining;
    private bool _gameActive = false;
    private int _totalCubes;
    private int _collectedCubes;

    public bool IsGameActive => _gameActive;
    public float TimeRemaining => _timeRemaining;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _timeRemaining = timeLimitSeconds;
        _totalCubes = FindObjectsOfType<CollectibleCube>().Length;
        _collectedCubes = 0;
        _gameActive = true;

        Debug.Log($"[GameManager] Game started. Total cubes: {_totalCubes}");
    }

    private void Update()
    {
        if (!_gameActive) return;

        HandleTimer();
        CheckFallDeath();
    }

    /// <summary>Counts down the timer and triggers game over on expiry.</summary>
    private void HandleTimer()
    {
        _timeRemaining -= Time.deltaTime;
        _timeRemaining = Mathf.Max(0f, _timeRemaining);
        OnTimerUpdate?.Invoke(_timeRemaining);

        if (_timeRemaining <= 0f)
            TriggerGameOver("Time's up!");
    }

    /// <summary>Checks if the player has fallen below the death threshold.</summary>
    private void CheckFallDeath()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.transform.position.y < fallDeathThreshold)
            TriggerGameOver("Fell off the level!");
    }

    /// <summary>Called by CollectibleCube when the player picks it up.</summary>
    public void RegisterCubeCollected()
    {
        _collectedCubes++;
        Debug.Log($"[GameManager] Cubes collected: {_collectedCubes}/{_totalCubes}");

        if (_collectedCubes >= _totalCubes)
            TriggerWin();
    }

    /// <summary>Ends the game with a loss.</summary>
    public void TriggerGameOver(string reason)
    {
        if (!_gameActive) return;
        _gameActive = false;
        Debug.Log($"[GameManager] GAME OVER — {reason}");
        OnGameOver?.Invoke();
    }

    /// <summary>Ends the game with a win.</summary>
    private void TriggerWin()
    {
        if (!_gameActive) return;
        _gameActive = false;
        Debug.Log("[GameManager] YOU WIN!");
        OnGameWin?.Invoke();
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void QuitGame() => Application.Quit();
}