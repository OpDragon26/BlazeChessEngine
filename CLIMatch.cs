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

public enum Side
{
    White,
    Black
}

public class CLIMatch
{
    private static readonly  Random random = new();

    private readonly Board board;
    private int movesMade;
    private int ply;
    private readonly bool WindowsMode;
    private bool inBook = true;
    private readonly List<PGNNode> game;
    private string? LasMove;
    private int depth;
    private readonly int OriginalDepth;
    private readonly bool debug;
    private readonly Type type;
    private readonly bool alwaysUseUnicode;
    private readonly int side;
    private readonly int moves;
    private readonly bool dynamicDepth;
    private readonly bool printBoard;

    public CLIMatch(Board board, Type type, Side side, int depth = 2, bool debug = false, int moves = 1000, bool alwaysUseUnicode = false, bool dynamicDepth = true, bool printBoard = true)
    {
        this.board = board;
        WindowsMode = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        game = new();
        this.depth = depth;
        OriginalDepth = depth;
    
        this.debug = debug;
        this.type = type;
        this.alwaysUseUnicode = alwaysUseUnicode;
        this.side = (int)side;
        this.moves = moves;
        this.dynamicDepth = dynamicDepth;
        this.printBoard = printBoard;
        RefutationTable.Init((int)Math.Pow(2, 20) + 7);
        
        Bitboards.Init();
        Hasher.Init();
        Book.Init(Books.Standard);
    }

    private static readonly int[,] Thresholds = new[,]
    {
        {0, 0, 0}, // 0
        {0, 1000, 1000}, // 1
        {0, 1000, 1000}, // 2
        {0, 1000, 1000}, // 3
        {50, 1000, 1000}, // 4
        {100, 5000, 2000}, // 5
        {500, 9000, 6000}, // 6
        {1000, 30000, 10000}, // 7
        {20000, 300000, 150000}, // 8
    };
    private static int increaseThreshold = 500;
    private static int decreaseThreshold = 9000;
    private static int endgameDecreaseThreshold = 6000;

