#if NETSTANDARD2_0
// Polyfill for System.HashCode (added in .NET Core 2.1 / netstandard2.1)
// Simple FNV-1a-inspired combining — good enough for GetHashCode purposes.
namespace System
{
    internal static class HashCode
    {
        private const int Seed = unchecked((int)2166136261);
        private const int Prime = 16777619;

        private static int Mix(int hash, int value) =>
            unchecked((hash ^ value) * Prime);

        public static int Combine<T1, T2>(T1 v1, T2 v2)
        {
            int h = Seed;
            h = Mix(h, v1?.GetHashCode() ?? 0);
            h = Mix(h, v2?.GetHashCode() ?? 0);
            return h;
        }

        public static int Combine<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
        {
            int h = Seed;
            h = Mix(h, v1?.GetHashCode() ?? 0);
            h = Mix(h, v2?.GetHashCode() ?? 0);
            h = Mix(h, v3?.GetHashCode() ?? 0);
            return h;
        }

        public static int Combine<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            int h = Seed;
            h = Mix(h, v1?.GetHashCode() ?? 0);
            h = Mix(h, v2?.GetHashCode() ?? 0);
            h = Mix(h, v3?.GetHashCode() ?? 0);
            h = Mix(h, v4?.GetHashCode() ?? 0);
            return h;
        }

        public static int Combine<T1, T2, T3, T4, T5>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
        {
            int h = Seed;
            h = Mix(h, v1?.GetHashCode() ?? 0);
            h = Mix(h, v2?.GetHashCode() ?? 0);
            h = Mix(h, v3?.GetHashCode() ?? 0);
            h = Mix(h, v4?.GetHashCode() ?? 0);
            h = Mix(h, v5?.GetHashCode() ?? 0);
            return h;
        }

        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
        {
            int h = Seed;
            h = Mix(h, v1?.GetHashCode() ?? 0);
            h = Mix(h, v2?.GetHashCode() ?? 0);
            h = Mix(h, v3?.GetHashCode() ?? 0);
            h = Mix(h, v4?.GetHashCode() ?? 0);
            h = Mix(h, v5?.GetHashCode() ?? 0);
            h = Mix(h, v6?.GetHashCode() ?? 0);
            return h;
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
        {
            int h = Seed;
            h = Mix(h, v1?.GetHashCode() ?? 0);
            h = Mix(h, v2?.GetHashCode() ?? 0);
            h = Mix(h, v3?.GetHashCode() ?? 0);
            h = Mix(h, v4?.GetHashCode() ?? 0);
            h = Mix(h, v5?.GetHashCode() ?? 0);
            h = Mix(h, v6?.GetHashCode() ?? 0);
            h = Mix(h, v7?.GetHashCode() ?? 0);
            return h;
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
        {
            int h = Seed;
            h = Mix(h, v1?.GetHashCode() ?? 0);
            h = Mix(h, v2?.GetHashCode() ?? 0);
            h = Mix(h, v3?.GetHashCode() ?? 0);
            h = Mix(h, v4?.GetHashCode() ?? 0);
            h = Mix(h, v5?.GetHashCode() ?? 0);
            h = Mix(h, v6?.GetHashCode() ?? 0);
            h = Mix(h, v7?.GetHashCode() ?? 0);
            h = Mix(h, v8?.GetHashCode() ?? 0);
            return h;
        }
    }
}
#endif
