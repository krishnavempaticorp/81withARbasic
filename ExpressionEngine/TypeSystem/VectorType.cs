﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;

namespace MetraTech.ExpressionEngine.TypeSystem
{
    [DataContract]
    public class VectorType : MtType
    {
        #region Enums
        public enum ComplexTypeEnum { None, ServiceDefinition, ProductView, ParameterTable, AccountType, AccountView, BusinessModelingEntity, Any, Metanga }
        #endregion

        #region Properties
        /// <summary>
        /// The type of complex type
        /// </summary>
        [DataMember]
        public VectorType.ComplexTypeEnum ComplexType { get; set; }

        /// <summary>
        /// The subtype of the Entity type. For example, a BME ma
        /// </summary>
        [DataMember]
        public string ComplexSubtype { get; set; }

        /// <summary>
        /// Indicates if the ComplexType is deemed an Entity
        /// </summary>
        [DataMember]
        public bool IsEntity { get; set; }

        /// <summary>
        /// Returns a string that can be used to determine if two types are directly compatible (which is differnt than castable)
        /// </summary>
        /// <returns></returns>
        public virtual string CompatibleKey
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}|{1}", BaseType, ComplexType);
            }
        }

        #endregion

        #region Constructor
        public VectorType(ComplexTypeEnum type, string subtype, bool isEntity):base(BaseType.Entity)
        {
            ComplexType = type;
            ComplexSubtype = subtype;
            IsEntity = isEntity;
        }
        #endregion

        #region Methods
        //This isn't quite right
        public override string ToString(bool robust)
        {
            var type = IsEntity ? "Entity" : "ComplexType";
            if (robust)
                return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", type, ComplexSubtype);
            return type;
        }

        public VectorType Copy()
        {
            var type = (VectorType)TypeFactory.Create(BaseType);
            InternalCopy(type);
            type.ComplexType = ComplexType;
            type.ComplexSubtype = ComplexSubtype;
            type.IsEntity = IsEntity;
            return type;
        }

        #endregion

    }
}
