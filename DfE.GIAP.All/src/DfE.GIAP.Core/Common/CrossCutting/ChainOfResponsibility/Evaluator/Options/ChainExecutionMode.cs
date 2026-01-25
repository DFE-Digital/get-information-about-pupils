using System.ComponentModel;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator.Options;

public enum ChainExecutionMode
{
    [Description("First handleable handler processes the input")]
    ChainOfResponsibility
}
