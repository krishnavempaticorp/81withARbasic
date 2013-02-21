﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Metanga.Miscellaneous.MetadataExport;
using MetraTech.ExpressionEngine.TypeSystem;

namespace MetraTech.ExpressionEngine
{
    //this is a total mess that loads data into the GlobalContext.... needs to replaced with clean data loading
    public static class DemoLoader
    {
        #region Properties
        public static string DirPath = @"C:\ExpressionEngine";
        public static string TopLevelDataDir = Path.Combine(DirPath, "Data");
        private static string DataPath;
        public static Context GlobalContext;
        #endregion

        #region General
        public static void LoadGlobalContext(Context.ProductTypeEnum product, string subDir)
        {
            GlobalContext = new Context(product);
            DataPath = Path.Combine(TopLevelDataDir, subDir);

            if (Context.ProductType == Context.ProductTypeEnum.MetraNet)
            {
                GlobalContext.AddEntity(DemoLoader.GetCloudComputeProductView());
                GlobalContext.AddEntity(DemoLoader.GetCorporateAccountType());
                GlobalContext.AddEntity(DemoLoader.GetAircraftLandingProductView());
                LoadEntities(GlobalContext, VectorType.ComplexTypeEnum.ProductView, Path.Combine(DataPath, "ProductViews.csv"));
                LoadEntities(GlobalContext, VectorType.ComplexTypeEnum.AccountView, Path.Combine(DataPath, "AccountViews.csv"));
                LoadEntities(GlobalContext, VectorType.ComplexTypeEnum.ServiceDefinition, Path.Combine(DataPath, "ServiceDefinitions.csv"));
                LoadXqg(GlobalContext, Expression.ExpressionTypeEnum.AQG, Path.Combine(DataPath, "AqgExpressions.csv"));
                LoadXqg(GlobalContext, Expression.ExpressionTypeEnum.UQG, Path.Combine(DataPath, "UqgExpressions.csv"));
            }
            else
            {
                LoadEntities(GlobalContext, VectorType.ComplexTypeEnum.Metanga, Path.Combine(DataPath, "Entities.csv"));
            }

            LoadEnumFile(GlobalContext, Path.Combine(DataPath, "Enums.csv"));
            LoadFunctions();
            LoadExpressions();
            LoadEmailTemplates(GlobalContext, Path.Combine(DataPath, "EmailTemplates"));
            LoadEmailInstances(GlobalContext, Path.Combine(DataPath, "EmailInstances"));

            var uomCategory = new UnitOfMeasureCategory("DigitalInformation");
            uomCategory.AddUnitOfMeasure("Gb", false);
            uomCategory.AddUnitOfMeasure("Mb", false);
            uomCategory.AddUnitOfMeasure("kb", false);
            GlobalContext.UoMs.Add(uomCategory.Name, uomCategory);

            uomCategory = new UnitOfMeasureCategory("Time");
            uomCategory.AddUnitOfMeasure("Millisecond", false);
            uomCategory.AddUnitOfMeasure("Second", false);
            uomCategory.AddUnitOfMeasure("Minute", false);
            uomCategory.AddUnitOfMeasure("Hour", false);
            GlobalContext.UoMs.Add(uomCategory.Name, uomCategory);
        }

        #endregion

        #region Expressions
        public static void LoadExpressions()
        {
            var dirInfo = new DirectoryInfo(Path.Combine(DataPath, "Expressions"));
            if (!dirInfo.Exists)
                return;

            foreach (var fileInfo in dirInfo.GetFiles("*.xml"))
            {
                var exp = Expression.CreateFromFile(fileInfo.FullName);
                GlobalContext.Expressions.Add(exp.Name, exp);
            }
        }
        #endregion

        #region Manual Entities
        public static Entity GetCloudComputeProductView()
        {
            //var entity = new Entity("CloudCompute", VectorType.ComplexTypeEnum.ProductView, null, true, "Models an cloud compute usage even");

            //var pv = entity.Properties;

            //Property property;

            ////Snapshot stuff
            //pv.AddInteger32("NumSnapshots", "The number of snapshots taken", true);
            //var charge = pv.AddCharge("SnapshotCharge", "The charge assoicated with snapshots", true);
            //charge.Type.UnitsProperty = "NumSnapshots";
            
            //pv.AddString("DataCenter", "The data center in which the server ran", true, null, 30);
            //pv.AddEnum("DataCenterCountry", "The country that the data center is located", true, "global", "countryname");
            //pv.AddEnum("ChargeModel", "The charinging model used to calculate the compute charge", true, "Cloud", "ChargeModel");
            //pv.AddDecimal("InstanceSize", "The size of the instance", true);
            //pv.AddEnum("OS", "The Operating System (OS)", true, "Cloud", "OperatingSystem");
            
            //var memory = pv.AddInteger32("Memory", "The amount of memory", true);
            //memory.Type.UnitOfMeasureMode = DataTypeInfo.UnitOfMeasureModeType.Fixed;
            //memory.DataTypeInfo.UnitOfMeasureQualifier = "DigitalInformation";

            //pv.AddDecimal("CpuCount", "The number of million CPU cycles", true);

            //property = pv.AddDecimal("Hours", "The number of hours the instance ran", true);
            //property.DataTypeInfo.UnitOfMeasureMode = DataTypeInfo.UnitOfMeasureModeType.Fixed;
            //property.DataTypeInfo.UnitOfMeasureQualifier = "Hour";

            //property = pv.AddDecimal("Duration", "The elapsed time", true);
            //property.DataTypeInfo.UnitOfMeasureMode = DataTypeInfo.UnitOfMeasureModeType.Category;
            //property.DataTypeInfo.UnitOfMeasureQualifier = "Time";

            //property = pv.AddDecimal("ScalingMetric", "The key scaling metric", true);
            //property.DataTypeInfo.UnitOfMeasureMode = DataTypeInfo.UnitOfMeasureModeType.Property;
            //property.DataTypeInfo.UnitOfMeasureQualifier = "ScalingMetricUom";

            //property = pv.AddString("ScalingMetricUom", "The UoM for the the ScalingMetric", true);

            //AppendCommonPvProperties(pv);
            //return entity;
            return null;
        }

