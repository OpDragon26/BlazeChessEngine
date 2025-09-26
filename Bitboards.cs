namespace Blaze;

public static class Bitboards
{
    /*
    The magic lookup returns a span of moves to be copied into the move array and its lenght, and a bitboard with squares that are captures, but might land on a friendly piece
    The returned moves all land on empty squares, while the bitboard shows moves that land on occupied squares. 
    Select the enemy pieces from those captured using the AND operation and a second magic lookup is initiated using that bitboard, which returns another span of moves
    */

    private static readonly ulong[,] RookMasks = new ulong[8,8];
    private static readonly ulong[,] BishopMasks = new ulong[8,8];

    private static readonly ulong[,][] RookBlockers = new ulong[8,8][];
    private static readonly ulong[,][] BishopBlockers = new ulong[8,8][];
    private static readonly (Move[] moves, ulong captures)[,][] RookMoves = new (Move[] moves, ulong captures)[8,8][];
    private static readonly (Move[] moves, ulong captures)[,][] BishopMoves = new (Move[] moves, ulong captures)[8,8][];
    private static readonly ulong[,][] RookCaptureCombinations = new ulong[8,8][]; // for each square, for all blockers each combination
    private static readonly ulong[,][] BishopCaptureCombinations = new ulong[8,8][];
    
    public static readonly ulong[,] KnightMasks = new ulong[8,8];
    private static readonly ulong[,][] KnightCombinations = new ulong[8,8][];
    public static readonly ulong[,] KingMasks = new ulong[8,8];
    private static readonly ulong[,][] KingCombinations = new ulong[8,8][];
    
    private static readonly ulong[,] WhitePawnMoveMasks = new ulong[8,8];
    private static readonly ulong[,][] WhitePawnMoveCombinations = new ulong[8,8][];
    private static readonly ulong[,] BlackPawnMoveMasks = new ulong[8,8];
    private static readonly ulong[,][] BlackPawnMoveCombinations = new ulong[8,8][];
    public static readonly ulong[,] WhitePawnCaptureMasks = new ulong[8,8];
    private static readonly ulong[,][] WhitePawnCaptureCombinations = new ulong[8,8][];
    public static readonly ulong[,] BlackPawnCaptureMasks = new ulong[8,8];
    private static readonly ulong[,][] BlackPawnCaptureCombinations = new ulong[8,8][];
    
    private static readonly ulong[,] SmallRookMasks = new ulong[8,8];
    private static readonly ulong[,] SmallBishopMasks = new ulong[8,8];
    private static readonly ulong[,][] SmallRookCombinations = new ulong[8,8][];
    private static readonly ulong[,][] SmallBishopCombinations = new ulong[8,8][];
    private static readonly ulong[,][] SmallRookBitboards = new ulong[8,8][];
    private static readonly ulong[,][] SmallBishopBitboards = new ulong[8,8][];
    
    private const ulong Frame = 0xFF818181818181FF;
    
    private static ulong[]? EnPassantMasks; // contains both the source and the destination
    
    public static readonly ulong WhiteShortCastleMask = 0x6000000000000000;
    public static readonly ulong WhiteLongCastleMask = 0xE00000000000000;
    public static readonly ulong BlackShortCastleMask = 0x60;
    public static readonly ulong BlackLongCastleMask = 0xE;
    public static readonly Move WhiteShortCastle = new((4,0), (6,0), type: 0b0010, priority: 6);
    public static readonly Move WhiteLongCastle = new((4,0), (2,0), type: 0b0011, priority: 3);
    public static readonly Move BlackShortCastle = new((4,7), (6,7), type: 0b1010, priority: 6);
    public static readonly Move BlackLongCastle = new((4,7), (2,7), type: 0b1011, priority: 3);
    
    private static readonly ulong[] PassedPawnMasks = new ulong[8];
    public static readonly ulong[] NeighbourMasks = new ulong[8];
    public static readonly int[] BitValues = new int[byte.MaxValue + 1];
    
    private static readonly int[,] PriorityWeights =
    {
        {0,1,2,3,3,2,1,0},
        {1,2,3,4,4,3,2,1},
        {2,3,4,5,5,4,3,2},
        {3,4,5,6,6,5,4,3},
        {3,4,5,6,6,5,4,3},
        {2,3,4,5,5,4,3,2},
        {1,2,3,4,4,3,2,1},
        {0,1,2,3,3,2,1,0},
    };

    public static readonly int[][] AdjacentFiles = [[0,1], [0,1,2], [1,2,3], [2,3,4], [3,4,5], [4,5,6], [5,6,7], [6,7]];
    
