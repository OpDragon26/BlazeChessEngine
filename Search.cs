namespace Blaze;

public static class Search
{
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
                    Span<Move> WPawnMoves = new Span<Move>(Bitboards.WhitePawnLookupMoves(pos, board.AllPieces()));
                    WPawnMoves.CopyTo(moveSpan);
                    index += WPawnMoves.Length;
                    captures = new Span<Move>(Bitboards.WhitePawnLookupCaptures(pos, board.bitboards[1]));
                    captures.CopyTo(moveSpan.Slice(index));
                    index += captures.Length;
                    
                    if (enPassant && pos.rank == 4)
                    {
                        moveSpan[index] = Bitboards.EnPassantLookup(Bitboards.GetSquare(pos) | Bitboards.GetSquare(board.enPassant));
                        index++;
                    }
                }
                else // black
                {
                    Span<Move> BPawnMoves = new Span<Move>(Bitboards.BlackPawnLookupMoves(pos, board.AllPieces()));
                    BPawnMoves.CopyTo(moveSpan);
                    index += BPawnMoves.Length;                                                                 
                    captures = new Span<Move>(Bitboards.BlackPawnLookupCaptures(pos, board.bitboards[0]));
                    captures.CopyTo(moveSpan.Slice(index));
                    index += captures.Length;
                    
                    if (enPassant && pos.rank == 3)
                    {
                        moveSpan[index] = Bitboards.EnPassantLookup(Bitboards.GetSquare(pos) | Bitboards.GetSquare(board.enPassant));
                        index++;
                    }
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
                captures = new Span<Move>(Bitboards.RookLookupCaptures(pos, rMoves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteBishop:
                (Move[] moves, ulong captures) bMoves = Bitboards.BishopLookupMoves(pos, board.AllPieces());
                new Span<Move>(bMoves.moves).CopyTo(moveSpan);
                index += bMoves.moves.Length;
                
                captures = new Span<Move>(Bitboards.BishopLookupCaptures(pos, bMoves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteQueen:
                // find rook moves
                (Move[] moves, ulong captures) moves = Bitboards.RookLookupMoves(pos, board.AllPieces());
                new Span<Move>(moves.moves).CopyTo(moveSpan);
                index += moves.moves.Length;
                
                captures = new Span<Move>(Bitboards.RookLookupCaptures(pos, moves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
                
                // find bishop moves
                moves = Bitboards.BishopLookupMoves(pos, board.AllPieces());
                new Span<Move>(moves.moves).CopyTo(moveSpan.Slice(index));
                index += moves.moves.Length;
                
                captures = new Span<Move>(Bitboards.BishopLookupCaptures(pos, moves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteKnight:
                // find moves, no captures
                Span<Move> knightMoves = new Span<Move>(Bitboards.KnightLookupMoves(pos, board.AllPieces()));
                knightMoves.CopyTo(moveSpan);
                index += knightMoves.Length;
                
                // find only captures
                captures = new Span<Move>(Bitboards.KnightLookupCaptures(pos, board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteKing:
                Span<Move> kingMoves = new Span<Move>(Bitboards.KingLookupMoves(pos, board.AllPieces()));
                kingMoves.CopyTo(moveSpan);
                index += kingMoves.Length;

                captures = new Span<Move>(Bitboards.KingLookupCaptures(pos, board.bitboards[1]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
        }

        return index;
    }
}