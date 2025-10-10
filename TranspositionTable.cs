namespace Blaze;

public class TranspositionTable(int size)
{
    private HashEntry[] table = new HashEntry[size];
    private const int replaceThreshold = 10;

    public bool TryGet(int hash, int depth, out HashEntry result)
    {
        result = table[hash % size];
        if (result.type != EntryType.None && result.zobrist == hash && result.depth >= depth)
            return true;
        
        return false;
    }

    public bool TrySet(int hash, EntryType type, int depth, int eval, int ply, Move move)
    {
        if (table[hash % size].ply < ply - replaceThreshold)
        {
            table[hash % size] = new HashEntry(hash, type, depth, eval, ply, move);
            return true;
        }
        return false;
    }
    
    public bool TrySet(int hash, EntryType type, int depth, int eval, int ply)
    {
        if (table[hash % size].ply < ply - replaceThreshold)
        {
            table[hash % size] = new HashEntry(hash, type, depth, eval, ply);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        table = new HashEntry[size];
    }
}

public readonly struct HashEntry(int zobrist, EntryType type, int depth, int eval, int ply)
{
    public readonly int zobrist = zobrist;
    public readonly EntryType type = type;
    public readonly int depth = depth;
    public readonly int eval = eval;
    public readonly Move? move = null;
    public readonly int ply = ply;

    public HashEntry(int zobrist, EntryType type, int depth, int eval, int ply, Move move) : this(zobrist, type, depth, eval, ply)
    {
        this.move = move;
    }
}

public enum EntryType : byte
{
    None,
    Exact,
    LowerBound,
    UpperBound,
}