﻿using Blaze;

Bitboards.Init();
Match match = new Match(new Board(Presets.StartingBoard), true);

match.board.MakeMove(new Move("e2e4", match.board));
Console.WriteLine(Search.Attacked((7,4), match.board, 0));

match.Print(0);

/*
Move[] moves = Search.SearchBoard(new Board(match.board));
match.board.MakeMove(moves[0]);

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