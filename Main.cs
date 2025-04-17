using Blaze;
using Type = Blaze.Type;

Hasher.Init();
Bitboards.Init();
Book.Init("/home/mate/Documents/C#/BlazeChessEngine/Book/book.txt");
//Match match = new Match(new Board(Presets.StartingBoard), Type.Analysis, side: 0, depth: 6, debug: false, moves: 400);
//match.board.MakeMove(Move.Parse("e3=N", match.board));
//Parser.PrintGame(Parser.ParsePGN("1. e4 e6 2. d4 d5 3. Nd2 c5 4. Ngf3 cxd4 5. Nxd4 Nf6 6. Bb5+ Bd7 7. Bxd7+ Qxd7 8. exd5 Qxd5 9. N2f3 Nc6\n"),0);
//Match match = new Match(new Board("3r1k2/8/8/8/8/8/8/3RK3 w - - 0 1"), Type.Analysis, depth: 6, debug: false, moves: 400);

//match.Print(0);


//match.SpeedTest();
//match.Play();

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