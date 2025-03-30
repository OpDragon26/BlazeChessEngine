﻿using Blaze;

Match match = new Match(new Board(Presets.StartingBoard), true);

match.board.MakeMove(new Move("e1g1", match.board));

//match.Print(0);
Bitboards.Init();
//match.Print(0);
//match.PrintBitboard(Bitboards.BishopBlockers[1,1][81], 0);
//match.PrintBitboard(Bitboards.BishopMoves[1,1][81].captures, 0);
//match.PrintBitboard(Bitboards.BishopLookupMoves((1,1),Bitboards.BishopBlockers[1,1][81]).captures, 0);
//PrintBitboard(Bitboards.GetSquare(1,1), 1);

/*
Console.WriteLine("Rooks");
for (int i = 0; i < 8; i++) // for every row in the array
{
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.RookCaptureMagicNumbers[i, j] + ",");
    }
    Console.Write("\n");
}

Console.WriteLine("Bishops");
for (int i = 0; i < 8; i++) // for every row in the array
{
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.BishopCaptureMagicNumbers[i, j] + ",");
    }
    Console.Write("\n");
}
*/
match.PrintBitboard(Bitboards.RookBlockers[0,0][150], 1);
match.PrintBitboard(Bitboards.RookLookupMoves((0,0), Bitboards.RookBlockers[0,0][150]).captures, 1);
Console.WriteLine(Bitboards.RookLookupMoves((0,0), Bitboards.RookBlockers[0,0][150]).moves.Length);
//Console.WriteLine(Bitboards.RookMoves[7,7][150].moves.Length);
//match.PrintBitboard(Bitboards.RookMoves[7,7][150].captures, 1);
//match.PrintBitboard(72340172838011324, 1);
//Console.WriteLine(Bitboards.GetMoves(72340172838011324, (0,7), Pieces.WhiteRook, true).moves.Length);

foreach (ulong bitboard in Bitboards.RookCaptureCombinations[0, 0])
{
    //match.PrintBitboard(bitboard, 0);
}