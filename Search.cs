namespace Blaze;

public static class Search
{
    public static (Move move, int eval) BestMove(Board board, int depth)
    {
        Move[] moves = FilterChecks(SearchBoard(board), board);
        int[] evals = new int[moves.Length];
        if (moves.Length == 0) throw new MoveNotFoundException();

        for (int i = 0; i < moves.Length; i++)
        {
            Board moveBoard = new(board);
            moveBoard.MakeMove(moves[i]);
            evals[i] = Minimax(moveBoard, depth - 1, int.MinValue, int.MaxValue);
        }
        if (board.side == 0)
            return (moves[Array.IndexOf(evals, evals.Max())], evals.Max()); // white
        return (moves[Array.IndexOf(evals, evals.Min())], evals.Min()); // black
    }
    
    private static int Minimax(Board board, int depth, int alpha, int beta)
    {
        if (board.IsDraw())
            return 0;

        if (depth == 0) // return heuristic evaluation
            return StaticEvaluate(board);
        
        if (board.side == 0)
        {
            // white - maximizing player
            int eval = int.MinValue;
            Move[] moves = SearchBoard(board);
            
            // denotes whether a legal move has been found - if the king is in check after the move it's not counted, of none are found, the player has no legal moves
            bool found = false;
            
            // for each child
            foreach (Move move in moves)
            {
                Board moveBoard = new(board);
                moveBoard.MakeMove(move);
                if (Attacked(moveBoard.KingPositions[0], moveBoard, 1)) // if the king is in check after the move
                    continue;
                found = true; // if a move is legal, set found to true
                
                eval = Math.Max(eval, Minimax(moveBoard, depth - 1, alpha, beta));
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break; // beta cutoff
            }
            
            if (found)
                return eval;
            
            // not found - no legal moves
            if (Attacked(board.KingPositions[0], board, 1)) // if the king is in check
                return int.MinValue; // worst outcome for white
            return 0; // game is a draw by stalemate
        }
        else
        {
            // black - minimizing player
            int eval = int.MaxValue;
            Move[] moves = SearchBoard(board);
            
            bool found = false;

            foreach (Move move in moves)
            {
                Board moveBoard = new(board);
                moveBoard.MakeMove(move);
                if (Attacked(moveBoard.KingPositions[1], moveBoard, 0)) // if the king is in check after the move
                    continue;
                found = true;
                
                eval = Math.Min(eval, Minimax(moveBoard, depth - 1, alpha, beta));
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break; // alpha cutoff
            }
            
            if (found)
                return eval;
            
            if (Attacked(board.KingPositions[1], board, 0)) // if the king is in check
                return int.MaxValue; // worst outcome for black
            return 0;
        }
    }

