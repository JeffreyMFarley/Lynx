using System;

namespace Lynx.Models
{
    [Flags]
    public enum ConnectivityClassificationType
    {
        Isolated = 0,
        HasInput = 1,
        HasOutput = 2,
        MultipleInput = 4,
        MultipleOutput = 8,

        Sink = HasInput,
        Source = HasOutput,
        Through = (HasInput | HasOutput),
        Terminal = (HasInput | MultipleInput),
        Consolidate = (HasInput | MultipleInput | HasOutput),
        Producer = (HasOutput | MultipleOutput),
        Distribute = (HasInput | HasOutput | MultipleOutput),
        Nexus = (HasInput | MultipleInput | HasOutput | MultipleOutput),
    }
}
