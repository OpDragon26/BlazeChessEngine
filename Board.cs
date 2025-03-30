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
    
    // basic values
    private readonly ulong[] board;
    public int side;
    public (int file, int rank) enPassant = (8, 8);
    
    // bitboards
    public ulong[] bitboards = new ulong[2];

    public Board(ulong[] board)
    {
        this.board = board;
        
        // init bitboards
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if (GetPiece(file, rank) != Pieces.Empty)
                    bitboards[GetPiece(file, rank) >> 3] |= Bitboards.GetSquare(file, rank);
            }
        }
    }

    public Board(Board board) // clone board
    {
        this.board = (ulong[])board.board.Clone();
        side = board.side;
        bitboards = new[] { board.bitboards[0], board.bitboards[1] };
        enPassant = board.enPassant;
    }

    public void MakeMove(Move move)
    {
        if (move.Promotion == 0b111)
        {
            if (GetPiece(move.Destination) != Pieces.Empty) // if the move is a capture
                bitboards[1 - side] ^= Bitboards.GetSquare(move.Destination); // switch the square on the other side's bitboard
            SetPiece(move.Destination, GetPiece(move.Source));
        }
        else
            SetPiece(move.Destination, (ulong)(side << 3) | move.Promotion);
        
        Clear(move.Source);
        enPassant = (8, 8);
        
        // update bitboards
        bitboards[side] ^= Bitboards.GetSquare(move.Source);
        bitboards[side] ^= Bitboards.GetSquare(move.Destination);

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
                bitboards[side] ^= Bitboards.GetSquare(7,0);
                bitboards[side] ^= Bitboards.GetSquare(5,0);
            break;
            
            case 0b0011: // white long castle
                Clear(0, 0);
                SetPiece(3,0, Pieces.WhiteRook);
                bitboards[side] ^= Bitboards.GetSquare(0,0);
                bitboards[side] ^= Bitboards.GetSquare(3,0);
            break;
            
            case 0b1010: // black short castle
                Clear(7, 7);
                SetPiece(5,7, Pieces.BlackRook);
                bitboards[side] ^= Bitboards.GetSquare(7,7);
                bitboards[side] ^= Bitboards.GetSquare(5,7);
            break;
            
            case 0b1011: // black long castle
                Clear(0, 7);
                SetPiece(3,7, Pieces.BlackRook);
                bitboards[side] ^= Bitboards.GetSquare(0,7);
                bitboards[side] ^= Bitboards.GetSquare(3,7);
            break;
            
            case 0b0100: // white en passant
                Clear(move.Destination.file, 5);
                bitboards[side] ^= Bitboards.GetSquare(move.Destination.file,5);
            break;
            
            case 0b1100: // black en passant
                Clear(move.Destination.file, 3);
                bitboards[side] ^= Bitboards.GetSquare(move.Destination.file,3);
            break;
        }

        side = 1 - side;
    }

    public ulong AllPieces()
    {
        return bitboards[0] | bitboards[1];
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
    public const ulong WhitePawn = 0b0000; // 0
    public const ulong WhiteRook = 0b0001; // 1
    public const ulong WhiteKnight = 0b0010; // 2
    public const ulong WhiteBishop = 0b0011; // 3
    public const ulong WhiteQueen = 0b0100; // 4
    public const ulong WhiteKing = 0b0101; // 5
    
    public const ulong BlackPawn = 0b1000; // 8
    public const ulong BlackRook = 0b1001; // 9
    public const ulong BlackKnight = 0b1010; // 10
    public const ulong BlackBishop = 0b1011; // 11
    public const ulong BlackQueen = 0b1100; // 12
    public const ulong BlackKing = 0b1101; // 13
    
    public const ulong Empty = 0b1111; // 15
    
    public const ulong TypeMask = 0b111;
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