namespace Blaze;

public class EngineUCI
{
    private Board board = new Board(Presets.StartingBoard);
    private readonly int depth = 6;
    private bool book = true;
    private string? bestMove;
    private bool stopped;
    
    public void ReceiveCommand(string command)
    {
        command = command.Trim();
        string type = command.Split(' ')[0].ToLower();

        switch (type)
        {
            case "uci":
                Console.WriteLine("uciok");
            break;
            case "isready":
                Hasher.Init();
                Bitboards.Init();
                Book.Init();
                Console.WriteLine("readyok");
            break;
            case "ucinewgame":
                board = new Board(Presets.StartingBoard);
            break;
            case "position":
                ProcessPosition(command);
            break;
            case "go":
                stopped = false;
                bestMove = BestMove().GetUCI();
            break;
            case "stop":
                if (!stopped) 
                    Console.WriteLine($"bestmove {bestMove}");
                stopped = true;
            break;
        }
    }

    private void ProcessPosition(string command)
    {
        if (command.Contains("startpos"))
        {
            if (command.Contains("moves"))
            {
                PGNNode[] game = Parser.ParseUCI(GetMoves(command));
                board = game.Length == 0 ? new Board(Presets.StartingBoard) : new Board(game[^1].board);
            }
            else
                board = new Board(Presets.StartingBoard);
        }
        else if (command.Contains("fen"))
        {
            string FEN = GetFEN(command);
            if (command.Contains("moves"))
            {
                PGNNode[] game = Parser.ParseUCI(GetMoves(command), FEN);
                board = game.Length == 0 ? new Board(FEN) : new Board(game[^1].board);
            }
            else
                board = new Board(FEN);
        }
        else
            throw new Exception($"Failed to process position command: {command}");
    }

    private string GetFEN(string command)
    {
        string[] message = command.Split(' ');
        for (int i = 0; i < message.Length - 1; i++)
        {
            if (message[i].Equals("fen"))
            {
                return message[i + 1];
            }
        }
        throw new Exception("Fen not found");   
    }

    private string GetMoves(string command)
    {
        string[] message = command.Split(' ');
        List<string> moves = new List<string>();
        bool found = false;
        
        foreach (string segment in message)
        {
            if (found)
            {
                if (Move.ValidUCI(segment))
                {
                    moves.Add(segment);
                }
            }
            else
            {            
                if (segment.Equals("moves"))
                {
                    found = true;
                }
            }
        }

        return string.Join(' ', moves);
    }

    private Move BestMove()
    {
        SearchResult result = Search.BestMove(board, depth, book,-1);
        book = result.book;
        
        return result.move;
    }
}