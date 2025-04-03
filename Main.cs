﻿using Blaze;

Match match = new Match(new Board(Presets.StartingBoard), true);
//match.SpeedTest();

Bitboards.Init();
//match.Print(0);

match.board.MakeMove(new Move("f1d3", match.board));
match.board.MakeMove(new Move("e7e5", match.board));
match.board.MakeMove(new Move("g1f3", match.board));
match.board.MakeMove(new Move("d7d5", match.board));

Move[] moves = Search.SearchBoard(match.board);
match.board.MakeMove(moves[0]);
match.Print(0);

//Match.PrintBitboard(match.board.bitboards[0], 0);

/*
Console.WriteLine("en passant");
Console.WriteLine("{");
for (int i = 0; i < 8; i++) // for every row in the array
{
    Console.Write("{");
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.MagicLookup.WhitePawnCapture[i, j] + ",");
    }
    Console.Write("},\n");
}
Console.WriteLine("}");

Console.WriteLine("Black pawn captures");
Console.WriteLine("{");
for (int i = 0; i < 8; i++) // for every row in the array
{
    Console.Write("{");
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.MagicLookup.BlackPawnCapture[i, j] + ",");
    }
    Console.Write("},\n");
}
Console.WriteLine("}");
*/