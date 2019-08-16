﻿using System.Collections.Generic;

namespace eze.schema.extract
{
    public class SchemaData
    {
        public string Version { get; set; }
        public List<DbObject> Objects { get; set; }
        public string Client { get; set; }
        public bool Entitled { get; set; }

        public SchemaData()
        {
            Objects = new List<DbObject>();
        }
    }

    public class DbObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int CheckSum { get; set; }
        public string Definition { get; set; }
    }
}