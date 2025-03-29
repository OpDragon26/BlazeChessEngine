using Blaze;

Match match = new Match(new Board(Presets.StartingBoard), true);

match.board.MakeMove(new Move("e1g1", match.board));

//match.Print(0);
Bitboards.Init();
//match.Print(0);
//match.PrintBitboard(Bitboards.BishopBlockers[1,1][81], 0);
match.PrintBitboard(Bitboards.BishopMoves[1,1][81].captures, 0);
match.PrintBitboard(Bitboards.BishopLookup[1,1][(Bitboards.BishopBlockers[1, 1][81] * Bitboards.BishopMagicNumbers[1, 1].magicNumber) >> Bitboards.BishopMagicNumbers[1, 1].push].captures, 0);
//PrintBitboard(Bitboards.GetSquare(1,1), 1);

/*
Console.WriteLine("Rooks");
for (int i = 0; i < 8; i++) // for every row in the array
{
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.RookMagicNumbers[i, j] + ",");
    }
    Console.Write("\n");
}

Console.WriteLine("Bishops");
for (int i = 0; i < 8; i++) // for every row in the array
{
    for (int j = 0; j < 8; j++) // for every item in row
    {
        Console.Write(Bitboards.BishopMagicNumbers[i, j] + ",");
    }
    Console.Write("\n");
}
*/