    public List<PGNNode> Play()
    {
        movesMade = 0;
        ply = 0;
        bool play = true;
        
        increaseThreshold = Thresholds[depth, 0];
        decreaseThreshold = Thresholds[depth, 1];
        endgameDecreaseThreshold = Thresholds[depth, 2];
        
        while (play)
        {
            if (debug) Console.WriteLine(Search.StaticEvaluate(board)); //Console.WriteLine(board.hashKey);
            switch (type)
            {
                case Type.Analysis:
                    PrintLastMove();
                    if (printBoard) 
                        if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                    play = PlayerTurn();
                break;
                
                case Type.Random:
                    Console.WriteLine($"Depth: {depth}");
                    if (!debug) Console.Clear();
                    PrintLastMove();
                    if (printBoard) 
                        if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                    if (board.side == side)
                        play = PlayerTurn();
                    else
                    {
                        // make a random move on the board
                        Move[] filtered = Search.FilterChecks(Search.SearchBoard(board,false), board);
                        Move move = filtered[random.Next(0, filtered.Length)];

                        LasMove = move.Notate(board);
                        board.MakeMove(move);
                        game.Add(new PGNNode { board = new Board(board) , move = move});
                        play = CheckOutcome();
                    } 
                    break;
                
                case Type.Standard:
                    Console.WriteLine($"Depth: {depth}");
                    if (!debug) Console.Clear();
                    PrintLastMove();
                    if (printBoard) 
                        if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                    if (board.side == side)
                        play = PlayerTurn();
                    else
                    {
                        BotMove();
                        play = CheckOutcome();
                    }
                    break;
                
                case Type.Autoplay:
                    for (int i = 0; i < moves; i++)
                    {
                        if (!debug) Console.Clear();

                        movesMade += 1 - board.side; // if the side is white, add one
                        
                        Console.WriteLine($"Depth: {depth}");
                        
                        string movingSide = board.side == 0 ? "White" : "Black";
                        Console.WriteLine($"Move {movesMade} - {movingSide}");
                        
                        PrintLastMove();
                        if (printBoard) 
                            if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                        
                        // make the top choice of the engine on the board
                        Search.SearchResult result = Search.BestMove(board, depth, inBook, ply);
                        Console.WriteLine($"Move made in {result.time}ms at depth {depth}");

                        if (dynamicDepth)
                        {
                            if (ply % 2 == 0)
                                if (result.time < increaseThreshold && !result.bookMove) depth++;
                                else if (result.time > (board.IsEndgame() ? endgameDecreaseThreshold : decreaseThreshold)) depth = Math.Max(OriginalDepth, depth - 1);
                            if ((result.move.Promotion & Pieces.TypeMask) == Pieces.WhiteQueen)
                                depth--;   
                        }
                        
                        inBook = result.bookMove;
                        Move botMove = result.move;

                        LasMove = botMove.Notate(board);
                        board.MakeMove(botMove);
                        game.Add(new PGNNode { board = new Board(board), move = botMove , time = result.time });
                        
                        // if the game ended, break the loop
                        if (!CheckOutcome())
                        {
                            if (!debug) Console.Clear();
                            CheckOutcome();
                            if (printBoard) 
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
                        
                        Console.WriteLine($"Depth: {depth}");
                        
                        string movingSide = board.side == 0 ? "White" : "Black";
                        Console.WriteLine($"Move {movesMade} - {movingSide}");
                        
                        PrintLastMove();
                        if (printBoard) 
                            if (!WindowsMode || alwaysUseUnicode) Print(side); else Print(side, IHateWindows);
                        
                        // make the top choice of the engine on the board
                        Search.SearchResult result = Search.BestMove(board, depth, inBook, ply);
                        Console.WriteLine($"Move made in {result.time}ms at depth {depth}");

                        if (dynamicDepth)
                        {
                            if (ply % 2 == 0)
                                if (result.time < increaseThreshold && !result.bookMove) depth++;
                                else if (result.time > (board.IsEndgame() ? endgameDecreaseThreshold : decreaseThreshold)) depth = Math.Max(OriginalDepth, depth - 1);
                            if ((result.move.Promotion & Pieces.TypeMask) == Pieces.WhiteQueen)
                                depth--;
                        }

                        inBook = result.bookMove;
                        Move botMove = result.move;

                        LasMove = botMove.Notate(board);
                        board.MakeMove(botMove);
                        game.Add(new PGNNode { board = new Board(board) , move = botMove , time = result.time });
                        
                        // if the game ended, break the loop
                        if (!CheckOutcome())
                        {
                            if (!debug) Console.Clear();
                            CheckOutcome();
                            if (printBoard) 
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

        long time = 0;
        foreach (var node in game)
            time += node.time;
        Console.WriteLine($"Average time per move = {time / game.Count}ms");
        
        if (!debug) Console.Clear();
        CheckOutcome();
        if (printBoard) 
            if (!WindowsMode) Print(side); else Print(side, IHateWindows);
        Console.WriteLine($"Full game:\n{GetPGN()}");
        
        return game;
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
            string num = i % 2 == 0 ? $"{i / 2 + 1}. " : "";
            
            pgn[i] = num + game[i].move.Notate(game[i-1].board);
        }
        
        return string.Join(' ', pgn);
    }

    public PGNNode[] GetNodes()
    {
        return game.ToArray();
    }

    public static void PrintBoard(Board board, int perspective, int imbalance = 0)
    {
        PrintBoard(board, perspective, PieceStrings, imbalance);
    }
    
    public static void PrintBoard(Board board, int perspective, string[] pieceStrings, int imbalance = 0)
    {
        if (perspective == 1)
        {
            // black's perspective
            Console.WriteLine(imbalance > 0 ? $"# h g f e d c b a  +{imbalance}" : "# h g f e d c b a");
            
            for (int rank = 0; rank < 8; rank++)
            {
                string rankStr = $"{rank + 1} ";
                
                for (int file = 7; file >= 0; file--)
                    rankStr += pieceStrings[board.GetPiece(file, rank)] + " ";
                
                if (imbalance < 0 && rank == 7) // black advantage
                    rankStr += $" +{-imbalance}";
                
                Console.WriteLine(rankStr);
            }
        }
        else
        {
            // white's perspective
            Console.WriteLine(imbalance < 0 ? $"# a b c d e f g h  +{-imbalance}" : "# a b c d e f g h");
            
            for (int rank = 7; rank >= 0; rank--)
            {
                string rankStr = $"{rank + 1} ";
                
                for (int file = 0; file < 8; file++)
                    rankStr += pieceStrings[board.GetPiece((file, rank))] + " ";
                
                if (imbalance > 0 && rank == 0)
                    rankStr += $" +{imbalance}";
                
                Console.WriteLine(rankStr);
            }
        }
    }

    private void Print(int perspective)
    {
        PrintBoard(board, perspective, PieceStrings, board.GetImbalance());
    }

    private void Print(int perspective, string[] pieceStrings)
    {
        int imbalance = board.GetImbalance() / 100;
        
        if (perspective == 1)
        {
            // black's perspective
            Console.WriteLine(imbalance > 0 ? $"# h g f e d c b a  +{imbalance}" : "# h g f e d c b a");
            
            for (int rank = 0; rank < 8; rank++)
            {
                string rankStr = $"{rank + 1} ";
                
                for (int file = 7; file >= 0; file--)
                    rankStr += pieceStrings[board.GetPiece(file, rank)] + " ";
                
                if (imbalance < 0 && rank == 7) // black advantage
                    rankStr += $" +{-imbalance}";
                
                Console.WriteLine(rankStr);
            }
        }
        else
        {
            // white's perspective
            Console.WriteLine(imbalance < 0 ? $"# a b c d e f g h  +{-imbalance}" : "# a b c d e f g h");
            
            for (int rank = 7; rank >= 0; rank--)
            {
                string rankStr = $"{rank + 1} ";
                
                for (int file = 0; file < 8; file++)
                    rankStr += pieceStrings[board.GetPiece((file, rank))] + " ";
                
                if (imbalance > 0 && rank == 0)
                    rankStr += $" +{imbalance}";
                
                Console.WriteLine(rankStr);
            }
        }
    }

    private bool PlayerTurn()
    {
        Timer timer = new Timer();
        timer.Start();
        
        // ask the player for a move
        Console.WriteLine("Enter your move:");
        string? playerMoveString = Console.ReadLine();
        if (!string.IsNullOrEmpty(playerMoveString))
        {
            if (playerMoveString == "exit") return false;
            if (playerMoveString.Contains("perft")) 
                try
                {
                    if (!debug) Console.Clear();
                    int perftDepth = int.Parse(playerMoveString.Split(' ')[1]);
                    Perft.Breakdown(board, perftDepth);
                    Console.ReadKey();
                }
                catch
                {
                    Console.WriteLine("perft error");
                    Console.ReadKey();
                }
            if (playerMoveString == "analyze")
            {
                Perft.AnalyzeBoard(board);
                Console.ReadKey();
            }
                        
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
                    game.Add(new PGNNode { board = new Board(board) , move = move , time = timer.Stop()});
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

    public Outcome GetOutcome()
    {
        return board.GetOutcome();
    }
    
    public void BotMove()
    {
        // make the top choice of the engine on the board
        Search.SearchResult result = Search.BestMove(board, depth, inBook, ply);
        Console.WriteLine($"Move made in {result.time}ms at depth {depth}");

        if (dynamicDepth)
        {
            if (result.time < increaseThreshold && !result.bookMove) depth++;
            else if (result.time > (board.IsEndgame() ? endgameDecreaseThreshold : decreaseThreshold)) depth = Math.Max(OriginalDepth, depth - 1);
            if ((result.move.Promotion & Pieces.TypeMask) == Pieces.WhiteQueen)
                depth--;   
        }

        inBook = result.bookMove;
        Move bestMove = result.move;

        LasMove = bestMove.Notate(board);
                        
        board.MakeMove(bestMove);
        game.Add(new PGNNode { board = new Board(board) , move = bestMove , time = result.time });
        ply++;
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

    public static readonly string[] IHateWindows =
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