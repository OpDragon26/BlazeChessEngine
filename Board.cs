using System.Diagnostics.CodeAnalysis;
[assembly: SuppressMessage("ReSharper","NonReadonlyMemberInGetHashCode")]

namespace Blaze;

public enum Outcome
{
    Ongoing,
    WhiteWin,
    BlackWin,
    Draw
}

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
    public readonly ulong[] bitboards = new ulong[4];
    // white pieces
    // black pieces
    // white pawns only
    // black pawns only
    
    // castling
    public byte castling = 0b1111; // white short, white long, black short, black long
    
    public readonly (int file, int rank)[] KingPositions = new (int file, int rank)[2];

    private int halfMoveClock;
    private int pawns = 16;
    private readonly int[] values = new int[2];
    private readonly Dictionary<int, int> repeat = new();
    private int hashKey;
    
    public Board(uint[] board)
    {
        this.board = board;
        
        // init bitboards
        AutoFillBitboards();

        hashKey = Hasher.ZobristHash(this);
    }
    
    public Board(Board board) // clone board
    {
        this.board = [board.board[0], board.board[1], board.board[2], board.board[3], board.board[4], board.board[5], board.board[6], board.board[7]];
        side = board.side;
        bitboards = [board.bitboards[0], board.bitboards[1], board.bitboards[2], board.bitboards[3]];
        enPassant = board.enPassant;
        castling = board.castling;
        KingPositions = [board.KingPositions[0], board.KingPositions[1]];
        halfMoveClock = board.halfMoveClock;
        pawns = board.pawns;
        values = [board.values[0], board.values[1]];
        repeat = new(board.repeat);
        hashKey = board.hashKey;
    }

    public Board(string FEN)
    {
        string[] fields = FEN.Split(' ');
        
        // piece placement data
        string[] ranks = fields[0].Split('/');
        board = new uint[8];
        for (int r = 0; r < 8; r++) // for each rank
        {
            // current file
            int indexer = 0;

            for (int c = 0; c < ranks[r].Length; c++) // for each character
            {
                if (Int32.TryParse(ranks[r][c].ToString(), out int v)) // if the character is a number
                {
                    // fill that many empty squares
                    for (int i = 0; i < v; i++)
                    {
                        SetPiece(indexer++, 7-r, Pieces.Empty);
                    }
                }
                else
                {
                    SetPiece(indexer++, 7-r, Pieces.Parse(ranks[r][c]));
                }
            }
        }
        
        AutoFillBitboards();
        CountPawns();

        // active side
        if (fields[1] == "w")
            side = 0;
        else if (fields[1] == "b")
            side = 1;
        else
            throw new Exception($"'{fields[1]}' is not a valid side");
        
        // castling availability
        castling = ParseCastling(fields[2]);
        
        // En passant target square
        enPassant = fields[3] == "-" ? (8, 8) : Move.ParseSquare(fields[3]);
        
        // half move clock
        halfMoveClock = Int32.Parse(fields[4]);

        hashKey = Hasher.ZobristHash(this);
    }

    private void AutoFillBitboards()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if (GetPiece(file, rank) != Pieces.Empty)
                {
                    bitboards[GetPiece(file, rank) >> 3] |= Bitboards.GetSquare(file, rank);
                    values[GetPiece(file, rank) >> 3] += Pieces.Value[GetPiece(file, rank)];
                    
                    if (GetPiece(file, rank) == Pieces.WhiteKing)
                        KingPositions[0] = (file, rank);
                    else if (GetPiece(file, rank) == Pieces.BlackKing)
                        KingPositions[1] = (file, rank);
                    else if (GetPiece(file, rank) == Pieces.WhitePawn)
                        bitboards[2] |= Bitboards.GetSquare(file, rank);
                    else if (GetPiece(file, rank) == Pieces.BlackPawn)
                        bitboards[3] |= Bitboards.GetSquare(file, rank);
                }
            }
        }
    }

    private void CountPawns()
    {
        pawns = 0;
        
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if (GetPiece(file, rank) is Pieces.WhitePawn or Pieces.BlackPawn)
                    pawns++;
            }
        }
    }
    
    public void MakeMove(Move move)
    {
        halfMoveClock++;
        if (side == 1)
            hashKey ^= Hasher.BlackToMove;

        // if the moved piece is a pawn, remove the source from the bitboard
        if (move.Pawn)
            bitboards[2 + side] ^= Bitboards.GetSquare(move.Source);

        if (GetPiece(move.Destination) != Pieces.Empty) // if the move is a capture
        {
            values[1-side] -= Pieces.Value[GetPiece(move.Destination)]; // subtract the value of the piece from the opponent
            bitboards[1-side] ^= Bitboards.GetSquare(move.Destination); // switch the square on the other side's bitboard
            halfMoveClock = 0;
            if ((GetPiece(move.Destination) & Pieces.TypeMask) == Pieces.WhitePawn) // if the taken piece was a pawn
            {
                pawns--;
                bitboards[3 - side] ^= Bitboards.GetSquare(move.Destination); // remove the square from the opponent's pawn bitboard
            }
        }
        
        if (move.Promotion == 0b111) // move is not a promotion
        {
            SetPiece(move.Destination, GetPiece(move.Source));
            
            if ((GetPiece(move.Destination) & Pieces.TypeMask) == Pieces.WhiteKing) // if the moved piece is a king
                KingPositions[side] = move.Destination;
            else if ((GetPiece(move.Destination) & Pieces.TypeMask) == Pieces.WhitePawn) // if the moved piece is a pawn
            {
                halfMoveClock = 0;
                bitboards[2 + side] ^= Bitboards.GetSquare(move.Destination); // add the destination to the pawn bitboard
            }
        }
        else // move is a promotion
        {
            SetPiece(move.Destination, ((uint)side << 3) | move.Promotion);
            halfMoveClock = 0; // the piece moved is definitely a pawn
            pawns--;
            values[side] += Pieces.Value[GetPiece(move.Destination)]; // add the value of the promoted piece to the moving side
            values[side] -= Pieces.Value[Pieces.WhitePawn | (side << 3)]; // subtract the value of the pawn from the moving side
        }
        
        // update the hash key
        hashKey ^= Hasher.PieceNumbers[GetPiece(move.Source), move.Source.file, move.Source.rank]; // remove the moving piece
        hashKey ^= Hasher.PieceNumbers[GetPiece(move.Destination), move.Destination.file, move.Destination.rank]; // add the moved piece, including promoted pieces
        hashKey ^= Hasher.CastlingNumbers[castling]; // remove the castling rights number
        // remove the en passant file if there was any, if it was 8, no need to change anything
        hashKey ^= Hasher.EnPassantFiles[enPassant.file];
        
        Clear(move.Source);
        enPassant = (8, 8);
        castling &= move.CastlingBan;
        
        hashKey ^= Hasher.CastlingNumbers[castling]; // add the new castling rights number
        
        // update bitboards
        bitboards[side] ^= Bitboards.GetSquare(move.Source);
        bitboards[side] ^= Bitboards.GetSquare(move.Destination);

        switch (move.Type)
        {
            case 0b0000: break;
            case 0b0001: // white double move
                enPassant = (move.Source.file, 2);
                hashKey ^= Hasher.EnPassantFiles[move.Source.file]; // add the en passant file
            break;
            
            case 0b1001: // black double move
                enPassant = (move.Source.file, 5);
                hashKey ^= Hasher.EnPassantFiles[move.Source.file]; // add the en passant file
            break;
            
            case 0b0010: // white short castle
                Clear(7, 0);
                SetPiece(5,0, Pieces.WhiteRook);
                bitboards[0] ^= Bitboards.GetSquare(7,0);
                bitboards[0] ^= Bitboards.GetSquare(5,0);
                // update the hash key
                hashKey ^= Hasher.PieceNumbers[Pieces.WhiteRook, 7, 0];
                hashKey ^= Hasher.PieceNumbers[Pieces.WhiteRook, 5, 0];
            break;
            
            case 0b0011: // white long castle
                Clear(0, 0);
                SetPiece(3,0, Pieces.WhiteRook);
                bitboards[0] ^= Bitboards.GetSquare(0,0);
                bitboards[0] ^= Bitboards.GetSquare(3,0);
                // update the hash key
                hashKey ^= Hasher.PieceNumbers[Pieces.WhiteRook, 0, 0];
                hashKey ^= Hasher.PieceNumbers[Pieces.WhiteRook, 3, 0];
            break;
            
            case 0b1010: // black short castle
                Clear(7, 7);
                SetPiece(5,7, Pieces.BlackRook);
                bitboards[1] ^= Bitboards.GetSquare(7,7);
                bitboards[1] ^= Bitboards.GetSquare(5,7);
                // update the hash key
                hashKey ^= Hasher.PieceNumbers[Pieces.BlackRook, 7, 7];
                hashKey ^= Hasher.PieceNumbers[Pieces.BlackRook, 5, 7];
            break;
            
            case 0b1011: // black long castle
                Clear(0, 7);
                SetPiece(3,7, Pieces.BlackRook);
                bitboards[1] ^= Bitboards.GetSquare(0,7);
                bitboards[1] ^= Bitboards.GetSquare(3,7);
                // update the hash key
                hashKey ^= Hasher.PieceNumbers[Pieces.BlackRook, 0, 7];
                hashKey ^= Hasher.PieceNumbers[Pieces.BlackRook, 3, 7];
            break;
            
            case 0b0100: // white en passant
                Clear(move.Destination.file, 4);
                bitboards[1] ^= Bitboards.GetSquare(move.Destination.file,4);
                bitboards[3] ^= Bitboards.GetSquare(move.Destination.file,4);
                values[1] += 100;
                // update the hash key
                hashKey ^= Hasher.PieceNumbers[Pieces.BlackPawn, move.Destination.file, 4];
            break;
            
            case 0b1100: // black en passant
                Clear(move.Destination.file, 3);
                bitboards[0] ^= Bitboards.GetSquare(move.Destination.file,3);
                bitboards[2] ^= Bitboards.GetSquare(move.Destination.file,3);
                values[0] -= 100;
                // update the hash key
                hashKey ^= Hasher.PieceNumbers[Pieces.WhitePawn, move.Destination.file, 3];
            break;
        }
        
        Add(); // adds the hash of the board to the dictionary

        side = 1 - side;
    }

    public bool IsDraw()
    {
        // threefold repetition or 50 move rule or each side has a minor piece or less and there are no pawns left (insufficient material)
        // stalemate requires searching for legal moves, so it's checked elsewhere
        return repeat.ContainsValue(3) || halfMoveClock > 100 || (pawns == 0 && values[0] <= 1300 && values[1] >= -1300);
    }
    
    public Outcome GetOutcome()
    {
        // gets the actual outcome of the match
        // requires searching for legal moves, shouldn't be used during a search
        if (IsDraw())
            return Outcome.Draw;
        
        Move[] moves = Search.FilterChecks(Search.SearchBoard(this, false), this);
        if (moves.Length == 0) // if there are no legal moves
            // if the king is attacked, the game ended in a checkmate, if it isn't the game is a draw by stalemate
            return Search.Attacked(KingPositions[side], this, 1-side) ? side == 0 ? Outcome.BlackWin : Outcome.WhiteWin : Outcome.Draw;

        return Outcome.Ongoing;
    }

    // adds the hash of the board 
    private void Add()
    {
        if (repeat.TryGetValue(hashKey, out int v)) // if the hash of the board is in already in the dictionary
        {
            // if it is found, v is at least 1, if it's more, this is the third time the position appears, so the game is a draw by threefold repetition
            repeat[hashKey] = v + 1;
        }
        else // the board position is entirely new
            repeat.Add(hashKey, 1);
    }

    private static readonly Dictionary<char, byte> CastlingAvailability = new()
    {
        {'K', 0b1000},
        {'Q', 0b0100},
        {'k', 0b0010},
        {'q', 0b0001}
    };
    
    private static byte ParseCastling(string s)
    {
        if (s == "-")
            return 0;

        byte c = 0;

        foreach (char cc in s)
        {
            if (CastlingAvailability.TryGetValue(cc, out byte ca))
                c |= ca;
            else
                throw new Exception($"Unable to parse FEN: Unknown castling availability char: {cc}");
        }

        return c;
    }

    public bool IsEndgame()
    {
        return values[0] + int.Abs(values[1]) < 3000;
    }

    public ulong AllPieces()
    {
        return bitboards[0] | bitboards[1];
    }
    
    private readonly uint PieceMask = 0xF; // covers the last 4 bits
    
    public uint GetPiece((int file, int rank) square) // overload that takes a tuple
    {
        return (board[square.rank] >> (square.file * 4)) & PieceMask;
    }
    
    public uint GetPiece(int file, int rank) // overload that takes individual values
    {
        return (board[rank] >> (file * 4)) & PieceMask;
    }

    private void Clear((int file, int rank) square) // overload that takes a tuple
    {
        board[square.rank] |= (PieceMask << (square.file * 4)); // set the given square to 1111
    }
    
    private void Clear(int file, int rank) // overload that takes individual values
    {
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
    // public const uint ColorMask = 0b1000;

    public static readonly int[] Value =
    [
        100, // 0
        500,
        300,
        300,
        900,
        1000, // 5
        0,
        0,
        -100, // 8
        -500,
        -300,
        -300,
        -900,
        -1000, // 13
        0,
        0
    ];

    private static readonly Dictionary<char, uint> PieceStrings = new()
    {
        {'P', WhitePawn },
        {'R', WhiteRook },
        {'N', WhiteKnight },
        {'B', WhiteBishop },
        {'Q', WhiteQueen },
        {'K', WhiteKing },
        {'p', BlackPawn},
        {'r', BlackRook },
        {'n', BlackKnight },
        {'b', BlackBishop },
        {'q', BlackQueen },
        {'k', BlackKing },
    };

    public static uint Parse(char s)
    {
        if (PieceStrings.TryGetValue(s, out uint piece))
            return piece;
        
        throw new FormatException($"Unable to parse FEN: Unknown piece: '{s}'");
    }
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