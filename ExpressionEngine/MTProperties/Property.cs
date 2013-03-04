﻿using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.Serialization;
using MetraTech.ExpressionEngine.MTProperties.Enumerations;
using MetraTech.ExpressionEngine.PropertyBags;
using MetraTech.ExpressionEngine.TypeSystem;
using MetraTech.ExpressionEngine.TypeSystem.Enumerations;
using MetraTech.ExpressionEngine.Validations;
using Type = MetraTech.ExpressionEngine.TypeSystem.Type;

namespace MetraTech.ExpressionEngine.MTProperties
{
    /// <summary>
    /// General abstraction for properties spanning MetraNet(ProductViews, BMEs, etc.) and Metanga. There will be subclasses to
    /// implement variants. For example, ProductView properties have many other attributes such as access levels.
    /// 
    /// TO DO:
    /// *Fix NameRegex
    /// *Unit tests
    /// </summary>
    [DataContract (Namespace = "MetraTech")]
    [KnownType(typeof(AccountViewProperty))]
    [KnownType(typeof(ProductViewProperty))]
    [KnownType(typeof(ServiceDefinitionProperty))]
    public class Property : IExpressionEngineTreeNode
    {
        #region Static Properties
        /// <summary>
        /// Used to validate the Name property
        /// </summary>
        private static readonly Regex NameRegex = new Regex("[a-zA-Z][a-zA-Z0-9_]*");
        #endregion

        #region Properties

        /// <summary>
        /// The collection to which the property belongs (may be null)
        /// </summary>
        public PropertyCollection PropertyCollection { get; set; }

        public PropertyBag PropertyBag
        {
            get
            {
                if (PropertyCollection == null || PropertyCollection.PropertyBag == null)
                    return null;
                return PropertyCollection.PropertyBag;
            }
        }

        /// <summary>
        /// The name of the property
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Rich data type class
        /// </summary>
        [DataMember]
        public Type Type { get; set; }

        /// <summary>
        /// A description that's used in tooltips, auto doc, etc.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Indicates if a value is required
        /// </summary>
        [DataMember]
        public bool Required { get; set; }

        /// <summary>
        /// The defult value for the property
        /// </summary>
        [DataMember]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Indicates if the property is something that is common within the context of the Parent property.
        /// For example, all usage events have a Timestamp property, therefore it's considered common. These by
        /// definition aren't editable.
        /// </summary>
        [DataMember]
        public bool IsCore { get; set; }

        /// <summary>
        /// The assoicated name, if any, in the database. In the case of property it's a column name, in the case of a PropertyBag, 
        /// it's a table name. Note that not all PropertyBag types are backed by database table.
        /// </summary>
        public virtual string DatabaseName { get { return Name; } }

        /// <summary>
        /// Indicates the how the Property is interacted with (e.g., Input, Output or InputOutput)
        /// </summary>
        [DataMember]
        public Direction Direction { get; set; }

        //
        //Determines if the Direction is Input or InputOutput
        //
        public bool IsInputOrInOut
        {
            get { return Direction == Direction.Input || Direction == Direction.InputOutput; }
        }

        //
        //Determines if the Direction is Ouput or InputOutput
        //
        public bool IsOutputOrInOut
        {
            get { return Direction == Direction.Output || Direction == Direction.InputOutput; }
        }

        public virtual string CompatibleKey { get { return Type.CompatibleKey; } }

        /// <summary>
        /// Used for end-user-drive testing etc. 
        /// </summary>
        public string Value { get; set; }

        #endregion Properties

        #region GUI Helper Properties (should be moved)
        public string TreeNodeLabel { get { return Name + Type.ListSuffix; } }
        /// <summary>
        /// Combines the data type and description
        /// </summary>
        public  virtual string ToolTip
        {
            get
            {
                {
                    var tooltipStr = Type.ToString(true);
                    if (!string.IsNullOrEmpty(Description))
                        tooltipStr += Environment.NewLine + Description;
                    if (UserContext.Settings.ShowActualMappings)
                        tooltipStr += string.Format(CultureInfo.InvariantCulture, "\r\n[ColumnName: {0}]", DatabaseName);
                    return tooltipStr;
                }
            }
        }

        public virtual string ImageDirection
        {
            get
            {
                switch (Direction)
                {
                    case Direction.InputOutput:
                        return "PropertyInOut.png";
                    case Direction.Input:
                        return "PropertyInput.png";
                    case Direction.Output:
                        return "PropertyOutput.png";
                    default:
                        return null;
                }
            }
        }

        public virtual string Image
        {
            get
            {
                var baseName = Type.BaseType.ToString();
                if (IsCore)
                    baseName += "IsCoreOverlay";
                return baseName + ".png";
            }
        }
        #endregion

        #region Constructors

        public Property(string name, Type type, bool isRequired, string description = null)
        {
            Name = name;
            Type = type;
            Required = isRequired;
            Description = description;

            IsCore = false;
        }

        #endregion Constructors

