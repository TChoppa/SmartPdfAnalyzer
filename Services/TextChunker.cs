public static class TextChunker
{
    public static List<string> Chunk(string text, int chunkSize = 800)
    {
        var list = new List<string>();

        for (int i = 0; i < text.Length; i += chunkSize)
        {
            list.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        }

        return list;
    }
}