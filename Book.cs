using System.Linq.Expressions;

namespace Blaze;

public static class Book
{
    static readonly List<BookBoard>[] boards = new List<BookBoard>[18];

    public static void Init(string path)
    {
        // set the first board to the staring board
        boards[0] = [new BookBoard { board = new Board(Presets.StartingBoard), moves = new List<BookMove>() }];
        for (int i = 1; i < 14; i++)
            boards[i] = new List<BookBoard>();
        
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            //Console.WriteLine(line);
            AddLine(Parser.ParsePGN(line));
        }
    }

    private static void AddLine(PGNNode[] line)
    {
        // for the first node
        // if the board has the move, increase its weight
        // for each move of the board
        bool found = false;
        for (int move = 0; move < boards[0][0].moves.Count; move++)
        {
            // if the board contains the move, increase its weight
            if (boards[0][0].moves[move].move.Equals(line[0].move))
            {
                found = true;
                boards[0][0].moves[move].weight += 1;
                break;
            }
        }
        // if the board does not contain the move, add it
        if (!found)
        {
            boards[0][0].moves.Add(new BookMove { move = line[0].move, weight = 1 });
            // add the resulting board to the next list
            boards[1].Add(new BookBoard { board = line[0].board, moves = new List<BookMove>() });
        }
        
        // for each node
        for (int i = 1; i < 14; i++)
        {
            // for each board at the given depth
            for (int board = 0; board < boards[i].Count; board++)
            {
                // if the board belongs to the previous node
                if (line[i-1].board.Equals(boards[i][board].board))
                {
                    // board is boards[i][board]
                    
                    // if the board has the move, increase its weight
                    // for each move of the board
                    found = false;
                    for (int move = 0; move < boards[i][board].moves.Count; move++)
                    {
                        // if the board contains the move, increase its weight
                        if (boards[i][board].moves[move].move.Equals(line[i].move))
                        {
                            found = true;
                            boards[i][board].moves[move].weight += 1;
                            break;
                        }
                    }
                    // if the board does not contain the move, add it
                    if (!found)
                    {
                        boards[i][board].moves.Add(new BookMove { move = line[i].move, weight = 1 });
                        // add the resulting board to the next list
                        if (i != 13)
                            boards[i+1].Add(new BookBoard { board = line[i].board, moves = new List<BookMove>() });
                    }
                    
                    break;
                }
            }
        }
    }
}

public struct BookBoard
{
    public Board board;
    public List<BookMove> moves;
}

public class BookMove
{
    public Move move;
    public int weight;
}

public static class Parser
{
    public static void PrintGame(PGNNode[] game, int perspective, int pause = 10)
    {
        foreach (PGNNode node in game)
        {
            Console.Clear();
            Match.PrintBoard(node.board, perspective);
            Thread.Sleep(pause * 100);
        }
    }

    public static PGNNode[] ParsePGN(string pgn)
    {
        List<PGNNode> nodes = new List<PGNNode>();
        string[] game = pgn.Replace("\n", " ").Split(' ');
        Board board = new Board(Presets.StartingBoard);

        foreach (string alg in game)
        {
            if (alg.Equals(string.Empty) || alg[^1] == '.') // notates the index of the move
                continue;
            Move move;
            try
            {
                move = Move.Parse(alg, board); // converts the move from algebraic notation to Move
            }
            catch
            {
                Console.WriteLine(alg);
                throw;
            }

            board.MakeMove(move);

            nodes.Add(new PGNNode { board = new Board(board), move = move });
        }

        return nodes.ToArray();
    }
}

public struct PGNNode
{
    public Board board;
    public Move move;
}