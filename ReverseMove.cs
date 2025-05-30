namespace Blaze;

public class ReverseMove
{
    // The source and destination are reverses of the original move
    public readonly (int file, int rank) Source;
    public readonly (int file, int rank) Destination;
    public readonly uint Captured;
    public readonly bool Promotion;
    public readonly int Type;
    public readonly byte CastlingRights;
    public readonly (int file, int rank) EnPassant;
    public readonly bool Pawn;

    public ReverseMove(Board board, Move move)
    {
        Source = move.Destination;
        Destination = move.Source;
        Captured = board.GetPiece(move.Destination);
        Promotion = move.Promotion != 0b111;
        CastlingRights = board.castling;
        Type = move.Type;
        EnPassant = board.enPassant;
        Pawn = move.Pawn;
    }
}