    private static class MagicLookup
    {
        public static readonly (ulong magicNumber, int push, int highest)[,] RookMove = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] BishopMove = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] RookCapture = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] BishopCapture = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] KnightMove = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] KingMove = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] WhitePawnMove = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] BlackPawnMove = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] WhitePawnCapture = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] BlackPawnCapture = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] RookBitboardNumbers = new (ulong magicNumber, int push, int highest)[8,8];
        public static readonly (ulong magicNumber, int push, int highest)[,] BishopBitboardNumbers = new (ulong magicNumber, int push, int highest)[8,8];
        public static (ulong magicNumber, int push, int highest) EnPassantNumbers;
        
        public static readonly (Move[] moves, ulong captures)[,][] RookLookup = new (Move[] moves, ulong captures)[8,8][];
        public static readonly (Move[] moves, ulong captures)[,][] BishopLookup = new (Move[] moves, ulong captures)[8,8][];
        public static readonly ulong[,][] RookLookupCapturesArray = new ulong[8,8][];
        public static readonly ulong[,][] BishopLookupCapturesArray = new ulong[8,8][];
        public static readonly Move[,][][] RookCaptureLookup = new Move[8,8][][];
        public static readonly Move[,][][] BishopCaptureLookup = new Move[8,8][][];
        public static readonly Move[,][][] KnightLookup = new Move[8,8][][];
        public static readonly Move[,][][] KnightCaptureLookup = new Move[8,8][][];
        public static readonly Move[,][][] KingLookup = new Move[8,8][][];
        public static readonly Move[,][][] KingCaptureLookup = new Move[8,8][][];
        public static readonly Move[,][][] WhitePawnLookup = new Move[8,8][][];
        public static readonly Move[,][][] BlackPawnLookup = new Move[8,8][][];
        public static readonly Move[,][][] WhitePawnCaptureLookup = new Move[8,8][][];
        public static readonly Move[,][][] BlackPawnCaptureLookup = new Move[8,8][][];
        public static readonly ulong[,][] RookBitboardLookup = new ulong[8,8][];
        public static readonly ulong[,][] BishopBitboardLookup = new ulong[8,8][];
        public static Move[] EnPassantLookupArray = [];
        public static readonly int[,][] KingSafetyLookup = new int[8,8][];
        
        public static readonly ulong[,][] RookPinLineBitboardLookup =  new ulong[8,8][];
        public static readonly ulong[,][] BishopPinLineBitboardLookup = new ulong[8,8][];
        public static readonly List<PinSearchResult>[,][] RookPinLookup = new List<PinSearchResult>[8,8][];
        public static readonly List<PinSearchResult>[,][] BishopPinLookup = new List<PinSearchResult>[8,8][];
    }

    private const ulong File = 0x8080808080808080;
    private const ulong Rank = 0xFF00000000000000;

    private const ulong UpDiagonal = 0x102040810204080;
    private const ulong DownDiagonal = 0x8040201008040201;

    private const ulong SmallFile = 0x80808080808000;
    private const ulong SmallRank = 0x7E00000000000000;

    public const ulong KingSafetyAppliesWhite = 0xC7C7000000000000; 
    public const ulong KingSafetyAppliesBlack = 0xC7C7;

    private static bool init;

    public static ref (Move[] moves, ulong captures) RookLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref MagicLookup.RookLookup[pos.file, pos.rank]
        [
            ((blockers & RookMasks[pos.file, pos.rank]) // blocker combination
             * MagicLookup.RookMove[pos.file, pos.rank].magicNumber) >> MagicLookup.RookMove[pos.file, pos.rank].push
        ];
    }
    
    public static ref (Move[] moves, ulong captures) BishopLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref MagicLookup.BishopLookup[pos.file, pos.rank]
        [
            ((blockers & BishopMasks[pos.file, pos.rank]) // blocker combination
            * MagicLookup.BishopMove[pos.file, pos.rank].magicNumber) >> MagicLookup.BishopMove[pos.file, pos.rank].push
        ];
    }
    
    public static ref Move[] RookLookupCaptures((int file, int rank) pos, ulong captures)
    {
        return ref MagicLookup.RookCaptureLookup[pos.file, pos.rank]
            [(captures * MagicLookup.RookCapture[pos.file, pos.rank].magicNumber) >> MagicLookup.RookCapture[pos.file, pos.rank].push];
    }
    
    public static ulong RookLookupCaptureBitboards((int file, int rank) pos, ulong blockers)
    {
        return MagicLookup.RookLookupCapturesArray[pos.file, pos.rank]
            [((blockers & RookMasks[pos.file, pos.rank]) * MagicLookup.RookMove[pos.file, pos.rank].magicNumber) >> MagicLookup.RookMove[pos.file, pos.rank].push];
    }
    
    public static ulong BishopLookupCaptureBitboards((int file, int rank) pos, ulong blockers)
    {
        return MagicLookup.BishopLookupCapturesArray[pos.file, pos.rank]
            [((blockers & BishopMasks[pos.file, pos.rank]) * MagicLookup.BishopMove[pos.file, pos.rank].magicNumber) >> MagicLookup.BishopMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] BishopLookupCaptures((int file, int rank) pos, ulong captures)
    {
        return ref MagicLookup.BishopCaptureLookup[pos.file, pos.rank]
            [(captures * MagicLookup.BishopCapture[pos.file, pos.rank].magicNumber) >> MagicLookup.BishopCapture[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KnightLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref MagicLookup.KnightLookup[pos.file, pos.rank]
            [((~blockers & KnightMasks[pos.file, pos.rank]) * MagicLookup.KnightMove[pos.file, pos.rank].magicNumber) >> MagicLookup.KnightMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KnightLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref MagicLookup.KnightCaptureLookup[pos.file, pos.rank]
            [((enemy & KnightMasks[pos.file, pos.rank]) * MagicLookup.KnightMove[pos.file, pos.rank].magicNumber) >> MagicLookup.KnightMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KingLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref MagicLookup.KingLookup[pos.file, pos.rank]
            [((~blockers & KingMasks[pos.file, pos.rank]) * MagicLookup.KingMove[pos.file, pos.rank].magicNumber) >> MagicLookup.KingMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KingLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref MagicLookup.KingCaptureLookup[pos.file, pos.rank]
            [((enemy & KingMasks[pos.file, pos.rank]) * MagicLookup.KingMove[pos.file, pos.rank].magicNumber) >> MagicLookup.KingMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] WhitePawnLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref MagicLookup.WhitePawnLookup[pos.file, pos.rank]
            [((blockers & WhitePawnMoveMasks[pos.file, pos.rank]) * MagicLookup.WhitePawnMove[pos.file, pos.rank].magicNumber) >> MagicLookup.WhitePawnMove[pos.file, pos.rank].push];
    }

    public static ref Move[] BlackPawnLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref MagicLookup.BlackPawnLookup[pos.file, pos.rank]
            [((blockers & BlackPawnMoveMasks[pos.file, pos.rank]) * MagicLookup.BlackPawnMove[pos.file, pos.rank].magicNumber) >> MagicLookup.BlackPawnMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] WhitePawnLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref MagicLookup.WhitePawnCaptureLookup[pos.file, pos.rank]
            [((enemy & WhitePawnCaptureMasks[pos.file, pos.rank]) * MagicLookup.WhitePawnCapture[pos.file, pos.rank].magicNumber) >> MagicLookup.WhitePawnCapture[pos.file, pos.rank].push];
    }
    
    public static ref Move[] BlackPawnLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref MagicLookup.BlackPawnCaptureLookup[pos.file, pos.rank]
            [((enemy & BlackPawnCaptureMasks[pos.file, pos.rank]) * MagicLookup.BlackPawnCapture[pos.file, pos.rank].magicNumber) >> MagicLookup.BlackPawnCapture[pos.file, pos.rank].push];
    }
    
    public static ref Move EnPassantLookup(ulong enPassant)
    {
        return ref MagicLookup.EnPassantLookupArray[(enPassant * MagicLookup.EnPassantNumbers.magicNumber) >> MagicLookup.EnPassantNumbers.push];
    }

    public static int KingSafetyBonusLookup((int file, int rank) pos, ulong blockers)
    {
        return MagicLookup.KingSafetyLookup[pos.file, pos.rank]
            [((~blockers & KingMasks[pos.file, pos.rank]) * MagicLookup.KingMove[pos.file, pos.rank].magicNumber) >> MagicLookup.KingMove[pos.file, pos.rank].push];
    }

    public static ulong RookMoveBitboardLookup((int file, int rank) pos, ulong blockers)
    {
        return MagicLookup.RookBitboardLookup[pos.file, pos.rank]
            [((blockers & SmallRookMasks[pos.file, pos.rank]) * MagicLookup.RookBitboardNumbers[pos.file, pos.rank].magicNumber) >> MagicLookup.RookBitboardNumbers[pos.file, pos.rank].push];
    }
    
    public static ulong BishopMoveBitboardLookup((int file, int rank) pos, ulong blockers)
    {
        return MagicLookup.BishopBitboardLookup[pos.file, pos.rank]
            [((blockers & SmallBishopMasks[pos.file, pos.rank]) * MagicLookup.BishopBitboardNumbers[pos.file, pos.rank].magicNumber) >> MagicLookup.BishopBitboardNumbers[pos.file, pos.rank].push];
    }

    public static ulong RookPinLineLookup((int file, int rank) pos, ulong blockers)
    {
        return MagicLookup.RookPinLineBitboardLookup[pos.file, pos.rank]
            [((blockers & RookMasks[pos.file, pos.rank]) * MagicLookup.RookMove[pos.file, pos.rank].magicNumber) >> MagicLookup.RookMove[pos.file, pos.rank].push];
    }
    
    public static ulong BishopPinLineLookup((int file, int rank) pos, ulong blockers)
    {
        return MagicLookup.BishopPinLineBitboardLookup[pos.file, pos.rank]
            [((blockers & BishopMasks[pos.file, pos.rank]) * MagicLookup.BishopMove[pos.file, pos.rank].magicNumber) >> MagicLookup.BishopMove[pos.file, pos.rank].push];
    }
    
    public static List<PinSearchResult> RookPinSearch((int file, int rank) pos, ulong selected)
    {
        return MagicLookup.RookPinLookup[pos.file, pos.rank]
            [((selected & RookMasks[pos.file, pos.rank]) * MagicLookup.RookMove[pos.file, pos.rank].magicNumber) >> MagicLookup.RookMove[pos.file, pos.rank].push];
    }
    
    public static List<PinSearchResult> BishopPinSearch((int file, int rank) pos, ulong selected)
    {
        return MagicLookup.BishopPinLookup[pos.file, pos.rank]
            [((selected & BishopMasks[pos.file, pos.rank]) * MagicLookup.BishopMove[pos.file, pos.rank].magicNumber) >> MagicLookup.BishopMove[pos.file, pos.rank].push];
    }

    public static void Init()
    {
        if (init) return;
        init = true;
        List<ulong> enPassantBitboards = new List<ulong>();

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
                
                SmallRookMasks[file, rank] = ((SmallRank >> (rank * 8)) ^ (SmallFile >> (7 - file))) & ~GetSquare(file, rank);
                SmallRookCombinations[file, rank] = Combinations(SmallRookMasks[file, rank]);
                SmallRookBitboards[file, rank] = new ulong[SmallRookCombinations[file, rank].Length];
                for (int i = 0; i < SmallRookCombinations[file, rank].Length; i++)
                    SmallRookBitboards[file, rank][i] = GetMoveBitboards(SmallRookCombinations[file, rank][i], (file, rank), Pieces.WhiteRook);
                
                SmallBishopMasks[file, rank] = (relativeUD ^ relativeDD) & ~Frame;
                SmallBishopCombinations[file, rank] = Combinations(SmallBishopMasks[file, rank]);
                SmallBishopBitboards[file, rank] = new ulong[SmallBishopCombinations[file, rank].Length];
                for (int i = 0; i < SmallBishopCombinations[file, rank].Length; i++)
                    SmallBishopBitboards[file, rank][i] = GetMoveBitboards(SmallBishopCombinations[file, rank][i], (file, rank), Pieces.WhiteBishop);
                
                // knight masks
                KnightMasks[file, rank] = GetMask((file, rank), KnightPattern);
                KnightCombinations[file, rank] = Combinations(KnightMasks[file, rank]);
                
                // king masks
                ulong kingMask = ulong.MaxValue;

                        
                for (int k = 0; k < 8; k++)
                {
                    if (!(k == file || k == file - 1 || k == file + 1))
                    {
                        kingMask &= ~(File >> (7 - k));
                    }
                            
                    if (!(k == rank || k == rank - 1 || k == rank + 1))
                    {
                        kingMask &= ~(Rank >> (k * 8));
                    }
                }
                
                kingMask &= ~GetSquare(file, rank);
                
                KingMasks[file, rank] = kingMask;
                KingCombinations[file, rank] = Combinations(kingMask);
                
                // pawn moves
                
                // white pawns
                ulong wpmMask = 0;
                ulong wpcMask = 0;
                wpmMask |= GetSquare(file, rank + 1);
                if (ValidSquare(file + 1, rank + 1)) wpcMask |= GetSquare(file + 1, rank + 1);
                if (ValidSquare(file - 1, rank + 1)) wpcMask |= GetSquare(file - 1, rank + 1);
                
                if (rank == 1) wpmMask |= GetSquare(file, rank + 2);
                
                WhitePawnMoveMasks[file, rank] = wpmMask;
                WhitePawnMoveCombinations[file, rank] = Combinations(wpmMask);
                WhitePawnCaptureMasks[file, rank] = wpcMask;
                WhitePawnCaptureCombinations[file, rank] = Combinations(wpcMask);
                if (rank == 4) // white en passant rank
                {
                    if (ValidSquare(file + 1, 5)) enPassantBitboards.Add(GetSquare(file, rank) | GetSquare(file + 1, 5));
                    if (ValidSquare(file - 1, 5)) enPassantBitboards.Add(GetSquare(file, rank) | GetSquare(file - 1, 5));
                }
                
                // black pawns
                ulong bpmMask = 0;
                ulong bpcMask = 0;
                bpmMask |= GetSquare(file, rank - 1);
                if (ValidSquare(file + 1, rank - 1)) bpcMask |= GetSquare(file + 1, rank - 1);
                if (ValidSquare(file - 1, rank - 1)) bpcMask |= GetSquare(file - 1, rank - 1);
                
                if (rank == 6) bpmMask |= GetSquare(file, rank - 2);
                
                BlackPawnMoveMasks[file, rank] = bpmMask;
                BlackPawnMoveCombinations[file, rank] = Combinations(bpmMask);
                BlackPawnCaptureMasks[file, rank] = bpcMask;
                BlackPawnCaptureCombinations[file, rank] = Combinations(bpcMask);
                
                if (rank == 3) // black en passant rank
                {
                    if (ValidSquare(file + 1, 2)) enPassantBitboards.Add(GetSquare(file, rank) | GetSquare(file + 1, 2));
                    if (ValidSquare(file - 1, 2)) enPassantBitboards.Add(GetSquare(file, rank) | GetSquare(file - 1, 2));
                }
                
                if (rank != 0) // only needs to be checked once per file
                    continue;
                
                // passed files
                // triple files, used to check for passed pawns

                ulong passedMask = ulong.MaxValue;
                
                for (int k = 0; k < 8; k++)
                {
                    if (!(k == file || k == file - 1 || k == file + 1))
                    {
                        passedMask &= ~(File >> (7 - k));
                    }
                }
                
                PassedPawnMasks[file] = passedMask;
                
                ulong neighborMask = ulong.MaxValue;
                
                for (int k = 0; k < 8; k++)
                {
                    if (!(k == file - 1 || k == file + 1))
                    {
                        neighborMask &= ~(File >> (7 - k));
                    }
                }
                
                NeighbourMasks[file] = neighborMask;
            }
        }
        
        EnPassantMasks = enPassantBitboards.ToArray();
        MagicLookup.EnPassantNumbers = (15417481889308385644, 58, 63); // MagicNumbers.GenerateRepeat(EnPassantMasks, 10000);
        MagicLookup.EnPassantLookupArray = new Move[MagicLookup.EnPassantNumbers.highest + 1];
        foreach (ulong mask in EnPassantMasks) // for each possible en passant
        {
            MagicLookup.EnPassantLookupArray[(mask * MagicLookup.EnPassantNumbers.magicNumber) >> MagicLookup.EnPassantNumbers.push] = GetEnPassantMoves(mask);
        }
        
        //Console.WriteLine("Generating Magic Numbers");
        
        //int done = 0;
        // create magic numbers and add to lookup
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // rook numbers
                MagicLookup.RookMove[file, rank] = MagicNumbers.RookNumbers[file, rank];
                MagicLookup.RookLookup[file, rank] = new (Move[] moves, ulong captures)[MagicLookup.RookMove[file, rank].highest + 1];
                MagicLookup.RookLookupCapturesArray[file, rank] = new ulong[MagicLookup.RookMove[file, rank].highest + 1];
                
                for (int i = 0; i < RookBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.RookLookup[file, rank][(RookBlockers[file, rank][i] * MagicLookup.RookMove[file, rank].magicNumber) >> MagicLookup.RookMove[file, rank].push] = RookMoves[file, rank][i];
                    MagicLookup.RookLookupCapturesArray[file, rank][(RookBlockers[file, rank][i] * MagicLookup.RookMove[file, rank].magicNumber) >> MagicLookup.RookMove[file, rank].push] = RookMoves[file, rank][i].captures;
                }
                
                // bishop numbers
                MagicLookup.BishopMove[file, rank] = MagicNumbers.BishopNumbers[file, rank];
                MagicLookup.BishopLookup[file, rank] = new (Move[] moves, ulong captures)[MagicLookup.BishopMove[file, rank].highest + 1];
                MagicLookup.BishopLookupCapturesArray[file, rank] = new ulong[MagicLookup.BishopMove[file, rank].highest + 1];
                
                for (int i = 0; i < BishopBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.BishopLookup[file, rank][(BishopBlockers[file, rank][i] * MagicLookup.BishopMove[file, rank].magicNumber) >> MagicLookup.BishopMove[file, rank].push] = BishopMoves[file, rank][i];
                    MagicLookup.BishopLookupCapturesArray[file, rank][(BishopBlockers[file, rank][i] * MagicLookup.BishopMove[file, rank].magicNumber) >> MagicLookup.BishopMove[file, rank].push] = BishopMoves[file, rank][i].captures;
                }
                
                // rook captures
                MagicLookup.RookCapture[file, rank] = MagicNumbers.RookCaptureNumbers[file, rank]; // MagicNumbers.GenerateRepeat(RookCaptureCombinations[file, rank], 1000);
                MagicLookup.RookCaptureLookup[file, rank] = new Move[MagicLookup.RookCapture[file, rank].highest + 1][];
                
                for (int i = 0; i < RookCaptureCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.RookCaptureLookup[file, rank][(RookCaptureCombinations[file, rank][i] * MagicLookup.RookCapture[file, rank].magicNumber) >> MagicLookup.RookCapture[file, rank].push] = GetBitboardMoves(RookCaptureCombinations[file, rank][i], (file, rank), 50, isCapture: true);
                }
                
                // bishop captures
                MagicLookup.BishopCapture[file, rank] = MagicNumbers.BishopCaptureNumbers[file, rank]; // MagicNumbers.GenerateRepeat(BishopCaptureCombinations[file, rank], 1000);
                MagicLookup.BishopCaptureLookup[file, rank] = new Move[MagicLookup.BishopCapture[file, rank].highest + 1][];
                
                for (int i = 0; i < BishopCaptureCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.BishopCaptureLookup[file, rank][(BishopCaptureCombinations[file, rank][i] * MagicLookup.BishopCapture[file, rank].magicNumber) >> MagicLookup.BishopCapture[file, rank].push] = GetBitboardMoves(BishopCaptureCombinations[file, rank][i], (file, rank), 50, isCapture: true);
                }
                
                MagicLookup.RookBitboardNumbers[file, rank] = MagicNumbers.RookBitboardNumbers[file, rank];
                MagicLookup.RookBitboardLookup[file, rank] = new ulong[MagicLookup.RookBitboardNumbers[file, rank].highest + 1];

                for (int i = 0; i < SmallRookCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.RookBitboardLookup[file, rank][(SmallRookCombinations[file, rank][i] * MagicLookup.RookBitboardNumbers[file, rank].magicNumber) >> MagicLookup.RookBitboardNumbers[file, rank].push] = SmallRookBitboards[file, rank][i];
                }
                
                MagicLookup.BishopBitboardNumbers[file, rank] = MagicNumbers.BishopBitboardNumbers[file, rank];
                MagicLookup.BishopBitboardLookup[file, rank] = new ulong[MagicLookup.BishopBitboardNumbers[file, rank].highest + 1];
                
                for (int i = 0; i < SmallBishopCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.BishopBitboardLookup[file, rank][(SmallBishopCombinations[file, rank][i] * MagicLookup.BishopBitboardNumbers[file, rank].magicNumber) >> MagicLookup.BishopBitboardNumbers[file, rank].push] = SmallBishopBitboards[file, rank][i];
                }
                
                // knight moves
                // since the potential captures and moves are based on the same combinations, the same magic numbers can be used
                MagicLookup.KnightMove[file, rank] = MagicNumbers.KnightNumbers[file, rank];
                MagicLookup.KnightLookup[file, rank] = new Move[MagicLookup.KnightMove[file, rank].highest + 1][];
                MagicLookup.KnightCaptureLookup[file, rank] = new Move[MagicLookup.KnightMove[file, rank].highest + 1][];
                
                for (int i = 0; i < KnightCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookup.KnightLookup[file, rank][(KnightCombinations[file, rank][i] * MagicLookup.KnightMove[file, rank].magicNumber) >> MagicLookup.KnightMove[file, rank].push] = GetBitboardMoves(KnightCombinations[file, rank][i], (file, rank), 5);
                    MagicLookup.KnightCaptureLookup[file, rank][(KnightCombinations[file, rank][i] * MagicLookup.KnightMove[file, rank].magicNumber) >> MagicLookup.KnightMove[file, rank].push] = GetBitboardMoves(KnightCombinations[file, rank][i], (file, rank), 50, isCapture: true);
                }
                
                // king moves
                MagicLookup.KingMove[file, rank] = MagicNumbers.KingNumbers[file, rank]; // MagicNumbers.GenerateRepeat(KingCombinations[file, rank], 5000);
                MagicLookup.KingLookup[file, rank] = new Move[MagicLookup.KingMove[file, rank].highest + 1][];
                MagicLookup.KingCaptureLookup[file, rank] = new Move[MagicLookup.KingMove[file, rank].highest + 1][];
                MagicLookup.KingSafetyLookup[file, rank] = new int[MagicLookup.KingMove[file, rank].highest + 1];
                
                for (int i = 0; i < KingCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookup.KingLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookup.KingMove[file, rank].magicNumber) >> MagicLookup.KingMove[file, rank].push] = GetBitboardMoves(KingCombinations[file, rank][i], (file, rank), 5);
                    MagicLookup.KingCaptureLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookup.KingMove[file, rank].magicNumber) >> MagicLookup.KingMove[file, rank].push] = GetBitboardMoves(KingCombinations[file, rank][i], (file, rank), 3, isCapture: true);
                    MagicLookup.KingSafetyLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookup.KingMove[file, rank].magicNumber) >> MagicLookup.KingMove[file, rank].push] = Weights.KingSafetyBonuses[UInt64.PopCount(KingCombinations[file, rank][i])];
                }
                
                //done++;
                //Console.WriteLine($"Square done {done}/64");
                // pawn moves
                if (rank == 0 || rank == 7)
                    continue;
                
                // white pawns
                // moves
                MagicLookup.WhitePawnMove[file, rank] = MagicNumbers.WhitePawnMoveNumbers[file, rank];
                MagicLookup.WhitePawnLookup[file, rank] = new Move[MagicLookup.WhitePawnMove[file, rank].highest + 1][];

                for (int i = 0; i < WhitePawnMoveCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookup.WhitePawnLookup[file, rank][(WhitePawnMoveCombinations[file, rank][i] * MagicLookup.WhitePawnMove[file, rank].magicNumber) >> MagicLookup.WhitePawnMove[file, rank].push] = GetPawnMoves(WhitePawnMoveCombinations[file, rank][i], (file, rank), 0);
                }
                // captures
                MagicLookup.WhitePawnCapture[file, rank] = MagicNumbers.WhiteCaptureMoveNumbers[file, rank];
                MagicLookup.WhitePawnCaptureLookup[file, rank] = new Move[MagicLookup.WhitePawnCapture[file, rank].highest + 1][];

                for (int i = 0; i < WhitePawnCaptureCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookup.WhitePawnCaptureLookup[file, rank][(WhitePawnCaptureCombinations[file, rank][i] * MagicLookup.WhitePawnCapture[file, rank].magicNumber) >> MagicLookup.WhitePawnCapture[file, rank].push] = GetPawnCaptures(WhitePawnCaptureCombinations[file, rank][i], (file, rank), 0);
                }
                
                // black pawns
                // moves
                MagicLookup.BlackPawnMove[file, rank] = MagicNumbers.BlackPawnMoveNumbers[file, rank];
                MagicLookup.BlackPawnLookup[file, rank] = new Move[MagicLookup.BlackPawnMove[file, rank].highest + 1][];

                for (int i = 0; i < BlackPawnMoveCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookup.BlackPawnLookup[file, rank][(BlackPawnMoveCombinations[file, rank][i] * MagicLookup.BlackPawnMove[file, rank].magicNumber) >> MagicLookup.BlackPawnMove[file, rank].push] = GetPawnMoves(BlackPawnMoveCombinations[file, rank][i], (file, rank), 1);
                }
                // captures
                MagicLookup.BlackPawnCapture[file, rank] = MagicNumbers.BlackCaptureMoveNumbers[file, rank];
                MagicLookup.BlackPawnCaptureLookup[file, rank] = new Move[MagicLookup.BlackPawnCapture[file, rank].highest + 1][];

                for (int i = 0; i < BlackPawnCaptureCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookup.BlackPawnCaptureLookup[file, rank][(BlackPawnCaptureCombinations[file, rank][i] * MagicLookup.BlackPawnCapture[file, rank].magicNumber) >> MagicLookup.BlackPawnCapture[file, rank].push] = GetPawnCaptures(BlackPawnCaptureCombinations[file, rank][i], (file, rank), 1);
                }
                
                // pin lines
                // rook pin lines
                MagicLookup.RookPinLineBitboardLookup[file, rank] = new ulong[MagicLookup.RookMove[file, rank].highest + 1];
                
                for (int i = 0; i < RookBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.RookPinLineBitboardLookup[file, rank][(RookBlockers[file, rank][i] * MagicLookup.RookMove[file, rank].magicNumber) >> MagicLookup.RookMove[file, rank].push] = GetPinLine(RookBlockers[file, rank][i], (file, rank), Pieces.WhiteRook);
                }
                
                // bishop pin lines
                MagicLookup.BishopPinLineBitboardLookup[file, rank] = new ulong[MagicLookup.BishopMove[file, rank].highest + 1];
                
                for (int i = 0; i < BishopBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.BishopPinLineBitboardLookup[file, rank][(BishopBlockers[file, rank][i] * MagicLookup.BishopMove[file, rank].magicNumber) >> MagicLookup.BishopMove[file, rank].push] = GetPinLine(BishopBlockers[file, rank][i], (file, rank), Pieces.WhiteBishop);
                }
                
                // pin search
                MagicLookup.RookPinLookup[file,rank] = new List<PinSearchResult>[MagicLookup.RookMove[file, rank].highest + 1];

                for (int i = 0; i < RookBlockers[file, rank].Length; i++)
                {
                    MagicLookup.RookPinLookup[file, rank][(RookBlockers[file, rank][i] * MagicLookup.RookMove[file, rank].magicNumber) >> MagicLookup.RookMove[file, rank].push] = GeneratePinResult((file, rank), RookBlockers[file, rank][i], Pieces.WhiteRook);
                }
                
                MagicLookup.BishopPinLookup[file, rank] = new List<PinSearchResult>[MagicLookup.BishopMove[file, rank].highest + 1];

                for (int i = 0; i < BishopBlockers[file, rank].Length; i++)
                {
                    MagicLookup.BishopPinLookup[file, rank][(BishopBlockers[file, rank][i] * MagicLookup.BishopMove[file, rank].magicNumber) >> MagicLookup.BishopMove[file, rank].push] = GeneratePinResult((file, rank), BishopBlockers[file, rank][i], Pieces.WhiteBishop);
                }
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

    private static readonly (int file, int rank)[] RookPattern =
    [
        (0, 1),
        (0, -1),
        (1, 0),
        (-1, 0),
    ];
    private static readonly (int file, int rank)[] BishopPattern =
    [
        (1, 1),
        (1, -1),
        (-1, 1),
        (-1, -1),
    ];

    private static readonly (int file, int rank)[] KnightPattern =
    [
        (2, 1),
        (2, -1),
        (-2, 1),
        (-2, -1),
        (1, 2),
        (1, -2),
        (-1, 2),
        (-1, -2),
    ];

    private static ulong GetPinLine(ulong blockers, (int file, int rank) pos, ulong piece)
    {
        ulong final = 0;
        
        (int file, int rank)[] pattern = piece == Pieces.WhiteRook ? RookPattern : BishopPattern;
        
        for (int i = 0; i < 4; i++) // for each pattern
        {
            bool blockerFound = false;
            for (int j = 1; j < 8; j++) // in each direction
            {
                (int file, int rank) target = (pos.file + pattern[i].file * j, pos.rank + pattern[i].rank * j);
                
                if (!ValidSquare(target.file, target.rank)) // if the square is outside the bounds of the board
                    break;
                if ((blockers & GetSquare(target)) != 0) // if the targeted square is occupied
                {
                    blockerFound = true;
                    break;
                }
            }
            
            if (!blockerFound)
                continue;
            
            for (int j = 1; j < 8; j++) // in each direction
            {
                (int file, int rank) target = (pos.file + pattern[i].file * j, pos.rank + pattern[i].rank * j);
                
                if (!ValidSquare(target.file, target.rank)) // if the square is outside the bounds of the board
                    break;
                if ((blockers & GetSquare(target)) == 0) // if the targeted square is empty
                    final |= GetSquare(target);
                else
                {
                    final |= GetSquare(target);
                    break;
                }
            }
        }
        
        return final;
    }

    private static List<PinSearchResult> GeneratePinResult((int file, int rank) pos, ulong pieces, uint piece)
    {
        List<PinSearchResult> results = new();
        (int file, int rank)[] pattern = piece == Pieces.WhiteRook ? RookPattern : BishopPattern;
        
        for (int i = 0; i < 4; i++) // for each direction
        {
            int found = 0;
            (int, int) pinPos = (0,0);
            (int, int) pinnedPos = (0, 0);
            ulong path = 0;
            
            for (int j = 1; j < 8; j++) // in each direction
            {
                (int file, int rank) target = (pos.file + pattern[i].file * j, pos.rank + pattern[i].rank * j);
                
                if (!ValidSquare(target.file, target.rank)) // if the square is outside the bounds of the board
                    break;
                if ((pieces & GetSquare(target)) != 0) // if the targeted square is not empty
                {
                    if (++found > 2) break;
                    if (found == 1) // pinned piece
                        pinnedPos = target;
                    if (found == 2) // pinning piece
                    {
                        pinPos = target;
                        path |= GetSquare(target);
                    }
                }
                else
                    path |= GetSquare(target);
            }
            
            if (found == 2)
                results.Add(new PinSearchResult(pinPos, GetSquare(pinnedPos), path));
        }
        
        return results;
    }

    public readonly struct PinSearchResult((int file, int rank) pinningPos, ulong pinnedPiece, ulong path)
    {
        public readonly (int file, int rank) pinningPos = pinningPos;
        public readonly ulong pinnedPiece = pinnedPiece;
        public readonly ulong path = path;
    }
    
    private static (Move[] moves, ulong captures) GetMoves(ulong blockers, (int file, int rank) pos, ulong piece)
    {
        ulong captures = 0;
        List<Move> moves = new List<Move>();

        (int file, int rank)[] pattern = piece == Pieces.WhiteRook ? RookPattern : BishopPattern;

        for (int i = 0; i < 4; i++) // for each pattern
        {
            for (int j = 1; j < 8; j++) // in each direction
            {
                (int file, int rank) target = (pos.file + pattern[i].file * j, pos.rank + pattern[i].rank * j);
                
                if (!ValidSquare(target.file, target.rank)) // if the square is outside the bounds of the board
                    break;
                if ((blockers & GetSquare(target)) == 0) // if the targeted square is empty
                    moves.Add(new Move(pos, target, priority: 5 + PriorityWeights[target.file, target.rank]));
                else
                {
                    captures |= GetSquare(target);
                    break;
                }
            }
        }
        
        return (moves.ToArray(), captures);
    }
    
    private static ulong GetMoveBitboards(ulong blockers, (int file, int rank) pos, ulong piece)
    {
        ulong moves = 0;

        (int file, int rank)[] pattern = piece == Pieces.WhiteRook ? RookPattern : BishopPattern;

        for (int i = 0; i < 4; i++) // for each pattern
        {
            for (int j = 1; j < 8; j++) // in each direction
            {
                (int file, int rank) target = (pos.file + pattern[i].file * j, pos.rank + pattern[i].rank * j);
                
                if (!ValidSquare(target.file, target.rank)) // if the square is outside the bounds of the board
                    break;
                if ((blockers & GetSquare(target)) == 0) // if the targeted square is empty
                    moves |= GetSquare(target);
                else
                {
                    moves |= GetSquare(target);
                    break;
                }
            }
        }
        
        return moves;
    }
    
    private static Move[] GetBitboardMoves(ulong bitboard, (int file, int rank) pos, int priority, byte castlingBan = 0b1111, bool isCapture = false)
    {
        List<Move> moves = new List<Move>();
        
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if ((bitboard & GetSquare(file, rank)) != 0) // if the given square is on
                    moves.Add(new Move(pos, (file,rank), priority: priority + PriorityWeights[file, rank], castlingBan: castlingBan, capture: isCapture));
            }
        }
        
        return moves.ToArray();
    }
    
    private static Move GetEnPassantMoves(ulong bitboard)
    {
        (int file, int rank) source = (0, 0);
        (int file, int rank) target = (0, 0);
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if ((bitboard & GetSquare(file, rank)) != 0) // if the given square is on
                {
                    if (rank == 4 || rank == 3) 
                        source = (file, rank);
                    else if (rank == 5 || rank == 2)
                        target = (file, rank);
                }
            }
        }

        Move move = new Move(source, target, type: source.rank == 4 ? 0b0100 : 0b1100, priority: 3, pawn: true); // source.rank == 4 => white

        return move;
    }

    private static Move[] GetPawnMoves(ulong combination, (int file, int rank) pos, int color)
    {
        List<Move> moves = new List<Move>();
        
        if (color == 0)
        {
            if (pos.rank == 6) // white promotion rank
            {
                if ((combination & GetSquare(pos.file, 7)) == 0) // if the square in front is empty
                {
                    moves.Add(new Move(pos, (pos.file, 7), Pieces.WhiteQueen, priority: 30, pawn: true));
                    moves.Add(new Move(pos, (pos.file, 7), Pieces.WhiteRook, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file, 7), Pieces.WhiteBishop, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file, 7), Pieces.WhiteKnight, priority: 2, pawn: true));
                }
            }
            else // not a promotion
            {
                if ((combination & GetSquare(pos.file, pos.rank + 1)) == 0) // if the square in front is empty
                {
                    moves.Add(new Move(pos, (pos.file, pos.rank + 1), priority: 5 + PriorityWeights[pos.file, pos.rank + 2] + pos.rank, pawn: true));
                    
                    if (pos.rank == 1 && (combination & GetSquare(pos.file, pos.rank + 2)) == 0) // check if the double move square is empty
                        moves.Add(new Move(pos, (pos.file, pos.rank + 2), priority: 6 + PriorityWeights[pos.file, pos.rank + 2] + pos.rank, type: 0b0001, pawn: true));
                }
            }
        }
        else
        {
            if (pos.rank == 1) // black promotion rank
            {
                if ((combination & GetSquare(pos.file, 0)) == 0) // if the square behind is empty
                {
                    moves.Add(new Move(pos, (pos.file, 0), Pieces.BlackQueen, priority: 30, pawn: true));
                    moves.Add(new Move(pos, (pos.file, 0), Pieces.BlackRook, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file, 0), Pieces.BlackBishop, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file, 0), Pieces.BlackKnight, priority: 2, pawn: true));
                }
            }
            else // not a promotion
            {
                if ((combination & GetSquare(pos.file, pos.rank - 1)) == 0) // if the square behind is empty
                {
                    moves.Add(new Move(pos, (pos.file, pos.rank - 1), priority: 12 + PriorityWeights[pos.file, pos.rank - 1] - pos.rank, pawn: true));
                    
                    if (pos.rank == 6 && (combination & GetSquare(pos.file, pos.rank - 2)) == 0) // check if the double move square is empty
                        moves.Add(new Move(pos, (pos.file, pos.rank - 2), priority: 13 + PriorityWeights[pos.file, pos.rank - 2] - pos.rank, type: 0b1001, pawn: true));
                }
            }
        }
        
        return moves.ToArray();
    }

    private static Move[] GetPawnCaptures(ulong combination, (int file, int rank) pos, int color)
    {
        List<Move> moves = new List<Move>();
        
        if (color == 0)
        {
            if (pos.rank == 6) // white promotion rank
            {
                // check if the capture squares are occupied
                if ((combination & GetSquare(pos.file + 1, 7)) != 0)
                {
                    moves.Add(new Move(pos, (pos.file + 1, 7), Pieces.WhiteQueen, priority: 65, pawn: true));
                    moves.Add(new Move(pos, (pos.file + 1, 7), Pieces.WhiteRook, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file + 1, 7), Pieces.WhiteBishop, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file + 1, 7), Pieces.WhiteKnight, priority: 2, pawn: true));
                }
                if ((combination & GetSquare(pos.file - 1, 7)) != 0)
                {
                    moves.Add(new Move(pos, (pos.file - 1, 7), Pieces.WhiteQueen, priority: 65, pawn: true));
                    moves.Add(new Move(pos, (pos.file - 1, 7), Pieces.WhiteRook, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file - 1, 7), Pieces.WhiteBishop, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file - 1, 7), Pieces.WhiteKnight, priority: 2, pawn: true));
                }
            }
            else // not a promotion
            {
                // check if the capture squares are occupied
                if ((combination & GetSquare(pos.file + 1, pos.rank + 1)) != 0)
                    moves.Add(new Move(pos, (pos.file + 1, pos.rank + 1), priority: 60, pawn: true));
                if ((combination & GetSquare(pos.file - 1, pos.rank + 1)) != 0)
                    moves.Add(new Move(pos, (pos.file - 1, pos.rank + 1), priority: 60, pawn: true));
            }
        }
        else
        {
            if (pos.rank == 1) // black promotion rank
            {
                // check if the capture squares are occupied
                if ((combination & GetSquare(pos.file + 1, 0)) != 0)
                {
                    moves.Add(new Move(pos, (pos.file + 1, 0), Pieces.BlackQueen, priority: 65, pawn: true));
                    moves.Add(new Move(pos, (pos.file + 1, 0), Pieces.BlackRook, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file + 1, 0), Pieces.BlackBishop, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file + 1, 0), Pieces.BlackKnight, priority: 2, pawn: true));
                }
                if ((combination & GetSquare(pos.file - 1, 0)) != 0)
                {
                    moves.Add(new Move(pos, (pos.file - 1, 0), Pieces.BlackQueen, priority: 65, pawn: true));
                    moves.Add(new Move(pos, (pos.file - 1, 0), Pieces.BlackRook, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file - 1, 0), Pieces.BlackBishop, priority: 2, pawn: true));
                    moves.Add(new Move(pos, (pos.file - 1, 0), Pieces.BlackKnight, priority: 2, pawn: true));
                }
            }
            else // not a promotion
            {
                // check if the capture squares are occupied
                if ((combination & GetSquare(pos.file + 1, pos.rank - 1)) != 0)
                    moves.Add(new Move(pos, (pos.file + 1, pos.rank - 1), priority: 60, pawn: true));
                if ((combination & GetSquare(pos.file - 1, pos.rank - 1)) != 0)
                    moves.Add(new Move(pos, (pos.file - 1, pos.rank - 1), priority: 60, pawn: true));
            }
        }
        
        return moves.ToArray();
    }

    private static ulong GetMask((int file, int rank) pos, (int file, int rank)[] pattern)
    {
        ulong mask = 0;
        
        for (int i = 0; i < pattern.Length; i++) // for each pattern
        {
            (int file, int rank) target = (pos.file + pattern[i].file, pos.rank + pattern[i].rank);

            if (ValidSquare(target.file, target.rank))
                mask |= GetSquare(target);
        }
        
        return mask;
    }

    public static ulong GetMoveBitboard(Move[] moveList)
    {
        ulong bitboard = 0;

        foreach (Move move in moveList)
        {
            bitboard |= GetSquare(move.Destination);
        }
        
        return bitboard;
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

    public static ulong GetFile(int file)
    {
        return File >> (7 - file);
    }

    public static ulong GetWhitePassedPawnMask(int file, int rank)
    {
        return PassedPawnMasks[file] >> (rank * 8 + 8);
    }
    public static ulong GetBlackPassedPawnMask(int file, int rank)
    {
        return PassedPawnMasks[file] << ((8 - rank) * 8);
    }

    private static bool ValidSquare(int file, int rank)
    {
        return file is >= 0 and < 8 && rank is >= 0 and < 8;
    }
}