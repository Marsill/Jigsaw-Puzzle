using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button easyButton;  // Reference to the Easy button
    public Button hardButton;  // Reference to the Hard button
    public Button mediumButton; // Reference to the Medium button (optional)
    public Puzzle puzzleManager;  // Reference to the Puzzle script

    void Start()
    {
        // Attach methods to button clicks
        easyButton.onClick.AddListener(OnEasyButtonClick);
        hardButton.onClick.AddListener(OnHardButtonClick);
        mediumButton.onClick.AddListener(OnMediumButtonClick); // Add listener for Medium button (optional)
    }

    void OnEasyButtonClick()
    {
        Debug.Log("Easy button clicked!");
        DifficultyManager.SetDifficulty("easy"); // Set difficulty to easy
        puzzleManager.SetPuzzleSize(3); // Set 3x3 grid for Easy difficulty
        HideButtons(); // Hide the buttons after selection
    }

    void OnHardButtonClick()
    {
        Debug.Log("Hard button clicked!");
        DifficultyManager.SetDifficulty("hard"); // Set difficulty to hard
        puzzleManager.SetPuzzleSize(5); // Set 5x5 grid for Hard difficulty
        HideButtons(); // Hide the buttons after selection
    }

    void OnMediumButtonClick()
    {
        Debug.Log("Medium button clicked!");
        DifficultyManager.SetDifficulty("medium"); // Set difficulty to medium
        puzzleManager.SetPuzzleSize(4); // Set 4x4 grid for Medium difficulty
        HideButtons(); // Hide the buttons after selection
    }

    // Method to hide all difficulty buttons
    void HideButtons()
    {
        easyButton.gameObject.SetActive(false);  // Disable Easy button
        hardButton.gameObject.SetActive(false);  // Disable Hard button
        mediumButton.gameObject.SetActive(false); // Disable Medium button
    }
}
