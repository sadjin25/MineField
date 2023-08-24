using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] int width = 16;
    [SerializeField] int height = 16;
    [SerializeField] int mineCount = 4;

    Board board;
    Cell[,] state;

    void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    void Start()
    {
        NewGame();
    }

    void NewGame()
    {
        state = new Cell[width, height];
        // WARNING: Don't break this generation order.
        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

        board.Draw(state);
    }

    void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y);
                cell.type = Cell.Type.Invalid;
                state[x, y] = cell;
            }
        }
    }

    void GenerateMines()
    {
        int mineNumToGenerate = mineCount > width * height ? width * height : mineCount;
        for (int i = 0; i < mineNumToGenerate; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            if (GetCell(x, y).type != Cell.Type.Invalid)
            {
                i--;
                continue;
            }
            state[x, y].type = Cell.Type.Mine;
        }
    }

    void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (state[x, y].type != Cell.Type.Invalid) continue;

                // NOTICE: Cell is value type(struct)
                Cell cell = state[x, y];

                cell.number = CountNumbers(state[x, y]);
                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }
                else
                {
                    cell.type = Cell.Type.Empty;
                }

                state[x, y] = cell;
            }
        }
    }

    int CountNumbers(Cell cell)
    {
        if (cell.type != Cell.Type.Invalid) return -1;

        int toReturn = 0;
        for (int dx = -1; dx < 2; dx++)
        {
            for (int dy = -1; dy < 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int x = cell.position.x + dx;
                int y = cell.position.y + dy;

                if (!IsValidPosition(x, y)) continue;
                if (GetCell(x, y).type == Cell.Type.Mine)
                {
                    toReturn++;
                }
            }
        }
        return toReturn;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            MakeFlag();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Click();
        }
    }

    void MakeFlag()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = board.tilemap.WorldToCell(worldPos);
        Cell cell = GetCell(cellPos.x, cellPos.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed)
        {
            return;
        }

        cell.isFlagged = !cell.isFlagged;
        state[cellPos.x, cellPos.y] = cell;
        board.Draw(state);
    }

    Cell GetCell(int x, int y)
    {
        if (!IsValidPosition(x, y))
        {
            // return invalid type cell instance
            // NOTICE: Don't return null because Cell struct can't get null.
            return new Cell();
        }
        return state[x, y];
    }

    bool IsValidPosition(int x, int y)
    {
        return !(x < 0 || y < 0 || x >= width || y >= height);
    }
}
