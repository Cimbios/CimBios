using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoSchemaSerializer : ICimSchemaSerializer
{
    public const string BaseSchemaUri = "http://cim.bios/schemas/auto#";

    public Dictionary<string, Uri> Namespaces => _Namespaces;

    public Dictionary<Uri, ICimMetaResource> Deserialize()
    {
        _Namespaces.Clear();
        _ObjectsCache.Clear();

        var objectsModel = _Reader.ReadAll().ToArray();
        ForwardReaderNamespaces();
        BuildInternalDatatypes();

        CreateSchemaEntitiesFromModel(objectsModel);

        return _ObjectsCache;
    }

    public void Load(TextReader reader)
    {
        _Reader.AddNamespace("base", new(BaseSchemaUri));
        _Reader.Load(reader);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nodes"></param>
    private void CreateSchemaEntitiesFromModel(IEnumerable<RdfNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (_ObjectsCache.ContainsKey(node.TypeIdentifier))
            {
                continue;
            }

            AddClass(node.TypeIdentifier, false, false);

            HandleProperties(node);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="property"></param>
    private void HandleProperties(RdfNode node)
    {
        foreach (var property in node.Triples)
        {
            if (TryGetClassUriFromProperty(property.Predicate, 
                out var classUri) == false)
            {
                return;
            }

            if (_ObjectsCache.ContainsKey(classUri) == false)
            {
                AddClass(classUri, false, false);
            }

            if (RdfUtils.RdfUriEquals(classUri, node.TypeIdentifier) == false)
            {
                AddAncestorToClass(node.TypeIdentifier, classUri);
            }

            var propertyKind = CimMetaPropertyKind.NonStandard;
            if (property.Object is Uri)
            {
                propertyKind = CimMetaPropertyKind.Assoc1ToM;
            }
            else
            {
                propertyKind = CimMetaPropertyKind.Attribute;
            }

            AddProperty(property.Predicate, 
                _ObjectsCache[classUri] as CimAutoClass,
                propertyKind);
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeIdentifier"></param>
    /// <param name="isEnum"></param>
    /// <param name="IsCompound"></param>
    /// <returns></returns>
    private bool AddClass(Uri typeIdentifier, bool isEnum, bool IsCompound)
    {
        if (_ObjectsCache.ContainsKey(typeIdentifier))
        {
            return false;
        }

        if (RdfUtils.TryGetEscapedIdentifier(typeIdentifier, 
            out var shortName) == false)
        {
            shortName = typeIdentifier.AbsoluteUri;
        }

        var autoClass = new CimAutoClass()
        {
            BaseUri = typeIdentifier,
            ShortName = shortName,
            Description = string.Empty,
            IsEnum = false,
            IsCompound = false
        };

        _ObjectsCache.Add(autoClass.BaseUri, autoClass);

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyUri"></param>
    /// <param name="ownerClass"></param>
    /// <returns></returns>
    public bool AddProperty(Uri propertyUri, CimAutoClass? ownerClass,
        CimMetaPropertyKind propertyKind)
    {
        if (_ObjectsCache.ContainsKey(propertyUri))
        {
            return false;
        }

        if (RdfUtils.TryGetEscapedIdentifier(propertyUri, 
            out var shortName) == false)
        {
            shortName = propertyUri.AbsoluteUri;
        }
        else
        {
            shortName = shortName[(shortName.IndexOf('.') + 1) ..];
        }

        var autoProperty = new CimAutoProperty()
        {
            BaseUri = propertyUri,
            ShortName = shortName,
            Description = string.Empty,
            OwnerClass = ownerClass,
            PropertyKind = propertyKind,
            PropertyDatatype = _ObjectsCache[new("http://www.w3.org/2001/XMLSchema#string")] as CimAutoClass
        };

        _ObjectsCache.Add(propertyUri, autoProperty);

        return true;
    }

    /// <summary>
    /// Add ancestor inheritance link to class.
    /// </summary>
    /// <param name="classUri">Source class URI.</param>
    /// <param name="ancestorUri">Ancestor class URI.</param>
    /// <returns>True if link was been created.</returns>
    private bool AddAncestorToClass(Uri classUri, Uri ancestorUri)
    {
        var thisClass = _ObjectsCache[classUri] 
            as CimAutoClass;

        var ancestorClass = _ObjectsCache[ancestorUri]  
            as CimAutoClass;

        if (thisClass == null || ancestorClass == null)
        {
            return false;
        }
        
        thisClass.AddAncestor(ancestorClass);

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyUri"></param>
    /// <param name="classUri"></param>
    /// <returns></returns>
    private bool TryGetClassUriFromProperty(Uri propertyUri, out Uri classUri)
    {
        classUri = propertyUri;

        if (RdfUtils.TryGetEscapedIdentifier(propertyUri, 
            out var propertyId) == false)
        {
            return false;
        }

        var namespaceUri = propertyUri.AbsoluteUri
            .Replace(propertyId, "");

        var classId = propertyId[..propertyId.IndexOf('.')];

        classUri = new(namespaceUri + classId);

        return true;
    }

    /// <summary>
    /// Move namespaces from reader doc.
    /// </summary>
    private void ForwardReaderNamespaces()
    {
        foreach (var item in _Reader.Namespaces)
        {
            _Namespaces.Add(item.Key, item.Value);
        }
    }

    /// <summary>
    /// Build internal schema datatypes.
    /// </summary>
    private void BuildInternalDatatypes()
    {
        foreach (var typeUri in XmlDatatypesMapping.UriSystemTypes.Keys)
        {
            var datatype = XmlDatatypesMapping.UriSystemTypes[typeUri];

            var uri = new Uri(typeUri);
            RdfUtils.TryGetEscapedIdentifier(uri, out var label);
            var metaDatatype = new CimAutoDatatype()
            {
                BaseUri = uri,
                SystemType = datatype,
                ShortName = label,
                Description = "Build-in xsd datatype."
            };

            _ObjectsCache.Add(metaDatatype.BaseUri, metaDatatype);
        }
    }

    private readonly RdfXmlReader _Reader = new();

    private readonly Dictionary<Uri, ICimMetaResource> _ObjectsCache 
        = new(new RdfUriComparer());

    private Dictionary <string, Uri> _Namespaces = [];
}
