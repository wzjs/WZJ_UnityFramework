using UnityEngine;

[CreateAssetMenu(fileName = "InitialBoard", menuName = "Match3/InitialBoard", order =1)]
public class InitialBoard : ScriptableObject
{
    public int width =9;
    public int height =9;
    public int normalGemTypes =3;
    public int[] cells;

    public void EnsureSize()
    {
        int size = Mathf.Max(0, width * height);
        if (cells == null || cells.Length != size)
        {
            var newCells = new int[size];
            if (cells != null)
            {
                int copy = Mathf.Min(cells.Length, newCells.Length);
                for (int i =0; i < copy; i++) newCells[i] = cells[i];
            }
            // default initialize remaining to 0
            for (int i =0; i < newCells.Length; i++) { }
            cells = newCells;
        }
    }

    public int GetInt(int x, int y)
    {
        if (cells == null) return 0;
        if (x <0 || x >= width || y <0 || y >= height) return 0;
        return cells[y * width + x];
    }

    public void SetInt(int x, int y, int value)
    {
        EnsureSize();
        if (x <0 || x >= width || y <0 || y >= height) return;
        cells[y * width + x] = value;
    }
}