        public static Entity GetAircraftLandingProductView()
        {
            var entity = new Entity("AircraftLanding", VectorType.ComplexTypeEnum.ProductView, null, true, "Models an cloud compute usage even");

            var pv = entity.Properties;
            pv.AddInteger32("MTOW", "Maximum TakeOff Weight", true);
            pv.AddInteger32("AircraftWeight", "The Weight of the aircraft in tons", true);
            pv.AddInteger32("NumPassengers", "The Weight of the aircraft in tons", true);
            pv.AddInteger32("NumTransferPassengers", "The Weight of the aircraft in tons", true);
            pv.AddInteger32("NumCrew", "The Weight of the aircraft in tons", true);
            AppendCommonPvProperties(pv);

            return entity;
        }
        public static void AppendCommonZvProperties(PropertyCollection props)
        {
            props.AddInteger32("AccountId", "The account associated with the event", true);
        }
        public static void AppendCommonPvProperties(PropertyCollection props)
        {
            props.AddDateTime("Timestamp", "The time the event is deemed to have occurred", true);
            props.AddInteger32("AccountId", "The account associated with the event", true);

            var name = UserSettings.NewSyntax ? "EventCharge" : "Amount";
            props.AddCharge(name, "The charge assoicated with the event which may summarize other charges within the event", true);
        }

        public static Entity GetCorporateAccountType()
        {
            var entity = new Entity("CorporateAccount", VectorType.ComplexTypeEnum.AccountType, null, true, "Models an corporate account");

            var pv = entity.Properties;
            pv.AddString("FirstName", "The data center in which the server ran", true, null, 30);
            pv.AddString("MiddleName", "The data center in which the server ran", true, null, 30);
            pv.AddString("LastName", "The data center in which the server ran", true, null, 30);
            pv.AddString("City", "The data center in which the server ran", true, null, 30);
            pv.AddString("State", "The data center in which the server ran", true, null, 30);
            pv.AddString("ZipCode", "The data center in which the server ran", true, null, 30);
            pv.AddEnum("Country", "The Operating System (OS)", true, "Global", "Country");

            return entity;
        }
        #endregion

        #region File-Based Entities
        public static void LoadEntities(Context context, VectorType.ComplexTypeEnum entityType, string filePath)
        {
            var entityList = ReadRecordsFromCsv<EntityRecord>(filePath);
            foreach (var entityRecord in entityList)
            {
                var entityParts = entityRecord.EntityName.Split('/');
                var entityNamespace = entityParts[0];
                var entityName = entityParts[1];
                var propName = entityRecord.PropertyName;
                var required = Helper.GetBoolean(entityRecord.IsRequired);
                var typeStr = entityRecord.PropertyType;
                var enumSpace = entityRecord.Namespace;
                var enumType = entityRecord.EnumType;

                var entityDescription = Helper.CleanUpWhiteSpace(entityRecord.EntityDescription);
                var propertyDescription = Helper.CleanUpWhiteSpace(entityRecord.PropertyDescription);

                Entity entity;
                if (!context.Entities.TryGetValue(entityName, out entity))
                {
                    entity = new Entity(entityName, entityType, null, entityRecord.IsEntity, entityDescription);
                    context.Entities.Add(entity.Name, entity);

                    //Add common properties, if any
                    switch (entityType)
                    {
                        case VectorType.ComplexTypeEnum.ProductView:
                            AppendCommonPvProperties(entity.Properties);
                            break;
                        case VectorType.ComplexTypeEnum.AccountView:
                            AppendCommonZvProperties(entity.Properties);
                            break;
                    }
                }

                MtType dtInfo;
                if (Context.ProductType == Context.ProductTypeEnum.MetraNet)
                {
                    //var baseType = TypeHelper.PropertyTypeId_BaseTypeMapping[Int32.Parse(typeStr)];
                    //dtInfo = TypeFactory.Create(baseType);
                    dtInfo = TypeFactory.Create(typeStr);
                }
                else
                    dtInfo = TypeFactory.Create(typeStr);

                switch (dtInfo.BaseType)
                {
                    case BaseType.Enumeration:
                        var _enumType = (EnumerationType)dtInfo;
                        _enumType.Namespace = enumSpace;
                        _enumType.Category = enumType;
                        break;
                    case BaseType.ComplexType:
                        var vectorType = (VectorType)dtInfo;
                        vectorType.ComplexType = entityType;
                        vectorType.ComplexSubtype = enumType; //we overrode the column
                        break;
                }

                if (entityRecord.ListType == null)
                {
                    dtInfo.ListType = MtType.ListTypeEnum.None;
                }
                else
                {
                    dtInfo.ListType = (MtType.ListTypeEnum)Enum.Parse(typeof(MtType.ListTypeEnum), entityRecord.ListType, true);
                }

                var property = new Property(propName, dtInfo, propertyDescription);
                property.Required = required;
                entity.Properties.Add(property);
            }
        }

