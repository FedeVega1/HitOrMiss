using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DifficultyLevel { Easy, Medium, Hard }

public class LevelManager : MonoBehaviour
{
    public static LevelManager INS;

    [SerializeField] ObjectSpawner objectSpawner;
    [SerializeField] InputManager inputManager;

    float _RoundTime;
    public float CurrentRoundTime
    {
        get => _RoundTime;

        set
        {
            _RoundTime = value;
            if (_RoundTime < 0) GameOver(false);

            UIManager.INS.UpdateRoundTimer(_RoundTime);
        }
    }

    int _PlayerScore;
    public int PlayerScore
    {
        get => _PlayerScore;

        set
        {
            _PlayerScore = value;
            if (_PlayerScore >= 100) GameOver(true);

            UIManager.INS.UpdatePlayerScore(_PlayerScore, 100);
        }
    }


    public DifficultyLevel CurrentDifficultyLevel { get; private set; }

    bool canCountTime;
    Dictionary<DifficultyLevel, DifficultyData> difficulityData;

    void Awake()
    {
        if (INS == null) INS = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        DifficultyData[] data = Resources.LoadAll<DifficultyData>("DifficultyData");

        if (data != null)
        {
            UIManager.INS.SetUpDifficultyButtons(ref data);
            difficulityData = new Dictionary<DifficultyLevel, DifficultyData>();

            int size = data.Length;
            for (int i = 0; i < size; i++) difficulityData.Add(data[i].difficultyLevel, data[i]);
        }
        else
        {
            Debug.LogError("Couldn't load DifficultyData");
        }
    }

    void Update()
    {
        if (!canCountTime) return;
        CurrentRoundTime -= Time.deltaTime;
    }

    public void StartGame(DifficultyLevel difficultyLevel)
    {
        CurrentDifficultyLevel = difficultyLevel;
        DifficultyData data = difficulityData[CurrentDifficultyLevel];

        objectSpawner.OnGameStart(data);
        inputManager.EnableInput = true;

        canCountTime = true;
        CurrentRoundTime = data.maxRoundTime;
        UIManager.INS.StartGame();
    }

    void GameOver(bool win)
    {
        canCountTime = false;
        _RoundTime = 0;

        UIManager.INS.OnGameOver(win);
        objectSpawner.OnGameOver();
        inputManager.EnableInput = false;
    }

    public void RestartLevel() => UnityEngine.SceneManagement.SceneManager.LoadScene("Level01");

    public void ScorePoints(int ammount) => PlayerScore += ammount;
    public void RemovePoints(int ammount) => PlayerScore -= ammount;
}
