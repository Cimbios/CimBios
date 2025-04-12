using CimBios.Core.CimModel.CimDatatypeLib;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CimBios.Core.CimModel.Validation.Script
{
    public class Script
    {
        private ScriptState? _scriptState;

        public ScriptGlobals? Globals { get; private set; }

        public static async Task<Script> Create(
            IEnumerable<Assembly> references,
            IEnumerable<string> usings)
        {
            var options = ScriptOptions.Default.
                WithReferences(references).
                WithImports(usings);

            var globals = new ScriptGlobals();

            var state = await CSharpScript.RunAsync("", options, globals);

            return new Script() { _scriptState = state, Globals = globals };
        }

        public void ExecuteNext(string code, IReadOnlyModelObject modelObject)
        {
            if (_scriptState == null || Globals == null)
            {
                throw new NullReferenceException();
            }

            Globals.ModelObject = modelObject;

            _scriptState.ContinueWithAsync(code);
        }
    }
}
