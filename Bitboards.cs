namespace Blaze;

public static class Bitboards
{
    /*
    The magic lookup returns a span of moves to be copied into the move array and its lenght, and a bitboard with squares that are captures, but might land on a friendly piece
    The returned moves all land on empty squares, while the bitboard shows moves that land on occupied squares. 
    Select the enemy pieces from those captured using the AND operation and a second magic lookup is initiated using that bitboard, which returns another span of moves
    */
    
    public static ulong[,] RookMasks = new ulong[8,8];
    public static ulong[,] BishopMasks = new ulong[8,8];
    
    public static ulong[,][] RookBlockers = new ulong[8,8][];
    public static ulong[,][] BishopBlockers = new ulong[8,8][];
    public static (Move[] moves, ulong captures)[,][] RookMoves = new (Move[] moves, ulong captures)[8,8][];
    public static (Move[] moves, ulong captures)[,][] BishopMoves = new (Move[] moves, ulong captures)[8,8][];
    public static ulong[,][] RookCaptureCombinations = new ulong[8,8][]; // for each square, for all blockers each combination
    public static ulong[,][] BishopCaptureCombinations = new ulong[8,8][];
    
    public static (Move[] moves, ulong captures)[,][] RookLookup = new (Move[] moves, ulong captures)[8,8][];
    public static (Move[] moves, ulong captures)[,][] BishopLookup = new (Move[] moves, ulong captures)[8,8][];
    
    public static (ulong magicNumber, int push, int highest)[,] RookMagicNumbers = new (ulong magicNumber, int push, int highest)[8,8];
    public static (ulong magicNumber, int push, int highest)[,] BishopMagicNumbers = new (ulong magicNumber, int push, int highest)[8,8];
    
    private const ulong File = 0x8080808080808080;
    private const ulong Rank = 0xFF00000000000000;
    
    private const ulong UpDiagonal = 0x102040810204080;
    private const ulong DownDiagonal = 0x8040201008040201;

    private static bool init;

