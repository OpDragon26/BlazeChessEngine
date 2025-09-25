namespace Blaze;

public static class Search
{
    public static SearchResult BestMove(Board board, int depth, bool useBook, int bookDepth)
    {
        if (useBook)
        {
            Output output = Book.Retrieve(board, bookDepth);
            if (output.result == Result.Found)
            {
                Console.WriteLine("Book move");
                return new SearchResult(output.move, 1, true, 0);
            }
        }
        
        Timer timer = new Timer();
        timer.Start();
        
        Move[] moves = FilterChecks(SearchBoard(board), board);
        int[] evals = new int[moves.Length];
        if (moves.Length == 0) throw new Exception("No move found");

        
        Parallel.For(0, moves.Length, i =>
        {
            Board moveBoard = new(board, moves[i].PermaChange);
            moveBoard.MakeMove(moves[i]);
            evals[i] = Minimax(moveBoard, depth - 1, int.MinValue, int.MaxValue);
        });
        

        /*
        for (int i = 0; i < moves.Length; i++)
        {
            Board moveBoard = new(board);
            moveBoard.MakeMove(moves[i]);
            evals[i] = Minimax(moveBoard, depth - 1, int.MinValue, int.MaxValue);
        }
        */
        
        if (board.side == 0)
            return  new SearchResult(moves[Array.IndexOf(evals, evals.Max())], evals.Max(), false, timer.Stop()); // white
        return  new SearchResult(moves[Array.IndexOf(evals, evals.Min())], evals.Min(), false, timer.Stop()); // black
    }

    public readonly struct SearchResult(Move move, int eval, bool bookMove, long time)
    {
        public readonly Move move = move;
        public readonly int eval = eval;
        public readonly bool bookMove = bookMove;
        public readonly long time = time;
    }
    
    private static int Minimax(Board board, int depth, int alpha, int beta)
    {
        if (board.IsDraw())
            return 0;

        if (depth == 0) // return heuristic evaluation
            return StaticEvaluate(board);
        
        Board moveBoard;
        if (board.side == 0)
        {
            // white - maximizing player
            int eval = int.MinValue;
            Span<Move> moves = SearchBoard(board);
            
            // denotes whether a legal move has been found - if the king is in check after the move it's not counted, of none are found, the player has no legal moves
            bool found = false;
            
            // for each child
            foreach (Move move in moves)
            {
                moveBoard = new(board, move.PermaChange);
                moveBoard.MakeMove(move);
                if (Attacked(moveBoard.KingPositions[0], moveBoard, 1)) // if the king is in check after the move
                    continue;
                found = true; // if a move is legal, set found to true
                
                eval = Math.Max(eval, Minimax(moveBoard, depth - 1, alpha, beta));
                alpha = Math.Max(alpha, eval);
                if (eval >= beta) break; // beta cutoff
            }
            
            if (found)
                return eval;
            
            // not found - no legal moves
            if (Attacked(board.KingPositions[0], board, 1)) // if the king is in check
                // black won by checkmate
                // the higher the depth, the closer to the origin, the worse for white
                return int.MinValue + 100 - depth;
            return 0; // game is a draw by stalemate
        }
        else
        {
            // black - minimizing player
            int eval = int.MaxValue;
            Span<Move> moves = SearchBoard(board);
            
            bool found = false;

            foreach (Move move in moves)
            {
                moveBoard = new(board, move.PermaChange);
                moveBoard.MakeMove(move);
                if (Attacked(moveBoard.KingPositions[1], moveBoard, 0)) // if the king is in check after the move
                    continue;
                found = true;
                
                eval = Math.Min(eval, Minimax(moveBoard, depth - 1, alpha, beta));
                beta = Math.Min(beta, eval);
                if (eval <= alpha) break; // alpha cutoff
            }
            
            if (found)
                return eval;
            
            if (Attacked(board.KingPositions[1], board, 0)) // if the king is in check
                // white won by checkmate
                // the higher the depth, the closer to the origin, and better for white
                return int.MaxValue - 100 + depth;
            return 0;
        }
    }