    public static int StaticEvaluate(Board board)
    {
        int eval = 0;
        
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // the square is only worth checking if the searched side has a piece there
                if ((board.AllPieces() & Bitboards.GetSquare(file, rank)) != 0)
                {
                    eval += Pieces.Value[board.GetPiece(file, rank)] + Weights.Pieces[board.GetPiece(file, rank), file, rank];
                }
            }
        }

        return eval;
    }
    
    // returns pseudo legal moves: abides by the rules of piece movement, but does not account for checks
    public static Move[] SearchBoard(Board board, bool ordering = true)
    {
        Move[] moveArray = new Move[219]; // max moves possible from 1 position
        bool enPassant = board.enPassant.file != 8; // if there is an en passant square

        int index = 0;
        // loop through every square
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // the square is only worth checking if the searched side has a piece there
                if ((board.bitboards[board.side] & Bitboards.GetSquare(file, rank)) != 0)
                {
                    Span<Move> moveSpan = new Span<Move>(moveArray, index, moveArray.Length - index); // creates a span to fill with moves
                    index += SearchPiece(board, board.GetPiece(file, rank), (file, rank), board.side, moveSpan, enPassant);
                }
            }
        }

        if (ordering)
        {
            Move[] sortedMoveArray = new Span<Move>(moveArray, 0, index).ToArray();
            Array.Sort(sortedMoveArray, (x,y) => y.Priority.CompareTo(x.Priority));
            return sortedMoveArray;
        }

        return new Span<Move>(moveArray, 0, index).ToArray();
    }

    private static int SearchPiece(Board board, ulong piece, (int file, int rank) pos, int side, Span<Move> moveSpan, bool enPassant = false)
    {
        int index = 0;
        Span<Move> captures;
        
        switch (piece & Pieces.TypeMask)
        {
            case Pieces.WhitePawn:
                if (side == 0) // white
                {
                    Span<Move> WPawnMoves = new(Bitboards.WhitePawnLookupMoves(pos, board.AllPieces()));
                    WPawnMoves.CopyTo(moveSpan);
                    index += WPawnMoves.Length;
                    captures = new(Bitboards.WhitePawnLookupCaptures(pos, board.bitboards[1]));
                    captures.CopyTo(moveSpan.Slice(index));
                    index += captures.Length;
                    
                    // if there is an en passant capture available, and it can be made from the current square
                    if (enPassant && (Bitboards.WhitePawnCaptureMasks[pos.file, pos.rank] & Bitboards.GetSquare(board.enPassant)) != 0)
                        moveSpan[index++] = Bitboards.EnPassantLookup(Bitboards.GetSquare(pos) | Bitboards.GetSquare(board.enPassant));
                    
                }
                else // black
                {
                    Span<Move> BPawnMoves = new(Bitboards.BlackPawnLookupMoves(pos, board.AllPieces()));
                    BPawnMoves.CopyTo(moveSpan);
                    index += BPawnMoves.Length;                                                                 
                    captures = new(Bitboards.BlackPawnLookupCaptures(pos, board.bitboards[0]));
                    captures.CopyTo(moveSpan.Slice(index));
                    index += captures.Length;
                    
                    // if there is an en passant capture available, and it can be made from the current square
                    if (enPassant && (Bitboards.BlackPawnCaptureMasks[pos.file, pos.rank] & Bitboards.GetSquare(board.enPassant)) != 0)
                        moveSpan[index++] = Bitboards.EnPassantLookup(Bitboards.GetSquare(pos) | Bitboards.GetSquare(board.enPassant));
                }
            break;
            
            case Pieces.WhiteRook:
                // magic lookup moves
                // no captures
                (Move[] moves, ulong captures) rMoves = Bitboards.RookLookupMoves(pos, board.AllPieces());
                new Span<Move>(rMoves.moves).CopyTo(moveSpan);
                index += rMoves.moves.Length;

                // magic lookup of only captures
                // form a slice out of the span to ensure that none of the already added moves are overwritten
                captures = new(Bitboards.RookLookupCaptures(pos, rMoves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteBishop:
                (Move[] moves, ulong captures) bMoves = Bitboards.BishopLookupMoves(pos, board.AllPieces());
                new Span<Move>(bMoves.moves).CopyTo(moveSpan);
                index += bMoves.moves.Length;
                
                captures = new(Bitboards.BishopLookupCaptures(pos, bMoves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteQueen:
                // find rook moves
                (Move[] moves, ulong captures) moves = Bitboards.RookLookupMoves(pos, board.AllPieces());
                new Span<Move>(moves.moves).CopyTo(moveSpan);
                index += moves.moves.Length;
                
                captures = new(Bitboards.RookLookupCaptures(pos, moves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
                
                // find bishop moves
                moves = Bitboards.BishopLookupMoves(pos, board.AllPieces());
                new Span<Move>(moves.moves).CopyTo(moveSpan.Slice(index));
                index += moves.moves.Length;
                
                captures = new(Bitboards.BishopLookupCaptures(pos, moves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteKnight:
                // find moves, no captures
                Span<Move> knightMoves = new(Bitboards.KnightLookupMoves(pos, board.AllPieces()));
                knightMoves.CopyTo(moveSpan);
                index += knightMoves.Length;
                
                // find only captures
                captures = new(Bitboards.KnightLookupCaptures(pos, board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteKing:
                Span<Move> kingMoves = new(Bitboards.KingLookupMoves(pos, board.AllPieces()));
                kingMoves.CopyTo(moveSpan);
                index += kingMoves.Length;
                
                captures = new(Bitboards.KingLookupCaptures(pos, board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
                
                // castling
                if (side == 0) // white
                {
                    int check = 0; // 0: not checked
                    
                    if ((board.castling & 0b1000) != 0 && (board.bitboards[0] & Bitboards.WhiteShortCastleMask) == 0) // white can castle short
                    {
                        check = Attacked(board.KingPositions[0], board, 1) ? 1 : 2; // check here whether the king is in check 1 if it is, 2 if it isn't

                        if (check == 2 && !Attacked((5,0), board, 1))
                            moveSpan[index++] = Bitboards.WhiteShortCastle;
                    }

                    if ((board.castling & 0b0100) != 0 && (board.bitboards[0] & Bitboards.WhiteLongCastleMask) == 0) // white can castle long
                    {
                        if (check == 0) // if the king check hasn't been checked before
                            check = Attacked(board.KingPositions[0], board, 1) ? 1 : 2; // check here whether the king is in check 1 if it is, 2 if it isn't
                            
                        if (check == 2 && !Attacked((3,0), board, 1))
                            moveSpan[index++] = Bitboards.WhiteLongCastle;
                    }
                }
                else // black
                {
                    int check = 0; // 0: not checked
                    
                    if ((board.castling & 0b0010) != 0 && (board.bitboards[1] & Bitboards.BlackShortCastleMask) == 0) // black can castle short
                    {
                        check = Attacked(board.KingPositions[1], board, 0) ? 1 : 2; // check here whether the king is in check 1 if it is, 2 if it isn't

                        if (check == 2 && !Attacked((5,7), board, 0))
                            moveSpan[index++] = Bitboards.BlackShortCastle;
                    }

                    if ((board.castling & 0b0001) != 0 && (board.bitboards[1] & Bitboards.BlackLongCastleMask) == 0) // black can castle long
                    {
                        if (check == 0) // if the king check hasn't been checked before
                            check = Attacked(board.KingPositions[1], board, 0) ? 1 : 2; // check here whether the king is in check 1 if it is, 2 if it isn't
                            
                        if (check == 2 && !Attacked((3,7), board, 0))
                            moveSpan[index++] = Bitboards.BlackLongCastle;
                    }
                }
            break;
        }

        return index;
    }

    public static bool Attacked((int file, int rank) pos, Board board,int side) // attacker side
    {
        ulong rookAttack = Bitboards.RookLookupCaptureBitboards(pos, board.AllPieces()) & board.bitboards[side];
        ulong bishopAttack = Bitboards.BishopLookupCaptureBitboards(pos, board.AllPieces()) & board.bitboards[side];
        ulong knightAttacks = Bitboards.KnightMasks[pos.file, pos.rank] & board.bitboards[side];
        ulong pawnAttacks = side == 0 ? Bitboards.BlackPawnCaptureMasks[pos.file, pos.rank] & board.bitboards[0] : Bitboards.WhitePawnCaptureMasks[pos.file, pos.rank] & board.bitboards[1];
        ulong kingAttacks = Bitboards.KingMasks[pos.file, pos.rank] & board.bitboards[side];
        
        if ((rookAttack | bishopAttack | knightAttacks | pawnAttacks | kingAttacks) == 0) // if no pieces could attack a certain square, there is no need to look further
            return false;
        
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                if ((Bitboards.GetSquare(file, rank) & rookAttack) != 0 && (board.GetPiece(file, rank) & Pieces.TypeMask) is Pieces.WhiteRook or Pieces.WhiteQueen)
                    return true;
                if ((Bitboards.GetSquare(file, rank) & bishopAttack) != 0 && (board.GetPiece(file, rank) & Pieces.TypeMask) is Pieces.WhiteBishop or Pieces.WhiteQueen)
                    return true;
                if ((Bitboards.GetSquare(file, rank) & knightAttacks) != 0 && (board.GetPiece(file, rank) & Pieces.TypeMask) == Pieces.WhiteKnight)
                    return true;
                if ((Bitboards.GetSquare(file, rank) & pawnAttacks) != 0 && (board.GetPiece(file, rank) & Pieces.TypeMask) == Pieces.WhitePawn)
                    return true;
                if ((Bitboards.GetSquare(file, rank) & kingAttacks) != 0 && (board.GetPiece(file, rank) & Pieces.TypeMask) == Pieces.WhiteKing)
                    return true;
            }
        }
        
        return false;
    }

    public static Move[] FilterChecks(Move[] moves, Board board)
    {
        List<Move> MoveList = moves.ToList();

        for (int i = moves.Length - 1; i >= 0; i--)
        {
            Board moveBoard = new(board);
            moveBoard.MakeMove(MoveList[i]);
            // if the king of the moving side is in check after the move, the move is illegal
            if (Attacked(moveBoard.KingPositions[1-moveBoard.side], moveBoard, moveBoard.side))
                MoveList.RemoveAt(i);
        }
        
        return MoveList.ToArray();
    }
}

[Serializable]
public class MoveNotFoundException : Exception
{
    // occurs when trying to get the best move in a position with no legal moves
    public MoveNotFoundException() {}
    public MoveNotFoundException(string message) : base(message) {}
    public MoveNotFoundException(string message, Exception inner) : base(message, inner) {}
}