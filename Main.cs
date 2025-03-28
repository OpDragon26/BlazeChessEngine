using Blaze;

Match match = new Match(new Board(Presets.StartingBoard));

match.board.MakeMove(new Move((4,1),(4,3)));

Console.WriteLine(Convert.ToString(match.board.bitboards[0]));
match.Print(0);