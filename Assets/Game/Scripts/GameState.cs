using System;
using Characters.Player.Scripts;
using Game;
using Game.Facts;
using Game.StatusEffects;
using Props.Rooms.Scripts;
using Settings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class GameState : MonoBehaviour
{
    public UnityEvent onGamePaused = new();
    public UnityEvent onGameUnpaused = new();

    public bool paused { get; private set; }
    
    public StatusEffectList effectList { get; private set; }
    public FourSwordSettings settings { get; private set; }

    public FactState factState { get; private set; }

    [SerializeField]
    public float score;
    
    private static GameState _instance;
    public static GameState instance
    {
        get
        {
            if (_instance)
            {
                return _instance;
            }
            
            var container = new GameObject("GameState");
            
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            // Only called once per game
            _instance = container.AddComponent<GameState>();
            return _instance;
        }
    }

    public PerceptionSubsystem perceptionSubsystem { get; private set; } = new();
    
    public PlayerController activePlayer { get; private set; }

    private RoomArea _activeRoom;
    
    public void Awake()
    {
        _instance = this;
        settings = Resources.Load<FourSwordSettings>("FourSwordSettings");
        effectList = settings.statusEffects;
        
        factState = new FactState();
        var factRegistry = Resources.Load<FactRegistry>(FactRegistry.DefaultFactRegistryPath);
        var saveFile = new FactGameSave();
        factState.Initialize(factRegistry, saveFile);
        // DontDestroyOnLoad(gameObject);
    }

    /**
     * <summary>Pause the game!</summary>
     */
    public void Pause()
    {
        if (paused)
        {
            return;
        }
        
        onGamePaused.Invoke();
        Time.timeScale = 0.0f;
        paused = true;
    }

    /**
     * <summary>Unpause the game!</summary>
     */
    public void Unpause()
    {
        if (!paused)
        {
            return;
        }
        
        onGameUnpaused.Invoke();
        Time.timeScale = 1.0f;
        paused = false;
    }

    public void TogglePause()
    {
        if (paused)
        {
            Unpause();
            return;
        }
        
        Pause();
    }

    public void Save()
    {
        factState.Save();
    }

    public RoomArea GetActiveRoom()
    {
        return _activeRoom;
    }

    public void SetActiveRoom(RoomArea newRoom, RoomDoorTrigger entryPoint = null)
    {
        if (_activeRoom)
        {
            _activeRoom.DeactivateRoom();
        }

        _activeRoom = newRoom;
        
        if (!entryPoint)
        {
            _activeRoom.StartTransition();
            _activeRoom.EnterRoom();
            return;
        }
        
        activePlayer.SetDestination(entryPoint.transform.position);
        _activeRoom.StartTransition();
    }

    private void EndRoomTransition()
    {
        _activeRoom.EnterRoom();
    }

    public void RegisterPlayer(PlayerController player)
    {
        if (activePlayer)
        {
            activePlayer.forcedDestinationReached.RemoveListener(EndRoomTransition);
        }
        
        activePlayer = player;
        activePlayer.forcedDestinationReached.AddListener(EndRoomTransition);
    }

    private void OnGUI()
    {
        if (paused)
        {
            GUI.Label(new Rect(10, 10, 200, 50), "Paused");
        }
    }
}