        private static IEnumerable<T> ReadRecordsFromCsv<T>(string filePath) where T : class
        {
            var configuration = new CsvConfiguration
                                    {
                                        IsStrictMode = false,
                                        HasHeaderRecord = true
                                    };
            using (var streamReader = new StreamReader(filePath))
            {
                var csv = new CsvReader(streamReader, configuration);
                try
                {
                    var entityList = csv.GetRecords<T>().ToList();
                    return entityList;
                }
                catch (CsvReaderException e)
                {
                    throw new Exception(string.Format("Error loading {0} line {1} [{2}]", filePath, e.Row, e.Message), e);
                }
            }
        }

        #endregion

        #region Enums
        public static void LoadEnumFile(Context context, string filePath)
        {
            var enumList = ReadRecordsFromCsv<EnumRecord>(filePath);

            foreach (var enumRecord in enumList)
            {
                var enumValue = enumRecord.EnumValue;
                var spaceAndType = enumRecord.Namespace;
                var idStr = enumRecord.EnumDataId;

                var entityDescription = Helper.CleanUpWhiteSpace(enumRecord.EnumDescription);
                var propertyDescription = Helper.CleanUpWhiteSpace(enumRecord.ValueDescription);

                int id;
                if (string.IsNullOrEmpty(idStr))
                    id = 0;
                else
                {
                    if (!Int32.TryParse(idStr, out id))
                        id = 0;
                }

                if (string.IsNullOrWhiteSpace(spaceAndType))
                {
                    continue;
                }

                var enumParts = spaceAndType.Split('/');

                //Namespace?
                if (enumParts.Length == 1)
                    continue;

                var enumType = enumParts[enumParts.Length - 1];
                var enumNamespace = spaceAndType.Substring(0, spaceAndType.Length - enumType.Length - 1); //account for one slash

                var enumValueObj = EnumSpace.AddEnum(context, enumNamespace, enumType, -1, enumValue, id);
                enumValueObj.Description = propertyDescription;
                enumValueObj.EnumType.Description = entityDescription;
            }
        }

        #endregion

        #region XQGs

        public static void LoadXqg(Context context, Expression.ExpressionTypeEnum type, string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            for (int index = 1; index < lines.Length; index++)
            {
                var cols = lines[index].Split(',');
                var name = cols[0];
                var expression = cols[1];
                var description = string.Empty;
                switch (type)
                {
                    case Expression.ExpressionTypeEnum.AQG:
                        var aqg = new AQG(name, description, expression);
                        context.AQGs.Add(aqg.Name, aqg);
                        break;
                    case Expression.ExpressionTypeEnum.UQG:
                        var uqg = new UQG(name, description, expression);
                        context.UQGs.Add(uqg.Name, uqg);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        #region Emails
        public static void LoadEmailInstances(Context context, string dirPath)
        {
            var dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
                return;
            foreach (var file in dirInfo.GetFiles("*.xml"))
            {
                var emailInstance = EmailInstance.CreateFromFile(file.FullName);
                context.EmailInstances.Add(emailInstance.Name, emailInstance);
            }
        }
        public static void LoadEmailTemplates(Context context, string dirPath)
        {
            var dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
                return;
            foreach (var file in dirInfo.GetFiles("*.xml"))
            {
                var emailTemplate = EmailTemplate.CreateFromFile(file.FullName);
                context.EmailTemplates.Add(emailTemplate.Name, emailTemplate);
            }
        }
        #endregion

        #region Functions
        public static void LoadFunctions()
        {
            DemoLoader.GlobalContext.Functions.Clear();
            var dirInfo = new DirectoryInfo(Path.Combine(DirPath, "Functions"));
            foreach (var file in dirInfo.GetFiles("*.xml"))
            {
                var func = Function.CreateFromFile(file.FullName);
                GlobalContext.Functions.Add(func.Name, func);
            }
        }
        #endregion
    }
}