        #region Static Create Methods
        public static Property CreateUnknown(string name, bool isRequired, string description)
        {
            return new Property(name, TypeFactory.CreateUnknown(), isRequired, description);
        }
        public static Property CreateInteger32(string name, bool isRequired, string description)
        {
            return new Property(name, TypeFactory.CreateInteger32(UnitOfMeasureMode.None, null), isRequired, description);
        }
        public static Property CreateString(string name, bool isRequired, string description, int length)
        {
            var property = new Property(name, TypeFactory.CreateString(length), isRequired, description);
            return property;
        }

        public static Property CreateBoolean(string name, bool isRequired, string description)
        {
            var property = new Property(name, TypeFactory.CreateBoolean(), isRequired, description);
            return property;
        }

        public static Property CreateEnum(string name, bool isRequired, string description, string enumSpace, string enumType)
        {
            var property = new Property(name, TypeFactory.CreateEnumeration(enumSpace, enumType), isRequired, description);
            return property;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Returns the Units property associated with this property. Only valid for Charges.
        /// </summary>
        public Property GetUnitsProperty()
        {
            if (!Type.IsMoney || PropertyCollection == null)
                return null;

            return null;
            //throw new NotImplementedException("need to decide right model");
            //return PropertyCollection.Get(((MoneyType)Type).UnitsProperty);
        }

        /// <summary>
        /// Returns the UOM property associated with this property. Only valid for Numerics.
        /// </summary>
        public Property GetUnitOfMeasureProperty()
        {
            return null;
            //if (!Type.IsNumeric || Type.IsMoney)
            //    return null;

            //var type = (NumberType)Type;
            //if (!Type.IsNumeric || type.UnitOfMeasureMode != UnitOfMeasureMode.Property || PropertyCollection == null)
            //    return null;
            //return PropertyCollection.Get(type.UnitOfMeasureQualifier);
        }

        public virtual object Clone()
        {
            throw new NotImplementedException();
            //May want to be more judicious when creating a copy of the property
            //but using MemberwiseClone works for the moment
            //var property = this.MemberwiseClone() as Property;
            //property.DataTypeInfo = this.DataTypeInfo.Copy();
            //return property;
        }

        private void AddError(ValidationMessageCollection messages, string message)
        {
            messages.Error(GetPrefixedMessage(message));
        }
        private void AddWarning(ValidationMessageCollection messages, string message)
        {
            messages.Warn(GetPrefixedMessage(message));
        }

        private string GetPrefixedMessage(string message = null)
        {
            var prefix = string.Format(CultureInfo.CurrentUICulture, Localization.PropertyMessagePrefix, QualifiedName);
            prefix += message;
            return prefix;
        }



        //Not sure that I need prefixMsg here
        public virtual ValidationMessageCollection Validate(bool prefixMsg, ValidationMessageCollection messages, Context context)
        {
            if (messages == null)
                throw new ArgumentException("messages is null");

            //Validate the name
            if (string.IsNullOrWhiteSpace(Name))
                AddError(messages, Localization.NameNotSpecified);
            else
            {
                if (!NameRegex.IsMatch(Name))
                    AddError(messages, Localization.InvalidName);
                //FUTURE FEATURE
                //else
                //    SpellingEngine.CheckWord(Name, null, messages);
            }

            //Validate the type
            Type.Validate(GetPrefixedMessage(), messages, context);

            //Validate the default value, if any
            if (DefaultValue != null && TypeHelper.ValueIsValid(Type.BaseType, DefaultValue, true))
                AddError(messages, Localization.InvalidDefaultValue);

            //Validate the description
            if (string.IsNullOrEmpty(Description))
                AddWarning(messages, Localization.InsufficientDescription);
            //FUTURE FEATURE
            //SpellingEngine.CheckString(Description, null, messages);

            return messages;
        }

        /// <summary>
        /// Useful for debugging.
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", Name, Type.ToString(true));
        }

        public virtual string ToExpressionSnippet
        {
            get
            {
                var entity = PropertyBag;
                if (entity == null)
                    return null;

                string snippet;
                if (UserContext.Settings.NewSyntax)
                    snippet = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", entity.XqgPrefix, Name);
                else
                    snippet = string.Format(CultureInfo.InvariantCulture, "{0}.c_{1}", entity.XqgPrefix, Name);

                return snippet + Type.ListSuffix;
            }
        }

        public string QualifiedName
        {
            get
            {
                var entity = PropertyBag;
                if (entity == null)
                    return Name;
                return entity.Name + "." + Name;
            }
        }

        /// <summary>
        /// Used when searching for properties across entities. The underlying datatype might not be the same. Perhaps
        /// the DataType level formatting should be moved to DataTypeInfo class
        /// NOTE THAT WE'RE NOT DEALING WITH UOMs
        /// </summary>
        //public string CompatibleKey
        //{
        //{get{return null;}}
        //    //string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Name, Type.CompatibleKey);}}

        public string GetFullyQualifiedName(bool prefix)
        {
            var propertyBag = PropertyBag;
            if (PropertyBag == null)
                return Name;

            var name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", propertyBag.Name, Name);
            if (prefix)
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", propertyBag.XqgPrefix, name);
            return name;
        }

        #endregion
    }
}
