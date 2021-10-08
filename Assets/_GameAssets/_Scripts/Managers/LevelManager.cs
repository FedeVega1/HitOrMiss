using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DifficultyLevel { Easy, Medium, Hard }

public class LevelManager : MonoBehaviour
{
    public static LevelManager INS;

    [SerializeField] ObjectSpawner objectSpawner;
    [SerializeField] InputManager inputManager;
    [SerializeField] AudioSource gameOverASrc, mainMusicASrc;
    [SerializeField] AudioClip[] gameOverClips;
    [SerializeField] float maxMusicVolume = .5f, maxSFXVolume = .7f;

    float _RoundTime;
    public float CurrentRoundTime
    {
        get => _RoundTime;

        set
        {
            _RoundTime = value;
            if (_RoundTime < 0) GameOver(false);

            UIManager.INS.UpdateRoundTimer(_RoundTime, _RoundTime / maxTime);
        }
    }

    int _PlayerScore;
    public int PlayerScore
    {
        get => _PlayerScore;

        set
        {
            _PlayerScore = Mathf.Clamp(value, 0, 100);
            if (_PlayerScore >= 100) GameOver(true);

            UIManager.INS.UpdatePlayerScore(_PlayerScore, 100);
        }
    }

    int _EnableHelperPoints = -1;
    public bool EnableHelperPoints 
    { 
        get
        {
            if (_EnableHelperPoints == -1)
            {
                if (PlayerPrefs.HasKey("EnableHelperPoints"))
                {
                    _EnableHelperPoints = PlayerPrefs.GetInt("EnableHelperPoints");
                }
                else
                {
                    _EnableHelperPoints = 1;
                    PlayerPrefs.SetInt("EnableHelperPoints", _EnableHelperPoints);
                }
            }

            return _EnableHelperPoints == 1;
        }

        set
        {
            _EnableHelperPoints = value ? 1 : 0;
            PlayerPrefs.SetInt("EnableHelperPoints", _EnableHelperPoints);
        }
    }

    float _MusicVolume = -1;
    public float MusicVolume
    {
        get
        {
            if (_MusicVolume == -1)
            {
                if (PlayerPrefs.HasKey("Music"))
                {
                    _MusicVolume = PlayerPrefs.GetFloat("Music");
                }
                else
                {
                    _MusicVolume = maxMusicVolume;
                    PlayerPrefs.SetFloat("Music", _MusicVolume);
                }

                mainMusicASrc.volume = _MusicVolume;
            }

            return _MusicVolume;
        }

        set
        {
            _MusicVolume = Mathf.Clamp(maxMusicVolume * value, 0, maxMusicVolume);
            mainMusicASrc.volume = _MusicVolume;
            PlayerPrefs.SetFloat("Music", _MusicVolume);
        }
    }

    float _SFXVolume = -1;
    public float SFXVolume
    {
        get
        {
            if (_SFXVolume == -1)
            {
                if (PlayerPrefs.HasKey("SFX"))
                {
                    _SFXVolume = PlayerPrefs.GetFloat("SFX");
                }
                else
                {
                    _SFXVolume = maxSFXVolume;
                    PlayerPrefs.SetFloat("SFX", _SFXVolume);
                }

                gameOverASrc.volume = _SFXVolume;
                objectSpawner.SetVolumeOfAllObjects(_SFXVolume);
            }

            return _SFXVolume;
        }

        set
        {
            _SFXVolume = Mathf.Clamp(maxSFXVolume * value, 0, maxSFXVolume);

            gameOverASrc.volume = _SFXVolume;
            objectSpawner.SetVolumeOfAllObjects(_SFXVolume);

            PlayerPrefs.SetFloat("SFX", _SFXVolume);
        }
    }

    public DifficultyLevel CurrentDifficultyLevel { get; private set; }

    bool canCountTime;
    float maxTime;
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

        SetupSound();
    }

    void Update()
    {
        if (!canCountTime) return;
        CurrentRoundTime -= Time.deltaTime;
    }

    void SetupSound()
    {
        float normalizedMusic = MusicVolume / maxMusicVolume;
        float normalizedSFX = SFXVolume / maxSFXVolume;

        if (PlayerPrefs.HasKey("MusicMuted"))
        {
            bool toggle = PlayerPrefs.GetInt("MusicMuted") == 1;
            if (toggle) normalizedMusic = 0;
            ToggleMusic(toggle);
        }
        else
        {
            PlayerPrefs.SetInt("MusicMuted", 0);
        }

        if (PlayerPrefs.HasKey("SFXMuted"))
        {
            bool toggle = PlayerPrefs.GetInt("SFXMuted") == 1;
            if (toggle) normalizedSFX = 0;
            ToggleMusic(toggle);
        }
        else
        {
            PlayerPrefs.SetInt("SFXMuted", 0);
        }

        UIManager.INS.InitVolumePanels(normalizedMusic, normalizedSFX);
    }

    public void StartGame(DifficultyLevel difficultyLevel)
    {
        CurrentDifficultyLevel = difficultyLevel;
        DifficultyData data = difficulityData[CurrentDifficultyLevel];

        objectSpawner.OnGameStart(data);
        inputManager.EnableInput = true;

        canCountTime = true;
        CurrentRoundTime = maxTime = data.maxRoundTime;
        UIManager.INS.StartGame();

        mainMusicASrc.Play();
        LeanTween.value(0, MusicVolume, .25f).setOnUpdate((value) => { mainMusicASrc.volume = value; });
    }

    void GameOver(bool win)
    {
        canCountTime = false;
        _RoundTime = 0;

        gameOverASrc.clip = gameOverClips[win ? 0 : 1];
        gameOverASrc.Play();

        UIManager.INS.OnGameOver(win);
        objectSpawner.OnGameOver();
        inputManager.EnableInput = false;

        LeanTween.value(MusicVolume, 0, .25f).setOnUpdate((value) => { mainMusicASrc.volume = value; }).setOnComplete(() => { mainMusicASrc.Stop(); });
    }

    public void ExitGame() => Application.Quit();
    public void RestartLevel() => UnityEngine.SceneManagement.SceneManager.LoadScene("Level01");

    public void ScorePoints(int ammount) => PlayerScore += ammount;
    public void RemovePoints(int ammount) => PlayerScore -= ammount;

    public void ToggleMusic(bool toggle)
    {
        mainMusicASrc.volume = toggle ? MusicVolume : 0;
        PlayerPrefs.SetInt("MusicMuted", toggle ? 1 : 0);
    }

    public void ToggleSFX(bool toggle)
    {
        gameOverASrc.volume = toggle ? SFXVolume : 0;
        objectSpawner.SetVolumeOfAllObjects(toggle ? SFXVolume : 0);
        PlayerPrefs.SetInt("SFXMuted", toggle ? 1 : 0);
    }
}
