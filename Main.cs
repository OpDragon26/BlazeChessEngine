using Blaze;

Match match = new Match(new Board(Presets.StartingBoard));

match.board.MakeMove(new Move {source = (4,1), destination = (4, 3)});

match.Print(false);