    // returns the heuristic evaluation of the board
public static int StaticEvaluate(Board board)
    {
        int eval = 0;

        if (!board.IsEndgame())
        {
            for (int file = 0; file < 8; file++)
            {
                // counts pawns on the file and applies a penalty for multiple on one file
                eval += Weights.DoublePawnPenalties[UInt64.PopCount(Bitboards.GetFile(file) & board.bitboards[2])];
                eval -= Weights.DoublePawnPenalties[UInt64.PopCount(Bitboards.GetFile(file) & board.bitboards[3])];
                
                for (int rank = 7; rank >= 0; rank--)
                {
                    // the square is only worth checking if the searched side has a piece there
                    if ((board.bitboards[0] & Bitboards.GetSquare(file, rank)) != 0) // white piece
                    {
                        eval += (int)(Weights.MaterialMultiplier * Pieces.Value[board.GetPiece(file, rank)]) + Weights.Pieces[board.GetPiece(file, rank), file, rank];
                        
                        if ((Bitboards.GetSquare(file, rank) & board.bitboards[2]) != 0) // if the searched square is a white pawn
                        {
                            if ((Bitboards.GetWhitePassedPawnMask(file, rank) & board.bitboards[3]) == 0) // if the pawn is a passed pawn
                                eval += Weights.WhitePassedPawnBonuses[rank];
                            if ((Bitboards.NeighbourMasks[file] & board.bitboards[2]) == 0) // if the pawn has no neighbours
                                eval -= 20;
                        }
                        else if (rank == 0 && board.GetPiece(file, rank) == Pieces.WhiteRook) // rook on white's back rank
                        {
                            if ((Bitboards.GetFile(file) & board.AllPawns()) == 0) // on an open file
                                eval += 30;
                        }
                        
                        eval += (int)(UInt64.PopCount(SearchPieceBitboard(board, board.GetPiece(file, rank), (file, rank), 0)) * Weights.MobilityMultiplier);
                    }
                    else if ((board.bitboards[1] & Bitboards.GetSquare(file, rank)) != 0)
                    {
                        eval += (int)(Weights.MaterialMultiplier * Pieces.Value[board.GetPiece(file, rank)]) - Weights.Pieces[board.GetPiece(file, rank) & Pieces.TypeMask, file, 7-rank];

                        if ((Bitboards.GetSquare(file, rank) & board.bitboards[3]) != 0) // if the searched square is a black pawn
                        {
                            if ((Bitboards.GetBlackPassedPawnMask(file, rank) & board.bitboards[2]) == 0) // if the pawn is a passed pawn
                                eval += Weights.BlackPassedPawnBonuses[rank];
                            if ((Bitboards.NeighbourMasks[file] & board.bitboards[3]) == 0) // if the pawn has no neighbours
                                eval += 20;
                        }
                        else if (rank == 7 && board.GetPiece(file, rank) == Pieces.BlackRook) // rook on black's back rank
                        {
                            if ((Bitboards.GetFile(file) & board.AllPawns()) == 0) // on an open file
                                eval -= 30;
                        }

                        eval -= (int)(UInt64.PopCount(SearchPieceBitboard(board, board.GetPiece(file, rank), (file, rank), 1)) * Weights.MobilityMultiplier);
                    }
                }
            }

            // add or take eval according to which side has castled
            if ((board.castled & 0b10) != 0) // white castled
                eval += Weights.CastlingBonus;
            else
            {
                if ((board.castling & 0b1000) != 0) // can't short castle
                    eval -= Weights.NoCastlingPenalty;
                if ((board.castling & 0b100) != 0) // can't long castle
                    eval -= Weights.NoCastlingPenalty;
            }
            
            if ((board.castled & 0b1) != 0) // black castled
                eval -= Weights.CastlingBonus;
            else
            {
                if ((board.castling & 0b10) != 0) // can't short castle
                    eval += Weights.NoCastlingPenalty;
                if ((board.castling & 0b1) != 0) // can't long castle
                    eval += Weights.NoCastlingPenalty;
            }
            
            
            // check if white's king is in the right spot (likely castled) to have its safety evaluated
            if ((Bitboards.KingSafetyAppliesWhite & Bitboards.GetSquare(board.KingPositions[0])) != 0)
            {
                // add to the eval based on the safety of white's king
                eval += Bitboards.KingSafetyBonusLookup(board.KingPositions[0], board.bitboards[0]);
                if ((Bitboards.KingMasks[board.KingPositions[0].file, board.KingPositions[0].rank] & board.bitboards[1]) != 0) // if there is an enemy piece adjacent to the king
                    eval -= 50;
            
                // take from eval if the pawns in front of the king are missing
                foreach (int file in Bitboards.AdjacentFiles[board.KingPositions[0].file])
                    if ((Bitboards.GetFile(file) & board.bitboards[2]) == 0)
                        eval -= 25;
            }

            if ((Bitboards.KingSafetyAppliesBlack & Bitboards.GetSquare(board.KingPositions[1])) != 0)
            {
                eval -= Bitboards.KingSafetyBonusLookup(board.KingPositions[1], board.bitboards[1]);
                if ((Bitboards.KingMasks[board.KingPositions[1].file, board.KingPositions[1].rank] & board.bitboards[0]) != 0) // if there is an enemy piece adjacent to the king
                    eval += 50;
            
                foreach (int file in Bitboards.AdjacentFiles[board.KingPositions[1].file])
                    if ((Bitboards.GetFile(file) & board.bitboards[3]) == 0)
                        eval += 25;
            }
        }
        else
        {
            for (int file = 0; file < 8; file++)
            {
                // counts pawns on the file and applies a penalty for multiple on one file
                eval += Weights.DoublePawnPenalties[UInt64.PopCount(Bitboards.GetFile(file) & board.bitboards[2])];
                eval -= Weights.DoublePawnPenalties[UInt64.PopCount(Bitboards.GetFile(file) & board.bitboards[3])];
                
                for (int rank = 7; rank >= 0; rank--)
                {
                    // the square is only worth checking if the searched side has a piece there
                    if ((board.bitboards[0] & Bitboards.GetSquare(file, rank)) != 0) // white piece
                    {
                        eval += (int)(Weights.MaterialMultiplier * Pieces.Value[board.GetPiece(file, rank)]) + Weights.EndgamePieces[board.GetPiece(file, rank), file, rank];
                        
                        if ((Bitboards.GetSquare(file, rank) & board.bitboards[2]) != 0)
                        {
                            if ((Bitboards.GetWhitePassedPawnMask(file, rank) & board.bitboards[3]) == 0) // if the pawn is a passed pawn
                                eval += Weights.EndgameWhitePassedPawnBonuses[rank];
                            if ((Bitboards.GetSquare(file, rank + 1) & board.bitboards[2]) == 0) // if the pawns are doubled
                                eval -= 30;
                            if ((Bitboards.NeighbourMasks[file] & board.bitboards[2]) == 0) // if the pawn has no neighbours
                                eval -= 35;
                        }
                        else if (rank == 0 && board.GetPiece(file, rank) == Pieces.WhiteRook) // rook on white's back rank
                        {
                            if ((Bitboards.GetFile(file) & board.AllPawns()) == 0) // on an open file
                                eval += 40;
                        }
                        
                        eval += (int)(UInt64.PopCount(SearchPieceBitboard(board, board.GetPiece(file, rank), (file, rank), 0)) * Weights.MobilityMultiplier);
                    }
                    else if ((board.bitboards[1] & Bitboards.GetSquare(file, rank)) != 0)
                    {
                        eval += (int)(Weights.MaterialMultiplier * Pieces.Value[board.GetPiece(file, rank)]) - Weights.EndgamePieces[board.GetPiece(file, rank) & Pieces.TypeMask, file, 7-rank];

                        if ((Bitboards.GetSquare(file, rank) & board.bitboards[3]) != 0)
                        {
                            if ((Bitboards.GetBlackPassedPawnMask(file, rank) & board.bitboards[2]) == 0) // if the pawn is a passed pawn
                                eval += Weights.EndgameBlackPassedPawnBonuses[rank];
                            if ((Bitboards.GetSquare(file, rank - 1) & board.bitboards[3]) == 0) // if the pawns are doubled
                                eval += 30;
                            if ((Bitboards.NeighbourMasks[file] & board.bitboards[3]) == 0) // if the pawn has no neighbours
                                eval += 35;
                        }
                        else if (rank == 7 && board.GetPiece(file, rank) == Pieces.BlackRook) // rook on black's back rank
                        {
                            if ((Bitboards.GetFile(file) & board.AllPawns()) == 0) // on an open file
                                eval -= 40;
                        }
                        eval += (int)(UInt64.PopCount(SearchPieceBitboard(board, board.GetPiece(file, rank), (file, rank), 1)) * Weights.MobilityMultiplier);
                    }
                }
            }
        }

        return eval;
    }
    
