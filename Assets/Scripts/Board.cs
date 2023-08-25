using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }

    [SerializeField] public Tile tileUnknown;
    [SerializeField] public Tile tileEmpty;
    [SerializeField] public Tile tileMine;
    [SerializeField] public Tile tileExploded;
    [SerializeField] public Tile tileFlag;
    [SerializeField] public Tile[] tileNum;
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state)
    {
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    Tile GetTile(Cell cell)
    {
        if (cell.isRevealed)
        {
            return GetRevealedTile(cell);
        }
        else if (cell.isFlagged)
        {
            return tileFlag;
        }
        else
        {
            return tileUnknown;
        }
    }

    Tile GetRevealedTile(Cell cell)
    {
        switch (cell.type)
        {
            case Cell.Type.Empty:
                return tileEmpty;
            case Cell.Type.Mine:
                return cell.isExploded ? tileExploded : tileMine;
            case Cell.Type.Number:
                return GetNumberTile(cell);

            default: return null;
        }
    }

    Tile GetNumberTile(Cell cell)
    {
        if (cell.type != Cell.Type.Number) return null;

        return tileNum[cell.number - 1];
    }
}
