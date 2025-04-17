using Blaze;
using Type = Blaze.Type;

Hasher.Init();
Bitboards.Init();
Match match = new Match(new Board(Presets.StartingBoard), Type.Analysis, side: 0, depth: 6, debug: false, moves: 400);
//match.board.MakeMove(Move.Parse("e3=N", match.board));

//Match match = new Match(new Board("3r1k2/8/8/8/8/8/8/3RK3 w - - 0 1"), Type.Analysis, depth: 6, debug: false, moves: 400);
//match.Print(0);

//match.SpeedTest();
match.Play();

//Search.SearchBoard(match.board);

//Move[] moves = Search.SearchBoard(match.board);

/*
Console.WriteLine("Rook bitboards");
Console.WriteLine("{");
for (int i = 0; i < 8; i++) // for every row in the array
{
    Console.Write("{");
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.MagicLookup.RookBitboardNumbers[i, j] + ",");
    }
    Console.Write("},\n");
}
Console.WriteLine("}");

Console.WriteLine("Bishop bitboards");
Console.WriteLine("{");
for (int i = 0; i < 8; i++) // for every row in the array
{
    Console.Write("{");
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.MagicLookup.BishopBitboardNumbers[i, j] + ",");
    }
    Console.Write("},\n");
}
Console.WriteLine("}");
*/