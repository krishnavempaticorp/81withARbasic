﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.Serialization;
using MetraTech.ExpressionEngine.Components.Enumerations;
using MetraTech.ExpressionEngine.MTProperties;
using MetraTech.ExpressionEngine.MTProperties.Enumerations;
using MetraTech.ExpressionEngine.TypeSystem;
using MetraTech.ExpressionEngine.TypeSystem.Constants;
using MetraTech.ExpressionEngine.TypeSystem.Enumerations;
using System.IO;
using MetraTech.ExpressionEngine.Validations;
using Type = MetraTech.ExpressionEngine.TypeSystem.Type;

namespace MetraTech.ExpressionEngine.PropertyBags
{
    /// <summary>
    /// Implements a ComplexType, esentially something that PropertyCollection which may include properties and
    /// other complex types. Note that DataTypeInfo.IsEntity determines if it's deemed an PropertyBag (an important destinction for Metanga)
    /// </summary>
    [DataContract(Namespace = "MetraTech")]
    [KnownType(typeof(AccountViewEntity))]
    [KnownType(typeof(BusinessModelingEntity))]
    [KnownType(typeof(ParameterTableEntity))]
    [KnownType(typeof(ProductViewEntity))]
    [KnownType(typeof(ServiceDefinitionEntity))]
    public class PropertyBag : Property
    {
        #region Properties

        //This doesn't belong here
        public Context Context { get; set; }
  
        /// <summary>
        /// The name prefixed with the namespace, if any
        /// </summary>
        public override string FullName
        {
            get { return Namespace + "." + Name; }
        }

        /// <summary>
        /// The entity's namespace. Primarly used to prevent name collisions for MetraNet
        /// </summary>
        [DataMember]
        public string Namespace { get; set; }

        public virtual string DatabaseTableNamePrefix { get { return null; } }
 
        public string DatabaseTableName { get { return DatabaseTableNamePrefix + Name; } }
        
        /// <summary>
        /// Star-table-based entities (e.g., Product Views, Account Views, Service Definitions, etc.) have their
        /// "core" properties stored in a seperate table. This optional field is used when determing the the property
        /// name for a core property in a star schema.
        /// </summary>
        [DataMember]
        public string DatabaseReservedPropertyTableName { get; set; }

        /// <summary>
        /// The properties contained in the property bag which may include other property bags
        /// </summary>
        [DataMember]
        public PropertyCollection Properties { get; private set; }

        /// <summary>
        /// The extension that the PropertyBag is associated with
        /// </summary>
        public string Extension { get; set; }

        public override string CompatibleKey
        {
            get { return string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Name, Type.CompatibleKey); }
        }

        public virtual string XqgPrefix { get { return null; } }

        public virtual string SubDirectoryName { get { return ((PropertyBagType)Type).Name + "s"; } }

        ////public bool IsAccountView
        ////{
        ////    get { ((PropertyBagType)Type).Name == PropertyBagConstants.AccountView; }
        ////}

        #endregion

        #region GUI Helper Properties (move in future)
        public override string ToolTip
        {
            get
            {
                var tip = ((PropertyBagType)Type).Name;
                if (!string.IsNullOrEmpty(Description))
                    tip += Environment.NewLine + Description;
                if (UserContext.Settings.ShowActualMappings)
                    tip += string.Format(CultureInfo.InvariantCulture, "\r\n[TableName: {0}]", DatabaseColumnName);
                return tip;
            }
        }

        public override string Image {get { return ((PropertyBagType) Type).PropertyBagMode.ToString() + ".png"; }}

        public override string ImageDirection
        {
            get
            {
                switch (Direction)
                {
                    case Direction.InputOutput:
                        return "EntityInOut.png";
                    case Direction.Input:
                        return "EntityInput.png";
                    case Direction.Output:
                        return "EntityOutput.png";
                    default:
                        return null;
                }
            }
        }
        #endregion

