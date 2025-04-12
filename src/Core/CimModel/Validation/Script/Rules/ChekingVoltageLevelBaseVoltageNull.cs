using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Validation;

namespace CimBios.Core.CimModel.Validation.Script.Rules
{
    public class ChekingVoltageLevelBaseVoltageNull
    {
        public string Code { get; private set; } = string.Empty;

        public ChekingVoltageLevelBaseVoltageNull()
        {
            Code = "var mo = ModelObject as VoltageLevel;\r\n" +
                    "if (mo?.BaseVoltage == null)\r\n" +
                    "{\r\n" +
                        "ValidationResults = new List<IValidationResult>()\r\n" +
                        "{\r\n" +
                            "new ModelObjectValidationResult(\r\n" +
                                "ValidationResultKind.Fail,\r\n" +
                                "\"BaseVoltage is null\",\r\n" +
                                "ModelObject\r\n)\r\n" +
                        "};\r\n" +
                    "}\r\n" +
                    "else\r\n" +
                    "{\r\n" +
                        "ValidationResults = new List<IValidationResult>()\r\n" +
                        "{\r\n" +
                            "new PassValidationResult(\r\n" +
                                "\"BaseVoltage is not null\"\r\n)\r\n" +
                        "};\r\n" +
                    "}";
        }
    }
}
