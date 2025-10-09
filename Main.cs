using Blaze;

Bitboards.Init();
//Book.Init(Books.Test);

//Match.PrintBitboard(0xf0f0f0f0f0f000, 0);
new Match(new Board(Presets.StartingBoard), Blaze.Type.Autoplay, Side.White, depth: 6, debug: false, dynamicDepth: true).Play();

/*
Board test = new Board("3r2k1/Bp3pbp/4b1p1/1B2p3/4P3/1PN3nP/1PP3P1/4K2R w K - 1 19");
Search.SearchResult result = Search.BestMove(test, 6, false, 0);
Perft.BreakdownEval(test, 6);
Console.WriteLine(result.move.Notate(test));
Console.WriteLine(result.eval);
test.MakeMove(result.move);
Match.PrintBoard(test, 0, Match.IHateWindows);
*/

//Perft.TestGameSpeed(15, 6, false);
//Perft.Run(5, "start");

//Console.WriteLine(Bitboards.GetValidCombinations(32, 8).Count());

//Perft.BreakdownWithExamine(new Board("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"), 3, [0]);

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