using System.ComponentModel;
using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.CimDatatypeLib;

public class ModelObject : DynamicModelObjectBase, IModelObject
{
    public override string Uuid => _uuid;
    public override bool IsAuto => _isAuto;
    public override ICimMetaClass MetaClass => _MetaClass;

    internal ModelObject(string uuid, ICimMetaClass metaClass, 
        bool isAuto = false)
    {
        _uuid = uuid;
        _isAuto = isAuto;

        _MetaClass = metaClass;
        _PropertiesData = [];
    }

    public override bool HasProperty(string propertyName)
    {
        var metaProperty = TryGetMetaPropertyByName(propertyName);
        if (metaProperty != null)
        {
            return MetaClass.AllProperties.Contains(metaProperty);
        }

        return false;
    }

    #region AttributesLogic

    public override object? GetAttribute(ICimMetaProperty metaProperty)
    {
        return GetDataByProperty(metaProperty, CimMetaPropertyKind.Attribute);
    }

    public override object? GetAttribute(string attributeName)
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return GetAttribute(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public override T? GetAttribute<T>(ICimMetaProperty metaProperty) 
        where T: default
    {
        if (GetAttribute(metaProperty) is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    public override T? GetAttribute<T>(string attributeName) 
        where T: default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            return GetAttribute<T>(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    public override void SetAttribute<T>(ICimMetaProperty metaProperty, T? value) 
        where T: default
    {
        ValidatePropertyValueAssignition(metaProperty, 
            value, CimMetaPropertyKind.Attribute);

        if (CanChangeProperty(metaProperty) == false)
        {
            return;
        }

        if (_PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.Add(metaProperty, null);
        }

        if (_PropertiesData.ContainsKey(metaProperty))
        {
            if ((value == null && _PropertiesData[metaProperty] == null)
                || value!.Equals(_PropertiesData[metaProperty]))
            {
                return;
            }

            if (value == null)
            {
                _PropertiesData.Remove(metaProperty);

                PropertyChanged?.Invoke(this, 
                    new CimMetaPropertyChangedEventArgs(metaProperty));
            }
            else
            {
                _PropertiesData[metaProperty] = value;

                PropertyChanged?.Invoke(this, 
                    new CimMetaPropertyChangedEventArgs(metaProperty));
            }
        }
        else
        {
            throw new ArgumentException(
                $"Attribute {metaProperty.ShortName} does not exist!");
        }
    }

    public override void SetAttribute<T>(string attributeName, T? value)
        where T: default
    {
        var metaProperty = TryGetMetaPropertyByName(attributeName);
        if (metaProperty != null)
        {
            SetAttribute<T>(metaProperty, value);
        }

        throw new ArgumentException(
            $"No such meta property with name {attributeName}!");
    }

    #endregion AttributesLogic

    #region Assocs11Logic

    public override T? GetAssoc1To1<T>(ICimMetaProperty metaProperty)
        where T: default
    {
        if (GetDataByProperty(metaProperty, CimMetaPropertyKind.Assoc1To1)
            is T typedObject)
        {
            return typedObject;
        }

        return default;
    }

    public override T? GetAssoc1To1<T>(string assocName)
        where T: default
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            return GetAssoc1To1<T>(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

     public override void SetAssoc1To1(ICimMetaProperty metaProperty, 
        IModelObject? obj)
     {
        ValidatePropertyValueAssignition(metaProperty, 
            obj, CimMetaPropertyKind.Assoc1To1);

        if (CanChangeProperty(metaProperty) == false)
        {
            return;
        }

        if (obj != null && _PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.Add(metaProperty, null);
        }

        if (_PropertiesData.ContainsKey(metaProperty))
        {
            if (_PropertiesData[metaProperty] == obj)
            {
                return;
            }

            var assocObj = _PropertiesData[metaProperty] as IModelObject;

            SetAssociationWithInverse(metaProperty, assocObj, obj);

            if (_PropertiesData[metaProperty] == null)
            {
                _PropertiesData.Remove(metaProperty);
            }

            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));
        }
        else
        {
            throw new ArgumentException(
                $"Association {metaProperty.ShortName} does not exist!");
        }
     }

    public  override void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            SetAssoc1To1(metaProperty, obj);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

    #endregion Assocs11Logic

    #region Assocs1MLogic

    public override IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        var data = GetDataByProperty(metaProperty, 
            CimMetaPropertyKind.Assoc1ToM) as ICollection<IModelObject>;

        if (data != null)
        {
            return data.ToArray();
        }

        return [];
    }

    public override IModelObject[] GetAssoc1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            return GetAssoc1ToM(metaProperty);
        }

        throw new ArgumentException(
            $"No such meta property with name {assocName}!");
    }

    public override T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
    {
        return GetAssoc1ToM(metaProperty).Cast<T>().ToArray();
    }

