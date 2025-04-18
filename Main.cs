using Blaze;
using Type = Blaze.Type;

string Path = Directory.GetCurrentDirectory()[..^16] + "Book/";

Hasher.Init();
Bitboards.Init();
Book.Init(Path + "book.txt");

int side;
int depth;
Type type;

Console.WriteLine("Start a game with the following format: 'new w 6 Standard':");
while (true)
{
    string? line = Console.ReadLine();
    if (line == null) continue;
    string[] lines = line.Split(' ');
    if (lines.Length != 4) continue;
    if (lines[0] != "new") continue;
    
    if (lines[1] == "w")
        side = 0;
    else if (lines[1] == "b")
        side = 1;
    else
        continue;

    try
    {
        depth = int.Parse(lines[2]);
    }
    catch
    {
        continue;
    }

    try
    {
        type = lines[3] switch
        {
            "Standard" => Type.Standard,
            "Analysis" => Type.Analysis,
            "Random" => Type.Random,
            "Self" => Type.Self,
            "Autoplay" => Type.Autoplay,
            _ => throw new Exception()
        };
    }
    catch
    {
        continue;
    }

    break;
}

Match match = new Match(new Board(Presets.StartingBoard), type, side: side, depth: depth, debug: false, moves: 1000);
//Parser.PrintGame(Parser.ParseUCI("e2e4 d7d5 e4d5 g8f6 b1c3 f6d5 c3d5 d8d5 d2d4 b8c6 g1f3 c8g4 f1e2 e8c8\n"),0);
//Match match = new Match(new Board("3r1k2/8/8/8/8/8/8/3RK3 w - - 0 1"), Type.Analysis, depth: 6, debug: false, moves: 1000);

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

/*
using (StreamWriter sw = new StreamWriter("/home/mate/Documents/C#/BlazeChessEngine/Book/result.txt"))
{
    foreach (string line in File.ReadAllLines("/home/mate/Documents/C#/BlazeChessEngine/Book/book.txt"))
    {
        sw.WriteLine(Parser.ToUCI(Parser.ParsePGN(line)));
    }
}
*/