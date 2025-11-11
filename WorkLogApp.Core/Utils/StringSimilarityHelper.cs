namespace WorkLogApp.Core.Utils
{
    public static class StringSimilarityHelper
    {
        public static double Similarity(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return 0.0;
            return a == b ? 1.0 : 0.0;
        }
    }
}