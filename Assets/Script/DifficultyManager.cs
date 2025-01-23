using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static string selectedDifficulty = "easy";  // Default difficulty is "easy"

    // You can provide a way to change difficulty through UI or other mechanisms
    public static void SetDifficulty(string difficulty)
    {
        if (difficulty == "easy" || difficulty == "medium" || difficulty == "hard")
        {
            selectedDifficulty = difficulty;
        }
        else
        {
            Debug.LogError("Invalid difficulty level");
        }
    }
}
