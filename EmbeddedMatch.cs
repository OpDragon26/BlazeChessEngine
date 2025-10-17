namespace Blaze;

public class EmbeddedMatch(Board board, int depth, bool dynamicDepth = true) : Match(board, depth, dynamicDepth)
{
    private bool complete = true;
    PGNNode last = new PGNNode {board = board};

    public void StartSearch()
    {
        Thread t = new Thread(() =>
        {
            complete = false;
            last = BotMove();
            complete = true;
        });
        t.Start();
    }

    public bool Poll(out PGNNode result)
    {
        result = last;
        return complete;
    }

    public PGNNode WaitMove()
    {
        while (!complete)
            Thread.Sleep(10);
        return last;
    }
}