﻿using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Report_App_WASM.Server.Models
{
    public class ActivityDbConnection : BaseTraceability
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string ConnectionType { get; set; } = "SQL";
        public TypeDb TypeDb { get; set; }
        private string? _typeDbName;

        [MaxLength(20)]
        public string? TypeDbName { get=> _typeDbName; set { _typeDbName = TypeDb.ToString(); } }
        [Required]
        public string? ConnectionPath { get; set; }
        public int Port { get; set; }
        [MaxLength(100)]
        public string? ConnectionLogin { get; set; }
        private string? _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value)
                {
                    _password = value;
                }
                else
                {
                    _password = EncryptDecrypt.EncryptString(value);
                }
            }
        }
        public bool UseDbSchema { get; set; }
        public bool ADAuthentication { get; set; } = false;
        public bool IntentReadOnly { get; set; }
        [MaxLength(100)]
        public string? DbSchema { get; set; }
        public int CommandTimeOut { get; set; } = 300;
        public int CommandFetchSize { get; set; } = 131072;
        public string DbConnectionParameters { get; set; } = "[]";
        public virtual Activity? Activity { get; set; }
    }
}