using Blaze;

Match match = new Match(new Board(Presets.StartingBoard), true);

match.board.MakeMove(new Move("e1g1", match.board));

Console.WriteLine(Convert.ToString(match.board.bitboards[0]));
//match.Print(0);
Bitboards.Init();
match.Print(0);
//match.PrintBitboard(Bitboards.RookBlockers[0,0][0], 1);
