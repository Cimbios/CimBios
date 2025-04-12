using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

namespace CimBios.Core.CimModel.Validation.Script;

public class ScriptValidationRule : ValidationRuleBase
{
    private Script? _script;

    public string Code { get; set; } = string.Empty;

    public override IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject)
    {
        _script?.ExecuteNext(Code, modelObject);

        if (_script == null || _script.Globals == null)
        {
            throw new NullReferenceException();
        }

        return _script.Globals.ValidationResults;
    }

    public override bool NeedExecute(IReadOnlyModelObject modelObject) => true;

    public ScriptValidationRule(Script script) : base()
    {
        _script = script;
    }
}
