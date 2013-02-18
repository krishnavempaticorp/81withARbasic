﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MetraTech.ExpressionEngine
{
    public class EnumValue : IExpressionEngineTreeNode
    {
        #region Properties
        public readonly EnumType Parent;
        public string Name { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public string ToolTip { get {
            var toolTip = Description;
            if (Settings.ShowActualMappings)
                toolTip += string.Format("[DatabaseId={0}]", Description, Id);
            return toolTip;
        } }
        public string Image { get { return "EnumValue.png"; } }
        #endregion

        #region Constructor
        public EnumValue(EnumType parent, string value, int id)
        {
            Parent = parent;
            Name = value;
            Id = id;
        }
        #endregion

        #region Methods
        public string ToMtsql()
        {
            return string.Format("#{0}/{1}/{2}#", Parent.Parent.Name, Parent.Name, Name);
        }
        public string ToExpressionSnippet
        {
            get
            {
                if (Settings.NewSyntax)
                {
                    var enumSpace = Parent.Parent.Name.Replace('.', '_');
                    return string.Format("ENUM.{0}.{1}.{2}", enumSpace, Parent.Name, Name);
                }
                else
                    return ToMtsql();
            }
        }

        public void WriteXmlNode(XmlNode parentNode)
        {
            //TODO: We need to write out more stuff
            var valueNode = parentNode.AddChildNode("Value", Name);
        }

        #endregion
    }

}
