using UnityEngine;
using UnityEngine.UI;
using Tactile.TactileMatch3Challenge.ViewComponents;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int MaximunNumberOfMoves;
    private int NumberOfMovesUsed;
    private int NumberOfMovesLeft;
    [SerializeField] private int[] LevelObjective;
    private Text[] ScoreTexts;

    [SerializeField] private Text MovesUsed;
    [SerializeField] private Text MovesLeft;
    [SerializeField] private GameObject scoreTextRoot;

    private BoardRenderer boardRenderer;
    [SerializeField] private GameObject WinPanel;
    [SerializeField] private GameObject LosePanel;

    private void Start()
    {
        boardRenderer = FindObjectOfType<BoardRenderer>();
        boardRenderer.OnScore += Score;
        boardRenderer.OnTurnFinish += UseMove;
        NumberOfMovesLeft = MaximunNumberOfMoves;
        UpdateMovesText();
        ScoreTexts = scoreTextRoot.GetComponentsInChildren<Text>();
        for (int i = 0; i < ScoreTexts.Length; i++)
        {
            UpdateScoreText(i);
        }
    }

    public void UpdateMovesText()
    {
        MovesUsed.text = $"Moves used: {NumberOfMovesUsed}";
        MovesLeft.text = $"Moves left: {NumberOfMovesLeft}";
    }

    public void Score(int piece)
    {
        LevelObjective[piece]--;
        LevelObjective[piece] = Mathf.Clamp(LevelObjective[piece], 0, 1000);
        UpdateScoreText(piece);
        if (LevelIsCleared())
        {
            ShowWinMesssage();
        }
    }

    public bool LevelIsCleared()
    {
        for (int i = 0; i < LevelObjective.Length; i++)
        {
            if (LevelObjective[i] > 0)
            {
                return false;
            }
        }
        return true;
    }

    public void UseMove()
    {
        NumberOfMovesUsed++;
        NumberOfMovesLeft--;
        UpdateMovesText();
        if (NumberOfMovesLeft < 0)
        {
            ShowLoseMessage();
        }
    }

    public void ShowWinMesssage()
    {
        WinPanel.SetActive(true);
    }

    public void ShowLoseMessage()
    {
        LosePanel.SetActive(true);
    }

    public void UpdateScoreText(int index)
    {
        ScoreTexts[index].text = LevelObjective[index].ToString();
    }
}