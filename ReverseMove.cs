namespace Blaze;

public struct ReverseMove(Board board, Move move)
{
    // The source and destination are reverses of the original move
    public readonly (int file, int rank) Source = move.Destination;
    public readonly (int file, int rank) Destination = move.Source;
    public readonly uint Captured = board.GetPiece(move.Destination);
    public readonly bool Promotion = move.Promotion != 0b111;
    public readonly int Type = move.Type;
    public readonly byte CastlingRights = board.castling;
    public readonly (int file, int rank) EnPassant = board.enPassant;
    public readonly bool Pawn = move.Pawn;
    public readonly int HalfMoveClock = board.halfMoveClock;
    public readonly int hashkey = board.hashKey;
}