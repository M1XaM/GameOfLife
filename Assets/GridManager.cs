using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public int gridSize = 1000; // Grid size (100x100)
    public int gridWidth = 96;
    public int gridHeight = 54;
    public int cellSize = 20; // Each cell is 10x10 pixels
    private Cell[,] grid; // 2D array to store cells
    public float updateInterval = 2.0f; // Time between updates
    private float timer;
    private int generations;
    private int genCount = 0;

    public Button PlayButton;
    public Button PauseButton;
    private bool isRunning = false;

    public Slider GenerationsSlider;
    public Text GenerationText; 
    public Text currentGen; 
    
    private int minGenerations = 1;
    private int maxGenerations = 1000;

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    // Start is called before the first frame update
    public void Initiate()
    { 
        CreateGrid();

        PlayButton.onClick.AddListener(PlayGame);
        PauseButton.onClick.AddListener(PauseGame);

        GenerationsSlider.onValueChanged.AddListener(UpdateGenerations);

        // Set initial slider value to max
        GenerationsSlider.value = 0.0f; 
        UpdateGenerations(GenerationsSlider.value);
    }

    public void UpdateGenerations(float value)
    {
        // Convert scrollbar value (0 to 1) into a range (1 to 1000)
        generations = Mathf.RoundToInt(Mathf.Lerp(minGenerations, maxGenerations, value));
        
        // Update text with the selected value
        GenerationText.text = $"Number of Generations: {generations}";
    }

    public void PlayGame()
    {
        isRunning = true;
    }

    public void PauseGame()
    {
        isRunning = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            if (genCount < generations)
            {
                timer += Time.deltaTime;
                if (timer >= updateInterval)
                {
                    timer = 0f;
                    UpdateGrid();
                    genCount++; 
                    currentGen.text = $"Current Generation: {genCount}";
                }
            }
            else
            {
                // Stop updating the grid if we've reached the maximum generations
                Debug.Log("Maximum generations reached.");
            }
        }
    }

    public void CreateGrid()
    {
        grid = new Cell[gridWidth, gridHeight];

        for(int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {

                
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);
                Debug.Log($"Creating cell at position: {position}");
        
                GameObject cellObj = GameObject.CreatePrimitive(PrimitiveType.Cube); // Create a cube instead of a sprite
                cellObj.transform.position = position;
                cellObj.transform.localScale = new Vector3(cellSize, cellSize, cellSize); // Ensure the cubes are scaled correctly



                cellObj.transform.SetParent(transform); // Ensure the parent is set correctly


                Cell cell = cellObj.AddComponent<Cell>();
                grid[x,y] = cell;
                
                int zone = GetZone(x, y);
                cell.Initialize(this, x, y, zone);


                if (grid[x, y] == null)
            {
                Debug.LogError($"Cell at ({x},{y}) is null.");
                continue;
            }
            }

        }
    }

    void UpdateGrid()
    {
        bool[,] newStates = new bool[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {

                if (grid[x, y] == null)
            {
                Debug.LogError($"Cell at ({x}, {y}) is null in UpdateGrid.");
                continue; // Skip this cell
            }

                int zone = GetZone(x, y);

                int aliveNeighbors = grid[x, y].GetAliveNeighbors();
                bool isAlive = grid[x, y].isAlive;
                
                if (zone == 1 || zone == 2)
                {
                    if (isAlive && (aliveNeighbors < 2 || aliveNeighbors > 3))
                        newStates[x, y] = false; // Cell dies
                    else if (!isAlive && aliveNeighbors == 3)
                        newStates[x, y] = true; // Cell becomes alive
                    else
                        newStates[x, y] = isAlive; // Remains the same
                }
                else if (zone == 0)
                {
                    if (isAlive && (aliveNeighbors != 2))
                        newStates[x, y] = false; // Cell dies
                    else if (!isAlive && aliveNeighbors > 3)
                        newStates[x, y] = true; // Cell becomes alive
                    else
                        newStates[x, y] = isAlive; // Remains the same
                }
                else
                {
                    //sensory cortex zone
                    if (isAlive && aliveNeighbors == 2) {
                        newStates[x, y] = false; // dies
                    } else if (!isAlive && aliveNeighbors > 1 && aliveNeighbors < 4){
                        newStates[x, y] = true; // becomes alive
                    } else {
                        newStates[x, y] = isAlive;
                    }
                }
            }
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y].SetState(newStates[x, y]);
            }
        }
    }    

    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public Cell GetCell(int x, int y)
    {
        return grid[x, y];
    }   

    int GetZone(int x, int y)
    {
        if (x < gridWidth / 2 && y < gridHeight / 2) return 0; // Top-left
        if (x >= gridWidth / 2 && y < gridHeight / 2) return 1; // Top-right
        if (x < gridWidth / 2 && y >= gridHeight / 2) return 2; // Bottom-left
        return 3; // Bottom-right
    }

    public void ActivateCellsAtMousePosition(Vector3 mousePosition)
    {
        // Convert world position to grid position
        Vector2 worldPoint = new Vector2(mousePosition.x, mousePosition.y);
        
        // Convert world coordinates to grid cell coordinates
        int x = Mathf.FloorToInt(worldPoint.x / cellSize);
        int y = Mathf.FloorToInt(worldPoint.y / cellSize);
        
        // Ensure we're within the grid bounds
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            Cell cell = grid[x, y];
            if (cell != null)
            {
                cell.MakeAlive(); // Activate the cell
            }
        }
    }
}