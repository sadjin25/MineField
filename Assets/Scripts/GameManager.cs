using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    enum ScoreTypes
    {
        MINE_EXPLODE = -40,
        REVEALED = 10,
    }

    [SerializeField] int width = 16;
    [SerializeField] int height = 16;
    [SerializeField] int mineCount = 4;

    Board board;
    Cell[,] state;

    int score;

    bool isMouseActivated = true;
    bool nextMouseIsActive;
    [SerializeField] float mouseWaitTime = 0.2f;

    void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    void Start()
    {
        CameraControl.Instance.OnDrag += MouseToggle;
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

    int CountFoundMines(Cell cell)
    {
        if (!cell.isRevealed) return -1;

        int toReturn = 0;
        for (int dx = -1; dx < 2; dx++)
        {
            for (int dy = -1; dy < 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int x = cell.position.x + dx;
                int y = cell.position.y + dy;

                if (!IsValidPosition(x, y)) continue;
                Cell nextCell = GetCell(x, y);

                if (nextCell.isFlagged || (nextCell.isRevealed && nextCell.type == Cell.Type.Mine))
                {
                    toReturn++;
                }
            }
        }
        return toReturn;
    }

    bool IsCellNearEmpty(Cell cell)
    {
        if (cell.type != Cell.Type.Number && cell.type != Cell.Type.Empty) return false;

        for (int dx = -1; dx < 2; dx++)
        {
            for (int dy = -1; dy < 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int x = cell.position.x + dx;
                int y = cell.position.y + dy;

                if (!IsValidPosition(x, y)) continue;

                if (GetCell(x, y).type == Cell.Type.Empty)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Update()
    {
        if (!isMouseActivated) return;
        if (Input.GetMouseButtonDown(1))
        {
            MakeFlag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = board.tilemap.WorldToCell(worldPos);
            Cell cell = GetCell(cellPos.x, cellPos.y);

            if (cell.isRevealed)
            {
                OpenNearCells(cell);
            }
            else
            {
                Reveal(cell);
            }
        }
    }

    void MouseToggle(object s, CameraControl.OnDragEventArgs e)
    {
        nextMouseIsActive = !e.isDragging;
        if (nextMouseIsActive)
        {
            Debug.Log("EVENT CALLED = MOUSE TRUE");
            StartCoroutine(WaitMouseActivate(mouseWaitTime));
        }
        else
        {
            Debug.Log("EVENT CALLED = MOUSE FALSE");
            isMouseActivated = false;
        }
    }

    IEnumerator WaitMouseActivate(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        isMouseActivated = true;
    }

    void MakeFlag()
    {
        // TODO: If player flags to the wrong place, then how many score would he loss?
        //       but this is kinda cheating for players to easily find the mine cells. 
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

    void OpenNearCells(Cell cell)
    {
        // If player clicked revealed cell, then open near cells when flagging is right/wrong - update score. 
        if (!cell.isRevealed)
        {
            return;
        }
        int mineNums = CountNumbers(cell);
        int foundMineNums = CountFoundMines(cell);
        if (mineNums == foundMineNums)
        {
            for (int dx = -1; dx < 2; dx++)
            {
                for (int dy = -1; dy < 2; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int x = cell.position.x + dx;
                    int y = cell.position.y + dy;

                    if (!IsValidPosition(x, y)) continue;
                    Reveal(GetCell(x, y));
                }
            }
        }
    }

    void Reveal(Cell cell)
    {
        // if you've clicked bomb, then game over.
        // else if clicked the number/empty that is near the empty cell, then dfs 
        if (cell.isFlagged || cell.isRevealed || cell.type == Cell.Type.Invalid)
        {
            return;
        }

        else
        {
            switch (cell.type)
            {
                case Cell.Type.Mine:
                    ChangeScore(ScoreTypes.MINE_EXPLODE);
                    Explode(cell);
                    break;
                default:
                    ChangeScore(ScoreTypes.REVEALED);
                    if (IsCellNearEmpty(cell))
                    {
                        RevealAllEmptyCell(cell);
                    }
                    cell.isRevealed = true;
                    state[cell.position.x, cell.position.y] = cell;
                    break;
            }
        }

        board.Draw(state);
    }

    void Explode(Cell cell)
    {
        // Reduce player's score. 
        if (cell.type != Cell.Type.Mine) return;
        cell.isExploded = true;
        cell.isRevealed = true;
        state[cell.position.x, cell.position.y] = cell;
    }

    void RevealAllEmptyCell(Cell cell)
    {
        if (cell.isRevealed) return;
        if (cell.type != Cell.Type.Number && cell.type != Cell.Type.Empty) return;

        cell.isRevealed = true;
        state[cell.position.x, cell.position.y] = cell;
        if (cell.type == Cell.Type.Number)
        {
            return;
        }

        RevealAllEmptyCell(GetCell(cell.position.x - 1, cell.position.y));
        RevealAllEmptyCell(GetCell(cell.position.x + 1, cell.position.y));
        RevealAllEmptyCell(GetCell(cell.position.x, cell.position.y + 1));
        RevealAllEmptyCell(GetCell(cell.position.x, cell.position.y - 1));
        RevealAllEmptyCell(GetCell(cell.position.x - 1, cell.position.y + 1));
        RevealAllEmptyCell(GetCell(cell.position.x - 1, cell.position.y - 1));
        RevealAllEmptyCell(GetCell(cell.position.x + 1, cell.position.y + 1));
        RevealAllEmptyCell(GetCell(cell.position.x + 1, cell.position.y - 1));
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

    void ChangeScore(ScoreTypes scoreType)
    {
        score += (int)scoreType;
    }
}
