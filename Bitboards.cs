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
    
    public static readonly ulong[,] WhitePawnMoveMasks = new ulong[8,8];
    private static readonly ulong[,][] WhitePawnMoveCombinations = new ulong[8,8][];
    public static readonly ulong[,] BlackPawnMoveMasks = new ulong[8,8];
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
    
    private static readonly ulong[,][] BlockCaptures = new ulong[8,8][];
    private static ulong[]? BlockMoves;
    
    private const ulong Frame = 0xFF818181818181FF;

    private const ulong BlackPossibleEnPassant = 0x100000000;
    private const ulong WhitePossibleEnPassant = 0x1000000;
    
    private static ulong[]? EnPassantMasks; // contains both the source and the destination
    
    public static readonly ulong WhiteShortCastleMask = 0x6000000000000000;
    public static readonly ulong WhiteLongCastleMask = 0xC00000000000000;
    public static readonly ulong BlackShortCastleMask = 0x60;
    public static readonly ulong BlackLongCastleMask = 0xC;
    public static readonly Move WhiteShortCastle = new((4,0), (6,0), type: 0b0010, priority: 6);
    public static readonly Move WhiteLongCastle = new((4,0), (2,0), type: 0b0011, priority: 3);
    public static readonly Move BlackShortCastle = new((4,7), (6,7), type: 0b1010, priority: 6);
    public static readonly Move BlackLongCastle = new((4,7), (2,7), type: 0b1011, priority: 3);
    
    private static readonly ulong[] PassedPawnMasks = new ulong[8];
    private static readonly ulong[] NeighbourMasks = new ulong[8];
    public static readonly ulong[,,,] PathLookup =  new ulong[8,8,8,8];
    
    private const ulong RightPawns = 0xf0f0f0f0f0f000;
    private const ulong LeftPawns = 0xf0f0f0f0f0f00;
    private const ulong CenterPawns = 0x3c3c3c3c3c3c00;
    private const ulong LeftPawnMask = 0x7070707070700;
    private const ulong RightPawnMask = 0xe0e0e0e0e0e000;
    private const ulong CenterPawnMask = 0x18181818181800;

    private const ulong FirstSlice =  0xffff;
    private const ulong SecondSlice = 0xffff0000;
    private const ulong ThirdSlice =  0xffff00000000;
    private const ulong FourthSlice = 0xffff000000000000;
    
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
        public static Move[] EnPassantLookupArray = [];
        public static readonly int[,][] KingSafetyLookup = new int[8,8][];
        public static readonly Move[,][] BlockCaptureMoveLookup = new Move[8,8][];
        public static readonly Move[,][][] BlockMoveLookup = new Move[8,8][][];
        public static readonly Move[,][][] BlockCaptureMovePawnLookup = new Move[8,8][][];
        public static readonly Move[,][][] BlockMovePawnLookup = new Move[8,8][][];
        
        public static readonly ulong[,][] RookPinLineBitboardLookup =  new ulong[8,8][];
        public static readonly ulong[,][] BishopPinLineBitboardLookup = new ulong[8,8][];
        public static readonly List<PinSearchResult>[,][] RookPinLookup = new List<PinSearchResult>[8,8][];
        public static readonly List<PinSearchResult>[,][] BishopPinLookup = new List<PinSearchResult>[8,8][];

        public static PawnEvaluation[] RightPawnEvalLookup = [];
        public static PawnEvaluation[] LeftPawnEvalLookup = [];
        public static PawnEvaluation[] CenterPawnEvalLookup = [];
        
        public static RookEvaluation[] FirstRookEvaluationLookup = [];
        public static RookEvaluation[] SecondRookEvaluationLookup = [];
        public static RookEvaluation[] ThirdRookEvaluationLookup = [];
        public static RookEvaluation[] FourthRookEvaluationLookup = [];
        
        public static StandardEvaluation[] FirstQueenEvaluationLookup = [];
        public static StandardEvaluation[] SecondQueenEvaluationLookup = [];
        public static StandardEvaluation[] ThirdQueenEvaluationLookup = [];
        public static StandardEvaluation[] FourthQueenEvaluationLookup = [];
        
        public static StandardEvaluation[] FirstKnightEvaluationLookup = [];
        public static StandardEvaluation[] SecondKnightEvaluationLookup = [];
        public static StandardEvaluation[] ThirdKnightEvaluationLookup = [];
        public static StandardEvaluation[] FourthKnightEvaluationLookup = [];
        
        public static StandardEvaluation[] FirstBishopEvaluationLookup = [];
        public static StandardEvaluation[] SecondBishopEvaluationLookup = [];
        public static StandardEvaluation[] ThirdBishopEvaluationLookup = [];
        public static StandardEvaluation[] FourthBishopEvaluationLookup = [];
        public static readonly StandardEvaluation[,] KingEvaluation = new StandardEvaluation[8,8];
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

    public static Move BlockCaptureLookup((int file, int rank) pos, ulong square)
    {
        return MagicLookup.BlockCaptureMoveLookup[pos.file, pos.rank]
            [(square * MagicLookup.BlockCaptureNumbers[pos.file, pos.rank].magicNumber) >> MagicLookup.BlockCaptureNumbers[pos.file, pos.rank].push];
    }
    
    public static Move[] BlockLookup((int file, int rank) pos, ulong squares)
    {
        return MagicLookup.BlockMoveLookup[pos.file, pos.rank]
            [(squares * MagicLookup.BlockMoveNumber.magicNumber) >> MagicLookup.BlockMoveNumber.push];
    }
    
    public static Move[] BlockCapturePawnLookup((int file, int rank) pos, ulong square)
    {
        return MagicLookup.BlockCaptureMovePawnLookup[pos.file, pos.rank]
            [(square * MagicLookup.BlockCaptureNumbers[pos.file, pos.rank].magicNumber) >> MagicLookup.BlockCaptureNumbers[pos.file, pos.rank].push];
    }
    
    public static Move[] BlockPawnLookup((int file, int rank) pos, ulong squares)
    {
        return MagicLookup.BlockMovePawnLookup[pos.file, pos.rank]
            [(squares * MagicLookup.BlockMoveNumber.magicNumber) >> MagicLookup.BlockMoveNumber.push];
    }

    public static PawnEvaluation PawnEvaluationLookupRight(ulong pawns)
    {
        return MagicLookup.RightPawnEvalLookup[((pawns & RightPawns) * MagicLookup.RightPawnEvalNumber.magicNumber) >> MagicLookup.RightPawnEvalNumber.push];
    }
    
    public static PawnEvaluation PawnEvaluationLookupLeft(ulong pawns)
    {
        return MagicLookup.LeftPawnEvalLookup[((pawns & LeftPawns) * MagicLookup.LeftPawnEvalNumber.magicNumber) >> MagicLookup.LeftPawnEvalNumber.push];
    }
    
    public static PawnEvaluation PawnEvaluationLookupCenter(ulong pawns)
    {
        return MagicLookup.CenterPawnEvalLookup[((pawns & CenterPawns) * MagicLookup.CenterPawnEvalNumber.magicNumber) >> MagicLookup.CenterPawnEvalNumber.push];
    }

    public static RookEvaluation FirstRookEvalLookup(ulong rooks)
    {
        return MagicLookup.FirstRookEvaluationLookup[rooks & FirstSlice];
    }

    public static RookEvaluation SecondRookEvalLookup(ulong rooks)
    {
        return MagicLookup.SecondRookEvaluationLookup[(rooks & SecondSlice) >> 16];
    }

    public static RookEvaluation ThirdRookEvalLookup(ulong rooks)
    {
        return MagicLookup.ThirdRookEvaluationLookup[(rooks & ThirdSlice) >> 32];
    }

    public static RookEvaluation FourthRookEvalLookup(ulong rooks)
    {
        return MagicLookup.FourthRookEvaluationLookup[(rooks & FourthSlice) >> 48];
    }
    
    
    public static StandardEvaluation FirstQueenEvalLookup(ulong rooks)
    {
        return MagicLookup.FirstQueenEvaluationLookup[rooks & FirstSlice];
    }

    public static StandardEvaluation SecondQueenEvalLookup(ulong rooks)
    {
        return MagicLookup.SecondQueenEvaluationLookup[(rooks & SecondSlice) >> 16];
    }

    public static StandardEvaluation ThirdQueenEvalLookup(ulong rooks)
    {
        return MagicLookup.ThirdQueenEvaluationLookup[(rooks & ThirdSlice) >> 32];
    }

    public static StandardEvaluation FourthQueenEvalLookup(ulong rooks)
    {
        return MagicLookup.FourthQueenEvaluationLookup[(rooks & FourthSlice) >> 48];
    }
    
    public static StandardEvaluation FirstKnightEvalLookup(ulong rooks)
    {
        return MagicLookup.FirstKnightEvaluationLookup[rooks & FirstSlice];
    }

    public static StandardEvaluation SecondKnightEvalLookup(ulong rooks)
    {
        return MagicLookup.SecondKnightEvaluationLookup[(rooks & SecondSlice) >> 16];
    }

    public static StandardEvaluation ThirdKnightEvalLookup(ulong rooks)
    {
        return MagicLookup.ThirdKnightEvaluationLookup[(rooks & ThirdSlice) >> 32];
    }

    public static StandardEvaluation FourthKnightEvalLookup(ulong rooks)
    {
        return MagicLookup.FourthKnightEvaluationLookup[(rooks & FourthSlice) >> 48];
    }
    
    public static StandardEvaluation FirstBishopEvalLookup(ulong rooks)
    {
        return MagicLookup.FirstBishopEvaluationLookup[rooks & FirstSlice];
    }

    public static StandardEvaluation SecondBishopEvalLookup(ulong rooks)
    {
        return MagicLookup.SecondBishopEvaluationLookup[(rooks & SecondSlice) >> 16];
    }

    public static StandardEvaluation ThirdBishopEvalLookup(ulong rooks)
    {
        return MagicLookup.ThirdBishopEvaluationLookup[(rooks & ThirdSlice) >> 32];
    }

    public static StandardEvaluation FourthBishopEvalLookup(ulong rooks)
    {
        return MagicLookup.FourthBishopEvaluationLookup[(rooks & FourthSlice) >> 48];
    }

    public static StandardEvaluation KingEvalLookup((int file, int rank) pos)
    {
        return MagicLookup.KingEvaluation[pos.file, pos.rank];
    }

    public static void Init()
    {
        if (init) return;
        init = true;
        List<ulong> enPassantBitboards = new List<ulong>();
        List<ulong> blockMoveList = new();
        Timer t = new Timer();
        t.Start();
        
        Console.WriteLine("Initializing magic bitboards. This should take approximately 10-20 seconds");
        
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
                
                // blocking checks
                
                // captures
                BlockCaptures[file, rank] = GetSingleBits(RookMasks[file, rank] | BishopMasks[file, rank] | KnightMasks[file, rank]);
                
                // regular moves
                blockMoveList.AddRange(Combinations(relativeUD, 3));
                blockMoveList.AddRange(Combinations(relativeDD, 3));
                blockMoveList.AddRange(Combinations(Rank >> (rank * 8), 3));
                blockMoveList.AddRange(Combinations(File >> (7 - file), 3));
                blockMoveList.AddRange(Combinations(KnightMasks[file, rank], 3));
                
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

        BlockMoves = blockMoveList.Distinct().ToArray();
        MagicLookup.BlockMoveNumber = (4154364917966041783, 46, 262133); //MagicNumbers.GenerateRepeat(BlockMoves, 1, 46);
        EnPassantMasks = enPassantBitboards.ToArray();
        MagicLookup.EnPassantNumbers = (15417481889308385644, 58, 63); // MagicNumbers.GenerateRepeat(EnPassantMasks, 10000);
        MagicLookup.EnPassantLookupArray = new Move[MagicLookup.EnPassantNumbers.highest + 1];
        foreach (ulong mask in EnPassantMasks) // for each possible en passant
        {
            MagicLookup.EnPassantLookupArray[(mask * MagicLookup.EnPassantNumbers.magicNumber) >> MagicLookup.EnPassantNumbers.push] = GetEnPassantMoves(mask);
        }
        
        // pawn eval combinations
        List<ulong> rightPawns = Combinations(RightPawns, 8);
        List<ulong> leftPawns = Combinations(LeftPawns, 8);
        List<ulong> centerPawns = Combinations(CenterPawns, 8);
        
        MagicLookup.RightPawnEvalNumber = (17067507152026048335, 37, 134217725); // MagicNumbers.GenerateMagicNumberParallel(rightPawns.Distinct().ToArray(),37 ,7, false);
        MagicLookup.LeftPawnEvalNumber = (615594976254142229, 37, 134217609); // MagicNumbers.GenerateMagicNumberParallel(leftPawns.Distinct().ToArray(), 37, 7, false);
        MagicLookup.CenterPawnEvalNumber = (15570990422680516493, 37, 134217566); // MagicNumbers.GenerateMagicNumberParallel(centerPawns.Distinct().ToArray(), 37, 7, false);
        
        MagicLookup.RightPawnEvalLookup = new PawnEvaluation[MagicLookup.RightPawnEvalNumber.highest+ 1];
        MagicLookup.LeftPawnEvalLookup = new PawnEvaluation[MagicLookup.LeftPawnEvalNumber.highest + 1];
        MagicLookup.CenterPawnEvalLookup = new PawnEvaluation[MagicLookup.CenterPawnEvalNumber.highest + 1];

        Parallel.For(0, 3, e =>
        {
            switch (e)
            {
                case 0:
                    foreach (ulong combination in rightPawns)
                        MagicLookup.RightPawnEvalLookup[(combination * MagicLookup.RightPawnEvalNumber.magicNumber) >> MagicLookup.RightPawnEvalNumber.push] = 
                            GeneratePawnEval(combination, Section.Right);
                    break;
                case 1:
                    foreach (ulong combination in leftPawns)
                        MagicLookup.LeftPawnEvalLookup[(combination * MagicLookup.LeftPawnEvalNumber.magicNumber) >> MagicLookup.LeftPawnEvalNumber.push] = 
                            GeneratePawnEval(combination, Section.Left);
                    break;
                case 2:
                    foreach (ulong combination in centerPawns)
                        MagicLookup.CenterPawnEvalLookup[(combination * MagicLookup.CenterPawnEvalNumber.magicNumber) >> MagicLookup.CenterPawnEvalNumber.push] = 
                            GeneratePawnEval(combination, Section.Center);
                    break;
            }
        });
        
        List<ulong> firstSlice = Combinations(FirstSlice, 9);
        List<ulong> secondSlice = Combinations(SecondSlice, 9);
        List<ulong> thirdSlice = Combinations(ThirdSlice, 9);
        List<ulong> fourthSlice = Combinations(FourthSlice, 9);
        
        MagicLookup.FirstRookEvaluationLookup = new RookEvaluation[firstSlice.Max() + 1];
        MagicLookup.SecondRookEvaluationLookup = new RookEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookup.ThirdRookEvaluationLookup = new RookEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookup.FourthRookEvaluationLookup = new RookEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        MagicLookup.FirstQueenEvaluationLookup = new StandardEvaluation[firstSlice.Max() + 1];
        MagicLookup.SecondQueenEvaluationLookup = new StandardEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookup.ThirdQueenEvaluationLookup = new StandardEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookup.FourthQueenEvaluationLookup = new StandardEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        MagicLookup.FirstKnightEvaluationLookup = new StandardEvaluation[firstSlice.Max() + 1];
        MagicLookup.SecondKnightEvaluationLookup = new StandardEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookup.ThirdKnightEvaluationLookup = new StandardEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookup.FourthKnightEvaluationLookup = new StandardEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        MagicLookup.FirstBishopEvaluationLookup = new StandardEvaluation[firstSlice.Max() + 1];
        MagicLookup.SecondBishopEvaluationLookup = new StandardEvaluation[secondSlice.Max(n => n >> 16) + 1];
        MagicLookup.ThirdBishopEvaluationLookup = new StandardEvaluation[thirdSlice.Max(n => n >> 32) + 1];
        MagicLookup.FourthBishopEvaluationLookup = new StandardEvaluation[fourthSlice.Max(n => n >> 48) + 1];
        
        Parallel.For(0, 4, e =>
        {
            switch (e)
            {
                case 0:
                    foreach (ulong combination in firstSlice)
                    {
                        MagicLookup.FirstRookEvaluationLookup[combination] = GenerateRookEval(combination, Slice.First);
                        MagicLookup.FirstQueenEvaluationLookup[combination] = GenerateStandardEval(combination, Slice.First, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookup.FirstKnightEvaluationLookup[combination] = GenerateStandardEval(combination, Slice.First, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookup.FirstBishopEvaluationLookup[combination] = GenerateStandardEval(combination, Slice.First, Pieces.WhiteBishop, Pieces.BlackBishop);
                    }

                    break;
                case 1:
                    foreach (ulong combination in secondSlice)
                    {
                        MagicLookup.SecondRookEvaluationLookup[combination >> 16] = GenerateRookEval(combination, Slice.Second);
                        MagicLookup.SecondQueenEvaluationLookup[combination >> 16] = GenerateStandardEval(combination, Slice.Second, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookup.SecondKnightEvaluationLookup[combination >> 16] = GenerateStandardEval(combination, Slice.Second, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookup.SecondBishopEvaluationLookup[combination >> 16] = GenerateStandardEval(combination, Slice.Second, Pieces.WhiteBishop, Pieces.BlackBishop);
                    }

                    break;
                case 2:
                    foreach (ulong combination in thirdSlice)
                    {
                        MagicLookup.ThirdRookEvaluationLookup[combination >> 32] = GenerateRookEval(combination, Slice.Third);
                        MagicLookup.ThirdQueenEvaluationLookup[combination >> 32] = GenerateStandardEval(combination, Slice.Third, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookup.ThirdKnightEvaluationLookup[combination >> 32] = GenerateStandardEval(combination, Slice.Third, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookup.ThirdBishopEvaluationLookup[combination >> 32] = GenerateStandardEval(combination, Slice.Third, Pieces.WhiteBishop, Pieces.BlackBishop);
                    }
                    break;
                case 3:
                    foreach (ulong combination in fourthSlice)
                    {
                        MagicLookup.FourthRookEvaluationLookup[combination >> 48] = GenerateRookEval(combination, Slice.Fourth);
                        MagicLookup.FourthQueenEvaluationLookup[combination >> 48] = GenerateStandardEval(combination, Slice.Fourth, Pieces.WhiteQueen, Pieces.BlackQueen);
                        MagicLookup.FourthKnightEvaluationLookup[combination >> 48] = GenerateStandardEval(combination, Slice.Fourth, Pieces.WhiteKnight, Pieces.BlackKnight);
                        MagicLookup.FourthBishopEvaluationLookup[combination >> 48] = GenerateStandardEval(combination, Slice.Fourth, Pieces.WhiteBishop, Pieces.BlackBishop);
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
                    MagicLookup.RookCaptureLookup[file, rank][(RookCaptureCombinations[file, rank][i] * MagicLookup.RookCapture[file, rank].magicNumber) >> MagicLookup.RookCapture[file, rank].push] = GetBitboardMoves(RookCaptureCombinations[file, rank][i], (file, rank), 50);
                }
                
                // bishop captures
                MagicLookup.BishopCapture[file, rank] = MagicNumbers.BishopCaptureNumbers[file, rank]; // MagicNumbers.GenerateRepeat(BishopCaptureCombinations[file, rank], 1000);
                MagicLookup.BishopCaptureLookup[file, rank] = new Move[MagicLookup.BishopCapture[file, rank].highest + 1][];
                
                for (int i = 0; i < BishopCaptureCombinations[file, rank].Length; i++) // for each blocker
                {
                    MagicLookup.BishopCaptureLookup[file, rank][(BishopCaptureCombinations[file, rank][i] * MagicLookup.BishopCapture[file, rank].magicNumber) >> MagicLookup.BishopCapture[file, rank].push] = GetBitboardMoves(BishopCaptureCombinations[file, rank][i], (file, rank), 50);
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
                    MagicLookup.KnightCaptureLookup[file, rank][(KnightCombinations[file, rank][i] * MagicLookup.KnightMove[file, rank].magicNumber) >> MagicLookup.KnightMove[file, rank].push] = GetBitboardMoves(KnightCombinations[file, rank][i], (file, rank), 50);
                }
                
                // king moves
                MagicLookup.KingMove[file, rank] = MagicNumbers.KingNumbers[file, rank]; // MagicNumbers.GenerateRepeat(KingCombinations[file, rank], 5000);
                MagicLookup.KingLookup[file, rank] = new Move[MagicLookup.KingMove[file, rank].highest + 1][];
                MagicLookup.KingCaptureLookup[file, rank] = new Move[MagicLookup.KingMove[file, rank].highest + 1][];
                MagicLookup.KingSafetyLookup[file, rank] = new int[MagicLookup.KingMove[file, rank].highest + 1];
                
                for (int i = 0; i < KingCombinations[file, rank].Length; i++) // for each combination
                {
                    MagicLookup.KingLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookup.KingMove[file, rank].magicNumber) >> MagicLookup.KingMove[file, rank].push] = GetBitboardMoves(KingCombinations[file, rank][i], (file, rank), 5);
                    MagicLookup.KingCaptureLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookup.KingMove[file, rank].magicNumber) >> MagicLookup.KingMove[file, rank].push] = GetBitboardMoves(KingCombinations[file, rank][i], (file, rank), 3);
                    MagicLookup.KingSafetyLookup[file, rank][(KingCombinations[file, rank][i] * MagicLookup.KingMove[file, rank].magicNumber) >> MagicLookup.KingMove[file, rank].push] = Weights.KingSafetyBonuses[UInt64.PopCount(KingCombinations[file, rank][i])];
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
                
                // blocking checks
                // block captures
                MagicLookup.BlockCaptureNumbers[file, rank] = MagicNumbers.BlockCaptureNumbers[file, rank]; //MagicNumbers.GenerateRepeat(BlockCaptures[file, rank], 10000);
                MagicLookup.BlockCaptureMoveLookup[file, rank] = new Move[MagicLookup.BlockCaptureNumbers[file, rank].highest + 1];
                MagicLookup.BlockCaptureMovePawnLookup[file, rank] = new Move[MagicLookup.BlockCaptureNumbers[file, rank].highest + 1][];
                
                for (int i = 0; i < BlockCaptures[file, rank].Length; i++)
                {
                    MagicLookup.BlockCaptureMoveLookup[file, rank][(BlockCaptures[file, rank][i] * MagicLookup.BlockCaptureNumbers[file, rank].magicNumber) >> MagicLookup.BlockCaptureNumbers[file, rank].push] = GetBitboardMoves(BlockCaptures[file, rank][i], (file, rank), 25)[0];
                    if (rank != 0 && rank != 7) 
                        MagicLookup.BlockCaptureMovePawnLookup[file, rank][(BlockCaptures[file, rank][i] * MagicLookup.BlockCaptureNumbers[file, rank].magicNumber) >> MagicLookup.BlockCaptureNumbers[file, rank].push] = GetBitboardMoves(BlockCaptures[file, rank][i], (file, rank), 25, pawn: true);
                }
                
                // block moves
                MagicLookup.BlockMoveLookup[file, rank] = new Move[MagicLookup.BlockMoveNumber.highest + 1][];
                MagicLookup.BlockMovePawnLookup[file, rank] = new Move[MagicLookup.BlockMoveNumber.highest + 1][];
                
                foreach (ulong move in BlockMoves)
                {
                    MagicLookup.BlockMoveLookup[file, rank][(move * MagicLookup.BlockMoveNumber.magicNumber) >> MagicLookup.BlockMoveNumber.push] = GetBitboardMoves(move, (file, rank), 5);
                    if (rank != 0 && rank != 7) 
                        MagicLookup.BlockMovePawnLookup[file, rank][(move * MagicLookup.BlockMoveNumber.magicNumber) >> MagicLookup.BlockMoveNumber.push] = GetBitboardMoves(move, (file, rank), 5, pawn: true);
                }

                MagicLookup.KingEvaluation[file, rank] = new StandardEvaluation
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
                    path |= GetSquare(startFile, current);
                    current += moveBy;
                } while (current != endRank);
            }
            
            else if (endRank == startRank) // both are from the same file
            {
                int current = startFile;
                int moveBy = startFile < endFile ? 1 : -1;
                do
                {
                    path |= GetSquare(current, startRank);
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
                    path |= GetSquare(currentFile, currentRank);
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
                    path |= GetSquare(currentFile, currentRank);
                    currentFile += moveBy.file;
                    currentRank += moveBy.rank;
                } while ((currentFile, currentRank) != (endFile, endRank));
            }

            // in an L shape
            if (path != 0 || (Math.Abs(startFile - endFile) == 1 && Math.Abs(startRank - endRank) == 2) || (Math.Abs(startFile - endFile) == 2 && Math.Abs(startRank - endRank) == 1)) 
                path |= GetSquare(startFile, startRank) | GetSquare(endFile, endRank);
            
            PathLookup[startFile, startRank, endFile, endRank] = path;
        }

        Console.WriteLine($"Bitboards initialized in {t.Stop()}ms");
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

    private static List<ulong> Combinations(ulong blockerMask, int limit)
    {
        List<int> allIndices = new List<int>();
        int l = 0;
        for (int i = 0; i < 64; i++)
        {
            if (((blockerMask << 63 - i) >> 63) != 0)
            {
                l++;
                allIndices.Add(i);
            }
        }
        
        List<ulong> combinations = new();

        foreach (ulong i in GetValidCombinations(l, Math.Min(l, limit)))
        {
            ulong combination = 0;

            // for each index in the mask, push the bits of the combination to the right indices 
            for (int j = 0; j < l; j++)
            {
                combination ^= ((i << 63 - j) >> 63) << allIndices[j];
            }
            combinations.Add(combination);
        }
        
        return combinations;
    }

    enum Section
    {
        Right, Left, Center
    }
    
    private static PawnEvaluation GeneratePawnEval(ulong pawnCombination, Section boardSide)
    {
        PawnEvaluation eval = new();
        List<PassedBonus> wPassedBonuses = new();
        List<PassedBonus> bPassedBonuses = new();
        List<PassedBonus> wPassedBonusesEndgame = new();
        List<PassedBonus> bPassedBonusesEndgame = new();

        ulong relevantPawns = pawnCombination & boardSide switch
        {
            Section.Right => RightPawnMask,
            Section.Left => LeftPawnMask,
            Section.Center => CenterPawnMask,
            _ => throw new Exception("no")
        };
        int startAtFile = boardSide switch
        {
            Section.Left => 0,
            Section.Center => 3,
            Section.Right => 5,
            _ => throw new Exception("no")
        };
        int endAtFile = boardSide switch {
            Section.Left => 3,
            Section.Center => 5,
            Section.Right => 8,
            _ => throw new Exception("no")
        };

        for (int file = startAtFile; file < endAtFile; file++)
        {
            if ((GetFile(file) & relevantPawns) == 0)
                continue;
            
            for (int rank = 1; rank < 7; rank++)
            {
                if ((GetSquare(file, rank) & relevantPawns) != 0)
                {
                    // material and weight at the square
                    eval.wEval += (int)(Pieces.Value[Pieces.WhitePawn] * Weights.MaterialMultiplier + Weights.Pieces[Pieces.WhitePawn, file, rank]);
                    eval.bEval += (int)(Pieces.Value[Pieces.BlackPawn] * Weights.MaterialMultiplier - Weights.Pieces[Pieces.WhitePawn, file, 7-rank]);
                    
                    eval.wEval += (int)(Pieces.Value[Pieces.WhitePawn] * Weights.MaterialMultiplier + Weights.EndgamePieces[Pieces.WhitePawn, file, rank]);
                    eval.bEval += (int)(Pieces.Value[Pieces.BlackPawn] * Weights.MaterialMultiplier - Weights.EndgamePieces[Pieces.WhitePawn, file, 7-rank]);
                    
                    // protected
                    eval.wEval += Weights.ProtectedPawnBonus * (int)ulong.PopCount(pawnCombination & WhitePawnCaptureMasks[file, rank]);
                    eval.bEval -= Weights.ProtectedPawnBonus * (int)ulong.PopCount(pawnCombination & BlackPawnCaptureMasks[file, rank]);
                    
                    eval.wEvalEndgame += Weights.ProtectedPawnBonus * (int)ulong.PopCount(pawnCombination & WhitePawnCaptureMasks[file, rank]);
                    eval.bEvalEndgame -= Weights.ProtectedPawnBonus * (int)ulong.PopCount(pawnCombination & BlackPawnCaptureMasks[file, rank]);
                    
                    // passed masks
                    wPassedBonuses.Add(new PassedBonus(GetWhitePassedPawnMask(file, rank), Weights.WhitePassedPawnBonuses[rank]));
                    bPassedBonuses.Add(new PassedBonus(GetBlackPassedPawnMask(file, rank), Weights.BlackPassedPawnBonuses[rank]));
                    
                    wPassedBonusesEndgame.Add(new PassedBonus(GetWhitePassedPawnMask(file, rank), Weights.EndgameWhitePassedPawnBonuses[rank]));
                    bPassedBonusesEndgame.Add(new PassedBonus(GetBlackPassedPawnMask(file, rank), Weights.EndgameBlackPassedPawnBonuses[rank]));
                    
                    if ((NeighbourMasks[file] & pawnCombination) == 0)
                    {
                        eval.wEval += Weights.IsolatedPawnPenalty;
                        eval.bEval -= Weights.IsolatedPawnPenalty;
                        
                        eval.wEvalEndgame += Weights.IsolatedPawnPenalty;
                        eval.bEvalEndgame -= Weights.IsolatedPawnPenalty;
                    }
                }
            }
        }
        
        eval.wPassedPawnChecks = wPassedBonuses.ToArray();
        eval.bPassedPawnChecks = bPassedBonuses.ToArray();
        eval.wPassedPawnChecksEndgame = wPassedBonusesEndgame.ToArray();
        eval.bPassedPawnChecksEndgame = bPassedBonusesEndgame.ToArray();
        
        return eval;
    }
    
    public class PawnEvaluation
    {
        public int wEval;
        public int bEval;
        public int wEvalEndgame;
        public int bEvalEndgame;
        public PassedBonus[] wPassedPawnChecks = [];
        public PassedBonus[] bPassedPawnChecks = [];
        public PassedBonus[] wPassedPawnChecksEndgame = [];
        public PassedBonus[] bPassedPawnChecksEndgame = [];

        public int GetFinal(ulong enemyPawns, int side)
        {
            int final;
            if (side == 0)
            {
                final = wEval;
                foreach (PassedBonus p in wPassedPawnChecks)
                    if (p.Test(enemyPawns)) final += p.bonus;
            }
            else
            {
                final = bEval;
                foreach (PassedBonus p in bPassedPawnChecks)
                    if (p.Test(enemyPawns)) final += p.bonus;
            }
            
            return final;
        }
        
        public int GetFinalEndgame(ulong enemyPawns, int side)
        {
            int final;
            if (side == 0)
            {
                final = wEvalEndgame;
                foreach (PassedBonus p in wPassedPawnChecksEndgame)
                    if (p.Test(enemyPawns)) final += p.bonus;
            }
            else
            {
                final = bEvalEndgame;
                foreach (PassedBonus p in bPassedPawnChecksEndgame)
                    if (p.Test(enemyPawns)) final += p.bonus;
            }
            
            return final;
        }
    }

    enum Slice
    {
        First,
        Second,
        Third,
        Fourth,
    }

    private static RookEvaluation GenerateRookEval(ulong combination, Slice slice)
    {
        RookEvaluation eval = new();

        int startRank = slice switch
        {
            Slice.First => 6,
            Slice.Second => 4,
            Slice.Third => 2,
            Slice.Fourth => 0,
            _ => throw new Exception("no")
        };

        List<OpenFileCheck> fileChecks = new();

        for (int rank = startRank; rank < startRank + 2; rank++)
        {
            if ((GetRank(rank) & combination) == 0)
                continue;
            
            for (int file = 0; file < 8; file++)
            {
                // square occupied
                if ((combination & GetSquare(file, rank)) != 0)
                {
                    // material and weight multiplier
                    eval.wEval += (int)(Pieces.Value[Pieces.WhiteRook] * Weights.MaterialMultiplier) + Weights.Pieces[Pieces.WhiteRook, file, rank];
                    eval.bEval += (int)(Pieces.Value[Pieces.BlackRook] * Weights.MaterialMultiplier) - Weights.Pieces[Pieces.WhiteRook, file, 7-rank];
                    
                    eval.wEvalEndgame += (int)(Pieces.Value[Pieces.WhiteRook] * Weights.MaterialMultiplier) + Weights.EndgamePieces[Pieces.WhiteRook, file, rank];
                    eval.bEvalEndgame += (int)(Pieces.Value[Pieces.BlackRook] * Weights.MaterialMultiplier) - Weights.EndgamePieces[Pieces.WhiteRook, file, 7-rank];
                    
                    // open files
                    fileChecks.Add(new(GetFile(file)));
                }
            }
        }
        
        eval.fileChecks = fileChecks.ToArray();

        return eval;
    }

    private static StandardEvaluation GenerateStandardEval(ulong combination, Slice slice, uint wPiece, uint bPiece)
    {
        StandardEvaluation eval = new();
        
        int startRank = slice switch
        {
            Slice.First => 6,
            Slice.Second => 4,
            Slice.Third => 2,
            Slice.Fourth => 0,
            _ => throw new Exception("no")
        };
        
        for (int rank = startRank; rank < startRank + 2; rank++)
        {
            if ((GetRank(rank) & combination) == 0)
                continue;
            
            for (int file = 0; file < 8; file++)
            {
                // square occupied
                if ((combination & GetSquare(file, rank)) != 0)
                {
                    eval.wEval += (int)(Pieces.Value[wPiece] * Weights.MaterialMultiplier) + Weights.Pieces[wPiece, file, rank];
                    eval.bEval += (int)(Pieces.Value[bPiece] * Weights.MaterialMultiplier) - Weights.Pieces[wPiece, file, 7-rank];
                    
                    eval.wEvalEndgame += (int)(Pieces.Value[wPiece] * Weights.MaterialMultiplier) + Weights.EndgamePieces[wPiece, file, rank];
                    eval.bEvalEndgame += (int)(Pieces.Value[bPiece] * Weights.MaterialMultiplier) - Weights.EndgamePieces[wPiece, file, 7-rank];
                }
            }
        }

        return eval;
    }

    public class RookEvaluation
    {
        public int wEval;
        public int bEval;
        public int wEvalEndgame;
        public int bEvalEndgame;
        public OpenFileCheck[] fileChecks = [];

        public int GetFinal(ulong enemyPawns, ulong friendlyPawns, int side)
        {
            int final;
            if (side == 0)
            {
                final = wEval;
                foreach (OpenFileCheck f in fileChecks)
                    final += f.Test(enemyPawns, friendlyPawns);
            }
            else
            {
                final = bEval;
                foreach (OpenFileCheck f in fileChecks)
                    final -= f.Test(enemyPawns, friendlyPawns);
            }
            
            return final;
        }
    }

    public class StandardEvaluation
    {
        public int wEval;
        public int bEval;
        public int wEvalEndgame;
        public int bEvalEndgame;
    }

    public readonly struct OpenFileCheck(ulong file)
    {
        public int Test(ulong enemy, ulong friendly)
        {
            if ((file & friendly) == 0) // at least semi open
                return (file & enemy) == 0 ? Weights.OpenFileAdvantage : Weights.SemiOpenFileAdvantage;
            return 0;
        }
    }
    
    public readonly struct PassedBonus(ulong mask, int bonus)
    {
        public readonly int bonus = bonus;

        public bool Test(ulong enemyPawns)
        {
            return (mask & enemyPawns) == 0;
        }
    }

    private static IEnumerable<ulong> GetValidCombinations(int max, int limit)
    {
        if (max < 0 || max > 64)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be between 0 and 64 inclusive.");

        if (limit < 0 || limit > max)
            throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 0 and max inclusive.");
        
        for (int ones = 0; ones <= limit; ones++)
        {
            foreach (ulong combination in GenerateBitCombinations(max, ones))
            {
                yield return combination;
            }
        }
    }

    // Generates all ulong numbers with exactly 'ones' bits set within 'max' bit positions
    private static IEnumerable<ulong> GenerateBitCombinations(int max, int ones)
    {
        if (ones == 0)
        {
            yield return 0;
            yield break;
        }

        int[] indices = new int[ones];
        for (int i = 0; i < ones; i++)
            indices[i] = i;

        while (indices[0] <= max - ones)
        {
            // Build ulong from bit indices
            ulong value = 0;
            foreach (int index in indices)
            {
                value |= 1UL << index;
            }

            yield return value;

            // Generate next combination
            int pos = ones - 1;
            while (pos >= 0 && indices[pos] == max - ones + pos)
                pos--;

            if (pos < 0)
                break;

            indices[pos]++;
            for (int i = pos + 1; i < ones; i++)
                indices[i] = indices[i - 1] + 1;
        }
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

    private static ulong[] GetSingleBits(ulong mask)
    {
        List<ulong> bits = new();
        for (int rank = 0; rank < 8; rank++)
            for (int file = 0; file < 8; file++)
                if ((mask & GetSquare(file, rank)) != 0) // square occupied on the mask
                    bits.Add(GetSquare(file, rank));
        return bits.ToArray();
    }
    
    private static Move[] GetBitboardMoves(ulong bitboard, (int file, int rank) pos, int priority, bool pawn = false)
    {
        List<Move> moves = new List<Move>();
        
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if ((bitboard & GetSquare(file, rank)) != 0) // if the given square is on
                {
                    if (!pawn)
                        moves.Add(new Move(pos, (file,rank), priority: priority + PriorityWeights[file, rank]));
                    else if (pawn)
                    {
                        if (rank == 0 || rank == 7) // promotion
                        {
                            moves.Add(new Move(pos, (file,rank), promotion: Pieces.WhiteQueen, priority: priority + PriorityWeights[file, rank] + 50, pawn: pawn));
                            moves.Add(new Move(pos, (file,rank), promotion: Pieces.WhiteRook, priority: priority + PriorityWeights[file, rank] + 5, pawn: pawn));
                            moves.Add(new Move(pos, (file,rank), promotion: Pieces.WhiteBishop, priority: priority + PriorityWeights[file, rank], pawn: pawn));
                            moves.Add(new Move(pos, (file,rank), promotion: Pieces.WhiteKnight, priority: priority + PriorityWeights[file, rank], pawn: pawn));
                        }
                        else
                            moves.Add(new Move(pos, (file,rank), priority: priority + PriorityWeights[file, rank] + 20, pawn: pawn));

                    }
                }
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

    public static ulong GetPossibleEnPassantSquare(int file, int side)
    {
        return side == 0 ? WhitePossibleEnPassant << file : BlackPossibleEnPassant << file;
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

    private static ulong GetRank(int rank)
    {
        return Rank >> (8 * rank);
    }

    private static ulong GetWhitePassedPawnMask(int file, int rank)
    {
        return PassedPawnMasks[file] >> (rank * 8 + 8);
    }
    private static ulong GetBlackPassedPawnMask(int file, int rank)
    {
        return PassedPawnMasks[file] << ((8 - rank) * 8);
    }

    private static bool ValidSquare(int file, int rank)
    {
        return file is >= 0 and < 8 && rank is >= 0 and < 8;
    }
}