using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class CimSchemaEntityNodeModel : TreeViewNodeModel
{
    public ICimMetaResource CimSchemaEntity { get; }
    
    public CimSchemaEntityNodeModel(ICimMetaResource cimSchemaEntity)
    {
        CimSchemaEntity = cimSchemaEntity;
        Description = $"{cimSchemaEntity.BaseUri.AbsoluteUri}\n{cimSchemaEntity.Description}";
    }
}
