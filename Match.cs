using System.Text.RegularExpressions;

namespace Blaze;

public enum Type
{
    Random,
    Analysis,
}

public class Match
{
    private readonly Board board;
    private readonly Type type;
    private readonly int side; // side of the player
    private static readonly  Random random = new();

    public Match(Board board, Type type, int side = 0)
    {
        this.board = board;
        this.type = type;
        this.side = side;
    }

    public void Play()
    {
        bool play = true;
        
        while (play)
        {
            switch (type)
            {
                case Type.Analysis:
                    Print(side);
                    Console.WriteLine("Enter your move:");
                    string? moveString = Console.ReadLine();
                    if (!string.IsNullOrEmpty(moveString))
                    {
                        if (moveString == "end") play = false;
                        
                        // if the move is in the correct notation
                        else if (Regex.IsMatch(moveString, @"^[a-h][1-8][a-h][1-8][qrbn]?"))
                        {
                            Move[] filtered = Search.FilterChecks(Search.SearchBoard(board, false), board);
                            Move move = new Move(moveString, board);

                            Console.Clear();

                            // if the move is legal
                            if (filtered.Contains(move))
                            {
                                board.MakeMove(move);

                                Console.WriteLine(board.KingPositions[side]);
                                Console.WriteLine(Search.Attacked(board.KingPositions[side], board, 1-side));
                            }
                            else
                                Console.WriteLine("Illegal move");
                        }
                        else
                        {
                            Console.Clear(); 
                            Console.WriteLine("Incorrect notation");
                        }
                    }
                break;
                
                case Type.Random:
                    Print(side);
                    if (board.side == side)
                    {
                        // ask the player for a move
                        Console.WriteLine("Enter your move:");
                        string? playerMoveString = Console.ReadLine();
                        if (!string.IsNullOrEmpty(playerMoveString))
                        {
                            if (playerMoveString == "end") play = false;
                        
                            // if the move is in the correct notation
                            else if (Regex.IsMatch(playerMoveString, @"^[a-h][1-8][a-h][1-8][qrbn]?"))
                            {
                                Move[] filtered = Search.FilterChecks(Search.SearchBoard(board, false), board);
                                Move move = new Move(playerMoveString, board);

                                Console.Clear();

                                // if the move is legal
                                if (filtered.Contains(move))
                                    board.MakeMove(move);
                                else
                                    Console.WriteLine("Illegal move");
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Incorrect notation");
                            }
                        }
                    }
                    else
                    {
                        Console.Clear();
                        // make a random move on the board
                        Move[] filtered = Search.FilterChecks(Search.SearchBoard(board, false), board);
                        board.MakeMove(filtered[random.Next(0, filtered.Length)]);
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