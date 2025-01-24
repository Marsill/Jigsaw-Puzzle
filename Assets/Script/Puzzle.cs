using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private AudioSource audioSource; // Reference to AudioSource
    [SerializeField] private AudioClip completionClip; // Sound for game completion
    [SerializeField] private AudioClip gameOverClip;   // Sound for game over
    [SerializeField] private int maxTrials = 20; // Maximum number of trials

    private int emptyLocation;
    private int size; // Grid size (3x3, 5x5, or 6x6)
    private int trials = 0; // Count the number of trials
    private bool shuffling = false;

    private List<Transform> pieces = new List<Transform>();

    // Method to change puzzle size (Easy, Medium, or Hard)
    public void SetPuzzleSize(int newSize)
    {
        size = newSize;
        trials = 0; // Reset the trial count when the size changes
        pieces.Clear(); // Clear existing pieces
        CreatePieces(0.01f); // Recreate the pieces with the new size
    }

    void CreatePieces(float gapThickness)
    {
        float width = 1f / size;

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);
                piece.localPosition = new Vector3(
                    -1f + (2f * width * col) + width,
                    1f - (2f * width * row) - width,
                    0f
                );
                piece.localScale = ((2f * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";

                // Set up UV mapping for the piece
                MeshFilter meshFilter = piece.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.mesh;
                    Vector2[] uv = new Vector2[4];

                    uv[0] = new Vector2(col * width, 1 - ((row + 1) * width));   // Bottom-left
                    uv[1] = new Vector2((col + 1) * width, 1 - ((row + 1) * width)); // Bottom-right
                    uv[2] = new Vector2(col * width, 1 - (row * width));       // Top-left
                    uv[3] = new Vector2((col + 1) * width, 1 - (row * width)); // Top-right

                    mesh.uv = uv;
                }

                // Hide the last piece to create an empty space
                if (row == size - 1 && col == size - 1)
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false); // Hide the last piece
                }
            }
        }
    }


    void Start()
    {
        string selectedDifficulty = DifficultyManager.selectedDifficulty;

        // Set the grid size based on selected difficulty.
        if (selectedDifficulty == "easy")
        {
            size = 3; // Easy: 3x3 grid
        }
        else if (selectedDifficulty == "medium")
        {
            size = 4; // Medium: 4x4 grid
        }
        else if (selectedDifficulty == "hard")
        {
            size = 5; // Hard: 5x5 grid
        }
        else
        {
            size = 3; // Default to easy if no valid selection
        }

        // Initialize the puzzle with the selected grid size
        pieces = new List<Transform>();
        CreatePieces(0.01f); // Recreate the pieces with the new size
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == hit.transform)
                    {
                        if (SwapIfValid(i, -size, size)) { break; }
                        if (SwapIfValid(i, +size, size)) { break; }
                        if (SwapIfValid(i, -1, 0)) { break; }
                        if (SwapIfValid(i, +1, size - 1)) { break; }
                    }
                }
                trials++; // Increment the trial count
                CheckGameOver(); // Check if game over condition is met
            }
        }

        if (!shuffling && CheckCompletion())
        {
            PlayAudio(completionClip); // Play completion sound
            Debug.Log("Puzzle completed!");
            shuffling = true;
            StartCoroutine(WaitShuffle(0.5f));
        }
    }

    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }
        return true;
    }

    private void CheckGameOver()
    {
        if (trials >= maxTrials)
        {
            PlayAudio(gameOverClip); // Play game over sound
            Debug.Log("Game Over! You have exceeded the maximum number of trials.");
            // Optionally, restart or end the game here
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false;
    }

    private void Shuffle()
    {
        int count = 0;
        int last = 0;
        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);
            if (rnd == last) { continue; }
            last = emptyLocation;

            if (SwapIfValid(rnd, -size, size)) { count++; }
            else if (SwapIfValid(rnd, +size, size)) { count++; }
            else if (SwapIfValid(rnd, -1, 0)) { count++; }
            else if (SwapIfValid(rnd, +1, size - 1)) { count++; }
        }
    }

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].localPosition, pieces[i + offset].localPosition) =
                (pieces[i + offset].localPosition, pieces[i].localPosition);

            emptyLocation = i;
            return true;
        }
        return false;
    }
}
