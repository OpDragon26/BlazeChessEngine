using Blaze;
using Type = Blaze.Type;

Hasher.Init();
Bitboards.Init();
Match match = new Match(new Board(Presets.StartingBoard), Type.Self, depth: 6, debug: false, moves: 400);
//Match match = new Match(new Board("3r1k2/8/8/8/8/8/8/3RK3 w - - 0 1"), Type.Analysis, depth: 6, debug: false, moves: 400);
match.Print(0);

//Match.PrintBitboard(Bitboards.GetMoveBitboard(Bitboards.RookLookupMoves((3,0), match.board.AllPieces()).moves),0);

//match.SpeedTest();
match.Play();

//Search.SearchBoard(match.board);

//Move[] moves = Search.SearchBoard(match.board);

/*
Console.WriteLine("Rook captures");
Console.WriteLine("{");
for (int i = 0; i < 8; i++) // for every row in the array
{
    Console.Write("{");
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.MagicLookup.RookCapture[i, j] + ",");
    }
    Console.Write("},\n");
}
Console.WriteLine("}");

Console.WriteLine("Bishop captures");
Console.WriteLine("{");
for (int i = 0; i < 8; i++) // for every row in the array
{
    Console.Write("{");
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.MagicLookup.BishopCapture[i, j] + ",");
    }
    Console.Write("},\n");
}
Console.WriteLine("}");
*/