namespace REslava.Result.Flow.Generators.ResultFlow
{
    /// <summary>
    /// FNV-1a 32-bit deterministic hash — produces an 8-character lowercase hex string.
    /// Safe to embed in C# identifiers and Mermaid node IDs.
    /// Deterministic across platforms and .NET runtimes (does not use GetHashCode).
    /// </summary>
    internal static class ShortHash
    {
        private const uint FnvPrime  = 16777619u;
        private const uint FnvOffset = 2166136261u;

        public static string Compute(params string[] parts)
        {
            var hash = FnvOffset;
            foreach (var part in parts)
            {
                foreach (var c in part)
                {
                    hash ^= (byte)(c & 0xFF);
                    hash *= FnvPrime;
                    hash ^= (byte)((c >> 8) & 0xFF);
                    hash *= FnvPrime;
                }
                // Part separator — prevents "ab"+"c" colliding with "a"+"bc"
                hash ^= 0x01;
                hash *= FnvPrime;
            }
            return hash.ToString("x8");
        }
    }
}
