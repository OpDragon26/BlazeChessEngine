namespace Blaze;

public static class Search
{
    // returns pseudo legal moves: abides by the rules of piece movement, but does not account for checks
    public static Move[] SearchBoard(Board board)
    {
        Move[] moveArray = new Move[219]; // max moves possible from 1 position

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
                    index += SearchPiece(board, board.GetPiece(file, rank), (file, rank), board.side, moveSpan);
                }
            }
        }

        return new Span<Move>(moveArray, 0, index).ToArray();
    }

    private static int SearchPiece(Board board, ulong piece, (int file, int rank) pos, int side, Span<Move> moveSpan)
    {
        int index = 0;
        (Move[] moves, ulong captures) moves;
        Span<Move> captures;
        
        switch (piece & Pieces.TypeMask)
        {
            case Pieces.WhitePawn:
                if (board.side == 0) // white
                {
                    
                }
                else // black
                {
                    
                }
            break;
            
            case Pieces.WhiteRook:
                // magic lookup moves
                // no captures
                moves = Bitboards.RookLookupMoves(pos, board.AllPieces());
                new Span<Move>(moves.moves).CopyTo(moveSpan);
                index += moves.moves.Length;

                // magic lookup of only captures
                // form a slice out of the span to ensure that none of the already added moves are overwritten
                captures = new Span<Move>(Bitboards.RookLookupCaptures(pos, moves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteBishop:
                moves = Bitboards.BishopLookupMoves(pos, board.AllPieces());
                new Span<Move>(moves.moves).CopyTo(moveSpan);
                index += moves.moves.Length;
                
                captures = new Span<Move>(Bitboards.BishopLookupCaptures(pos, moves.captures & board.bitboards[1-side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteQueen:
                // find rook moves
                moves = Bitboards.RookLookupMoves(pos, board.AllPieces());
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
                captures = new Span<Move>(Bitboards.KnightLookupCaptures(pos, board.bitboards[1 - side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
            
            case Pieces.WhiteKing:
                Span<Move> kingMoves = new Span<Move>(Bitboards.KingLookupMoves(pos, board.AllPieces()));
                kingMoves.CopyTo(moveSpan);
                index += kingMoves.Length;

                captures = new Span<Move>(Bitboards.KingLookupCaptures(pos, board.bitboards[1 - side]));
                captures.CopyTo(moveSpan.Slice(index));
                index += captures.Length;
            break;
        }

        return index;
    }
}