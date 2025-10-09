namespace Blaze;

public static class Bitboards
{
    /*
    The magic lookup returns a span of moves to be copied into the move array and its lenght, and a bitboard with squares that are captures, but might land on a friendly piece
    The returned moves all land on empty squares, while the bitboard shows moves that land on occupied squares. 
    Select the enemy pieces from those captured using the AND operation and a second magic lookup is initiated using that bitboard, which returns another span of moves
    */

    public static readonly ulong[,] RookMasks = new ulong[8,8];
    public static readonly ulong[,] BishopMasks = new ulong[8,8];

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
    
    public static readonly ulong[,] WhitePawnMoveMasks = new ulong[8,8];
    private static readonly ulong[,][] WhitePawnMoveCombinations = new ulong[8,8][];
    public static readonly ulong[,] BlackPawnMoveMasks = new ulong[8,8];
    private static readonly ulong[,][] BlackPawnMoveCombinations = new ulong[8,8][];
    public static readonly ulong[,] WhitePawnCaptureMasks = new ulong[8,8];
    private static readonly ulong[,][] WhitePawnCaptureCombinations = new ulong[8,8][];
    public static readonly ulong[,] BlackPawnCaptureMasks = new ulong[8,8];
    private static readonly ulong[,][] BlackPawnCaptureCombinations = new ulong[8,8][];
    
    public static readonly ulong[,] SmallRookMasks = new ulong[8,8];
    public static readonly ulong[,] SmallBishopMasks = new ulong[8,8];
    private static readonly ulong[,][] SmallRookCombinations = new ulong[8,8][];
    private static readonly ulong[,][] SmallBishopCombinations = new ulong[8,8][];
    private static readonly ulong[,][] SmallRookBitboards = new ulong[8,8][];
    private static readonly ulong[,][] SmallBishopBitboards = new ulong[8,8][];
    
    private static readonly ulong[,][] BlockCaptures = new ulong[8,8][];
    private static ulong[]? BlockMoves;
    
    private const ulong Frame = 0xFF818181818181FF;

    public const ulong BlackPossibleEnPassant = 0x100000000;
    public const ulong WhitePossibleEnPassant = 0x1000000;
    
    private static ulong[]? EnPassantMasks; // contains both the source and the destination
    
    public static readonly ulong WhiteShortCastleMask = 0x6000000000000000;
    public static readonly ulong WhiteLongCastleMask = 0xC00000000000000;
    public static readonly ulong BlackShortCastleMask = 0x60;
    public static readonly ulong BlackLongCastleMask = 0xC;
    public static readonly Move WhiteShortCastle = new((4,0), (6,0), type: 0b0010, priority: 6);
    public static readonly Move WhiteLongCastle = new((4,0), (2,0), type: 0b0011, priority: 3);
    public static readonly Move BlackShortCastle = new((4,7), (6,7), type: 0b1010, priority: 6);
    public static readonly Move BlackLongCastle = new((4,7), (2,7), type: 0b1011, priority: 3);
    
    public static readonly ulong[] PassedPawnMasks = new ulong[8];
    public static readonly ulong[] NeighbourMasks = new ulong[8];
    public static readonly ulong[,,,] PathLookup =  new ulong[8,8,8,8];
    
    public const ulong RightPawns = 0xf0f0f0f0f0f000;
    public const ulong LeftPawns = 0xf0f0f0f0f0f00;
    public const ulong CenterPawns = 0x3c3c3c3c3c3c00;
    public const ulong LeftPawnMask = 0x7070707070700;
    public const ulong RightPawnMask = 0xe0e0e0e0e0e000;
    public const ulong CenterPawnMask = 0x18181818181800;

    public const ulong FirstSlice =  0xffff;
    public const ulong SecondSlice = 0xffff0000;
    public const ulong ThirdSlice =  0xffff00000000;
    public const ulong FourthSlice = 0xffff000000000000;
    
    public static readonly int[,] PriorityWeights =
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
    
    public static class MagicLookupConsts
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
        public static readonly (ulong magicNumber, int push, int highest)[,] BlockCaptureNumbers = new (ulong magicNumber, int push, int highest)[8,8];
        public static (ulong magicNumber, int push, int highest) BlockMoveNumber;
        
        public static (ulong magicNumber, int push, int highest) RightPawnEvalNumber;
        public static (ulong magicNumber, int push, int highest) LeftPawnEvalNumber;
        public static (ulong magicNumber, int push, int highest) CenterPawnEvalNumber;
        
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
        public static readonly int[,][] RookMobilityLookupArray = new int[8,8][];
        public static readonly int[,][] BishopMobilityLookupArray = new int[8,8][];
        public static readonly int[,] KnightMobilityLookup = new int[8,8];
        public static Move[] EnPassantLookupArray = [];
        public static readonly int[,][] KingSafetyLookup = new int[8,8][];
        public static readonly Move[,][] BlockCaptureMoveLookup = new Move[8,8][];
        public static readonly Move[,][][] BlockMoveLookup = new Move[8,8][][];
        public static readonly Move[,][][] BlockCaptureMovePawnLookup = new Move[8,8][][];
        public static readonly Move[,][][] BlockMovePawnLookup = new Move[8,8][][];
        
        public static readonly ulong[,][] RookPinLineBitboardLookup =  new ulong[8,8][];
        public static readonly ulong[,][] BishopPinLineBitboardLookup = new ulong[8,8][];
        public static readonly List<BitboardUtils.PinSearchResult>[,][] RookPinLookup = new List<BitboardUtils.PinSearchResult>[8,8][];
        public static readonly List<BitboardUtils.PinSearchResult>[,][] BishopPinLookup = new List<BitboardUtils.PinSearchResult>[8,8][];