        #region Constructor

        public PropertyBag(string _namespace, string name, string propertyBagTypeName, PropertyBagMode propertyBagMode, string description)
            : base(name, TypeFactory.CreatePropertyBag(propertyBagTypeName, propertyBagMode), true, description)
        {
            ComponentType = ComponentType.PropertyBag;
            Namespace = _namespace;
            Type = TypeFactory.CreatePropertyBag(propertyBagTypeName, propertyBagMode);
            Description = description;
            Properties = new PropertyCollection(this);
        }

        [OnDeserializedAttribute]
        private void FixDeserialization(StreamingContext sc)
        {
            Properties.FixDeserialization(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines if the Properties collection exactly matches the nameFilter or the typeFilter. Useful
        /// for filtering in the GUI.
        /// TODO: Add recursive option to look sub entities
        /// </summary>
        public bool HasPropertyMatch(Regex nameFilter, Type typeFilter)
        {
            foreach (var property in Properties)
            {
                if (nameFilter != null && !nameFilter.IsMatch(property.Name))
                    continue;
                if (typeFilter != null && !property.Type.IsBaseTypeFilterMatch(typeFilter))
                    continue;
                return true;
            }
            return false;
        }

        public override string ToExpressionSnippet
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", XqgPrefix, Name);
            }
        }

        public override object Clone()
        {
            throw new NotImplementedException();
            //var newEntity = new ComplexType(Name, Type.ComplexType, Description);
            //newEntity.Properties = Properties.Clone();
            //return newEntity;
        }

        /// <summary>
        /// Note that the component parameter is ignored!
        /// </summary>
        public override ValidationMessageCollection Validate(ValidationMessageCollection messages, Context context)
        {
            if (messages == null)
                throw new ArgumentException("messages is null");
            if (context == null)
                throw new ArgumentException("context is null");

            if (!BasicHelper.FullNameIsValid(FullName))
                messages.Error(this, Localization.InvalidName);
            if (string.IsNullOrWhiteSpace(Description))
                messages.Info(Localization.NoDescription);

            ValidateProperties(messages, context);
            return messages;
        }

        protected virtual void ValidateProperties(ValidationMessageCollection messages, Context context)
        {
            foreach (var property in Properties)
            {
                property.Validate(messages, context);
            }
        }

        public virtual IEnumerable<Property> GetCoreProperties()
        {
            return new List<Property>();
        }

        public virtual void AddCoreProperties()
        {
#warning, not sure why I need this here. I'm getting crash when trying to create a multipoint parent in the flow
            if (Properties == null)
                return;

            Properties.AddRange(GetCoreProperties());
        }

        #endregion

        #region IO Methods

        public void Save(string file)
        {
            IOHelper.Save(file, this);
        }

        public static T CreateFromFile<T>(string file)
        {
            var xmlContent = File.ReadAllText(file);
            var propertyBag = CreateFromString<T>(xmlContent);
            //propertyBag.Extension = IOHelper.GetMetraNetExtension(file);
            return propertyBag;
        }

        public static T CreateFromString<T>(string xmlContent)
        {
            var propertyBag = IOHelper.CreateFromString<T>(xmlContent);
            var pb = (PropertyBag)(object)propertyBag;
            pb.Properties.Parent = propertyBag;
            pb.Properties.SetPropertyParentReferences();
            return propertyBag;
        }

        public string GetFileNameGivenExtensionsDirectory(string extensionsDir)
        {
            var dirPath = IOHelper.GetMetraNetConfigPath(extensionsDir, Extension, SubDirectoryName);
            return string.Format(CultureInfo.InvariantCulture, @"{0}\{1}.xml", dirPath, Name);
        }

        public void SaveInExtensionsDirectory(string extensionsDir)
        {
            var file = GetFileNameGivenExtensionsDirectory(extensionsDir);
            Save(file);
        }
        #endregion
    }
}
