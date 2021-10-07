using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager INS;

    [SerializeField] RectTransform difficultyButtonPanel;
    [SerializeField] Text timer, playerScore;

    void Awake()
    {
        if (INS == null) INS = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        LeanTween.moveY(timer.rectTransform, 0, .25f).setEaseOutBounce();
        LeanTween.moveY(playerScore.rectTransform, -73, .25f).setEaseOutBounce();
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

    public void UpdateRoundTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(Mathf.Abs((minutes * 60) - time));
        timer.text = $"{minutes:00}:{seconds:00}";
    }
    public void UpdatePlayerScore(int score, int maxScore)
    {
        if (score < 0) return;
        playerScore.text = $"{score:000}/{maxScore:000}";
    }
}
