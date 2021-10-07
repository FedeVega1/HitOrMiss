using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager INS;

    [SerializeField] RectTransform difficultyButtonPanel;

    void Awake()
    {
        if (INS == null) INS = this;
        else Destroy(gameObject);
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
}