    // returns pseudo legal moves: abides by the rules of piece movement, but does not account for checks
    public static Span<Move> SearchBoard(Board board, bool ordering = true)
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
            Span<Move> moveSpan = new Span<Move>(moveArray, 0, index);
            
            int[] keys = new int[moveSpan.Length];
            
            for (int i = 0; i < moveSpan.Length; i++)
                keys[i] = Reevaluate(board, moveSpan[i]);

            new Span<int>(keys).Sort(moveSpan, (x, y) => y.CompareTo(x));
            
            return moveSpan;
        }

        return new Span<Move>(moveArray, 0, index);
    }

    private static int Reevaluate(Board board, Move move)
    {
        int priority = move.Priority;
        
        priority += Pieces.Value[board.GetPiece(move.Destination) & Pieces.TypeMask];

        switch (board.GetPiece(move.Source))
        {
            case Pieces.WhitePawn:
                if ((Bitboards.WhitePawnCaptureMasks[move.Destination.file, move.Destination.rank] & Bitboards.GetSquare(board.KingPositions[1])) != 0)
                    priority += 50;
                break;
            case Pieces.BlackPawn:
                if ((Bitboards.BlackPawnCaptureMasks[move.Destination.file, move.Destination.rank] & Bitboards.GetSquare(board.KingPositions[0])) != 0)
                    priority += 50;
                break;
            case Pieces.WhiteRook:
                if ((Bitboards.RookLookupCaptureBitboards(move.Destination, board.bitboards[1]) & Bitboards.GetSquare(board.KingPositions[1])) != 0)
                    priority += 50;
                break;
            case Pieces.BlackRook:
                if ((Bitboards.RookLookupCaptureBitboards(move.Destination, board.bitboards[0]) & Bitboards.GetSquare(board.KingPositions[0])) != 0)
                    priority += 50;
                break;
            case Pieces.WhiteKnight:
                if ((Bitboards.KnightMasks[move.Destination.file, move.Destination.rank] & Bitboards.GetSquare(board.KingPositions[1])) != 0)
                    priority += 50;
                break;
            case Pieces.BlackKnight:
                if ((Bitboards.KnightMasks[move.Destination.file, move.Destination.rank] & Bitboards.GetSquare(board.KingPositions[0])) != 0)
                    priority += 50;
                break;
            case Pieces.WhiteBishop:
                if ((Bitboards.BishopLookupCaptureBitboards(move.Destination, board.bitboards[1]) & Bitboards.GetSquare(board.KingPositions[1])) != 0)
                    priority += 50;
                break;
            case Pieces.BlackBishop:
                if ((Bitboards.BishopLookupCaptureBitboards(move.Destination, board.bitboards[0]) & Bitboards.GetSquare(board.KingPositions[0])) != 0)
                    priority += 50;
                break;
            case Pieces.WhiteQueen:
                ulong wCaptures = Bitboards.BishopLookupCaptureBitboards(move.Destination, board.bitboards[1]) | Bitboards.RookLookupCaptureBitboards(move.Destination, board.bitboards[1]);
                if ((wCaptures & Bitboards.GetSquare(board.KingPositions[1])) != 0)
                    priority += 50;
                break;
            case Pieces.BlackQueen:
                ulong bCaptures = Bitboards.BishopLookupCaptureBitboards(move.Destination, board.bitboards[0]) | Bitboards.RookLookupCaptureBitboards(move.Destination, board.bitboards[0]);
                if ((bCaptures & Bitboards.GetSquare(board.KingPositions[0])) != 0)
                    priority += 50;
                break;
        }
        
        return priority;
    }

    private static ulong SearchPieceBitboard(Board board, ulong piece, (int file, int rank) pos, int side)
    {
        switch (piece & Pieces.TypeMask)
        {
            case Pieces.WhitePawn:
                return side == 0 ? Bitboards.WhitePawnCaptureMasks[pos.file, pos.rank] : Bitboards.BlackPawnCaptureMasks[pos.file, pos.rank];
            case Pieces.WhiteRook:
                return Bitboards.RookMoveBitboardLookup(pos, board.AllPieces());
            case Pieces.WhiteBishop:
                return Bitboards.BishopMoveBitboardLookup(pos, board.AllPieces());
            case Pieces.WhiteKnight:
                return Bitboards.KnightMasks[pos.file, pos.rank];
            case Pieces.WhiteQueen:
                return Bitboards.RookMoveBitboardLookup(pos, board.AllPieces()) | Bitboards.BishopMoveBitboardLookup(pos, board.AllPieces());
            case Pieces.WhiteKing:
                return Bitboards.KingMasks[pos.file, pos.rank];
            default:
                throw new Exception($"Unknown piece: {piece & Pieces.TypeMask}");
        }
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
        ulong pawnAttacks = side == 0 ? Bitboards.BlackPawnCaptureMasks[pos.file, pos.rank] & board.bitboards[2] : Bitboards.WhitePawnCaptureMasks[pos.file, pos.rank] & board.bitboards[3];
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

    public static (ulong pinned, Dictionary<ulong, ulong> pinStates) GetPinStates(Board board, int side)
    {
        ulong rookSelected = Bitboards.RookPinLineLookup(board.KingPositions[side], board.bitboards[1 - side]) & board.AllPieces();
        ulong bishopSelected = Bitboards.BishopPinLineLookup(board.KingPositions[side], board.bitboards[1 - side]) & board.AllPieces();
        
        throw new NotImplementedException();
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
    
    public static Move[] FilterChecks(Span<Move> moves, Board board)
    {
        List<Move> MoveList = moves.ToArray().ToList();

        for (int i = moves.Length - 1; i >= 0; i--)
        {
            Board moveBoard = new(board, MoveList[i].PermaChange);
            moveBoard.MakeMove(MoveList[i]);
            // if the king of the moving side is in check after the move, the move is illegal
            if (Attacked(moveBoard.KingPositions[1-moveBoard.side], moveBoard, moveBoard.side))
                MoveList.RemoveAt(i);
        }
        
        return MoveList.ToArray();
    }
}