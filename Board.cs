namespace Blaze;

public class Board
{
    /*
    13 pieces -> 4 bits per piece -> 4 ulongs, each corresponding to two rows
    
    Black's perspective
      7 6 5 4 3 2 1 0
    0 0 0 0 0 0 0 0 0
    1 0 0 0 0 0 0 0 0
    2 1 1 1 1 1 1 1 1
    3 1 1 1 1 1 1 1 1
    4 2 2 2 2 2 2 2 2
    5 2 2 2 2 2 2 2 2
    6 3 3 3 3 3 3 3 3
    7 3 3 3 3 3 3 3 3
    */
    
    
    private readonly ulong[] board;
    private byte side = 0;
    private (int file, int rank) enPassant = (8, 8);

    public Board(ulong[] board)
    {
        this.board = board;
    }

    public void MakeMove(Move move)
    {
        if (move.Promotion == 0b1111)
            SetPiece(move.Destination, GetPiece(move.Source));
        else
            SetPiece(move.Destination, move.Promotion);
        Clear(move.Source);
        enPassant = (8, 8);

        switch (move.Type)
        {
            case 0b0000: break;
            case 0b1000: break;
            case 0b0001: // white double move
                enPassant = (move.Source.file, 2);
            break;
            
            case 0b1001: // black double move
                enPassant = (move.Source.file, 5);
            break;
            
            case 0b0010: // white short castle
                Clear(7, 0);
                SetPiece(5,0, Pieces.WhiteRook);
            break;
            
            case 0b0011: // white long castle
                Clear(0, 0);
                SetPiece(3,0, Pieces.WhiteRook);
            break;
            
            case 0b1010: // black short castle
                Clear(7, 7);
                SetPiece(5,7, Pieces.BlackRook);
            break;
            
            case 0b1011: // black long castle
                Clear(0, 7);
                SetPiece(3,7, Pieces.BlackRook);
            break;
            
            case 0b0100: // white en passant
                Clear(move.Destination.file, 5);
            break;
            
            case 0b1100: // black en passant
                Clear(move.Destination.file, 3);
            break;
        }
    }
    
    private readonly ulong PieceMask = 0xF; // covers the last 4 bits
    public ulong GetPiece((int file, int rank) square) // overload that takes a tuple
    {
        // divide rank by two to get the right ulong, push by 32 for first row, push by file for the piece
        return (board[square.rank / 2] >> ((1 - (square.rank % 2)) * 32 + square.file * 4)) & PieceMask;
    }
    
    public ulong GetPiece(int file, int rank) // overload that takes individual values
    {
        // divide rank by two to get the right ulong, push by 32 for first row, push by file for the piece
        return (board[rank / 2] >> ((1 - (rank % 2)) * 32 + file * 4)) & PieceMask;
    }

    private void Clear((int file, int rank) square) // overload that takes a tuple
    {
        // divide rank by two to get the right ulong, push left by 32 if first row, push by file for piece
        board[square.rank / 2] |= (PieceMask << (1 - (square.rank % 2)) * 32 + square.file * 4); // set the given square to 1111
    }
    private void Clear(int file, int rank) // overload that takes individual values
    {
        // divide rank by two to get the right ulong, push left by 32 if first row, push by file for piece
        board[rank / 2] |= (PieceMask << (1 - (rank % 2)) * 32 + file * 4); // set the given square to 1111
    }

    private void SetPiece((int file, int rank) square, ulong piece) // overload that takes a tuple
    {
        board[square.rank / 2] &= ~(PieceMask << (1 - (square.rank % 2)) * 32 + square.file * 4); // set the given square to 0000
        board[square.rank / 2] |= (piece << (1 - (square.rank % 2)) * 32 + square.file * 4); // set the square to the given piece
    }
    
    private void SetPiece(int file, int rank, ulong piece) // overload that takes individual values
    {
        board[rank / 2] &= ~(PieceMask << (1 - (rank % 2)) * 32 + file * 4); // set the given square to 0000
        board[rank / 2] |= (piece << (1 - (rank % 2)) * 32 + file * 4); // set the square to the given piece
    }
}

public static class Pieces
{
    // 4 bits per piece
    // white and black pieces only differ in the first bit
    public static readonly ulong WhitePawn = 0b0000; // 0
    public static readonly ulong WhiteRook = 0b0001; // 1
    public static readonly ulong WhiteKnight = 0b0010; // 2
    public static readonly ulong WhiteBishop = 0b0011; // 3
    public static readonly ulong WhiteQueen = 0b0100; // 4
    public static readonly ulong WhiteKing = 0b0101; // 5
    
    public static readonly ulong BlackPawn = 0b1000; // 8
    public static readonly ulong BlackRook = 0b1001; // 9
    public static readonly ulong BlackKnight = 0b1010; // 10
    public static readonly ulong BlackBishop = 0b1011; // 11
    public static readonly ulong BlackQueen = 0b1100; // 12
    public static readonly ulong BlackKing = 0b1101; // 13
    
    public static readonly ulong Empty = 0b1111; // 15
}

public static class Presets
{
    public static readonly ulong[] StartingBoard = new ulong[]
    {
        0b0001_0010_0011_0101_0100_0011_0010_0001_0000_0000_0000_0000_0000_0000_0000_0000, // white pieces
        ulong.MaxValue, // full empty row
        ulong.MaxValue,
        0b1000_1000_1000_1000_1000_1000_1000_1000_1001_1010_1011_1101_1100_1011_1010_1001 // black pieces
    };
}