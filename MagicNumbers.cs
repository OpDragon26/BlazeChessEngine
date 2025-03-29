namespace Blaze;

public static class MagicNumbers
{
    private static (ulong magicNumber, int highest) GenerateMagicNumber(ulong[] combinations, int expectedPush) // generate magic number with the expected push
    {
        while (true) // until a number is found
        {
            ulong magicNumber = RandomUlong();
            
            ulong[] results = new ulong[combinations.Length];
            for (int i = 0; i < combinations.Length; i++)
            {
                results[i] = (combinations[i] * magicNumber) >> expectedPush;
            }
            
            // if results contains no duplicates, the number is *magic*
            if (!results.GroupBy(x => x).Any(g => g.Count() > 1))
                return (magicNumber, (int)results.Max());
        }
    }
    
    // generate a magic number with a push of at least 48
    // reused code from my previous attempt
    public static (ulong magicNumber, int push, int highest) GenerateMagicNumber(ulong[] combinations)
    {
        ulong magicNumber;
        int push = 0;
        
        ulong[] results = new ulong[combinations.Length];
        
        while (true) // keep generating magic numbers until one is found
        {
            // generate random ulong
            ulong candidateNumber = RandomUlong();
            
            // multiply every combination with the magic number and push them right by 48, only leaving the leftmost 16 bits
            for (int i = 0; i < combinations.Length; i++)
            {
                results[i] = (combinations[i] * candidateNumber) >> 48;
            }

            // if the result array contains duplicates, the number isn't magic, so don't bother checking it for further pushes
            if (!results.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                ulong[] temp = (ulong[])results.Clone();
                
                for (int i = 0; i < 16; i++)
                {
                    // push further right by a certain amount, and check for duplicates again
                    for (int j = 0; j < temp.Length; j++)
                    { 
                        temp[j] >>= 2;
                    }
                    
                    // if there are no duplicates in temp
                    if (!temp.GroupBy(x => x).Any(g => g.Count() > 1))
                    {
                        
                        for (int j = 0; j < results.Length; j++) 
                        { 
                            results[j] >>= 1;
                        }

                        push++;
                    }
                    else break;
                        
                }
                magicNumber = candidateNumber;
                break;
            }
        }
        
        return (magicNumber, push + 48, (int)results.Max());
    }
    
    private static readonly List<ulong> UsedNumbers = new();
    private static Random RandGen = new();
    
    private static ulong RandomUlong()
    {
        while (true)
        {
            var buffer = new byte[sizeof(ulong)];
            RandGen.NextBytes(buffer);
            ulong result = BitConverter.ToUInt64(buffer, 0);
            if (!UsedNumbers.Contains(result))
            {
                UsedNumbers.Add(result);
                return result;
            }
        }
    }
}