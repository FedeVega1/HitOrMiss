using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DifficultyLevel { Easy, Medium, Hard }

public class LevelManager : MonoBehaviour
{
    public static LevelManager INS;

    [SerializeField] ObjectSpawner objectSpawner;
    [SerializeField] InputManager inputManager;

    public DifficultyLevel CurrentDifficultyLevel { get; private set; }

    public System.Action OnGameStart;

    void Awake()
    {
        if (INS == null) INS = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        DifficultyData[] data = Resources.LoadAll<DifficultyData>("DifficultyData");
        if (data != null) UIManager.INS.SetUpDifficultyButtons(ref data);
        else Debug.LogError("Couldn't load DifficultyData");
    }

    public void StartGame(DifficultyLevel difficultyLevel)
    {
        CurrentDifficultyLevel = DifficultyLevel.Easy;
        OnGameStart?.Invoke();

        inputManager.EnableInput = true;
    }
}
