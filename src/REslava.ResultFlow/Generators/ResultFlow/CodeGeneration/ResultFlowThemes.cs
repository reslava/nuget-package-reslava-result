namespace REslava.ResultFlow.Generators.ResultFlow.CodeGeneration;

internal static class ResultFlowThemes
{
    internal const string MermaidInit     = "%%{init: {'theme': 'base', 'flowchart': {'scale': 1}}}%%";
    internal const string MermaidInitDark = "%%{init: {'theme': 'base', 'flowchart': {'scale': 1}, 'themeVariables': {'primaryTextColor': '#fff', 'titleColor': '#fff', 'edgeLabelBackground': '#2a2a2a'}}}%%";

    internal const string Light = @"classDef entry      fill:none,stroke:none
classDef operation  fill:#faf0e3,color:#b45f2b
classDef bind       fill:#e3f0e8,color:#2f7a5c,stroke:#1a5c3c,stroke-width:3px
classDef map        fill:#e3f0e8,color:#2f7a5c
classDef transform  fill:#e3f0e8,color:#2f7a5c
classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
classDef sideeffect fill:#fff4d9,color:#b8882c
classDef terminal   fill:#f2e3f5,color:#8a4f9e
classDef success    fill:#e8f4f0,color:#1c7e6f
classDef failure    fill:#f8e3e3,color:#b13e3e
classDef note       fill:#f5f5f5,color:#555555,stroke:#cccccc
classDef subgraphStyle fill:#ffffde,stroke:#aa3,stroke-width:1px
linkStyle default stroke:#888,stroke-width:1.5px
classDef Layer0_Style fill:#eff4ff,color:#2b4c7e,stroke:#c0d0f0,stroke-width:1px
classDef Layer1_Style fill:#f0f8f0,color:#1e6f43,stroke:#b8d8c0,stroke-width:1px
classDef Layer2_Style fill:#eff4ff,color:#2b4c7e,stroke:#c0d0f0,stroke-width:1px
classDef Layer3_Style fill:#f0f8f0,color:#1e6f43,stroke:#b8d8c0,stroke-width:1px
classDef Layer4_Style fill:#eff4ff,color:#2b4c7e,stroke:#c0d0f0,stroke-width:1px";

    internal const string Dark = @"classDef entry      fill:none,stroke:none
classDef operation  fill:#3a2b1f,color:#f2c2a0
classDef bind       fill:#1f3a2d,color:#9fe0c0,stroke:#2d5a4a,stroke-width:3px
classDef map        fill:#1f3a2d,color:#9fe0c0
classDef transform  fill:#1f3a2d,color:#9fe0c0
classDef gatekeeper fill:#1f263a,color:#b8c8f2
classDef sideeffect fill:#3a331f,color:#f2e0a0
classDef terminal   fill:#33203a,color:#d6b0f2
classDef success    fill:#1f3a36,color:#9fe0d6
classDef failure    fill:#3a1f1f,color:#f2b8b8
classDef note       fill:#2a2a2a,color:#aaaaaa,stroke:#555555
classDef subgraphStyle fill:#252520,stroke:#665,stroke-width:1px
linkStyle default stroke:#666,stroke-width:1.5px
classDef Layer0_Style fill:#1a2535,color:#8aa8d0,stroke:#2a3a55,stroke-width:1px
classDef Layer1_Style fill:#1a2a20,color:#70b890,stroke:#2a3a30,stroke-width:1px
classDef Layer2_Style fill:#1a2535,color:#8aa8d0,stroke:#2a3a55,stroke-width:1px
classDef Layer3_Style fill:#1a2a20,color:#70b890,stroke:#2a3a30,stroke-width:1px
classDef Layer4_Style fill:#1a2535,color:#8aa8d0,stroke:#2a3a55,stroke-width:1px";
}
