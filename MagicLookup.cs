namespace Blaze;

public static class MagicLookup
{
        public static ref (Move[] moves, ulong captures) RookLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref Bitboards.MagicLookupConsts.RookLookup[pos.file, pos.rank]
        [
            ((blockers & Bitboards.RookMasks[pos.file, pos.rank]) // blocker combination
             * Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].push
        ];
    }
    
    public static ref (Move[] moves, ulong captures) BishopLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref Bitboards.MagicLookupConsts.BishopLookup[pos.file, pos.rank]
        [
            ((blockers & Bitboards.BishopMasks[pos.file, pos.rank]) // blocker combination
            * Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].push
        ];
    }
    
    public static ref Move[] RookLookupCaptures((int file, int rank) pos, ulong captures)
    {
        return ref Bitboards.MagicLookupConsts.RookCaptureLookup[pos.file, pos.rank]
            [(captures * Bitboards.MagicLookupConsts.RookCapture[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.RookCapture[pos.file, pos.rank].push];
    }
    
    public static ulong RookLookupCaptureBitboards((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.RookLookupCapturesArray[pos.file, pos.rank]
            [((blockers & Bitboards.RookMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].push];
    }
    
    public static ulong BishopLookupCaptureBitboards((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.BishopLookupCapturesArray[pos.file, pos.rank]
            [((blockers & Bitboards.BishopMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] BishopLookupCaptures((int file, int rank) pos, ulong captures)
    {
        return ref Bitboards.MagicLookupConsts.BishopCaptureLookup[pos.file, pos.rank]
            [(captures * Bitboards.MagicLookupConsts.BishopCapture[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BishopCapture[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KnightLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref Bitboards.MagicLookupConsts.KnightLookup[pos.file, pos.rank]
            [((~blockers & Bitboards.KnightMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.KnightMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.KnightMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KnightLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref Bitboards.MagicLookupConsts.KnightCaptureLookup[pos.file, pos.rank]
            [((enemy & Bitboards.KnightMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.KnightMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.KnightMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KingLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref Bitboards.MagicLookupConsts.KingLookup[pos.file, pos.rank]
            [((~blockers & Bitboards.KingMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.KingMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.KingMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] KingLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref Bitboards.MagicLookupConsts.KingCaptureLookup[pos.file, pos.rank]
            [((enemy & Bitboards.KingMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.KingMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.KingMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] WhitePawnLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref Bitboards.MagicLookupConsts.WhitePawnLookup[pos.file, pos.rank]
            [((blockers & Bitboards.WhitePawnMoveMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.WhitePawnMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.WhitePawnMove[pos.file, pos.rank].push];
    }

    public static ref Move[] BlackPawnLookupMoves((int file, int rank) pos, ulong blockers)
    {
        return ref Bitboards.MagicLookupConsts.BlackPawnLookup[pos.file, pos.rank]
            [((blockers & Bitboards.BlackPawnMoveMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.BlackPawnMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BlackPawnMove[pos.file, pos.rank].push];
    }
    
    public static ref Move[] WhitePawnLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref Bitboards.MagicLookupConsts.WhitePawnCaptureLookup[pos.file, pos.rank]
            [((enemy & Bitboards.WhitePawnCaptureMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.WhitePawnCapture[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.WhitePawnCapture[pos.file, pos.rank].push];
    }
    
    public static ref Move[] BlackPawnLookupCaptures((int file, int rank) pos, ulong enemy)
    {
        return ref Bitboards.MagicLookupConsts.BlackPawnCaptureLookup[pos.file, pos.rank]
            [((enemy & Bitboards.BlackPawnCaptureMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.BlackPawnCapture[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BlackPawnCapture[pos.file, pos.rank].push];
    }
    
    public static ref Move EnPassantLookup(ulong enPassant)
    {
        return ref Bitboards.MagicLookupConsts.EnPassantLookupArray[(enPassant * Bitboards.MagicLookupConsts.EnPassantNumbers.magicNumber) >> Bitboards.MagicLookupConsts.EnPassantNumbers.push];
    }

    public static int KingSafetyBonusLookup((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.KingSafetyLookup[pos.file, pos.rank]
            [((~blockers & Bitboards.KingMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.KingMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.KingMove[pos.file, pos.rank].push];
    }

    public static ulong RookMoveBitboardLookup((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.RookBitboardLookup[pos.file, pos.rank]
            [((blockers & Bitboards.SmallRookMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.RookBitboardNumbers[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.RookBitboardNumbers[pos.file, pos.rank].push];
    }
    
    public static int RookMobilityLookup((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.RookMobilityLookupArray[pos.file, pos.rank]
            [((blockers & Bitboards.SmallRookMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.RookBitboardNumbers[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.RookBitboardNumbers[pos.file, pos.rank].push];
    }
    
    public static ulong BishopMoveBitboardLookup((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.BishopBitboardLookup[pos.file, pos.rank]
            [((blockers & Bitboards.SmallBishopMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.BishopBitboardNumbers[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BishopBitboardNumbers[pos.file, pos.rank].push];
    }
    
    public static int BishopMobilityLookup((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.BishopMobilityLookupArray[pos.file, pos.rank]
            [((blockers & Bitboards.SmallBishopMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.BishopBitboardNumbers[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BishopBitboardNumbers[pos.file, pos.rank].push];
    }

    public static ulong RookPinLineLookup((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.RookPinLineBitboardLookup[pos.file, pos.rank]
            [((blockers & Bitboards.RookMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].push];
    }
    
    public static ulong BishopPinLineLookup((int file, int rank) pos, ulong blockers)
    {
        return Bitboards.MagicLookupConsts.BishopPinLineBitboardLookup[pos.file, pos.rank]
            [((blockers & Bitboards.BishopMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].push];
    }
    
    public static List<BitboardUtils.PinSearchResult> RookPinSearch((int file, int rank) pos, ulong selected)
    {
        return Bitboards.MagicLookupConsts.RookPinLookup[pos.file, pos.rank]
            [((selected & Bitboards.RookMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.RookMove[pos.file, pos.rank].push];
    }
    
    public static List<BitboardUtils.PinSearchResult> BishopPinSearch((int file, int rank) pos, ulong selected)
    {
        return Bitboards.MagicLookupConsts.BishopPinLookup[pos.file, pos.rank]
            [((selected & Bitboards.BishopMasks[pos.file, pos.rank]) * Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BishopMove[pos.file, pos.rank].push];
    }

    public static Move BlockCaptureLookup((int file, int rank) pos, ulong square)
    {
        return Bitboards.MagicLookupConsts.BlockCaptureMoveLookup[pos.file, pos.rank]
            [(square * Bitboards.MagicLookupConsts.BlockCaptureNumbers[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BlockCaptureNumbers[pos.file, pos.rank].push];
    }
    
    public static Move[] BlockLookup((int file, int rank) pos, ulong squares)
    {
        return Bitboards.MagicLookupConsts.BlockMoveLookup[pos.file, pos.rank]
            [(squares * Bitboards.MagicLookupConsts.BlockMoveNumber.magicNumber) >> Bitboards.MagicLookupConsts.BlockMoveNumber.push];
    }
    
    public static Move[] BlockCapturePawnLookup((int file, int rank) pos, ulong square)
    {
        return Bitboards.MagicLookupConsts.BlockCaptureMovePawnLookup[pos.file, pos.rank]
            [(square * Bitboards.MagicLookupConsts.BlockCaptureNumbers[pos.file, pos.rank].magicNumber) >> Bitboards.MagicLookupConsts.BlockCaptureNumbers[pos.file, pos.rank].push];
    }
    
    public static Move[] BlockPawnLookup((int file, int rank) pos, ulong squares)
    {
        return Bitboards.MagicLookupConsts.BlockMovePawnLookup[pos.file, pos.rank]
            [(squares * Bitboards.MagicLookupConsts.BlockMoveNumber.magicNumber) >> Bitboards.MagicLookupConsts.BlockMoveNumber.push];
    }

    public static Evaluation.PawnEvaluation PawnEvaluationLookupRight(ulong pawns)
    {
        return Bitboards.MagicLookupConsts.RightPawnEvalLookup[((pawns & Bitboards.RightPawns) * Bitboards.MagicLookupConsts.RightPawnEvalNumber.magicNumber) >> Bitboards.MagicLookupConsts.RightPawnEvalNumber.push];
    }
    
    public static Evaluation.PawnEvaluation PawnEvaluationLookupLeft(ulong pawns)
    {
        return Bitboards.MagicLookupConsts.LeftPawnEvalLookup[((pawns & Bitboards.LeftPawns) * Bitboards.MagicLookupConsts.LeftPawnEvalNumber.magicNumber) >> Bitboards.MagicLookupConsts.LeftPawnEvalNumber.push];
    }
    
    public static Evaluation.PawnEvaluation PawnEvaluationLookupCenter(ulong pawns)
    {
        return Bitboards.MagicLookupConsts.CenterPawnEvalLookup[((pawns & Bitboards.CenterPawns) * Bitboards.MagicLookupConsts.CenterPawnEvalNumber.magicNumber) >> Bitboards.MagicLookupConsts.CenterPawnEvalNumber.push];
    }

    public static Evaluation.RookEvaluation FirstRookEvalLookup(ulong rooks)
    {
        return Bitboards.MagicLookupConsts.FirstRookEvaluationLookup[rooks & Bitboards.FirstSlice];
    }

    public static Evaluation.RookEvaluation SecondRookEvalLookup(ulong rooks)
    {
        return Bitboards.MagicLookupConsts.SecondRookEvaluationLookup[(rooks & Bitboards.SecondSlice) >> 16];
    }

    public static Evaluation.RookEvaluation ThirdRookEvalLookup(ulong rooks)
    {
        return Bitboards.MagicLookupConsts.ThirdRookEvaluationLookup[(rooks & Bitboards.ThirdSlice) >> 32];
    }

    public static Evaluation.RookEvaluation FourthRookEvalLookup(ulong rooks)
    {
        return Bitboards.MagicLookupConsts.FourthRookEvaluationLookup[(rooks & Bitboards.FourthSlice) >> 48];
    }
    
    
    public static Evaluation.QueenEvaluation FirstQueenEvalLookup(ulong queen)
    {
        return Bitboards.MagicLookupConsts.FirstQueenEvaluationLookup[queen & Bitboards.FirstSlice];
    }

    public static Evaluation.QueenEvaluation SecondQueenEvalLookup(ulong queen)
    {
        return Bitboards.MagicLookupConsts.SecondQueenEvaluationLookup[(queen & Bitboards.SecondSlice) >> 16];
    }

    public static Evaluation.QueenEvaluation ThirdQueenEvalLookup(ulong queen)
    {
        return Bitboards.MagicLookupConsts.ThirdQueenEvaluationLookup[(queen & Bitboards.ThirdSlice) >> 32];
    }

    public static Evaluation.QueenEvaluation FourthQueenEvalLookup(ulong queen)
    {
        return Bitboards.MagicLookupConsts.FourthQueenEvaluationLookup[(queen & Bitboards.FourthSlice) >> 48];
    }
    
    public static Evaluation.KnightEvaluation FirstKnightEvalLookup(ulong knights)
    {
        return Bitboards.MagicLookupConsts.FirstKnightEvaluationLookup[knights & Bitboards.FirstSlice];
    }

    public static Evaluation.KnightEvaluation SecondKnightEvalLookup(ulong knights)
    {
        return Bitboards.MagicLookupConsts.SecondKnightEvaluationLookup[(knights & Bitboards.SecondSlice) >> 16];
    }

    public static Evaluation.KnightEvaluation ThirdKnightEvalLookup(ulong knights)
    {
        return Bitboards.MagicLookupConsts.ThirdKnightEvaluationLookup[(knights & Bitboards.ThirdSlice) >> 32];
    }

    public static Evaluation.KnightEvaluation FourthKnightEvalLookup(ulong knights)
    {
        return Bitboards.MagicLookupConsts.FourthKnightEvaluationLookup[(knights & Bitboards.FourthSlice) >> 48];
    }
    
    public static Evaluation.BishopEvaluation FirstBishopEvalLookup(ulong bishops)
    {
        return Bitboards.MagicLookupConsts.FirstBishopEvaluationLookup[bishops & Bitboards.FirstSlice];
    }

    public static Evaluation.BishopEvaluation SecondBishopEvalLookup(ulong bishops)
    {
        return Bitboards.MagicLookupConsts.SecondBishopEvaluationLookup[(bishops & Bitboards.SecondSlice) >> 16];
    }

    public static Evaluation.BishopEvaluation ThirdBishopEvalLookup(ulong bishops)
    {
        return Bitboards.MagicLookupConsts.ThirdBishopEvaluationLookup[(bishops & Bitboards.ThirdSlice) >> 32];
    }

    public static Evaluation.BishopEvaluation FourthBishopEvalLookup(ulong bishops)
    {
        return Bitboards.MagicLookupConsts.FourthBishopEvaluationLookup[(bishops & Bitboards.FourthSlice) >> 48];
    }

    public static Evaluation.KingEvaluation KingEvalLookup((int file, int rank) pos)
    {
        return Bitboards.MagicLookupConsts.KingEvaluationLookup[pos.file, pos.rank];
    }
}