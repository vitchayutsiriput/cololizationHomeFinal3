using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectNation : MonoBehaviour
{
    [SerializeField]
    private Sprite[] leaderSprites;

    [SerializeField]
    private Image leaderImage;

    [SerializeField]
    private string nextScene = "Map01";

    [SerializeField]
    private int index = 0;

    [SerializeField]
    private GameObject blackPanel;

    [SerializeField]
    private GameObject[] panels;

    public void SelectEuropeNation(int i)
    {
        index = i;
        Settings.playerNationId = index;

        if (leaderSprites[index] != null)
            leaderImage.sprite = leaderSprites[index];
    }

    public void StartGame()
    {
        SceneManager.LoadScene(nextScene);
    }

    public void ShowKingsCommand()
    {
        blackPanel.SetActive(true);
        panels[0].SetActive(true);
    }

    public void ShowPrepareToLeave()
    {
        panels[0].SetActive(false);
        panels[1].SetActive(true);
    }

    public void ShowLeftThePort()
    {
        panels[1].SetActive(false);
        panels[2].SetActive(true);
    }
}
