﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace MetraTech.ExpressionEngine.TypeSystem
{
    public static class TypeHelper
    {
        #region Properties

        //public static DataTypeInfo[] AllTypes;
        public static readonly MtType[] AllTypes;

        /// <summary>
        /// BaseTypes supported by MSIX entities (e.g., Service Definitoins, ProductViews, etc.).
        /// Do not make changes to the objects in this array. Use CopyFrom to make a copy and make changes to the copy.
        /// </summary>
        public static readonly IEnumerable<BaseType> MsixBaseTypes;

        /// <summary>
        /// BaseTypes that exist as native database types (e.g., string, int, etc.). In other words, there is a 1:1 mapping.
        /// Do not make changes to the objects in this array. Use CopyFrom to make a copy and make changes to the copy.
        /// </summary>
        public static readonly IEnumerable<BaseType> DatabaseBaseTypes;

        /// <summary>
        /// Maps the integer data type IDs in metranet to a BaseType
        /// </summary>
        public static Dictionary<int, BaseType> PropertyTypeId_BaseTypeMapping = new Dictionary<int, BaseType>();

        #endregion

        #region Methods
        /// <summary>
        /// Indicates if compatible with MSIX entities (e.g., Service Definitions, Product Views, etc.)
        /// </summary>
        public static bool IsMsixCompatible(BaseType baseType)
        {
            return MsixBaseTypes.Contains(baseType);
        }

        public static BaseType GetBaseType(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
                return BaseType.Unknown;

            switch (typeString.ToLower(CultureInfo.InvariantCulture))
            {
                case "str":
                case "string":
                case "varchar":
                case "nvarchar":
                case "characters":
                    return BaseType.String;
                case "id":
                case "int32":
                case "integer32":
                case "integer":
                    return BaseType.Integer32;
                case "bigint":
                case "long":
                case "int64":
                case "integer64":
                    return BaseType.Integer64;
                case "timestamp":
                case "datetime":
                    return BaseType.DateTime;
                case "enum":
                case "enumeration":
                    return BaseType.Enumeration;
                case "decimal":
                    return BaseType.Decimal;
                case "float":
                    return BaseType.Float;
                case "double":
                    return BaseType.Double;
                case "boolean":
                case "bool":
                    return BaseType.Boolean;
                case "any":
                    return BaseType.Any;
                case "binary":
                    return BaseType.Binary;
                case "numeric":
                    return BaseType.Numeric;
                case "uniqueidentifier":
                case "uniqueid":
                    return BaseType.UniqueIdentifier;
                case "guid":
                    return BaseType.Guid;
                case "entity":
                    return BaseType.ComplexType;
                default:
                    throw new ArgumentException("Invalid internal data type string [" + typeString + "]");
            }
        }



        /// <summary>
        /// Returns a MTSQL version of the type
        /// </summary>
        /// <returns></returns>
        public static string GetMtsqlString(BaseType baseType)
        {
            switch (baseType)
            {
                case BaseType.Boolean:
                    return "BOOLEAN";
                case BaseType.Decimal:
                    return "DECIMAL";
                case BaseType.Double:
                    return "DOUBLE";
                case BaseType.Enumeration:
                    return "ENUM";
                case BaseType.Integer32:
                    return "INTEGER";
                case BaseType.Integer64:
                    return "BIGINT";
                case BaseType.String:
                    return "NVARCHAR";
                case BaseType.DateTime:
                    return "DATETIME";
                case BaseType.Binary:
                    return "BINARY";
                default:
                    throw new ApplicationException("Unhandled data type: " + baseType.ToString());
            }
        }

        public static string GetBmeString(BaseType baseType)
        {
            switch (baseType)
            {
                case BaseType.String:
                    return "String";
                case BaseType.Integer32:
                    return "Int32";
                case BaseType.Integer64:
                    return "Int64";
                case BaseType.DateTime:
                    return "DateTime";
                case BaseType.Enumeration:
                    return "Enum"; //I'm totally unsure about this here.
                case BaseType.Decimal:
                    return "Decimal";
                case BaseType.Float:
                case BaseType.Double:
                    return "Double";
                case BaseType.Boolean:
                    return "Boolean";
                default:
                    throw new ApplicationException("Unhandled data type: " + baseType.ToString());
            }
        }

        public static string GetCSharpString(BaseType baseType)
        {
            //Tried using ToCSharpType and using it's Name or ToString() but that didn't work.
            switch (baseType)
            {
                case BaseType.Boolean:
                    return "bool";
                case BaseType.Decimal:
                    return "decimal";
                case BaseType.Double:
                    return "double";
                case BaseType.Enumeration:
                    throw new NotImplementedException();
                //return MetraTech.DomainModel.Enums.EnumHelper.GetGeneratedEnumType(EnumSpace, EnumType, EnvironmentConfiguration.Instance.MetraNetBinariesDirectory).ToString();
                case BaseType.Integer32:
                    return "int";
                case BaseType.Integer64:
                    return "Int64";
                case BaseType.String:
                    return "string";
                case BaseType.DateTime:
                    return "DateTime";
                default:
                    throw new Exception("Unhandled DataType: " + baseType.ToString());
            }
        }


        public static System.Type GetCSharpType(BaseType type)
        {
            switch (type)
            {
                case BaseType.Boolean:
                    return typeof(bool);
                case BaseType.Decimal:
                    return typeof(decimal);
                case BaseType.Double:
                    return typeof(double);
                case BaseType.Enumeration: //Not sure about this one!
                    return typeof(EnumHelper); //typeof(Int32)
                case BaseType.Integer32:
                    return typeof(int);
                case BaseType.Integer64:
                    return typeof(Int64);
                case BaseType.String:
                    return typeof(string);
                case BaseType.DateTime:
                    return typeof(DateTime);
                default:
                    throw new Exception("Unhandled Data Type: " + type.ToString());
            }
        }

        /// <summary>
        /// Determines if the specified value is valid for the type
        /// </summary>
        /// <param name="value">The value to be checked</param>
        /// <param name="allowEmpty">Indicates if empty string and null values are allowed</param>
        /// <returns></returns>
        public static bool ValueIsValid(BaseType baseType, string value, bool allowEmpty)
        {
            //Check for null and empty string
            if (string.IsNullOrEmpty(value))
                return allowEmpty;

            //Try a conversion to see if it works
            switch (baseType)
            {
                case BaseType.Unknown:
                    return true;//Not sure if this is best behavior...
                case BaseType.String:
                    //Nothing to check
                    return true;
                case BaseType.Integer32:
                    Int32 the32;
                    return System.Int32.TryParse(value, out the32);
                case BaseType.Integer64:
                    Int64 the64;
                    return System.Int64.TryParse(value, out the64);
                case BaseType.DateTime:
                    DateTime theDT;
                    return System.DateTime.TryParse(value, out theDT);
                case BaseType.Enumeration:
                    throw new NotImplementedException();
                //return Config.Instance.EnumerationConfig.ValueExists(EnumSpace, EnumType, value);
                case BaseType.Decimal:
                    Decimal theDecimal;
                    return System.Decimal.TryParse(value, out theDecimal);
                case BaseType.Float:
                    float theFloat;
                    return float.TryParse(value, out theFloat);
                case BaseType.Boolean:
                    return (Helper.ParseBool(value) != null);
                case BaseType.Double:
                    Double theDouble;
                    return double.TryParse(value, out theDouble);
                default:
                    throw new ApplicationException(" Unknown data type '" + baseType.ToString());
            }
        }

        public static bool IsNumeric(BaseType baseType)
        {
            switch (baseType)
            {
                case BaseType.Decimal:
                case BaseType.Double:
                case BaseType.Float:
                case BaseType.Integer:
                case BaseType.Integer32:
                case BaseType.Integer64:
                case BaseType.Charge:
                    return true;
                default:
                    return false;
            }
        }

        #endregion



        #region Convert to constant
        /// <summary>
        /// Converts an explicit value its MTSQL representation (i.e., strings are quoted, enums are fully qualified, etc.)
        /// </summary>
        public static string ConvertValueToMtsqlConstant(MtType type, string value)
        {
            if (type == null)
                throw new ArgumentNullException("dtInfo");
            if (value == null)
                throw new ArgumentNullException("value");

            switch (type.BaseType)
            {
                //Decimals must have a decimal place
                case BaseType.Decimal:
                    if (value.StartsWith("."))
                        return "0" + value;
                    else if (!value.Contains('.'))
                        return value + ".0";
                    else
                        return value;

                case BaseType.String:
                    return "N'" + value + "'";

                case BaseType.DateTime:
                    return "'" + value + "'";

                case BaseType.Enumeration:
                    var enumType = (EnumerationType)type;
                    return string.Format(CultureInfo.InvariantCulture, "#{0}/{1}/{2}#", enumType.Namespace, enumType.Category, value);

                //Don't do anything special
                default:
                    return value;
            }
        }

        public static string ConvertValueStringToCSharpConstant(MtType type, string value)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (value == null)
                throw new ArgumentNullException("value");

            switch (type.BaseType)
            {
                //Strings and timestamps must be enclosed in quotes
                case BaseType.String:
                case BaseType.DateTime:
                    return '"' + value + '"';
                case BaseType.Decimal:
                    return value + "M";
                case BaseType.Enumeration:
                    throw new NotImplementedException();
                //Type enumType = EnumHelper.GetGeneratedEnumType(dtInfo.EnumSpace, dtInfo.EnumType, EnvironmentConfiguration.Instance.MetraNetBinariesDirectory);
                //object enumValue = EnumHelper.GetGeneratedEnumByEntry(enumType, value);
                //string enumValueName = System.Enum.GetName(enumType, enumValue);
                //return enumType.FullName + "." + enumValueName;
                default:
                    return value;
            }
        }

        public static object ConvertValueToNativeValue(MtType type, string value, bool useInvariantCulture)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (value == null)
                return null;

            var cultureInfo = useInvariantCulture ? CultureInfo.InvariantCulture : CultureInfo.CurrentUICulture;

            switch (type.BaseType)
            {
                case BaseType.Boolean:
                    return Helper.GetBoolean(value);
                case BaseType.Decimal:
                    return decimal.Parse(value, cultureInfo);
                case BaseType.Double:
                    return double.Parse(value, cultureInfo);
                //case BaseType.Enumeration:
                //    return EnumHelper.GetMetraNetIntValue(type, value);
                case BaseType.Float:
                    return float.Parse(value, cultureInfo);
                case BaseType.Integer32:
                    return int.Parse(value, cultureInfo);
                case BaseType.Integer64:
                    return long.Parse(value, cultureInfo);
                case BaseType.String:
                    return value;
                //case DataType._timestamp:
                //    return new DateTime.Parse(value);
                default:
                    throw new ArgumentException("Unhandled DataType " + type.BaseType.ToString());
            }
        }
        #endregion
    }
}
