using Blaze;

Match match = new Match(new Board(Presets.StartingBoard), true);


Bitboards.Init();
//match.SpeedTest();
/*
match.board.MakeMove(new Move("d2d4", match.board));

Move[] moves = Search.SearchBoard(match.board);
match.board.MakeMove(moves[0]);
//Console.WriteLine(moves[0].Type);

match.Print(0);
//Match.PrintBitboard(Bitboards.EnPassantMasks[2], 0);

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