﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class DbTablesColList
    {
        public List<DescriptionValues> Values { get; set; } = new();
        public bool HasDescription { get; set; }
    }

    public class DescriptionValues
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
    }
}
