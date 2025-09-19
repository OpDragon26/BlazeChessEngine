namespace Blaze;

public static class Perft
{
    private static void Run(int depth, Board board)
    {
        PerftResult Result = new(depth);
        Timer timer = new();
        timer.Start();

        board.considerRepetition = false;

        Move[] moves = Search.FilterChecks(Search.SearchBoard(board), board);
        
        Parallel.For(0, moves.Length, i =>
        {
            int[] threadResults = Result.GetNew();

            Board moveBoard = new Board(board, false);
            moveBoard.MakeMove(moves[i]);
            threadResults[depth]++;
        });
    }

    private static void PerftSearch(Board board, int depth, int[] results)
    {
        if (depth == 0)
            return;
        
        Span<Move> moves = Search.SearchBoard(board);
    }

    public static void Run(int depth)
    {
        Run(depth, new Board(Presets.StartingBoard));
    }

    private class PerftResult(int depth)
    {
        readonly List<int[]> Results = new();
        readonly Mutex mutex = new();

        public int[] GetNew()
        {
            int[] newArray = new int[depth + 1];
            mutex.WaitOne(); // locks the mutex for the duration of adding a new array
            Results.Add(newArray);
            mutex.ReleaseMutex();
            return newArray;
        }

        public int[] GetResult()
        {
            int[] final = new int[depth + 1];

            foreach (int[] threadResult in Results)
                for (int i = 0; i < threadResult.Length; i++)
                    final[i] += threadResult[i];
            
            return final;
        }
    }
}