namespace Blaze;

public class EmbeddedMatch(Board board, int depth, bool dynamicDepth = true)
{
    private Match internalMatch = new Match(board, depth, dynamicDepth);
    private Thread search;
    
    
}