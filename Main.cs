using Blaze;
using Type = Blaze.Type;

Hasher.Init();
Bitboards.Init();
Match match = new Match(new Board(Presets.StartingBoard), Type.Standard);

//match.SpeedTest();
match.Play();

//Move[] moves = Search.SearchBoard(match.board);

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