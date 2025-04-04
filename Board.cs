namespace Blaze;

public class Board
{
    /*
    13 pieces -> 4 bits per piece -> 8 uints, each corresponding to one row
    
    Black's perspective
      7 6 5 4 3 2 1 0
    0 0 0 0 0 0 0 0 0
    1 1 1 1 1 1 1 1 1
    2 2 2 2 2 2 2 2 2 
    3 3 3 3 3 3 3 3 3 
    4 4 4 4 4 4 4 4 4 
    5 5 5 5 5 5 5 5 5 
    6 6 6 6 6 6 6 6 6 
    7 7 7 7 7 7 7 7 7 
    */
    
    // basic values
    private readonly uint[] board;
    public int side;
    public (int file, int rank) enPassant = (8, 8);
    
    // bitboards
    public readonly ulong[] bitboards = new ulong[2];
    
    // castling
    public byte castling = 0b1111; // white short, white long, black short, black long
    
    public (int file, int rank)[] KingPositions = [(4,0),(4,7)];
    
    public Board(uint[] board)
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
        this.board = [board.board[0], board.board[1], board.board[2], board.board[3], board.board[4], board.board[5], board.board[6], board.board[7]];
        side = board.side;
        bitboards = [board.bitboards[0], board.bitboards[1]];
        enPassant = board.enPassant;
        castling = board.castling;
    }
    
    public void MakeMove(Move move)
    {
        if (move is null) Console.WriteLine("Huh");
        if (GetPiece(move.Destination) != Pieces.Empty) // if the move is a capture
            bitboards[1 - side] ^= Bitboards.GetSquare(move.Destination); // switch the square on the other side's bitboard
        
        if (move.Promotion == 0b111)
        {
            SetPiece(move.Destination, GetPiece(move.Source));
            
            if ((GetPiece(move.Destination) & Pieces.TypeMask) == Pieces.WhiteKing) // if the moved piece is a king
                KingPositions[side] = move.Destination;
        }
        else
        {
            SetPiece(move.Destination, ((uint)side << 3) | move.Promotion);
        }
        
        Clear(move.Source);
        enPassant = (8, 8);
        castling &= move.CastlingBan;
        
        // update bitboards
        bitboards[side] ^= Bitboards.GetSquare(move.Source);
        bitboards[side] ^= Bitboards.GetSquare(move.Destination);

        switch (move.Type)
        {
            case 0b0000: break;
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
                Clear(move.Destination.file, 4);
                bitboards[side] ^= Bitboards.GetSquare(move.Destination.file,4);
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
    
    private readonly uint PieceMask = 0xF; // covers the last 4 bits
    
    public uint GetPiece((int file, int rank) square) // overload that takes a tuple
    {
        // divide rank by two to get the right uint, push by 32 for first row, push by file for the piece
        return (board[square.rank] >> (square.file * 4)) & PieceMask;
    }
    
    public uint GetPiece(int file, int rank) // overload that takes individual values
    {
        // divide rank by two to get the right uint, push by 32 for first row, push by file for the piece
        return (board[rank] >> (file * 4)) & PieceMask;
    }

    private void Clear((int file, int rank) square) // overload that takes a tuple
    {
        // divide rank by two to get the right uint, push left by 32 if first row, push by file for piece
        board[square.rank] |= (PieceMask << (square.file * 4)); // set the given square to 1111
    }
    
    private void Clear(int file, int rank) // overload that takes individual values
    {
        // divide rank by two to get the right uint, push left by 32 if first row, push by file for piece
        board[rank] |= (PieceMask << (file * 4)); // set the given square to 1111
    }
    
    private void SetPiece((int file, int rank) square, uint piece) // overload that takes a tuple
    {
        board[square.rank] &= ~(PieceMask << (square.file * 4)); // set the given square to 0000
        board[square.rank] |= (piece << (square.file * 4)); // set the square to the given piece
    }
    
    private void SetPiece(int file, int rank, uint piece) // overload that takes individual values
    {
        board[rank] &= ~(PieceMask << (file * 4)); // set the given square to 0000
        board[rank] |= (piece << (file * 4)); // set the square to the given piece
    }
}

public static class Pieces
{
    // 4 bits per piece
    // white and black pieces only differ in the first bit
    public const uint WhitePawn = 0b0000; // 0
    public const uint WhiteRook = 0b0001; // 1
    public const uint WhiteKnight = 0b0010; // 2
    public const uint WhiteBishop = 0b0011; // 3
    public const uint WhiteQueen = 0b0100; // 4
    public const uint WhiteKing = 0b0101; // 5
    
    public const uint BlackPawn = 0b1000; // 8
    public const uint BlackRook = 0b1001; // 9
    public const uint BlackKnight = 0b1010; // 10
    public const uint BlackBishop = 0b1011; // 11
    public const uint BlackQueen = 0b1100; // 12
    public const uint BlackKing = 0b1101; // 13
    
    public const uint Empty = 0b1111; // 15
    
    public const uint TypeMask = 0b111;
    public const uint ColorMask = 0b1000;
}

public static class Presets
{
    public static readonly uint[] StartingBoard =
    [
        0b0001_0010_0011_0101_0100_0011_0010_0001, // white pieces
        0b0000_0000_0000_0000_0000_0000_0000_0000,
        uint.MaxValue, // full empty row
        uint.MaxValue,
        uint.MaxValue,
        uint.MaxValue,
        0b1000_1000_1000_1000_1000_1000_1000_1000, // black pieces
        0b1001_1010_1011_1101_1100_1011_1010_1001
    ];
}