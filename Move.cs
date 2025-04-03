namespace Blaze;

public struct Move
{
    public (int file, int rank) Source;
    public (int file, int rank) Destination;
    public readonly uint Promotion;
    public readonly int Type;
    public readonly int Priority = 0;
    public readonly byte CastlingBan;
    
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
    
    public Move((int file, int rank) source, (int file, int rank) destination, uint promotion = 0b111, int type = 0b0000, int priority = 0, byte castlingBan = 0b1111)
    {
        Source = source;
        Destination = destination;
        Promotion = promotion;
        Type = type;
        Priority = priority;
        CastlingBan = castlingBan;

        if (destination == (7,0) || source == (7,0)) CastlingBan &= 0b0111; // if a move is made from or to h1, remove white's short castle rights
        if (destination == (0,0) || source == (0,0)) CastlingBan &= 0b1011; // if a move is made from or to a1, remove white's long castle rights
        if (destination == (7,7) || source == (7,7)) CastlingBan &= 0b1101; // if a move is made from or to h8, remove black's short castle rights
        if (destination == (0,7) || source == (0,7)) CastlingBan &= 0b1110; // if a move is made from or to a8, remove black's long castle rights
        if (source == (4, 0)) CastlingBan = 0b0011; // if the origin of the move is the white king's starting position, remove white's castling rights
        if (source == (4, 7)) CastlingBan = 0b1100; // if the origin of the move is the black king's starting position, remove black's castling rights
    }

    private static readonly Dictionary<char, int> Indices = new()
    {
        { "a".ToCharArray()[0], 0 },
        { "b".ToCharArray()[0], 1 },
        { "c".ToCharArray()[0], 2 },
        { "d".ToCharArray()[0], 3 },
        { "e".ToCharArray()[0], 4 },
        { "f".ToCharArray()[0], 5 },
        { "g".ToCharArray()[0], 6 },
        { "h".ToCharArray()[0], 7 },
    };

    private static readonly Dictionary<char, uint> Promotions = new()
    {
        { "q".ToCharArray()[0], 0b100 },
        { "r".ToCharArray()[0], 0b001 },
        { "b".ToCharArray()[0], 0b011 },
        { "n".ToCharArray()[0], 0b010 },
    };
    public Move(string move, Board board)
    {
        Source = (Indices[move[0]], Convert.ToInt32(Convert.ToString(move[1])) - 1);
        Destination = (Indices[move[2]], Convert.ToInt32(Convert.ToString(move[3])) - 1);
        Promotion = move.Length == 5 ? Promotions[move[4]] : 0b111;

        // implicit special moves
        
        if ((board.GetPiece(Source) & Pieces.TypeMask) == Pieces.WhitePawn) // if the piece is a pawn
        {
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
}