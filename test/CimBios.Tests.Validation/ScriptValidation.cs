using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.Validation;
using CimBios.Core.CimModel.Validation.Script.Rules;
using CimBios.Core.CimModel.Validation.Script;
using CimBios.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.CimDatatypeLib.OID;

namespace CimBios.Tests.Validation
{
    public class ScriptValidation
    {
        [Fact]
        public async void BaseVoltageIsNull()
        {
            var references = AssemblyInfo.References;
            var usings = AssemblyInfo.Usings;

            var script = await Script.Create(references, usings);

            var voltageLevel = CreateVoltageLevelWithoutBaseVoltage();

            var valid = new ScriptValidationRule(script);
            valid.Code = new ChekingVoltageLevelBaseVoltageNull().Code;

            IEnumerable<IValidationResult> validationResults = [];
            try
            {
                validationResults = valid.Execute(voltageLevel);
            }
            catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
            {
                Assert.Fail(string.Join("", ex.Diagnostics));
            }

            if (validationResults?.FirstOrDefault()?.ResultType == 
                ValidationResultKind.Fail)
            {
                Assert.True(
                    validationResults?.FirstOrDefault()?.ResultType ==
                    ValidationResultKind.Fail
                    );
            }
            else
            {
                Assert.Fail();
            }
        }

        [Fact]
        public async void BaseVoltageIsNotNull()
        {
            var references = AssemblyInfo.References;
            var usings = AssemblyInfo.Usings;

            var script = await Script.Create(references, usings);

            var voltageLevel = CreateVoltageLevelWithBaseVoltage();

            var valid = new ScriptValidationRule(script);
            valid.Code = new ChekingVoltageLevelBaseVoltageNull().Code;

            IEnumerable<IValidationResult> validationResults = [];
            try
            {
                validationResults = valid.Execute(voltageLevel);
            }
            catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
            {
                Assert.Fail(string.Join("", ex.Diagnostics));
            }

            if (validationResults?.FirstOrDefault()?.ResultType ==
                ValidationResultKind.Pass)
            {
                Assert.True(
                    validationResults?.FirstOrDefault()?.ResultType ==
                    ValidationResultKind.Pass
                    );
            }
            else
            {
                Assert.Fail();
            }
        }

        private VoltageLevel CreateVoltageLevelWithBaseVoltage()
        {
            var descriptorVoltageLevel = new TextDescriptorFactory().Create(
                "http://iec.ch/TC57/CIM100#VoltageLevel");

            var descriptorBaseVoltage = new TextDescriptorFactory().Create(
                "http://iec.ch/TC57/CIM100#BaseVoltage");

            var cimDocument = ModelLoader.CreateCimModelInstance();

            var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
                descriptorVoltageLevel);

            var baseVoltage = cimDocument.Schema
                .TryGetResource<ICimMetaClass>(
                    new("http://iec.ch/TC57/CIM100#BaseVoltage"));

            if (baseVoltage == null)
            {
                throw new NullReferenceException();
            }

            voltageLevel.BaseVoltage = new BaseVoltage(
                descriptorBaseVoltage, baseVoltage);
            voltageLevel.BaseVoltage.nominalVoltage = 110;

            return voltageLevel;
        }

        private VoltageLevel CreateVoltageLevelWithoutBaseVoltage()
        {
            var descriptorVoltageLevel = new TextDescriptorFactory().Create(
                "http://iec.ch/TC57/CIM100#VoltageLevel");

            var cimDocument = ModelLoader.CreateCimModelInstance();

            var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
                descriptorVoltageLevel);

            return voltageLevel;
        }
    }
}
