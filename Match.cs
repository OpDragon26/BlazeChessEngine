using System.Runtime.InteropServices;

namespace Blaze;

public enum Type
{
    Random,
    Analysis,
    Standard,
    Self,
    Autoplay,
}

public class Match(Board board, Type type, int side = 0, int depth = 2, bool debug = false, int moves = 10, bool alwaysUseUnicode = false)
{
    private static readonly  Random random = new();

    private readonly Board board = board;
    private int movesMade;
    private int ply;
    private readonly bool WindowsMode = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private bool inBook = true;
    private readonly List<PGNNode> game = new();
    private string? LasMove;

    public void Play()
    {
        movesMade = 0;
        ply = 0;
        bool play = true;
        
        while (play)
        {
            if (debug) Console.WriteLine(Search.StaticEvaluate(board)); //Console.WriteLine(board.hashKey);
            switch (type)
            {
                case Type.Analysis:
                    PrintLastMove();
                    if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                    play = PlayerTurn();
                break;
                
                case Type.Random:
                    if (!debug) Console.Clear();
                    PrintLastMove();
                    if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                    if (board.side == side)
                        play = PlayerTurn();
                    else
                    {
                        // make a random move on the board
                        Move[] filtered = Search.FilterChecks(Search.SearchBoard(board, false), board);
                        Move move = filtered[random.Next(0, filtered.Length)];

                        LasMove = move.Notate(board);
                        board.MakeMove(move);
                        game.Add(new PGNNode { board = new Board(board) , move = move });
                        play = CheckOutcome();
                    } 
                    break;
                
                case Type.Standard:
                    if (!debug) Console.Clear();
                    PrintLastMove();
                    if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                    if (board.side == side)
                        play = PlayerTurn();
                    else
                    {
                        // make the top choice of the engine on the board
                        (Move move, int eval, bool bookMove) searchResult = Search.BestMove(board, depth, inBook, ply);
                        inBook = searchResult.bookMove;
                        Move bestMove = searchResult.move;

                        LasMove = bestMove.Notate(board);
                        
                        board.MakeMove(bestMove);
                        game.Add(new PGNNode { board = new Board(board) , move = bestMove });
                        play = CheckOutcome();
                    }
                    break;
                
                case Type.Autoplay:
                    for (int i = 0; i < moves; i++)
                    {
                        if (!debug) Console.Clear();

                        movesMade += 1 - board.side; // if the side is white, add one
                        
                        string movingSide = board.side == 0 ? "White" : "Black";
                        Console.WriteLine($"Move {movesMade} - {movingSide}");
                        
                        PrintLastMove();
                        if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                        
                        // make the top choice of the engine on the board
                        (Move move, int eval, bool bookMove) searchResult = Search.BestMove(board, depth, inBook, ply);
                        inBook = searchResult.bookMove;
                        Move botMove = searchResult.move;

                        LasMove = botMove.Notate(board);
                        board.MakeMove(botMove);
                        game.Add(new PGNNode { board = new Board(board), move = botMove });
                        
                        // if the game ended, break the loop
                        if (!CheckOutcome())
                        {
                            if (!debug) Console.Clear();
                            CheckOutcome();
                            if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                            break;
                        }

                        ply++;
                    }

                    play = false; // break loop
                break;
                
                case Type.Self:
                    for (int i = 0; i < moves; i++)
                    {
                        string? message = Console.ReadLine();
                        if (message == "exit") break;
                        
                        if (!debug) Console.Clear();

                        movesMade += 1 - board.side; // if the side is white, add one
                        
                        string movingSide = board.side == 0 ? "White" : "Black";
                        Console.WriteLine($"Move {movesMade} - {movingSide}");
                        
                        PrintLastMove();
                        if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                        
                        // make the top choice of the engine on the board
                        (Move move, int eval, bool bookMove) searchResult = Search.BestMove(board, depth, inBook, ply);
                        inBook = searchResult.bookMove;
                        Move botMove = searchResult.move;

                        LasMove = botMove.Notate(board);
                        board.MakeMove(botMove);
                        game.Add(new PGNNode { board = new Board(board) , move = botMove });
                        
                        // if the game ended, break the loop
                        if (!CheckOutcome())
                        {
                            if (!debug) Console.Clear();
                            CheckOutcome();
                            if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                            break;
                        }

                        ply++;
                    }

                    play = false; // break loop
                    break;
            }
            
            movesMade += 1 - board.side; // if the side is white, add one
            ply++;
        }
        
        if (!debug) Console.Clear();
        CheckOutcome();
        if (!WindowsMode) Print(side); else Print(side, IHateWindows);
        Console.WriteLine($"Full game:\n{GetPGN()}");
    }

    private void PrintLastMove()
    {
        if (LasMove != null)
            Console.WriteLine($"Last move: {LasMove}");
    }

    public string GetUCI()
    {
        string[] pgn = new string[game.Count];

        for (int i = 0; i < pgn.Length; i++)
        {
            pgn[i] = game[i].move.GetUCI();
        }
        
        return string.Join(' ', pgn);
    }

    private string GetPGN()
    {
        string[] pgn = new string[game.Count];

        pgn[0] = "1. " + game[0].move.Notate(new Board(Presets.StartingBoard));
        for (int i = 1; i < pgn.Length; i++)
        {
            string num = i % 2 == 0 ? $"{i / 2 + 2}. " : "";
            
            pgn[i] = num + game[i].move.Notate(game[i-1].board);
        }
        
        return string.Join(' ', pgn);
    }

    public PGNNode[] GetNodes()
    {
        return game.ToArray();
    }

    public void SpeedTest(int repetition = 1000000)
    {
        TimeSpan t1 = DateTime.UtcNow - new DateTime(1970, 1, 1);
        for (int i = 0; i < repetition; i++)
        {
            Board moveBoard = new(board);
            Move[] moveList = Search.SearchBoard(moveBoard);
            moveBoard.MakeMove(moveList[0]);
        }
        
        TimeSpan t2 = DateTime.UtcNow - new DateTime(1970, 1, 1);
        Console.WriteLine($"Test completed in {Math.Round(t2.TotalMilliseconds - t1.TotalMilliseconds)} milliseconds");
    }

    public static void PrintBoard(Board board, int perspective)
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

    private void Print(int perspective)
    {
        PrintBoard(board, perspective);
    }

    private void Print(int perspective, string[] pieceStrings)
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
                    rankStr += pieceStrings[board.GetPiece(file, rank)] + " ";
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
                    rankStr += pieceStrings[board.GetPiece((file, rank))] + " ";
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
            if (playerMoveString == "exit") return false;
                        
            // if the move is in the correct notation
            try
            {
                Move[] filtered = Search.FilterChecks(Search.SearchBoard(board, false), board);
                Move move = Move.Parse(playerMoveString, board);

                if (!debug) Console.Clear();

                // if the move is legal
                if (filtered.Contains(move))
                {
                    LasMove = move.Notate(board);
                    board.MakeMove(move);
                    game.Add(new PGNNode { board = new Board(board) , move = move });
                    if (!CheckOutcome())
                        return false;
                }
                else
                    Console.WriteLine("Illegal move");
            }
            catch
            {
                if (!debug) Console.Clear();
                Console.WriteLine("Invalid move");
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
            Console.WriteLine(Outcomes[(int)outcome] + $" on move {movesMade}");
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

    private static readonly string[] IHateWindows =
    [
        "P",
        "R",
        "N",
        "B",
        "Q",
        "K", // 5
        "?",
        "?",
        "p", // 8
        "r",
        "n",
        "b",
        "q",
        "k", // 13
        "?",
        " "
    ];
}