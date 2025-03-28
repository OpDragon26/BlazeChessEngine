namespace Blaze;

public class Match
{
    public readonly Board board;

    public Match(Board board)
    {
        this.board = board;
    }

    public void Print(int perspective)
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

    private static readonly string[] PieceStrings = new[]
    {
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
    };
}