        public static Evaluation.PawnEvaluation[] RightPawnEvalLookup = [];
        public static Evaluation.PawnEvaluation[] LeftPawnEvalLookup = [];
        public static Evaluation.PawnEvaluation[] CenterPawnEvalLookup = [];
        
        public static Evaluation.RookEvaluation[] FirstRookEvaluationLookup = [];
        public static Evaluation.RookEvaluation[] SecondRookEvaluationLookup = [];
        public static Evaluation.RookEvaluation[] ThirdRookEvaluationLookup = [];
        public static Evaluation.RookEvaluation[] FourthRookEvaluationLookup = [];
        
        public static Evaluation.QueenEvaluation[] FirstQueenEvaluationLookup = [];
        public static Evaluation.QueenEvaluation[] SecondQueenEvaluationLookup = [];
        public static Evaluation.QueenEvaluation[] ThirdQueenEvaluationLookup = [];
        public static Evaluation.QueenEvaluation[] FourthQueenEvaluationLookup = [];
        
        public static Evaluation.KnightEvaluation[] FirstKnightEvaluationLookup = [];
        public static Evaluation.KnightEvaluation[] SecondKnightEvaluationLookup = [];
        public static Evaluation.KnightEvaluation[] ThirdKnightEvaluationLookup = [];
        public static Evaluation.KnightEvaluation[] FourthKnightEvaluationLookup = [];
        
