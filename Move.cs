namespace Blaze;

public struct Move
{
    public (int file, int rank) Source;
    public (int file, int rank) Destination;
    public ulong Promotion;
    public int Type;
    
    /*
    Special moves
    0000 - regular move
    1000 - also regular move
    0001 - white double move
    1001 - black double move
    0010 - white short castle
    0011 - white long castle
    1010 - black short castle
    1011 - black long castle
    0100 - white en passant
    1100 - black en passant
    */

    public Move((int file, int rank) source, (int file, int rank) destination, ulong promotion = 0b111, int type = 0b0000)
    {
        Source = source;
        Destination = destination;
        Promotion = promotion;
        Type = type;
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

    private static readonly Dictionary<char, ulong> Promotions = new()
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
    }
    

}