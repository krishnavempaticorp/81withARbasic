﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;

namespace MetraTech.ExpressionEngine.TypeSystem
{
    [DataContract]
    public class EnumerationType : MtType
    {
        #region Properties
        /// <summary>
        /// The namespace; used to prevent name collisions
        /// </summary>
        [DataMember]
        public string Namespace { get; set; }

        /// <summary>
        /// The enum's category (what contains the actual values)
        /// </summary>
        [DataMember]
        public string Category { get; set; }

        /// <summary>
        /// Returns a string that can be used to determine if two types are directly compatible (which is differnt than castable)
        /// </summary>
        /// <returns></returns>
        public override string CompatibleKey
        {
            get
            {
             return string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}", BaseType, Namespace, Category);
            }
        }
        #endregion

        #region Constructor
        public EnumerationType(string enumSpace, string enumType):base(BaseType.Enumeration)
        {
            Namespace = enumSpace;
            Category = enumType;
        }
        #endregion

        #region Methods
        public override string ToString(bool robust)
        {
            if (robust)
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", BaseType, Namespace, Category);
            else
                return BaseType.ToString();
        }

        private string Check()
        {
            //Check if the EnumSpace was specified
            if (string.IsNullOrEmpty(Namespace))
                return Localization.EnumNamespaceNotSpecified;

            //Check if the NameSpace exists
            if (!EnumHelper.NameSpaceExists(Namespace))
                return string.Format(CultureInfo.InvariantCulture, Localization.UnableToFindEnumNamespace, Namespace);

            //Check if the EnumType was specified
            if (string.IsNullOrEmpty(this.Category))
                return Localization.EnumTypeNotSpecified;

            //Check if the EnumType exists
            if (!EnumHelper.TypeExists(Namespace, Category))
                return string.Format(CultureInfo.InvariantCulture, Localization.UnableToFindEnumType, Namespace + "." + Category);

            return null;
        }
        #endregion

    }
}
