using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager INS;

    [SerializeField] RectTransform difficultyButtonPanel, scoreRect, btnExit;
    [SerializeField] Text timer, playerScore, lblGameOver, lblRetry;
    [SerializeField] CanvasGroup gameOverCanvas, obscurerCanvas;
    [SerializeField] Toggle helperPointsToggle;
    [SerializeField] Image roundTimerFill;

    void Awake()
    {
        if (INS == null) INS = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        helperPointsToggle.isOn = LevelManager.INS.EnableHelperPoints;

        FadeOut();
        UpdatePlayerScore(0, 100);
    }

    public void StartGame()
    {
        LeanTween.moveY(roundTimerFill.rectTransform, -18, .25f).setEaseOutBounce();
        LeanTween.moveY(scoreRect, -50, .25f).setEaseOutSine();
        LeanTween.moveY(btnExit, 150, .25f).setEaseOutSine();
    }

    public void SetUpDifficultyButtons(ref DifficultyData[] data)
    {
        int index = 0;
        int size = data.Length;
        foreach (RectTransform rect in difficultyButtonPanel)
        {
            rect.gameObject.SetActive(true);
            Button buttonScript = rect.GetComponent<Button>();

            if (buttonScript != null)
            {
                DifficultyData diffData = data[index];

                buttonScript.GetComponentInChildren<Text>().text = diffData.difficultyLevel.ToString();
                buttonScript.onClick.AddListener(() => { LevelManager.INS.StartGame(diffData.difficultyLevel); });
                buttonScript.onClick.AddListener(HideDifficultyPanel);
            }

            index++;
            if (index >= size) break;
        }
    }

    void HideDifficultyPanel() => LeanTween.moveY(difficultyButtonPanel, -1500, .25f).setEaseOutSine();

    public void UpdateRoundTimer(float time, float normalizedRoundTime)
    {
        roundTimerFill.fillAmount = Mathf.Lerp(roundTimerFill.fillAmount, normalizedRoundTime, Time.deltaTime * 8);

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(Mathf.Abs((minutes * 60) - time));
        timer.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdatePlayerScore(int score, int maxScore) => playerScore.text = $"{score:000}/{maxScore:000}";

    public void OnGameOver(bool win)
    {
        LeanTween.alphaCanvas(gameOverCanvas, 1, .25f).setOnComplete(() =>
        {
            gameOverCanvas.interactable = gameOverCanvas.blocksRaycasts = true;
        });

        lblGameOver.text = win ? "You won!" : "You Lose";
        lblRetry.text = win ? "Continue" : "Retry";
    }

    public void RestartGame()
    {
        FadeIn();
        LeanTween.value(0, 1, .5f).setOnComplete(LevelManager.INS.RestartLevel);
    }

    public void FadeIn() => LeanTween.alphaCanvas(obscurerCanvas, 1, .5f).setEaseInSine();
    public void FadeOut() => LeanTween.alphaCanvas(obscurerCanvas, 0, .5f).setEaseOutSine();

    public void ToggleHelperPoints() => LevelManager.INS.EnableHelperPoints = helperPointsToggle.isOn;
}
