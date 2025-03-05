using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For accessing UI components
using TMPro;  // Add this at the top for TextMeshPro



public class Puzzle : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private AudioSource audioSource; // Reference to AudioSource
    [SerializeField] private AudioClip completionClip; // Sound for game completion

    [SerializeField] private AudioClip moveClip; // Sound for game move
    [SerializeField] private AudioClip gameOverClip;   // Sound for game over
    [SerializeField] private TMP_Text movesText; // Reference to the TextMeshPro Text UI
    [SerializeField] private TMP_Text successText; // Reference to the TextMeshPro Text UI

    [SerializeField] private Image uiImage;
    [SerializeField] private Image uiImage_2;


    [SerializeField] private TMP_Text gameOverText; // Reference to the TextMeshPro Text for "Game Over"
    [SerializeField] private Button tryAgainButton; // Reference to the Try Again Button




    public TMP_Dropdown difficultyDropdown;
    [SerializeField] private int maxTrials = 35; // Maximum number of trials

    private int emptyLocation;
    private int size;
    private int trials = 0; // Count the number of trials
    private bool shuffling = false;

    private List<Transform> pieces = new List<Transform>();

    public void SetPuzzleSize(int newSize)
    {
        Debug.Log($"Setting puzzle size to: {newSize}");  // Debug to check the size
        size = newSize;  // Update the puzzle size

        // Reset puzzle state to reinitialize with the new size
        ResetPuzzle();
        CreatePieces(0.01f);  // Recreate the pieces based on the new size

        trials = 0; // Reset moves to 0
        UpdateMovesText(); // Update the move count in the UI

        Shuffle(); // Shuffle the pieces after resizing
    }

    void CreatePieces(float gapThickness)
    {
        // Clear existing pieces before creating new ones
        foreach (var piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        pieces.Clear();

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
                    piece.gameObject.SetActive(false);
                }
            }
        }
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

    private void ResetPuzzle()
    {
        // Hide success text
        successText.gameObject.SetActive(false);
        uiImage.gameObject.SetActive(false);

        // Reset pieces to initial state, e.g., hide last piece
        for (int i = 0; i < pieces.Count; i++)
        {
            if (i == emptyLocation)
            {
                pieces[i].gameObject.SetActive(false); // Hide the empty piece
            }
            else
            {
                pieces[i].gameObject.SetActive(true); // Show all other pieces
            }
        }

        // Reset move count and update text
        trials = 0;
        UpdateMovesText();
    }



    void Start()
    {
        pieces = new List<Transform>();
        size = 3; // Default grid size is 3x3
        CreatePieces(0.01f);
        UpdateMovesText(); // Initialize moves text

        // Hide the "Game Over" text and try again button initially
        gameOverText.gameObject.SetActive(false);
        tryAgainButton.gameObject.SetActive(false);
        uiImage_2.gameObject.SetActive(false);

        // Add listener for the Try Again button
        tryAgainButton.onClick.AddListener(RestartGame);

        // Add listener for difficulty dropdown to change the puzzle size
        difficultyDropdown.onValueChanged.AddListener(SetPuzzleSizeFromDropdown);
    }

    public void SetPuzzleSizeFromDropdown(int index)
    {
        // You can assign different sizes based on the index of the dropdown
        switch (index)
        {
            case 0: // Easy: 3x3
                maxTrials = 35;
                SetPuzzleSize(3);

                break;
            case 1: // Medium: 4x4
                maxTrials = 40;
                SetPuzzleSize(4);

                break;
            case 2: // Hard: 5x5
                maxTrials = 50;
                SetPuzzleSize(5);
                break;
                // Add more cases for other difficulties if needed
        }
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
                UpdateMovesText(); // Update the UI text
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

    private void UpdateMovesText()
    {
        if (movesText != null)
        {
            movesText.text = $"Moves: {trials}/{maxTrials}"; //m Update the text to show the number of moves
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

        successText.gameObject.SetActive(true);
        uiImage.gameObject.SetActive(true);

        // Start the coroutine to wait for a specified time
        StartCoroutine(ShowSuccessForTime(9f)); 

        return true;
    }

    private IEnumerator ShowSuccessForTime(float duration)
    {
        // Wait for the specified time in real-time (independent of Time.timeScale)
        yield return new WaitForSecondsRealtime(duration);

        // After the wait, hide the success text and image
        successText.gameObject.SetActive(false);
        uiImage.gameObject.SetActive(false);
    }

    void CheckGameOver()
    {
        if (trials >= maxTrials)
        {
            PlayAudio(gameOverClip); // Play game over sound
            Debug.Log("Game Over! You have exceeded the maximum number of trials.");

            // Show the game over text and the try again button
            gameOverText.gameObject.SetActive(true);
            tryAgainButton.gameObject.SetActive(true);
            uiImage_2.gameObject.SetActive(true);



            // Optionally, stop the game or disable further interactions
            Time.timeScale = 0f; // Pause the game
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
        ResetPuzzle();
        shuffling = false;
    }
    public void RestartGame()
    {
        // Null checks for critical objects
        if (gameOverText != null && tryAgainButton != null)
        {
            // Reset the game
            Time.timeScale = 1f; // Resume the game
            gameOverText.gameObject.SetActive(false); // Hide the game over text
            tryAgainButton.gameObject.SetActive(false); // Hide the try again button
            uiImage_2.gameObject.SetActive(false);


            // Reset the puzzle
            ResetPuzzle();
            Shuffle();
            trials = 0; // Reset the trials count
            UpdateMovesText(); // Update moves text to 0

        }
        else
        {
            Debug.LogError("gameOverText or tryAgainButton is not assigned in the Inspector.");
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

            // Play audio when a valid swap occurs
            PlayAudio(moveClip); // This is just an example; replace with your specific sound for moving tiles.

            return true;
        }
        return false;
    }



}
