#if NETSTANDARD2_0
// Required for records and init-only setters on netstandard2.0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
