using Blaze;
using Type = Blaze.Type;

Bitboards.Init();

//Hasher.Init();
//Book.Init(Books.Test);

//new Match(new Board("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0"), Type.Analysis, side: 0, depth: 6, debug: false, dynamicDepth: false).Play();

//Search.SearchBoard(new(Presets.StartingBoard));
//new Match(new Board("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"), Type.Analysis).Play();

Perft.Run(5, "kiwipete");


Board kiwipete = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0");
//kiwipete.MakeMove(new Move("a1b1", kiwipete));
//Board plusOne = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/1R2K2R b Kkq - 1 1");

Perft.Breakdown(new Board(kiwipete), 3);
//Perft.Breakdown(new Board(plusOne), 2);


//plusOne.CompareTo(kiwipete);


//Parser.PrintGame(Parser.ParseUCI("e2e4 d7d5 e4d5 g8f6 b1c3 f6d5 c3d5 d8d5 d2d4 b8c6 g1f3 c8g4 f1e2 e8c8\n"),0);
//Perft.AnalyzeBoard(new("8/8/8/1KPpP1r1/7k/8/8/8 w - d6 0 3"));
//match.Print(0);


/*
Console.WriteLine("Block move lookup");
Console.WriteLine("{");
for (int i = 0; i < 8; i++) // for every row in the array
{
    Console.Write("{");
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.MagicLookup.BlockMoveNumbers[i, j] + ",");
    }
    Console.Write("},\n");
}
Console.WriteLine("}");
*/
/*
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

/*
using (StreamWriter sw = new StreamWriter("/home/mate/Documents/C#/BlazeChessEngine/Book/result.txt"))
{
    foreach (string line in File.ReadAllLines("/home/mate/Documents/C#/BlazeChessEngine/Book/book.txt"))
    {
        sw.WriteLine(Parser.ToUCI(Parser.ParsePGN(line)));
    }
}
*/