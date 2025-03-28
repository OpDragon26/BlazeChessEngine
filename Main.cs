using Blaze;

Match match = new Match(new Board(Presets.StartingBoard));

match.board.MakeMove(new Move((4,0),(6,0),type: 0b0010));

match.Print(0);