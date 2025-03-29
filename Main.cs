using Blaze;

Match match = new Match(new Board(Presets.StartingBoard), true);

match.board.MakeMove(new Move("e1g1", match.board));

Console.WriteLine(Convert.ToString(match.board.bitboards[0]));
//match.Print(0);
Bitboards.Init();
//match.Print(0);
match.PrintBitboard(Bitboards.BishopBlockers[1,1][81], 0);
match.PrintBitboard(Bitboards.BishopMoves[1,1][81].captures, 0);
//PrintBitboard(Bitboards.GetSquare(1,1), 1);