        public static Evaluation.BishopEvaluation[] FirstBishopEvaluationLookup = [];
        public static Evaluation.BishopEvaluation[] SecondBishopEvaluationLookup = [];
        public static Evaluation.BishopEvaluation[] ThirdBishopEvaluationLookup = [];
        public static Evaluation.BishopEvaluation[] FourthBishopEvaluationLookup = [];
        public static readonly Evaluation.KingEvaluation[,] KingEvaluationLookup = new Evaluation.KingEvaluation[8,8];
    }

    public const ulong File = 0x8080808080808080;
    public const ulong Rank = 0xFF00000000000000;

    private const ulong UpDiagonal = 0x102040810204080;
    private const ulong DownDiagonal = 0x8040201008040201;

    private const ulong SmallFile = 0x80808080808000;
    private const ulong SmallRank = 0x7E00000000000000;

    public const ulong KingSafetyAppliesWhite = 0xC7C7000000000000; 
    public const ulong KingSafetyAppliesBlack = 0xC7C7;

    private static bool init;

    public static void Init()
    {
        if (init) return;
        init = true;
        List<ulong> enPassantBitboards = new List<ulong>();
        List<ulong> blockMoveList = new();
        Timer t = new Timer();
        t.Start();
        
        Console.WriteLine("Initializing magic bitboards. This should take approximately 20 seconds");
        
        // Create the masks for every square on the board
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // The last bit also has to be evaluated in every direction, since it matters whether it's blocked or not
                RookMasks[file, rank] = (Rank >> (rank * 8)) ^ (File >> (7 - file));
                RookBlockers[file, rank] = BitboardUtils.Combinations(RookMasks[file, rank]);
                RookMoves[file, rank] = new (Move[] moves, ulong captures)[RookBlockers[file, rank].Length];
                
                List<ulong> rCombinations = new List<ulong>();
                for (int i = 0; i < RookBlockers[file, rank].Length; i++) // for every blocker combination
                {
                    RookMoves[file, rank][i] = BitboardUtils.GetMoves(RookBlockers[file, rank][i], (file, rank), Pieces.WhiteRook);
                    rCombinations.AddRange(BitboardUtils.Combinations(RookMoves[file, rank][i].captures));
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
                BishopBlockers[file, rank] = BitboardUtils.Combinations(BishopMasks[file, rank]);
                BishopMoves[file, rank] = new (Move[] moves, ulong captures)[BishopBlockers[file, rank].Length];
                
                List<ulong> bCombinations = new List<ulong>();
                for (int i = 0; i < BishopBlockers[file, rank].Length; i++)
                {
                    BishopMoves[file, rank][i] = BitboardUtils.GetMoves(BishopBlockers[file, rank][i], (file, rank), Pieces.WhiteBishop);
                    bCombinations.AddRange(BitboardUtils.Combinations(BishopMoves[file, rank][i].captures));
                }
                BishopCaptureCombinations[file, rank] = bCombinations.Distinct().ToArray();
                
                SmallRookMasks[file, rank] = ((SmallRank >> (rank * 8)) ^ (SmallFile >> (7 - file))) & ~BitboardUtils.GetSquare(file, rank);
                SmallRookCombinations[file, rank] = BitboardUtils.Combinations(SmallRookMasks[file, rank]);
                SmallRookBitboards[file, rank] = new ulong[SmallRookCombinations[file, rank].Length];
                for (int i = 0; i < SmallRookCombinations[file, rank].Length; i++)
                    SmallRookBitboards[file, rank][i] = BitboardUtils.GetMoveBitboards(SmallRookCombinations[file, rank][i], (file, rank), Pieces.WhiteRook);
                
                SmallBishopMasks[file, rank] = (relativeUD ^ relativeDD) & ~Frame;
                SmallBishopCombinations[file, rank] = BitboardUtils.Combinations(SmallBishopMasks[file, rank]);
                SmallBishopBitboards[file, rank] = new ulong[SmallBishopCombinations[file, rank].Length];
                for (int i = 0; i < SmallBishopCombinations[file, rank].Length; i++)
                    SmallBishopBitboards[file, rank][i] = BitboardUtils.GetMoveBitboards(SmallBishopCombinations[file, rank][i], (file, rank), Pieces.WhiteBishop);
                
                // knight masks
                KnightMasks[file, rank] = BitboardUtils.GetMask((file, rank), BitboardUtils.KnightPattern);
                MagicLookupConsts.KnightMobilityLookup[file, rank] = (int)(ulong.PopCount(KnightMasks[file, rank]) * Weights.MobilityMultiplier) * 3;
                KnightCombinations[file, rank] = BitboardUtils.Combinations(KnightMasks[file, rank]);
                
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
                
                kingMask &= ~BitboardUtils.GetSquare(file, rank);
                
                KingMasks[file, rank] = kingMask;
                KingCombinations[file, rank] = BitboardUtils.Combinations(kingMask);
                
                // blocking checks
                
                // captures
                BlockCaptures[file, rank] = BitboardUtils.GetSingleBits(RookMasks[file, rank] | BishopMasks[file, rank] | KnightMasks[file, rank]);
                
                // regular moves
                blockMoveList.AddRange(BitboardUtils.Combinations(relativeUD, 3));
                blockMoveList.AddRange(BitboardUtils.Combinations(relativeDD, 3));
                blockMoveList.AddRange(BitboardUtils.Combinations(Rank >> (rank * 8), 3));
                blockMoveList.AddRange(BitboardUtils.Combinations(File >> (7 - file), 3));
                blockMoveList.AddRange(BitboardUtils.Combinations(KnightMasks[file, rank], 3));
                
                // pawn moves
                
                // white pawns
                ulong wpmMask = 0;
                ulong wpcMask = 0;
                wpmMask |= BitboardUtils.GetSquare(file, rank + 1);
                if (BitboardUtils.ValidSquare(file + 1, rank + 1)) wpcMask |= BitboardUtils.GetSquare(file + 1, rank + 1);
                if (BitboardUtils.ValidSquare(file - 1, rank + 1)) wpcMask |= BitboardUtils.GetSquare(file - 1, rank + 1);
                
                if (rank == 1) wpmMask |= BitboardUtils.GetSquare(file, rank + 2);
                
                WhitePawnMoveMasks[file, rank] = wpmMask;
                WhitePawnMoveCombinations[file, rank] = BitboardUtils.Combinations(wpmMask);
                WhitePawnCaptureMasks[file, rank] = wpcMask;
                WhitePawnCaptureCombinations[file, rank] = BitboardUtils.Combinations(wpcMask);
                if (rank == 4) // white en passant rank
                {
                    if (BitboardUtils.ValidSquare(file + 1, 5)) enPassantBitboards.Add(BitboardUtils.GetSquare(file, rank) | BitboardUtils.GetSquare(file + 1, 5));
                    if (BitboardUtils.ValidSquare(file - 1, 5)) enPassantBitboards.Add(BitboardUtils.GetSquare(file, rank) | BitboardUtils.GetSquare(file - 1, 5));
                }
                
                // black pawns
                ulong bpmMask = 0;
                ulong bpcMask = 0;
                bpmMask |= BitboardUtils.GetSquare(file, rank - 1);
                if (BitboardUtils.ValidSquare(file + 1, rank - 1)) bpcMask |= BitboardUtils.GetSquare(file + 1, rank - 1);
                if (BitboardUtils.ValidSquare(file - 1, rank - 1)) bpcMask |= BitboardUtils.GetSquare(file - 1, rank - 1);
                
                if (rank == 6) bpmMask |= BitboardUtils.GetSquare(file, rank - 2);
                
                BlackPawnMoveMasks[file, rank] = bpmMask;
                BlackPawnMoveCombinations[file, rank] = BitboardUtils.Combinations(bpmMask);
                BlackPawnCaptureMasks[file, rank] = bpcMask;
                BlackPawnCaptureCombinations[file, rank] = BitboardUtils.Combinations(bpcMask);
                
                if (rank == 3) // black en passant rank
                {
                    if (BitboardUtils.ValidSquare(file + 1, 2)) enPassantBitboards.Add(BitboardUtils.GetSquare(file, rank) | BitboardUtils.GetSquare(file + 1, 2));
                    if (BitboardUtils.ValidSquare(file - 1, 2)) enPassantBitboards.Add(BitboardUtils.GetSquare(file, rank) | BitboardUtils.GetSquare(file - 1, 2));
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

        BlockMoves = blockMoveList.Distinct().ToArray();
        MagicLookupConsts.BlockMoveNumber = (4154364917966041783, 46, 262133); //MagicNumbers.GenerateRepeat(BlockMoves, 1, 46);
        EnPassantMasks = enPassantBitboards.ToArray();
        MagicLookupConsts.EnPassantNumbers = (15417481889308385644, 58, 63); // MagicNumbers.GenerateRepeat(EnPassantMasks, 10000);
        MagicLookupConsts.EnPassantLookupArray = new Move[MagicLookupConsts.EnPassantNumbers.highest + 1];
        foreach (ulong mask in EnPassantMasks) // for each possible en passant
        {
            MagicLookupConsts.EnPassantLookupArray[(mask * MagicLookupConsts.EnPassantNumbers.magicNumber) >> MagicLookupConsts.EnPassantNumbers.push] = BitboardUtils.GetEnPassantMoves(mask);
        }
        
        // pawn eval combinations
        List<ulong> rightPawns = BitboardUtils.Combinations(RightPawns, 8);
        List<ulong> leftPawns = BitboardUtils.Combinations(LeftPawns, 8);
        List<ulong> centerPawns = BitboardUtils.Combinations(CenterPawns, 8);
        
        MagicLookupConsts.RightPawnEvalNumber = (17067507152026048335, 37, 134217725); // MagicNumbers.GenerateMagicNumberParallel(rightPawns.Distinct().ToArray(),37 ,7, false);
        MagicLookupConsts.LeftPawnEvalNumber = (615594976254142229, 37, 134217609); // MagicNumbers.GenerateMagicNumberParallel(leftPawns.Distinct().ToArray(), 37, 7, false);
        MagicLookupConsts.CenterPawnEvalNumber = (15570990422680516493, 37, 134217566); // MagicNumbers.GenerateMagicNumberParallel(centerPawns.Distinct().ToArray(), 37, 7, false);
        
        MagicLookupConsts.RightPawnEvalLookup = new Evaluation.PawnEvaluation[MagicLookupConsts.RightPawnEvalNumber.highest+ 1];
        MagicLookupConsts.LeftPawnEvalLookup = new Evaluation.PawnEvaluation[MagicLookupConsts.LeftPawnEvalNumber.highest + 1];
        MagicLookupConsts.CenterPawnEvalLookup = new Evaluation.PawnEvaluation[MagicLookupConsts.CenterPawnEvalNumber.highest + 1];

        Parallel.For(0, 3, e =>
        {
            switch (e)
            {
                case 0:
                    foreach (ulong combination in rightPawns)
                        MagicLookupConsts.RightPawnEvalLookup[(combination * MagicLookupConsts.RightPawnEvalNumber.magicNumber) >> MagicLookupConsts.RightPawnEvalNumber.push] = 
                            Evaluation.GeneratePawnEval(combination, Evaluation.Section.Right);
                    break;
                case 1:
                    foreach (ulong combination in leftPawns)
                        MagicLookupConsts.LeftPawnEvalLookup[(combination * MagicLookupConsts.LeftPawnEvalNumber.magicNumber) >> MagicLookupConsts.LeftPawnEvalNumber.push] = 
                            Evaluation.GeneratePawnEval(combination, Evaluation.Section.Left);
                    break;
                case 2:
                    foreach (ulong combination in centerPawns)
                        MagicLookupConsts.CenterPawnEvalLookup[(combination * MagicLookupConsts.CenterPawnEvalNumber.magicNumber) >> MagicLookupConsts.CenterPawnEvalNumber.push] = 
                            Evaluation.GeneratePawnEval(combination, Evaluation.Section.Center);
                    break;
            }
        });
        
        List<ulong> firstSlice = BitboardUtils.Combinations(FirstSlice, 9);
        List<ulong> secondSlice = BitboardUtils.Combinations(SecondSlice, 9);
        List<ulong> thirdSlice = BitboardUtils.Combinations(ThirdSlice, 9);
        List<ulong> fourthSlice = BitboardUtils.Combinations(FourthSlice, 9);
        
        MagicLookupConsts.FirstRookEvaluationLookup = new Evaluation.RookEvaluation[firstSlice.Max() + 1];
        MagicLookupConsts.SecondRookEvaluationLookup = new Evaluation.RookEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookupConsts.ThirdRookEvaluationLookup = new Evaluation.RookEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookupConsts.FourthRookEvaluationLookup = new Evaluation.RookEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        MagicLookupConsts.FirstQueenEvaluationLookup = new Evaluation.QueenEvaluation[firstSlice.Max() + 1];
        MagicLookupConsts.SecondQueenEvaluationLookup = new Evaluation.QueenEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookupConsts.ThirdQueenEvaluationLookup = new Evaluation.QueenEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookupConsts.FourthQueenEvaluationLookup = new Evaluation.QueenEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        MagicLookupConsts.FirstKnightEvaluationLookup = new Evaluation.KnightEvaluation[firstSlice.Max() + 1];
        MagicLookupConsts.SecondKnightEvaluationLookup = new Evaluation.KnightEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookupConsts.ThirdKnightEvaluationLookup = new Evaluation.KnightEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookupConsts.FourthKnightEvaluationLookup = new Evaluation.KnightEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        MagicLookupConsts.FirstBishopEvaluationLookup = new Evaluation.BishopEvaluation[firstSlice.Max() + 1];
        MagicLookupConsts.SecondBishopEvaluationLookup = new Evaluation.BishopEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookupConsts.ThirdBishopEvaluationLookup = new Evaluation.BishopEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookupConsts.FourthBishopEvaluationLookup = new Evaluation.BishopEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        Parallel.For(0, 4, e =>
        {
            switch (e)
            {
                case 0:
                    foreach (ulong combination in firstSlice)
                    {
                        MagicLookupConsts.FirstRookEvaluationLookup[combination] = Evaluation.GenerateRookEval(combination, Evaluation.Slice.First);
                        MagicLookupConsts.FirstQueenEvaluationLookup[combination] = Evaluation.GenerateStandardEval<Evaluation.QueenEvaluation>(combination, Evaluation.Slice.First, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookupConsts.FirstKnightEvaluationLookup[combination] = Evaluation.GenerateStandardEval<Evaluation.KnightEvaluation>(combination, Evaluation.Slice.First, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookupConsts.FirstBishopEvaluationLookup[combination] = Evaluation.GenerateStandardEval<Evaluation.BishopEvaluation>(combination, Evaluation.Slice.First, Pieces.WhiteBishop, Pieces.BlackBishop);
                    }

                    break;
                case 1:
                    foreach (ulong combination in secondSlice)
                    {
                        MagicLookupConsts.SecondRookEvaluationLookup[combination >> 16] = Evaluation.GenerateRookEval(combination, Evaluation.Slice.Second);
                        MagicLookupConsts.SecondQueenEvaluationLookup[combination >> 16] = Evaluation.GenerateStandardEval<Evaluation.QueenEvaluation>(combination, Evaluation.Slice.Second, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookupConsts.SecondKnightEvaluationLookup[combination >> 16] = Evaluation.GenerateStandardEval<Evaluation.KnightEvaluation>(combination, Evaluation.Slice.Second, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookupConsts.SecondBishopEvaluationLookup[combination >> 16] = Evaluation.GenerateStandardEval<Evaluation.BishopEvaluation>(combination, Evaluation.Slice.Second, Pieces.WhiteBishop, Pieces.BlackBishop);
                    }

                    break;
                case 2:
                    foreach (ulong combination in thirdSlice)
                    {
                        MagicLookupConsts.ThirdRookEvaluationLookup[combination >> 32] = Evaluation.GenerateRookEval(combination, Evaluation.Slice.Third);
                        MagicLookupConsts.ThirdQueenEvaluationLookup[combination >> 32] = Evaluation.GenerateStandardEval<Evaluation.QueenEvaluation>(combination, Evaluation.Slice.Third, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookupConsts.ThirdKnightEvaluationLookup[combination >> 32] = Evaluation.GenerateStandardEval<Evaluation.KnightEvaluation>(combination, Evaluation.Slice.Third, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookupConsts.ThirdBishopEvaluationLookup[combination >> 32] = Evaluation.GenerateStandardEval<Evaluation.BishopEvaluation>(combination, Evaluation.Slice.Third, Pieces.WhiteBishop, Pieces.BlackBishop);
                    }
                    break;
                case 3:
                    foreach (ulong combination in fourthSlice)
                    {
                        MagicLookupConsts.FourthRookEvaluationLookup[combination >> 48] = Evaluation.GenerateRookEval(combination, Evaluation.Slice.Fourth);
                        MagicLookupConsts.FourthQueenEvaluationLookup[combination >> 48] = Evaluation.GenerateStandardEval<Evaluation.QueenEvaluation>(combination, Evaluation.Slice.Fourth, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookupConsts.FourthKnightEvaluationLookup[combination >> 48] = Evaluation.GenerateStandardEval<Evaluation.KnightEvaluation>(combination, Evaluation.Slice.Fourth, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookupConsts.FourthBishopEvaluationLookup[combination >> 48] = Evaluation.GenerateStandardEval<Evaluation.BishopEvaluation>(combination, Evaluation.Slice.Fourth, Pieces.WhiteBishop, Pieces.BlackBishop);
                    }
                    break;
            }
        });
        
        //Console.WriteLine("Generating Magic Numbers");
        
        //int done = 0;
        // create magic numbers and add to lookup
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // rook numbers
                MagicLookupConsts.RookMove[file, rank] = MagicNumbers.RookNumbers[file, rank];
                MagicLookupConsts.RookLookup[file, rank] = new (Move[] moves, ulong captures)[MagicLookupConsts.RookMove[file, rank].highest + 1];
                MagicLookupConsts.RookLookupCapturesArray[file, rank] = new ulong[MagicLookupConsts.RookMove[file, rank].highest + 1];
                
                for (int i = 0; i < RookBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.RookLookup[file, rank][(RookBlockers[file, rank][i] * MagicLookupConsts.RookMove[file, rank].magicNumber) >> MagicLookupConsts.RookMove[file, rank].push] = RookMoves[file, rank][i];
                    MagicLookupConsts.RookLookupCapturesArray[file, rank][(RookBlockers[file, rank][i] * MagicLookupConsts.RookMove[file, rank].magicNumber) >> MagicLookupConsts.RookMove[file, rank].push] = RookMoves[file, rank][i].captures;
                }
                
                // bishop numbers
                MagicLookupConsts.BishopMove[file, rank] = MagicNumbers.BishopNumbers[file, rank];
                MagicLookupConsts.BishopLookup[file, rank] = new (Move[] moves, ulong captures)[MagicLookupConsts.BishopMove[file, rank].highest + 1];
                MagicLookupConsts.BishopLookupCapturesArray[file, rank] = new ulong[MagicLookupConsts.BishopMove[file, rank].highest + 1];
                
                for (int i = 0; i < BishopBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.BishopLookup[file, rank][(BishopBlockers[file, rank][i] * MagicLookupConsts.BishopMove[file, rank].magicNumber) >> MagicLookupConsts.BishopMove[file, rank].push] = BishopMoves[file, rank][i];
                    MagicLookupConsts.BishopLookupCapturesArray[file, rank][(BishopBlockers[file, rank][i] * MagicLookupConsts.BishopMove[file, rank].magicNumber) >> MagicLookupConsts.BishopMove[file, rank].push] = BishopMoves[file, rank][i].captures;
                }
                
                // rook captures
                MagicLookupConsts.RookCapture[file, rank] = MagicNumbers.RookCaptureNumbers[file, rank]; // MagicNumbers.GenerateRepeat(RookCaptureCombinations[file, rank], 1000);
                MagicLookupConsts.RookCaptureLookup[file, rank] = new Move[MagicLookupConsts.RookCapture[file, rank].highest + 1][];
                
                for (int i = 0; i < RookCaptureCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.RookCaptureLookup[file, rank][(RookCaptureCombinations[file, rank][i] * MagicLookupConsts.RookCapture[file, rank].magicNumber) >> MagicLookupConsts.RookCapture[file, rank].push] = BitboardUtils.GetBitboardMoves(RookCaptureCombinations[file, rank][i], (file, rank), 50, capture: true);
                }
                
                // bishop captures
                MagicLookupConsts.BishopCapture[file, rank] = MagicNumbers.BishopCaptureNumbers[file, rank]; // MagicNumbers.GenerateRepeat(BishopCaptureCombinations[file, rank], 1000);
                MagicLookupConsts.BishopCaptureLookup[file, rank] = new Move[MagicLookupConsts.BishopCapture[file, rank].highest + 1][];
                
                for (int i = 0; i < BishopCaptureCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.BishopCaptureLookup[file, rank][(BishopCaptureCombinations[file, rank][i] * MagicLookupConsts.BishopCapture[file, rank].magicNumber) >> MagicLookupConsts.BishopCapture[file, rank].push] = BitboardUtils.GetBitboardMoves(BishopCaptureCombinations[file, rank][i], (file, rank), 50, capture: true);
                }
                
                MagicLookupConsts.RookBitboardNumbers[file, rank] = MagicNumbers.RookBitboardNumbers[file, rank];
                MagicLookupConsts.RookBitboardLookup[file, rank] = new ulong[MagicLookupConsts.RookBitboardNumbers[file, rank].highest + 1];
                MagicLookupConsts.RookMobilityLookupArray[file, rank] = new int[MagicLookupConsts.RookBitboardNumbers[file, rank].highest + 1];

                for (int i = 0; i < SmallRookCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.RookBitboardLookup[file, rank][(SmallRookCombinations[file, rank][i] * MagicLookupConsts.RookBitboardNumbers[file, rank].magicNumber) >> MagicLookupConsts.RookBitboardNumbers[file, rank].push] = SmallRookBitboards[file, rank][i];
                    MagicLookupConsts.RookMobilityLookupArray[file, rank][(SmallRookCombinations[file, rank][i] * MagicLookupConsts.RookBitboardNumbers[file, rank].magicNumber) >> MagicLookupConsts.RookBitboardNumbers[file, rank].push] = (int)(ulong.PopCount(SmallRookBitboards[file, rank][i]) * Weights.MobilityMultiplier);
                }
                
                MagicLookupConsts.BishopBitboardNumbers[file, rank] = MagicNumbers.BishopBitboardNumbers[file, rank];
                MagicLookupConsts.BishopBitboardLookup[file, rank] = new ulong[MagicLookupConsts.BishopBitboardNumbers[file, rank].highest + 1];
                MagicLookupConsts.BishopMobilityLookupArray[file, rank] = new int[MagicLookupConsts.BishopBitboardNumbers[file, rank].highest + 1];
                
                for (int i = 0; i < SmallBishopCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.BishopBitboardLookup[file, rank][(SmallBishopCombinations[file, rank][i] * MagicLookupConsts.BishopBitboardNumbers[file, rank].magicNumber) >> MagicLookupConsts.BishopBitboardNumbers[file, rank].push] = SmallBishopBitboards[file, rank][i];
                    MagicLookupConsts.BishopMobilityLookupArray[file, rank][(SmallBishopCombinations[file, rank][i] * MagicLookupConsts.BishopBitboardNumbers[file, rank].magicNumber) >> MagicLookupConsts.BishopBitboardNumbers[file, rank].push] = (int)(ulong.PopCount(SmallBishopBitboards[file, rank][i]) * Weights.MobilityMultiplier);
                }
                
                // knight moves
                // since the potential captures and moves are based on the same combinations, the same magic numbers can be used
                MagicLookupConsts.KnightMove[file, rank] = MagicNumbers.KnightNumbers[file, rank];
                MagicLookupConsts.KnightLookup[file, rank] = new Move[MagicLookupConsts.KnightMove[file, rank].highest + 1][];
                MagicLookupConsts.KnightCaptureLookup[file, rank] = new Move[MagicLookupConsts.KnightMove[file, rank].highest + 1][];
                
                for (int i = 0; i < KnightCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookupConsts.KnightLookup[file, rank][(KnightCombinations[file, rank][i] * MagicLookupConsts.KnightMove[file, rank].magicNumber) >> MagicLookupConsts.KnightMove[file, rank].push] = BitboardUtils.GetBitboardMoves(KnightCombinations[file, rank][i], (file, rank), 5);
                    MagicLookupConsts.KnightCaptureLookup[file, rank][(KnightCombinations[file, rank][i] * MagicLookupConsts.KnightMove[file, rank].magicNumber) >> MagicLookupConsts.KnightMove[file, rank].push] = BitboardUtils.GetBitboardMoves(KnightCombinations[file, rank][i], (file, rank), 50, capture: true);
                }
                
                // king moves
                MagicLookupConsts.KingMove[file, rank] = MagicNumbers.KingNumbers[file, rank]; // MagicNumbers.GenerateRepeat(KingCombinations[file, rank], 5000);
                MagicLookupConsts.KingLookup[file, rank] = new Move[MagicLookupConsts.KingMove[file, rank].highest + 1][];
                MagicLookupConsts.KingCaptureLookup[file, rank] = new Move[MagicLookupConsts.KingMove[file, rank].highest + 1][];
                MagicLookupConsts.KingSafetyLookup[file, rank] = new int[MagicLookupConsts.KingMove[file, rank].highest + 1];
                
                for (int i = 0; i < KingCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookupConsts.KingLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookupConsts.KingMove[file, rank].magicNumber) >> MagicLookupConsts.KingMove[file, rank].push] = BitboardUtils.GetBitboardMoves(KingCombinations[file, rank][i], (file, rank), 5);
                    MagicLookupConsts.KingCaptureLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookupConsts.KingMove[file, rank].magicNumber) >> MagicLookupConsts.KingMove[file, rank].push] = BitboardUtils.GetBitboardMoves(KingCombinations[file, rank][i], (file, rank), 3,  capture: true);
                    MagicLookupConsts.KingSafetyLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookupConsts.KingMove[file, rank].magicNumber) >> MagicLookupConsts.KingMove[file, rank].push] = Weights.KingSafetyBonuses[UInt64.PopCount(KingCombinations[file, rank][i])];
                }
                
                // pin lines
                // rook pin lines
                MagicLookupConsts.RookPinLineBitboardLookup[file, rank] = new ulong[MagicLookupConsts.RookMove[file, rank].highest + 1];
                
                for (int i = 0; i < RookBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.RookPinLineBitboardLookup[file, rank][(RookBlockers[file, rank][i] * MagicLookupConsts.RookMove[file, rank].magicNumber) >> MagicLookupConsts.RookMove[file, rank].push] = BitboardUtils.GetPinLine(RookBlockers[file, rank][i], (file, rank), Pieces.WhiteRook);
                }
                
                // bishop pin lines
                MagicLookupConsts.BishopPinLineBitboardLookup[file, rank] = new ulong[MagicLookupConsts.BishopMove[file, rank].highest + 1];
                
                for (int i = 0; i < BishopBlockers[file, rank].Length; i++) // for each blocker
                {
                    MagicLookupConsts.BishopPinLineBitboardLookup[file, rank][(BishopBlockers[file, rank][i] * MagicLookupConsts.BishopMove[file, rank].magicNumber) >> MagicLookupConsts.BishopMove[file, rank].push] = BitboardUtils.GetPinLine(BishopBlockers[file, rank][i], (file, rank), Pieces.WhiteBishop);
                }
                
                // pin search
                MagicLookupConsts.RookPinLookup[file,rank] = new List<BitboardUtils.PinSearchResult>[MagicLookupConsts.RookMove[file, rank].highest + 1];

                for (int i = 0; i < RookBlockers[file, rank].Length; i++)
                {
                    MagicLookupConsts.RookPinLookup[file, rank][(RookBlockers[file, rank][i] * MagicLookupConsts.RookMove[file, rank].magicNumber) >> MagicLookupConsts.RookMove[file, rank].push] = BitboardUtils.GeneratePinResult((file, rank), RookBlockers[file, rank][i], Pieces.WhiteRook);
                }
                
                MagicLookupConsts.BishopPinLookup[file, rank] = new List<BitboardUtils.PinSearchResult>[MagicLookupConsts.BishopMove[file, rank].highest + 1];

                for (int i = 0; i < BishopBlockers[file, rank].Length; i++)
                {
                    MagicLookupConsts.BishopPinLookup[file, rank][(BishopBlockers[file, rank][i] * MagicLookupConsts.BishopMove[file, rank].magicNumber) >> MagicLookupConsts.BishopMove[file, rank].push] = BitboardUtils.GeneratePinResult((file, rank), BishopBlockers[file, rank][i], Pieces.WhiteBishop);
                }
                
                // blocking checks
                // block captures
                MagicLookupConsts.BlockCaptureNumbers[file, rank] = MagicNumbers.BlockCaptureNumbers[file, rank]; //MagicNumbers.GenerateRepeat(BlockCaptures[file, rank], 10000);
                MagicLookupConsts.BlockCaptureMoveLookup[file, rank] = new Move[MagicLookupConsts.BlockCaptureNumbers[file, rank].highest + 1];
                MagicLookupConsts.BlockCaptureMovePawnLookup[file, rank] = new Move[MagicLookupConsts.BlockCaptureNumbers[file, rank].highest + 1][];
                
                for (int i = 0; i < BlockCaptures[file, rank].Length; i++)
                {
                    MagicLookupConsts.BlockCaptureMoveLookup[file, rank][(BlockCaptures[file, rank][i] * MagicLookupConsts.BlockCaptureNumbers[file, rank].magicNumber) >> MagicLookupConsts.BlockCaptureNumbers[file, rank].push] = BitboardUtils.GetBitboardMoves(BlockCaptures[file, rank][i], (file, rank), 25)[0];
                    if (rank != 0 && rank != 7) 
                        MagicLookupConsts.BlockCaptureMovePawnLookup[file, rank][(BlockCaptures[file, rank][i] * MagicLookupConsts.BlockCaptureNumbers[file, rank].magicNumber) >> MagicLookupConsts.BlockCaptureNumbers[file, rank].push] = BitboardUtils.GetBitboardMoves(BlockCaptures[file, rank][i], (file, rank), 25, pawn: true,  capture: true);
                }
                
                // block moves
                MagicLookupConsts.BlockMoveLookup[file, rank] = new Move[MagicLookupConsts.BlockMoveNumber.highest + 1][];
                MagicLookupConsts.BlockMovePawnLookup[file, rank] = new Move[MagicLookupConsts.BlockMoveNumber.highest + 1][];
                
                foreach (ulong move in BlockMoves)
                {
                    MagicLookupConsts.BlockMoveLookup[file, rank][(move * MagicLookupConsts.BlockMoveNumber.magicNumber) >> MagicLookupConsts.BlockMoveNumber.push] = BitboardUtils.GetBitboardMoves(move, (file, rank), 5);
                    if (rank != 0 && rank != 7) 
                        MagicLookupConsts.BlockMovePawnLookup[file, rank][(move * MagicLookupConsts.BlockMoveNumber.magicNumber) >> MagicLookupConsts.BlockMoveNumber.push] = BitboardUtils.GetBitboardMoves(move, (file, rank), 5, pawn: true);
                }

                MagicLookupConsts.KingEvaluationLookup[file, rank] = new Evaluation.KingEvaluation
                {
                    wEval = (int)(Pieces.Value[Pieces.WhiteKing] * Weights.MaterialMultiplier) + Weights.Pieces[Pieces.WhiteKing, file, rank],
                    bEval = (int)(Pieces.Value[Pieces.BlackKing] * Weights.MaterialMultiplier) - Weights.Pieces[Pieces.WhiteKing, file, 7-rank],
                    wEvalEndgame = (int)(Pieces.Value[Pieces.WhiteKing] * Weights.MaterialMultiplier) + Weights.EndgamePieces[Pieces.WhiteKing, file, rank],
                    bEvalEndgame = (int)(Pieces.Value[Pieces.BlackKing] * Weights.MaterialMultiplier) - Weights.EndgamePieces[Pieces.WhiteKing, file, 7-rank],
                };
                
                //Console.WriteLine($"Square done {++done}/64");
                // pawn moves
                if (rank == 0 || rank == 7)
                    continue;
                
                // white pawns
                // moves
                MagicLookupConsts.WhitePawnMove[file, rank] = MagicNumbers.WhitePawnMoveNumbers[file, rank];
                MagicLookupConsts.WhitePawnLookup[file, rank] = new Move[MagicLookupConsts.WhitePawnMove[file, rank].highest + 1][];

                for (int i = 0; i < WhitePawnMoveCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookupConsts.WhitePawnLookup[file, rank][(WhitePawnMoveCombinations[file, rank][i] * MagicLookupConsts.WhitePawnMove[file, rank].magicNumber) >> MagicLookupConsts.WhitePawnMove[file, rank].push] = BitboardUtils.GetPawnMoves(WhitePawnMoveCombinations[file, rank][i], (file, rank), 0);
                }
                // captures
                MagicLookupConsts.WhitePawnCapture[file, rank] = MagicNumbers.WhiteCaptureMoveNumbers[file, rank];
                MagicLookupConsts.WhitePawnCaptureLookup[file, rank] = new Move[MagicLookupConsts.WhitePawnCapture[file, rank].highest + 1][];

                for (int i = 0; i < WhitePawnCaptureCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookupConsts.WhitePawnCaptureLookup[file, rank][(WhitePawnCaptureCombinations[file, rank][i] * MagicLookupConsts.WhitePawnCapture[file, rank].magicNumber) >> MagicLookupConsts.WhitePawnCapture[file, rank].push] = BitboardUtils.GetPawnCaptures(WhitePawnCaptureCombinations[file, rank][i], (file, rank), 0);
                }
                
                // black pawns
                // moves
                MagicLookupConsts.BlackPawnMove[file, rank] = MagicNumbers.BlackPawnMoveNumbers[file, rank];
                MagicLookupConsts.BlackPawnLookup[file, rank] = new Move[MagicLookupConsts.BlackPawnMove[file, rank].highest + 1][];

                for (int i = 0; i < BlackPawnMoveCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookupConsts.BlackPawnLookup[file, rank][(BlackPawnMoveCombinations[file, rank][i] * MagicLookupConsts.BlackPawnMove[file, rank].magicNumber) >> MagicLookupConsts.BlackPawnMove[file, rank].push] = BitboardUtils.GetPawnMoves(BlackPawnMoveCombinations[file, rank][i], (file, rank), 1);
                }
                // captures
                MagicLookupConsts.BlackPawnCapture[file, rank] = MagicNumbers.BlackCaptureMoveNumbers[file, rank];
                MagicLookupConsts.BlackPawnCaptureLookup[file, rank] = new Move[MagicLookupConsts.BlackPawnCapture[file, rank].highest + 1][];

                for (int i = 0; i < BlackPawnCaptureCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookupConsts.BlackPawnCaptureLookup[file, rank][(BlackPawnCaptureCombinations[file, rank][i] * MagicLookupConsts.BlackPawnCapture[file, rank].magicNumber) >> MagicLookupConsts.BlackPawnCapture[file, rank].push] = BitboardUtils.GetPawnCaptures(BlackPawnCaptureCombinations[file, rank][i], (file, rank), 1);
                }
            }
        }
        
        // init pathfinder
        for (int startRank = 0; startRank < 8; startRank++)
        for (int startFile = 0; startFile < 8; startFile++)
        for (int endRank = 0; endRank < 8; endRank++)
        for (int endFile = 0; endFile < 8; endFile++)
        {
            if (startRank == endRank && startFile == endFile)
            {
                PathLookup[startFile, startRank, endFile, endRank] = 0;
                continue;
            }
            
            ulong path = 0;
            
            if (endFile == startFile) // both are from the same file
            {
                int current = startRank;
                int moveBy = startRank < endRank ? 1 : -1;
                do
                {
                    path |= BitboardUtils.GetSquare(startFile, current);
                    current += moveBy;
                } while (current != endRank);
            }
            
            else if (endRank == startRank) // both are from the same file
            {
                int current = startFile;
                int moveBy = startFile < endFile ? 1 : -1;
                do
                {
                    path |= BitboardUtils.GetSquare(current, startRank);
                    current += moveBy;
                } while (current != endFile);
            }
            
            else if (startFile - startRank == endFile - endRank) // on the same up diagonal
            {
                int currentFile = startFile;
                int currentRank = startRank;
                (int file, int rank) moveBy = startRank < endRank ? (1, 1) : (-1, -1);
                do
                {
                    path |= BitboardUtils.GetSquare(currentFile, currentRank);
                    currentFile += moveBy.file;
                    currentRank += moveBy.rank;
                } while ((currentFile, currentRank) != (endFile, endRank));
            }
            
            else if ((7 - startFile) - startRank == (7 - endFile) - endRank) // on the same down diagonal
            {
                int currentFile = startFile;
                int currentRank = startRank;
                (int file, int rank) moveBy = startRank < endRank ? (-1, 1) : (1, -1);
                do
                {
                    path |= BitboardUtils.GetSquare(currentFile, currentRank);
                    currentFile += moveBy.file;
                    currentRank += moveBy.rank;
                } while ((currentFile, currentRank) != (endFile, endRank));
            }

            // in an L shape
            if (path != 0 || (Math.Abs(startFile - endFile) == 1 && Math.Abs(startRank - endRank) == 2) || (Math.Abs(startFile - endFile) == 2 && Math.Abs(startRank - endRank) == 1)) 
                path |= BitboardUtils.GetSquare(startFile, startRank) | BitboardUtils.GetSquare(endFile, endRank);
            
            PathLookup[startFile, startRank, endFile, endRank] = path;
        }

        Console.WriteLine($"Bitboards initialized in {t.Stop()}ms");
    }
}