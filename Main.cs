using Blaze;

Match match = new Match(new Board(Presets.StartingBoard), true);

match.board.MakeMove(new Move("e1g1", match.board));

Console.WriteLine(Convert.ToString(match.board.bitboards[0]));
match.Print(0);