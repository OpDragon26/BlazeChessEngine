namespace Blaze;

public static class Perft
{
    private static readonly ulong[] CorrectResults = [
        1,
        20,
        400,
        8_902,
        197_281,
        4_865_609,
        119_060_324,
        3_195_901_860,
        84_998_978_956
    ];
    
    private static void Run(int depth, Board board, bool fromStartingPosition = false)
    {
        PerftResult Result = new(depth);
        Timer timer = new();
        timer.Start();

        board.considerRepetition = false;

        Move[] moves = Search.SearchBoard(board, false).ToArray();
        
        Parallel.For(0, moves.Length, i =>
        {
            ulong[] threadResults = Result.GetNew();

            Board moveBoard = new Board(board);
            moveBoard.MakeMove(moves[i]);
            threadResults[depth]++;
            PerftSearch(moveBoard, depth - 1, threadResults);
        });
        
        ulong[] perftResult = Result.GetResult();
        if (fromStartingPosition)
            for (int i = perftResult.Length - 1; i > 0; i--)
                    Console.WriteLine(CorrectResults.Length >= perftResult.Length - i ? 
                        $"Depth {perftResult.Length - i}: {perftResult[i]} {(perftResult[i] == CorrectResults[perftResult.Length - i] ? "✓" : $"✕ - should be {CorrectResults[perftResult.Length - i]}")}"
                        : $"Depth {perftResult.Length - i}: {perftResult[i]}");
                
        else
            for (int i = perftResult.Length - 1; i > 0; i--)
                Console.WriteLine($"Depth {perftResult.Length - i}: {perftResult[i]}");
        Console.WriteLine($"Depth {depth} perft completed in {timer.Stop()}ms");
    }

    private static void PerftSearch(Board board, int depth, ulong[] results)
    {
        if (depth == 1)
        {
            results[1] += (ulong)Search.SearchBoard(board, false).Length;
            return;
        }
        
        Span<Move> moves = Search.SearchBoard(board, false);

        foreach (Move move in moves)
        {
            Board MoveBoard = new Board(board);
            MoveBoard.MakeMove(move);

            results[depth]++;
                
            PerftSearch(MoveBoard, depth - 1, results);
        }
    }

    public static void Run(int depth)
    {
        Run(depth, new Board(Presets.StartingBoard), true);
    }

    private class PerftResult(int depth)
    {
        readonly List<ulong[]> Results = new();
        readonly Mutex mutex = new();

        public ulong[] GetNew()
        {
            ulong[] newArray = new ulong[depth + 1];
            mutex.WaitOne(); // locks the mutex for the duration of adding a new array
            Results.Add(newArray);
            mutex.ReleaseMutex();
            return newArray;
        }

        public ulong[] GetResult()
        {
            ulong[] final = new ulong[depth + 1];

            foreach (ulong[] threadResult in Results)
                for (int i = 0; i < threadResult.Length; i++)
                    final[i] += threadResult[i];
            
            return final;
        }
    }
}