using Microsoft.Data.SqlClient;
using LayerGenForDotNet4.Models;

namespace LayerGenForDotNet4.Database
{
    /// <summary>
    /// Reads schema information directly from SQL Server using system stored procedures.
    /// No OleDb, no plugins – integrated and direct.
    /// </summary>
    public class SqlSchemaReader
    {
        private readonly string _connectionString;

        public SqlSchemaReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Test whether the connection string works</summary>
        public bool TestConnection(out string error)
        {
            error = "";
            try
            {
                using var conn = OpenConnection();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>Returns all user tables and views (excluding system tables)</summary>
        public List<TableInfo> GetTables(string schema = "dbo")
        {
            var list = new List<TableInfo>();
            using var conn = OpenConnection();

            string schemaFilter = string.IsNullOrWhiteSpace(schema) ? "dbo" : schema;

            string sql = $@"
                SELECT TABLE_NAME, TABLE_TYPE
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_CATALOG = DB_NAME()
                  AND TABLE_SCHEMA = '{schemaFilter.Replace("'", "''")}'
                  AND TABLE_NAME NOT IN (
                    'dtproperties','syscolumns','sysdepends','syscomments',
                    'sysfilegroups','sysfiles','sysfiles1','sysforeignkeys',
                    'sysproperties','sysusers','sysconstraints','syssegments','sysdiagrams'
                  )
                ORDER BY TABLE_TYPE, TABLE_NAME";

            using var cmd = new SqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new TableInfo
                {
                    TableName = rdr.GetString(0),
                    IsView = rdr.GetString(1).Trim().Equals("VIEW", StringComparison.OrdinalIgnoreCase)
                });
            }
            return list;
        }

        /// <summary>Loads full field list, primary key, identity flag into the TableInfo</summary>
        public void LoadTableDetails(TableInfo table)
        {
            using var conn = OpenConnection();

            // ── Primary key ─────────────────────────────────────────────────────────
            table.PrimaryKey = null;
            table.IsPrimaryKeyIdentity = false;

            const string pkSql = @"
                SELECT c.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE c
                     ON tc.CONSTRAINT_NAME = c.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                  AND tc.TABLE_NAME = @tbl";

            using (var cmd = new SqlCommand(pkSql, conn))
            {
                cmd.Parameters.AddWithValue("@tbl", table.TableName);
                var val = cmd.ExecuteScalar();
                if (val != null) table.PrimaryKey = val.ToString();
            }

            // ── Columns ──────────────────────────────────────────────────────────────
            const string colSql = @"
                SELECT
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.CHARACTER_MAXIMUM_LENGTH,         -- -1 = MAX, NULL for non-char
                    c.NUMERIC_PRECISION,
                    c.NUMERIC_SCALE,
                    c.IS_NULLABLE,
                    COLUMNPROPERTY(OBJECT_ID(c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY,
                    COLUMNPROPERTY(OBJECT_ID(c.TABLE_NAME), c.COLUMN_NAME, 'IsComputed') AS IS_COMPUTED
                FROM INFORMATION_SCHEMA.COLUMNS c
                WHERE c.TABLE_NAME = @tbl
                ORDER BY c.ORDINAL_POSITION";

            table.Fields.Clear();
            using (var cmd = new SqlCommand(colSql, conn))
            {
                cmd.Parameters.AddWithValue("@tbl", table.TableName);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string colName = rdr.GetString(0);
                    string dataType = rdr.GetString(1).ToLowerInvariant();
                    int length = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2);  // -1 means MAX
                    int precision = rdr.IsDBNull(3) ? 0 : Convert.ToInt32(rdr.GetValue(3));
                    int scale = rdr.IsDBNull(4) ? 0 : Convert.ToInt32(rdr.GetValue(4));
                    bool isNullable = rdr.GetString(5).Equals("YES", StringComparison.OrdinalIgnoreCase);
                    bool isIdentity = !rdr.IsDBNull(6) && Convert.ToInt32(rdr.GetValue(6)) == 1;
                    bool isComputed = !rdr.IsDBNull(7) && Convert.ToInt32(rdr.GetValue(7)) == 1;

                    var fi = new FieldInfo
                    {
                        FieldName = colName,
                        SqlTypeName = dataType,
                        Length = length,
                        Precision = precision,
                        Scale = scale,
                        IsNullable = isNullable,
                        IsIdentity = isIdentity,
                        IsComputed = isComputed,
                        IsPrimaryKey = colName.Equals(table.PrimaryKey, StringComparison.OrdinalIgnoreCase)
                    };
                    table.Fields.Add(fi);
                }
            }

            // Mark identity on the primary key
            if (table.PrimaryKey != null)
            {
                var pkField = table.Fields.FirstOrDefault(f =>
                    f.FieldName.Equals(table.PrimaryKey, StringComparison.OrdinalIgnoreCase));
                if (pkField != null)
                    table.IsPrimaryKeyIdentity = pkField.IsIdentity;
            }

            // ── Foreign Keys (this table is the child) ───────────────────────────
            const string fkSql = @"
                SELECT
                    kcu.COLUMN_NAME          AS FK_COLUMN,
                    ccu.TABLE_NAME           AS PK_TABLE,
                    ccu.COLUMN_NAME          AS PK_COLUMN
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                     ON  kcu.CONSTRAINT_NAME  = rc.CONSTRAINT_NAME
                     AND kcu.TABLE_NAME       = @tbl
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu
                     ON  ccu.CONSTRAINT_NAME  = rc.UNIQUE_CONSTRAINT_NAME
                ORDER BY kcu.ORDINAL_POSITION";

            table.ForeignKeys.Clear();
            using (var cmd = new SqlCommand(fkSql, conn))
            {
                cmd.Parameters.AddWithValue("@tbl", table.TableName);
                using var rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                {
                    table.ForeignKeys.Add(new Models.ForeignKeyInfo
                    {
                        FkColumn = rdr2.GetString(0),
                        PkTable  = rdr2.GetString(1),
                        PkColumn = rdr2.GetString(2)
                    });
                }
            }
        }

        /// <summary>
        /// Build a connection string from parts.
        /// </summary>
        public static string BuildConnectionString(
            string server, string database,
            bool windowsAuth, string user = "", string password = "")
        {
            var sb = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                TrustServerCertificate = true
            };
            if (windowsAuth)
            {
                sb.IntegratedSecurity = true;
            }
            else
            {
                sb.UserID = user;
                sb.Password = password;
            }
            return sb.ConnectionString;
        }
    }
}