    public override T[] GetAssoc1ToM<T>(string assocName)
    {
        return GetAssoc1ToM(assocName).Cast<T>().ToArray();
    }

    public override void AddAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        ValidatePropertyValueAssignition(metaProperty, 
            obj, CimMetaPropertyKind.Assoc1ToM);

        if (_PropertiesData.ContainsKey(metaProperty) == false)
        {
            _PropertiesData.Add(metaProperty, new HashSet<IModelObject>());
        }

        if (_PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            if (assocCollection.Contains(obj))
            {
                return;
            }

            SetAssociationWithInverse(metaProperty, null, obj);

            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public override void AddAssoc1ToM(string assocName, IModelObject obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            AddAssoc1ToM(metaProperty, obj);
        }
        else
        {
            throw new ArgumentException(
                $"No such meta property with name {assocName}!");
        }
    }

    public override void RemoveAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            if (assocCollection.Contains(obj) == false)
            {
                return;
            }
            
            SetAssociationWithInverse(metaProperty, obj, null);
                            
            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public override void RemoveAssoc1ToM(string assocName, IModelObject obj)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            RemoveAssoc1ToM(metaProperty, obj);
        }
        else
        {
            throw new ArgumentException(
                $"No such meta property with name {assocName}!");
        }
    }

    public override void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && _PropertiesData.TryGetValue(metaProperty, out var value)
            && value is ICollection<IModelObject> assocCollection)
        {
            foreach (var assocObject in assocCollection)   
            {
                SetAssociationWithInverse(metaProperty, assocObject, null);
            }     

            PropertyChanged?.Invoke(this, 
                new CimMetaPropertyChangedEventArgs(metaProperty));   
        }
        else
        {
            throw new ArgumentException(
                $"Association 1 to M {metaProperty.ShortName} does not exist!"); 
        }
    }

    public override void RemoveAllAssocs1ToM(string assocName)
    {
        var metaProperty = TryGetMetaPropertyByName(assocName);
        if (metaProperty != null)
        {
            RemoveAllAssocs1ToM(metaProperty);
        }
        else
        {
            throw new ArgumentException(
                $"No such meta property with name {assocName}!");
        }
    }

    #endregion Assocs1MLogic

    #region UtilsPrivate

    /// <summary>
    /// Validation of meta property assigning with value according schema.
    /// </summary>
    /// <param name="metaProperty">Meta property.</param>
    /// <param name="value">Value to assigning.</param>
    /// <param name="callerPropertyKind">Property kind.</param>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentException"></exception>
    private void ValidatePropertyValueAssignition(ICimMetaProperty metaProperty, 
        object? value, CimMetaPropertyKind callerPropertyKind)
    {
        if (metaProperty.PropertyKind != callerPropertyKind)
        {
            throw new Exception(
                $"Invalid {metaProperty.ShortName} property kind {callerPropertyKind}");
        }

        if (MetaClass.AllProperties.Contains(metaProperty) == false)
        {
            throw new ArgumentException(
                $"Property {metaProperty.ShortName} is not in {MetaClass.ShortName} class domain");
        }

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute
            && CanAssignAttributeValue(metaProperty, value) == false)
        {
            throw new ArgumentException(
                $"Unable assign {value} to {metaProperty.ShortName} property of {MetaClass.ShortName} class domain");
        }
        else if ((metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
            || metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            && value is IModelObject modelObject
            && CanAssignAssociationObject(metaProperty, modelObject) == false)
        {
            throw new ArgumentException(
                $"Unable set association {metaProperty.ShortName} of {MetaClass.ShortName} class domain with {modelObject.MetaClass.ShortName} type");
        }
    }

    /// <summary>
    /// Check attribute data complience to set.
    /// </summary>
    /// <param name="metaProperty">Meta property.</param>
    /// <param name="value">Value to assigning.</param>
    /// <returns></returns>
    private static bool CanAssignAttributeValue(ICimMetaProperty metaProperty, 
        object? value)
    {
        if (metaProperty.PropertyDatatype == null)
        {
            return false;
        }

        if (value == null)
        {
            return true;
        }

        System.Type primitiveType = typeof(string);
        if (metaProperty.PropertyDatatype is ICimMetaDatatype datatype)
        {
            primitiveType = datatype.PrimitiveType;
        }

        if (value is IModelObject modelObject
            && metaProperty.PropertyDatatype.IsCompound
            && metaProperty.PropertyDatatype == modelObject.MetaClass)
        {
            return true;
        }
        else if (value is Uri uriValue
            && metaProperty.PropertyDatatype.IsEnum)
        {
            if (metaProperty.PropertyDatatype.AllIndividuals.Any(
                ind => RdfUtils.RdfUriEquals(ind.BaseUri, uriValue)))
            {
                return true;
            }
        }
        else if (value is Enum enumValue
            && metaProperty.PropertyDatatype.IsEnum)
        {
            if (metaProperty.PropertyDatatype.ShortName == enumValue.GetType().Name
                && metaProperty.PropertyDatatype.AllIndividuals.Any(
                ind => ind.ShortName == enumValue.ToString()))
            {
                return true;
            }
        }
        else if (value.GetType().IsAssignableTo(primitiveType))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check association data complience to set.
    /// </summary>
    /// <param name="metaProperty">Meta property.</param>
    /// <param name="modelObject">Association object to assigning.</param>
    /// <returns></returns>
    private static bool CanAssignAssociationObject(ICimMetaProperty metaProperty, 
        IModelObject? modelObject)
    {
        if (metaProperty.PropertyDatatype == null)
        {
            return false;
        }

        if (modelObject == null 
            || modelObject is ModelObjectUnresolvedReference)
        {
            return true;
        }

        if (metaProperty.PropertyDatatype.Equals(modelObject.MetaClass)
            || modelObject.MetaClass.Extensions
                .Any(a => a.Equals(metaProperty.PropertyDatatype)))
        {
            return true;
        }

        var allAncestors = modelObject.MetaClass.AllAncestors;
        if (allAncestors.Any(a => a.Equals(metaProperty.PropertyDatatype) 
            || a.Extensions.Any(a => a.Equals(metaProperty.PropertyDatatype))))
        {
            return true;
        }

        return false;
    }

    private object? GetDataByProperty(ICimMetaProperty metaProperty,
        CimMetaPropertyKind? expectableKind = null)
    {
        if (_PropertiesData.TryGetValue(metaProperty, out var value)
            && (metaProperty.PropertyKind == expectableKind
                || expectableKind == null))
        {
            return value;
        }

        if (MetaClass.AllProperties.Contains(metaProperty)
            && (metaProperty.PropertyKind == expectableKind
                || expectableKind == null))
        {
            return null;
        }

        throw new ArgumentException(
            $"Property {metaProperty.ShortName} does not exist!");        
    }
    
    private void SetAssociationWithInverse(ICimMetaProperty metaProperty, 
        IModelObject? oldObj, IModelObject? newObj)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            _PropertiesData[metaProperty] = newObj;
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
            && newObj != null
            && _PropertiesData[metaProperty] 
                is ICollection<IModelObject> assocCollection)
        {
            if (newObj != null)
            {
                assocCollection.Add(newObj);
            }
            else if (oldObj != null)
            {
                assocCollection.Remove(oldObj);
            }
        }

        if (oldObj != null && metaProperty.InverseProperty != null 
            && oldObj is not ModelObjectUnresolvedReference)
        {
            if (metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1To1)
            {
                oldObj.SetAssoc1To1(metaProperty.InverseProperty, null);
            }
            else if(metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1ToM)
            {
                oldObj.RemoveAssoc1ToM(metaProperty.InverseProperty, this);
            }
        }

        if (newObj != null && metaProperty.InverseProperty != null
            && newObj is not ModelObjectUnresolvedReference)
        {
            if (metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1To1)
            {
                newObj.SetAssoc1To1(metaProperty.InverseProperty, this);
            }
            else if(metaProperty.InverseProperty.PropertyKind
                == CimMetaPropertyKind.Assoc1ToM)
            {
                newObj.AddAssoc1ToM(metaProperty.InverseProperty, this);
            }
        }
    }

    private bool CanChangeProperty(ICimMetaProperty metaProperty)
    {
        if (PropertyChanging != null)
        {
            var arg = new CanCancelPropertyChangingEventArgs(metaProperty, false);

            PropertyChanging.Invoke(this, arg);
            
            if (arg.Cancel == true)
            {
                return false;
            }
        }      

        return true;
    }

    #endregion UtilsPrivate

    private string _uuid = string.Empty;
    private bool _isAuto;

    private ICimMetaClass _MetaClass;
    private readonly Dictionary<ICimMetaProperty, object?> _PropertiesData;

    public override event PropertyChangedEventHandler? PropertyChanged;

    public delegate void CanCancelPropertyChangingEventHandler(object? sender, 
        CanCancelPropertyChangingEventArgs e);
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;
}

public class ModelObjectFactory : IModelObjectFactory
{
    public System.Type ProduceType => typeof(ModelObject);

    public IModelObject Create(string uuid, 
        ICimMetaClass metaClass, bool isAuto)
    {
        return new ModelObject(uuid, metaClass, isAuto);
    }
}

/// <summary>
/// Class for unfindable by reference object in model
/// ObjectData - ClassType means predicate URI
/// </summary>
public sealed class ModelObjectUnresolvedReference 
    : ModelObject, IModelObject
{
    public Uri Predicate => MetaClass.BaseUri;

    public ModelObjectUnresolvedReference(string uuid, ICimMetaClass metaClass)
        : base(uuid, metaClass, true)
    {
    }
}
