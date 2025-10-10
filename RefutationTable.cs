namespace Blaze;

public class RefutationTable(int size)
{
    private readonly HashEntry[] table = new HashEntry[size];

    public bool TryGet(int zobrist, out HashEntry result)
    {
        result = table[zobrist % size];
        return result.filled && result.zobrist == zobrist;
    }

    public void Set(int zobrist, Move move, byte bonus)
    {
        table[zobrist % size] = new HashEntry(zobrist, move, bonus);
    }
}

public readonly struct HashEntry(int zobrist, Move move, byte bonus)
{
    public readonly bool filled = true;
    public readonly int zobrist = zobrist;
    public readonly Move move = move;
    public readonly byte bonus = bonus;
}