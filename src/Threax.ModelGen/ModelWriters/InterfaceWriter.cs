﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.ModelGen
{
    public class InterfaceWriter : ClassWriter
    {
        public InterfaceWriter(bool hasCreated, bool hasModified) : base(hasCreated, hasModified)
        {
        }

        public override String StartType(String name, String pluralName)
        {
            return $@"    public partial interface I{name} 
    {{";
        }

        public override String CreateProperty(String name, IWriterPropertyInfo info)
        {
            return $"        {info.ClrType} {name} {{ get; set; }}";
        }

        public override string AddTypeDisplay(string name)
        {
            return "";
        }

        public override string AddDisplay(string name)
        {
            return "";
        }

        public override string AddMaxLength(int length, string errorMessage)
        {
            return "";
        }

        public override string AddRequired(string errorMessage)
        {
            return "";
        }
    }
}