    public static (Move[] moves, ulong captures) RookLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return RookLookup[pos.file, pos.rank]
        [
            ((blockers & RookMasks[pos.file, pos.rank]) // blocker combination
             * RookMagicNumbers[pos.file, pos.rank].magicNumber) >> RookMagicNumbers[pos.file, pos.rank].push
        ];
    }
    
    public static (Move[] moves, ulong captures) BishopLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return BishopLookup[pos.file, pos.rank]
        [
            ((blockers & BishopMasks[pos.file, pos.rank]) // blocker combination
            * BishopMagicNumbers[pos.file, pos.rank].magicNumber) >> BishopMagicNumbers[pos.file, pos.rank].push
        ];
    }

    public static void Init()
    {
        if (init) return;
        init = true;

        // Create the masks for every square on the board
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // The last bit also has to be evaluated in every direction, since it matters whether it's blocked or not
                RookMasks[file, rank] = (Rank >> (rank * 8)) ^ (File >> (7 - file));
                RookBlockers[file, rank] = Combinations(RookMasks[file, rank]);
                RookMoves[file, rank] = new (Move[] moves, ulong captures)[RookBlockers[file, rank].Length];

                List<ulong> rCombinations = new List<ulong>();
                for (int i = 0; i < RookBlockers[file, rank].Length; i++) // for every blocker combination
                {
                    RookMoves[file, rank][i] = GetMoves(RookBlockers[file, rank][i], (file, rank), Pieces.WhiteRook);
                    rCombinations.AddRange(Combinations(RookMoves[file, rank][i].captures));
                }
                RookCaptureCombinations[file, rank] = rCombinations.Distinct().ToArray();
                //RookCaptureCombinations[file, rank] = rCombinations.ToArray();
                //Console.WriteLine(RookCaptureCombinations[file, rank].Length);

                // bishop masks
                ulong relativeUD = UpDiagonal;
                ulong relativeDD = DownDiagonal;

                int UDPush = rank - file;
                int DDPush = rank + file - 7;

                if (UDPush >= 0)
                    relativeUD >>= UDPush * 8;
                else // negative
                    relativeUD <<= -UDPush * 8;
                if (DDPush >= 0)
                    relativeDD >>= DDPush * 8;
                else
                    relativeDD <<= -DDPush * 8;

                BishopMasks[file, rank] = relativeUD ^ relativeDD;
                BishopBlockers[file, rank] = Combinations(BishopMasks[file, rank]);
                BishopMoves[file, rank] = new (Move[] moves, ulong captures)[BishopBlockers[file, rank].Length];

                List<ulong> bCombinations = new List<ulong>();
                for (int i = 0; i < BishopBlockers[file, rank].Length; i++)
                {
                    BishopMoves[file, rank][i] = GetMoves(BishopBlockers[file, rank][i], (file, rank), Pieces.WhiteBishop);
                    bCombinations.AddRange(Combinations(BishopMoves[file, rank][i].captures));
                }
                BishopCaptureCombinations[file, rank] = bCombinations.Distinct().ToArray();
            }
        }

        Console.WriteLine("Generating Magic Numbers");

        //int done = 0;
        // create magic numbers and add to lookup
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // rook numbers
                RookMagicNumbers[file, rank] = MagicNumbers.RookNumbers[file, rank];
                RookLookup[file, rank] = new (Move[] moves, ulong captures)[RookMagicNumbers[file, rank].highest + 1];

                for (int i = 0; i < RookBlockers[file, rank].Length; i++) // for each blocker
                {
                    RookLookup[file, rank][(RookBlockers[file, rank][i] * RookMagicNumbers[file, rank].magicNumber) >> RookMagicNumbers[file, rank].push] = RookMoves[file, rank][i];
                }

                // bishop numbers
                //BishopMagicNumbers[file, rank] = MagicNumbers.BishopNumbers[file, rank];
                BishopLookup[file, rank] = new (Move[] moves, ulong captures)[BishopMagicNumbers[file, rank].highest + 1];

                for (int i = 0; i < BishopBlockers[file, rank].Length; i++) // for each blocker
                {
                    BishopLookup[file, rank][(BishopBlockers[file, rank][i] * BishopMagicNumbers[file, rank].magicNumber) >> BishopMagicNumbers[file, rank].push] = BishopMoves[file, rank][i];
                }

                //done++;
                //Console.WriteLine($"Square done {done}/64");
            }
        }
    }

    // reused code from my previous attempt
    // generates every bit combination using a mask
    private static ulong[] Combinations(ulong blockerMask)
    {
        // count how many on bits are there in the mask, that's going to give us the amount of combinations
        List<int> indices = new List<int>();
        int l = 0;
        for (int i = 0; i < 64; i++)
        {
            if (((blockerMask << 63 - i) >> 63) != 0)
            {
                l++;
                indices.Add(i);
            }
        }
        ulong[] combinations = new ulong[(int)Math.Pow(2, l)];

        // for each combination
        for (ulong i = 0; i < (ulong)combinations.Length; i++)
        {
            ulong combination = 0;

            // for each index in the mask, push the bits of the combination to the right indices 
            for (int j = 0; j < l; j++)
            {
                combination ^= ((i << 63 - j) >> 63) << indices[j];
            }

            combinations[i] = combination;
        }

        return combinations;
    }

    private static readonly (int file, int rank)[] RookPattern = new[]
    {
        (0, 1),
        (0, -1),
        (1, 0),
        (-1, 0),
    };
    private static readonly (int file, int rank)[] BishopPattern = new[]
    {
        (1, 1),
        (1, -1),
        (-1, 1),
        (-1, -1),
    };

    public static (Move[] moves, ulong captures) GetMoves(ulong blockers, (int file, int rank) pos, ulong piece, bool debug = false)
    {
        ulong captures = 0;
        List<Move> moves = new List<Move>();

        (int file, int rank)[] pattern = piece == Pieces.WhiteRook ? RookPattern : BishopPattern;

        for (int i = 0; i < 4; i++) // for each pattern
        {
            for (int j = 1; j < 7; j++) // in each direction
            {
                (int file, int rank) target = (pos.file + pattern[i].file * j, pos.rank + pattern[i].rank * j);

                if (!ValidSquare(target.file, target.rank)) // if the square is outside the bounds of the board
                {
                    break;
                }

                if ((blockers & GetSquare(target)) == 0) // if the targeted square is empty
                    moves.Add(new Move(pos, target, priority: 5));
                else
                {
                    captures |= GetSquare(target);
                    break;
                }
            }
        }

        return (moves.ToArray(), captures);
    }

    public static Move[] GetCaptures(ulong captures, (int file, int rank) pos)
    {
        List<Move> moves = new List<Move>();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if ((captures & GetSquare(file, rank)) != 0) // if the given square is on
                    moves.Add(new Move(pos, (file,rank), priority: 50));
            }
        }

        return moves.ToArray();
    }
    
    private const ulong Square = 0x8000000000000000;
    public static ulong GetSquare(int file, int rank) // overload that takes individual values
    {
        return Square >> (rank * 8 + 7 - file);
    }
    public static ulong GetSquare((int file, int rank) square) // overload that takes individual values
    {
        return Square >> (square.rank * 8 + 7 - square.file);
    }

    private static bool ValidSquare(int file, int rank)
    {
        return file >= 0 && file < 8 && rank >= 0 && rank < 8;
    }
}