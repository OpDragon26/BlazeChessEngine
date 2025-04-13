namespace Blaze;

public class Move
{
    public readonly (int file, int rank) Source;
    public readonly (int file, int rank) Destination;
    public readonly uint Promotion;
    public readonly int Type;
    public readonly int Priority;
    public readonly byte CastlingBan;
    public readonly bool Pawn;
    
    /*
    Special moves
    0000 - regular move
    0001 - white double move
    1001 - black double move
    0010 - white short castle
    0011 - white long castle
    1010 - black short castle
    1011 - black long castle
    0100 - white en passant
    1100 - black en passant
    */

    // the castling mask has up to 4 bits. When the move is made, the mask is then AND-ed with the castling rights in the board, removing the bit that is 0
    
    public Move((int file, int rank) source, (int file, int rank) destination, uint promotion = 0b111, int type = 0b0000, int priority = 0, byte castlingBan = 0b1111, bool pawn = false)
    {
        Source = source;
        Destination = destination;
        Promotion = promotion;
        Type = type;
        Priority = priority;
        CastlingBan = castlingBan;
        Pawn = pawn;

        if (destination == (7,0) || source == (7,0)) CastlingBan &= 0b0111; // if a move is made from or to h1, remove white's short castle rights
        if (destination == (0,0) || source == (0,0)) CastlingBan &= 0b1011; // if a move is made from or to a1, remove white's long castle rights
        if (destination == (7,7) || source == (7,7)) CastlingBan &= 0b1101; // if a move is made from or to h8, remove black's short castle rights
        if (destination == (0,7) || source == (0,7)) CastlingBan &= 0b1110; // if a move is made from or to a8, remove black's long castle rights
        if (source == (4, 0)) CastlingBan = 0b0011; // if the origin of the move is the white king's starting position, remove white's castling rights
        if (source == (4, 7)) CastlingBan = 0b1100; // if the origin of the move is the black king's starting position, remove black's castling rights
    }

    public override bool Equals(object? obj)
    {
        var item = obj as Move;
        if (item == null) return false;
        return Source == item.Source && Destination == item.Destination && Promotion == item.Promotion && Type == item.Type;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        hash |= Source.file << 29;
        hash |= Source.rank << 26;
        hash |= Destination.file << 23;
        hash |= Destination.rank << 20;
        hash |= (int)Promotion << 17;
        hash |= Type << 13;
        return hash;
    }

    private static readonly Dictionary<char, int> Indices = new()
    {
        { "a"[0], 0 },
        { "b"[0], 1 },
        { "c"[0], 2 },
        { "d"[0], 3 },
        { "e"[0], 4 },
        { "f"[0], 5 },
        { "g"[0], 6 },
        { "h"[0], 7 },
    };

    private static readonly Dictionary<char, uint> Promotions = new()
    {
        { "q"[0], 0b100 },
        { "r"[0], 0b001 },
        { "b"[0], 0b011 },
        { "n"[0], 0b010 },
    };
    public Move(string move, Board board)
    {
        Source = (Indices[move[0]], Convert.ToInt32(Convert.ToString(move[1])) - 1);
        Destination = (Indices[move[2]], Convert.ToInt32(Convert.ToString(move[3])) - 1);
        Promotion = move.Length == 5 ? Promotions[move[4]] : 0b111;
        Pawn = false;
        // implicit special moves
        
        if ((board.GetPiece(Source) & Pieces.TypeMask) == Pieces.WhitePawn) // if the piece is a pawn
        {
            Pawn = true;
            
            if (Destination == board.enPassant) // if the target is enPassantSquare
                Type = 0b0100 | (board.side << 3);
            else if ((Source.rank == 1 && Destination.rank == 3) || (Source.rank == 6 && Destination.rank == 4)) // if the move is a double move
                Type = 0b0001 | (board.side << 3);
        }
        else if ((board.GetPiece(Source) & Pieces.TypeMask) == Pieces.WhiteKing) // if the piece is a king
        {
            if (Source.file == 4 && (Source.rank == 0 || Source.rank == 7) && (Destination.rank == 0 || Destination.rank == 7)) // if the move is from the king starting square and on the 1st/7th ranks
            {
                if (Destination.file == 2) // long castle
                    Type = 0b0011 | (board.side << 3);
                else if (Destination.file == 6) // short castle
                    Type = 0b0010 | (board.side << 3);
            }
        }

        CastlingBan = 0b1111;
        if (Destination == (7,0) || Source == (7,0)) CastlingBan &= 0b0111; // if a move is made from or to h1, remove white's short castle rights
        if (Destination == (0,0) || Source == (0,0)) CastlingBan &= 0b1011; // if a move is made from or to a1, remove white's long castle rights
        if (Destination == (7,7) || Source == (7,7)) CastlingBan &= 0b1101; // if a move is made from or to h8, remove black's short castle rights
        if (Destination == (0,7) || Source == (0,7)) CastlingBan &= 0b1110; // if a move is made from or to a8, remove black's long castle rights
        if (Source == (4, 0)) CastlingBan = 0b0011; // if the origin of the move is the white king's starting position, remove white's castling rights
        if (Source == (4, 7)) CastlingBan = 0b1100; // if the origin of the move is the black king's starting position, remove black's castling rights
    }

    public static (int file, int rank) ParseSquare(string square)
    {
        if (Indices.TryGetValue(square[0], out var file))
        {
            if (Convert.ToInt32(Convert.ToString(square[1])) - 1 is >= 0 and <= 7)
                return (file, Convert.ToInt32(Convert.ToString(square[1])) - 1);
            throw new IndexOutOfRangeException($"Failed to parse square: '{square}' rank not within the confines of the board: {Convert.ToInt32(Convert.ToString(square[1])) - 1}");
        }
        
        throw new ArgumentException($"Failed to parse square: '{square}' Invalid file: '{square[0]}'");
    }
}