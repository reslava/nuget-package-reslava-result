namespace REslava.ResultFlow.Generators.ResultFlow.Models
{
    internal enum NodeKind
    {
        Gatekeeper,        // Ensure, EnsureAsync · Filter (LanguageExt)
        TransformWithRisk, // Bind, BindAsync · Then, ThenAsync (ErrorOr)
        PureTransform,     // Map, MapAsync
        SideEffectSuccess, // Tap, TapAsync · Do, DoAsync (LanguageExt)
        SideEffectFailure, // TapOnFailure, TapOnFailureAsync · DoLeft, DoLeftAsync (LanguageExt)
        SideEffectBoth,    // TapBoth
        Terminal,          // Match, MatchAsync · Switch, SwitchAsync (ErrorOr/OneOf)
        Invisible,         // WithSuccess, WithSuccessAsync — traversed, not rendered
        Unknown,           // unrecognised method — render as generic "Operation" node
    }
}
