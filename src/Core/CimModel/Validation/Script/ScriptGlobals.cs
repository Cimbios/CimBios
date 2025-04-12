using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.Validation.Script
{
    public class ScriptGlobals
    {
        public IReadOnlyModelObject? ModelObject;

        public IEnumerable<IValidationResult> ValidationResults = [];
    }
}
