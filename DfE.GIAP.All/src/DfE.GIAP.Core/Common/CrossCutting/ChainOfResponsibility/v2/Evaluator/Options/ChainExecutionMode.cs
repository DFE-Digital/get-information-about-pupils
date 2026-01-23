using System.ComponentModel;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Options;

public enum ChainExecutionMode
{
    [Description("First handleable handler processes the input")]
    ChainOfResponsibility,

    [Description("All handlers process the input in sequence")]
    Pipeline
}
