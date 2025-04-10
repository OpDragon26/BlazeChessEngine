using System.Text.RegularExpressions;

namespace Blaze;

public enum Type
{
    Random,
    Analysis,
    Standard
}

public class Match
{
    private static readonly  Random random = new();
    
    private readonly Board board;
    private readonly Type type;
    private readonly int side; // side of the player
    private readonly bool debug;
    private readonly int depth;

    public Match(Board board, Type type, int side = 0, int depth = 2, bool debug = false)
    {
        this.board = board;
        this.type = type;
        this.side = side;
        this.debug = debug;
        this.depth = depth;
    }

    public void Play()
    {
        bool play = true;
        
        while (play)
        {
            if (debug) Console.WriteLine(Search.StaticEvaluate(board)); //Console.WriteLine(board.hashKey);
            switch (type)
            {
                case Type.Analysis:
                    Print(side);
                    play = PlayerTurn();
                break;
                
                case Type.Random:
                    Print(side);
                    if (board.side == side)
                        play = PlayerTurn();
                    else
                    {
                        if (!debug) Console.Clear();
                        // make a random move on the board
                        Move[] filtered = Search.FilterChecks(Search.SearchBoard(board, false), board);
                        board.MakeMove(filtered[random.Next(0, filtered.Length)]);
                        play = CheckOutcome();
                    }
                break;
                
                case Type.Standard:
                    Print(side);
                    if (board.side == side)
                        play = PlayerTurn();
                    else
                    {
                        if (!debug) Console.Clear();
                        // make the top choice of the engine on the board
                        Move bestMove = Search.BestMove(board, depth).move;
                        board.MakeMove(bestMove);
                        play = CheckOutcome();
                    }
                break;
            }
        }
    }

    public void SpeedTest(int repetition = 1000000)
    {
        TimeSpan t1 = DateTime.UtcNow - new DateTime(1970, 1, 1);
        for (int i = 0; i < repetition; i++)
        {
            Board moveBoard = new(board);
            Move[] moves = Search.SearchBoard(moveBoard);
            moveBoard.MakeMove(moves[0]);
        }
        
        TimeSpan t2 = DateTime.UtcNow - new DateTime(1970, 1, 1);
        Console.WriteLine($"Test completed in {Math.Round(t2.TotalMilliseconds - t1.TotalMilliseconds)} milliseconds");
    }

    private void Print(int perspective)
    {
        if (perspective == 1)
        {
            // black's perspective
            Console.WriteLine("# h g f e d c b a");
            
            for (int rank = 0; rank < 8; rank++)
            {
                string rankStr = $"{rank + 1} ";
                
                for (int file = 7; file >= 0; file--)
                {
                    rankStr += PieceStrings[board.GetPiece(file, rank)] + " ";
                }
                
                Console.WriteLine(rankStr);
            }
        }
        else
        {
            // white's perspective
            Console.WriteLine("# a b c d e f g h");
            
            for (int rank = 7; rank >= 0; rank--)
            {
                string rankStr = $"{rank + 1} ";
                
                for (int file = 0; file < 8; file++)
                {
                    rankStr += PieceStrings[board.GetPiece((file, rank))] + " ";
                }
                
                Console.WriteLine(rankStr);
            }
        }
    }

    private bool PlayerTurn()
    {
        // ask the player for a move
        Console.WriteLine("Enter your move:");
        string? playerMoveString = Console.ReadLine();
        if (!string.IsNullOrEmpty(playerMoveString))
        {
            if (playerMoveString == "quit") return false;
                        
            // if the move is in the correct notation
            if (Regex.IsMatch(playerMoveString, "^[a-h][1-8][a-h][1-8][qrbn]?"))
            {
                Move[] filtered = Search.FilterChecks(Search.SearchBoard(board, false), board);
                Move move = new Move(playerMoveString, board);

                if (!debug) Console.Clear();

                // if the move is legal
                if (filtered.Contains(move))
                {
                    board.MakeMove(move);
                    if (!CheckOutcome())
                        return false;
                }
                else
                    Console.WriteLine("Illegal move");
            }
            else
            {
                if (!debug) Console.Clear();
                Console.WriteLine("Incorrect notation");
            }
        }

        return true;
    }

    // if the game has ended, return false to break the Play() loop
    private static readonly string[] Outcomes = 
    [
        "", // ongoing (never printed)
        "Game over. White won",
        "Game over. Black won",
        "Game over. Draw",
    ];
    private bool CheckOutcome()
    {
        Outcome outcome = board.GetOutcome();
        if (outcome != Outcome.Ongoing)
        {
            Console.WriteLine(Outcomes[(int)outcome]);
            return false;
        }

        return true;
    }

    public static void PrintBitboard(ulong bitboard, int perspective, string on = "#", string off = " ")
    {
        string bitboardStr = "";

        if (perspective == 1)
        {
            Console.Write("# h g f e d c b a");
            
            for (int i = 63; i >= 0; i--)
            {
                if ((i + 1) % 8 == 0)
                    bitboardStr += $"\n{9 - ((i + 1) / 8)} ";
            
                if (((bitboard << 63 - i) >> 63) != 0)
                    bitboardStr += on + " ";
                else
                    bitboardStr += off + " ";
            }
        }
        else
        {
            Console.Write("# a b c d e f g h");
            
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    bitboardStr += $"\n{8 - (i / 8)} ";
            
                if (((bitboard << 63 - i) >> 63) != 0)
                    bitboardStr += on + " ";
                else
                    bitboardStr += off + " ";
            }
        }
        
        Console.WriteLine(bitboardStr);
    }

    private static readonly string[] PieceStrings =
    [
        "\u265f",
        "\u265c",
        "\u265e",
        "\u265d",
        "\u265b",
        "\u265a", // 5
        "?",
        "?",
        "\u2659", // 8
        "\u2656",
        "\u2658",
        "\u2657",
        "\u2655",
        "\u2654", // 13
        "?",
        " "
    ];
}