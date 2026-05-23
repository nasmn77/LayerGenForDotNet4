namespace LayerGenForDotNet4.Models
{
    public enum SqlNetType
    {
        String,
        Int,
        Long,
        Short,
        Byte,
        Bool,
        Decimal,
        Double,
        Float,
        DateTime,
        TimeSpan,
        Guid,
        ByteArray,
        Object
    }

    public class FieldInfo
    {
        public string FieldName { get; set; } = "";
        public string SqlTypeName { get; set; } = "";   // e.g. "nvarchar", "int", "datetime"
        public int Length { get; set; }                  // -1 = MAX
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }

        public SqlNetType NetType => GetNetType(SqlTypeName);

        public static SqlNetType GetNetType(string sqlType)
        {
            return sqlType.ToLowerInvariant() switch
            {
                "bigint" => SqlNetType.Long,
                "int" => SqlNetType.Int,
                "smallint" => SqlNetType.Short,
                "tinyint" => SqlNetType.Byte,
                "bit" => SqlNetType.Bool,
                "decimal" or "numeric" or "money" or "smallmoney" => SqlNetType.Decimal,
                "float" => SqlNetType.Double,
                "real" => SqlNetType.Float,
                "datetime" or "datetime2" or "smalldatetime" or "date" => SqlNetType.DateTime,
                "time" => SqlNetType.TimeSpan,
                "uniqueidentifier" => SqlNetType.Guid,
                "char" or "nchar" or "varchar" or "nvarchar" or "text" or "ntext" => SqlNetType.String,
                "binary" or "varbinary" or "image" or "timestamp" => SqlNetType.ByteArray,
                _ => SqlNetType.Object
            };
        }

        /// <summary>Returns C# type name respecting nullability</summary>
        public string CSharpType(bool forceNullable = false)
        {
            bool nullable = IsNullable || forceNullable;
            string q = nullable ? "?" : "";

            return NetType switch
            {
                SqlNetType.String => "string" + (nullable ? "?" : ""),   // strings are already reference types
                SqlNetType.ByteArray => "byte[]" + (nullable ? "?" : ""),
                SqlNetType.Int => "int" + q,
                SqlNetType.Long => "long" + q,
                SqlNetType.Short => "short" + q,
                SqlNetType.Byte => "byte" + q,
                SqlNetType.Bool => "bool" + q,
                SqlNetType.Decimal => "decimal" + q,
                SqlNetType.Double => "double" + q,
                SqlNetType.Float => "float" + q,
                SqlNetType.DateTime => "DateTime" + q,
                SqlNetType.TimeSpan => "TimeSpan" + q,
                SqlNetType.Guid => "Guid" + q,
                _ => "object" + (nullable ? "?" : "")
            };
        }

        /// <summary>Returns Microsoft.Data.SqlClient SqlDbType enum value as string</summary>
        public string SqlDbTypeStr()
        {
            return SqlTypeName.ToLowerInvariant() switch
            {
                "bigint" => "BigInt",
                "binary" => "Binary",
                "bit" => "Bit",
                "char" => "Char",
                "date" => "Date",
                "datetime" => "DateTime",
                "datetime2" => "DateTime2",
                "decimal" or "numeric" => "Decimal",
                "float" => "Float",
                "image" => "Image",
                "int" => "Int",
                "money" => "Money",
                "nchar" => "NChar",
                "ntext" => "NText",
                "nvarchar" => "NVarChar",
                "real" => "Real",
                "smalldatetime" => "SmallDateTime",
                "smallint" => "SmallInt",
                "smallmoney" => "SmallMoney",
                "text" => "Text",
                "time" => "Time",
                "timestamp" => "Timestamp",
                "tinyint" => "TinyInt",
                "uniqueidentifier" => "UniqueIdentifier",
                "varbinary" => "VarBinary",
                "varchar" => "VarChar",
                _ => "Variant"
            };
        }

        /// <summary>Returns default C# value for this type (used when value is null)</summary>
        public string DefaultValue()
        {
            if (IsNullable) return "null";
            return NetType switch
            {
                SqlNetType.String => "null",
                SqlNetType.ByteArray => "null",
                SqlNetType.Bool => "false",
                SqlNetType.Int or SqlNetType.Long or SqlNetType.Short or SqlNetType.Byte => "0",
                SqlNetType.Decimal => "0m",
                SqlNetType.Double => "0.0",
                SqlNetType.Float => "0f",
                SqlNetType.DateTime => "DateTime.MinValue",
                SqlNetType.TimeSpan => "TimeSpan.Zero",
                SqlNetType.Guid => "Guid.Empty",
                _ => "null"
            };
        }

        /// <summary>Build the SqlParameter size/precision suffix for stored proc params</summary>
        public string SqlParamSize()
        {
            string t = SqlTypeName.ToLowerInvariant();
            if (t == "nvarchar" || t == "varchar" || t == "char" || t == "nchar")
            {
                if (Length == -1) return ""; // MAX – no size needed; set to -1 at runtime
                return $"({Length})";
            }
            if (t == "decimal" || t == "numeric")
                return $"({Precision},{Scale})";
            return "";
        }
    }

    /// <summary>Describes a foreign-key relationship where THIS table is the child.</summary>
    public class ForeignKeyInfo
    {
        /// <summary>Column in this table (the FK column, e.g. "test1id")</summary>
        public string FkColumn { get; set; } = "";
        /// <summary>Parent table name (e.g. "tbl_1test")</summary>
        public string PkTable { get; set; } = "";
        /// <summary>Column in the parent table (e.g. "id")</summary>
        public string PkColumn { get; set; } = "";
    }

    public class TableInfo
    {
        public string TableName { get; set; } = "";
        public bool IsView { get; set; }
        public List<FieldInfo> Fields { get; set; } = new();
        public string? PrimaryKey { get; set; }
        public bool IsPrimaryKeyIdentity { get; set; }
        /// <summary>Foreign keys where this table is the child (many-side)</summary>
        public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
    }
}
