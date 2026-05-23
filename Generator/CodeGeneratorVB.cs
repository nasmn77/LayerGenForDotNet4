using LayerGenForDotNet4.Models;
using System.Text;

namespace LayerGenForDotNet4.Generator
{
    /// <summary>
    /// Generates VB.NET 4 / ASP.NET 4 compatible code – style matches original LayerGen MMX output.
    /// m_ prefix, no Using statements, manual Open/Close, CollectionBase, IComparer sort,
    /// Namespace wrapper is optional.
    /// </summary>
    public static class CodeGeneratorVB
    {
        // ─── VB keyword list ───────────────────────────────────────────────────────
        private static readonly HashSet<string> _vbKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "addhandler","addressof","alias","and","andalso","as","byref","boolean","byte",
            "byval","call","case","catch","cbool","cbyte","cchar","cdate","cdec","cdbl",
            "char","cint","class","clng","cobj","const","continue","csbyte","cshort","csng",
            "cstr","ctype","cuint","culng","cushort","date","decimal","declare","default",
            "delegate","dim","directcast","double","do","each","else","elseif","end","endif",
            "enum","erase","error","event","exit","false","finally","for","friend","function",
            "get","gettype","global","gosub","goto","handles","if","implements","imports",
            "in","inherits","integer","interface","is","isnot","let","lib","like","long",
            "loop","me","mod","module","mustinherit","mustoverride","mybase","myclass",
            "namespace","narrowing","new","next","not","nothing","notinheritable",
            "notoverridable","object","on","of","operator","option","optional","or","orelse",
            "overloads","overridable","overrides","paramarray","partial","private","property",
            "protected","public","raiseevent","readonly","redim","rem","removehandler",
            "resume","return","sbyte","select","set","shadows","shared","short","single",
            "static","step","stop","string","structure","sub","synclock","then","throw",
            "to","true","try","trycast","typeof","wend","variant","uinteger","ulong",
            "ushort","using","when","while","widening","with","withevents","writeonly","xor"
        };

        private static string SafeName(string name)
        {
            name = name.Replace(".", "");
            return _vbKeywords.Contains(name) ? $"[{name}]" : name;
        }

        private static void W(StringBuilder sb, int indent, string line = "")
        {
            if (string.IsNullOrEmpty(line))
                sb.AppendLine();
            else
                sb.AppendLine(new string('\t', indent) + line);
        }

        // ─── VB4 type mapping (no nullable ?) ─────────────────────────────────────
        private static string VbType(FieldInfo f, bool respectNullable = true)
        {
            // String and Byte() are reference types – they accept Nothing without Nullable(Of T)
            // Value types: nullable columns get ?, NOT NULL columns stay as-is
            bool isRefType = f.NetType is SqlNetType.String or SqlNetType.ByteArray;

            string baseType = f.NetType switch
            {
                SqlNetType.String    => "String",
                SqlNetType.ByteArray => "Byte()",
                SqlNetType.Int       => "Integer",
                SqlNetType.Long      => "Long",
                SqlNetType.Short     => "Short",
                SqlNetType.Byte      => "Byte",
                SqlNetType.Bool      => "Boolean",
                SqlNetType.Decimal   => "Decimal",
                SqlNetType.Double    => "Double",
                SqlNetType.Float     => "Single",
                SqlNetType.DateTime  => "Date",
                SqlNetType.TimeSpan  => "String",
                SqlNetType.Guid      => "String",
                _                    => "Object"
            };

            // Only nullable columns get ? — NOT NULL columns keep their plain type
            bool addNullable = respectNullable && f.IsNullable && !isRefType;
            return addNullable ? baseType + "?" : baseType;
        }

        // Default value when field IS NULL in fill (old style)
        private static string DefaultForFill(FieldInfo f)
        {
            bool isRefType = f.NetType is SqlNetType.String or SqlNetType.ByteArray;
            // Nullable columns and reference types → Nothing
            if (f.IsNullable || isRefType) return "Nothing";
            // NOT NULL value types → zero equivalent
            return f.NetType switch
            {
                SqlNetType.Bool                                                            => "False",
                SqlNetType.Int or SqlNetType.Long or SqlNetType.Short or SqlNetType.Byte  => "0",
                SqlNetType.Decimal or SqlNetType.Double or SqlNetType.Float               => "0",
                SqlNetType.DateTime                                                        => "Nothing",
                _                                                                          => "Nothing"
            };
        }

        /// <summary>
        /// Generate VB expression to read a field from a DataRow cell.
        /// Nullable value types use CType(cell, T?); NOT NULL value types use CType(cell, T).
        /// </summary>
        private static string VbReadFromRow(FieldInfo f, string cell)
        {
            bool isRefType = f.NetType is SqlNetType.String or SqlNetType.ByteArray;
            if (isRefType)
                return $"CType({cell}, {VbType(f)})";

            string baseType = VbType(f).TrimEnd('?');
            // Nullable → CType(cell, Boolean?); NOT NULL → CType(cell, Boolean)
            return f.IsNullable
                ? $"CType({cell}, {baseType}?)"
                : $"CType({cell}, {baseType})";
        }

        // ProperCase SP name: sp_Tbl_1Test_Select  (matches original exactly)
        private static string SpName(string tableName, string suffix)
        {
            // Format: sp_Tbl_1Test_Select
            // Each segment: capitalize the first alphabetic character (leading digits stay as-is)
            string[] parts = tableName.Split('_');
            var sb = new StringBuilder("sp");
            foreach (var p in parts)
            {
                sb.Append('_');
                if (p.Length == 0) continue;
                bool capitalized = false;
                for (int i = 0; i < p.Length; i++)
                {
                    if (!capitalized && char.IsLetter(p[i]))
                    {
                        sb.Append(char.ToUpperInvariant(p[i]));
                        capitalized = true;
                    }
                    else
                    {
                        sb.Append(p[i]);
                    }
                }
            }
            sb.Append('_');
            sb.Append(suffix);
            return sb.ToString();
        }

        // ─── namespace helpers ─────────────────────────────────────────────────────
        // suffix = "DataLayer" | "BusinessLayer" | "" (for Universal/Interfaces)
        private static string NsFull(string ns, string suffix)
        {
            bool hasNs     = !string.IsNullOrWhiteSpace(ns);
            bool hasSuffix = !string.IsNullOrWhiteSpace(suffix);
            if (!hasNs && !hasSuffix) return "";
            if (!hasNs)  return suffix;
            if (!hasSuffix) return ns;
            return $"{ns}.{suffix}";
        }

        private static void WriteNsOpen(StringBuilder sb, string ns, string suffix = "")
        {
            string full = NsFull(ns, suffix);
            if (!string.IsNullOrWhiteSpace(full))
                sb.AppendLine($"Namespace {full}");
        }
        private static void WriteNsClose(StringBuilder sb, string ns, string suffix = "")
        {
            string full = NsFull(ns, suffix);
            if (!string.IsNullOrWhiteSpace(full))
                sb.AppendLine("End Namespace");
        }

        // Prefix used when calling DataLayer/BusinessLayer types from BusinessLayer
        private static string DL(string ns) =>
            string.IsNullOrWhiteSpace(ns) ? "DataLayer." : $"{ns}.DataLayer.";
        private static string BL(string ns) =>
            string.IsNullOrWhiteSpace(ns) ? "BusinessLayer." : $"{ns}.BusinessLayer.";

        // ═══════════════════════════════════════════════════════════════════════════
        //  DATA LAYER
        // ═══════════════════════════════════════════════════════════════════════════

        public static string GenerateDataLayer(TableInfo table, string ns, bool suppressComments,
                                               bool falseErase = false, string falseEraseField = "IsDeleted")
        {
            var sb = new StringBuilder();
            string cn   = SafeName(table.TableName);
            string pk   = table.PrimaryKey ?? "";
            var pkField = pk != "" ? table.Fields.FirstOrDefault(f => f.FieldName == pk) : null;
            string dl   = DL(ns);
            string bl   = BL(ns);

            sb.AppendLine("Option Strict On");
            sb.AppendLine("Option Explicit On");
            sb.AppendLine();
            sb.AppendLine("Imports Microsoft.VisualBasic");
            sb.AppendLine("Imports System");
            sb.AppendLine("Imports System.Data");
            sb.AppendLine();
            WriteNsOpen(sb, ns, "DataLayer");
            if (!string.IsNullOrWhiteSpace(NsFull(ns, "DataLayer"))) sb.AppendLine();

            sb.AppendLine($"#Region \"{cn} Class\"");
            sb.AppendLine($"<Serializable()> Partial Public Class {cn}");
            sb.AppendLine();

            // ── Instance Variables ────────────────────────────────────────────────
            sb.AppendLine("#Region \"Instance Variables\"");
            foreach (var f in table.Fields)
                W(sb, 1, $"Protected m_{f.FieldName} As {VbType(f)}");
            W(sb, 1, "Protected m_IsDirty As Boolean");
            W(sb, 1, "Protected m_ConnectString As String");
            W(sb, 1, "Protected m_IsUpdate As Boolean");
            sb.AppendLine();
            // FK instance vars
            foreach (var fk in table.ForeignKeys)
                W(sb, 1, $"Protected my{fk.FkColumn} As {bl}{fk.PkTable}");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // ── Constructors ──────────────────────────────────────────────────────
            sb.AppendLine("#Region \"Constructors\"");
            W(sb, 1, "Public Sub New()");
            W(sb, 2, "SetConnectString()");
            W(sb, 2, "m_IsUpdate = False");
            W(sb, 1, "End Sub");
            sb.AppendLine();

            if (pkField != null)
            {
                W(sb, 1, $"Public Sub New(ByVal pkPrimaryKey As {VbType(pkField, respectNullable: false)})");
                W(sb, 2, "m_IsUpdate = True");
                W(sb, 2, "SetConnectString()");
                W(sb, 2, "Me.Get(pkPrimaryKey)");
                W(sb, 1, "End Sub");
                sb.AppendLine();
            }

            W(sb, 1, "Public Sub New(ByVal dtrRow As DataRow)");
            W(sb, 2, "m_IsUpdate = True");
            W(sb, 2, "SetConnectString()");
            W(sb, 2, "Me.Fill(dtrRow)");
            W(sb, 1, "End Sub");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // ── Properties ────────────────────────────────────────────────────────
            sb.AppendLine("#Region \"Properties\"");
            foreach (var f in table.Fields)
            {
                string vt = VbType(f);
                if (f.IsComputed)
                {
                    W(sb, 1, $"Public ReadOnly Property {SafeName(f.FieldName)}() As {vt}");
                    W(sb, 2, "Get");
                    W(sb, 3, $"Return Me.m_{f.FieldName}");
                    W(sb, 2, "End Get");
                    W(sb, 1, "End Property");
                }
                else
                {
                    W(sb, 1, $"Public Property {SafeName(f.FieldName)}() As {vt}");
                    W(sb, 2, "Get");
                    W(sb, 3, $"Return Me.m_{f.FieldName}");
                    W(sb, 2, "End Get");
                    W(sb, 2, $"Set(ByVal Value As {vt})");
                    W(sb, 3, "Me.m_IsDirty = True");
                    W(sb, 3, $"Me.m_{f.FieldName} = Value");
                    W(sb, 2, "End Set");
                    W(sb, 1, "End Property");
                }
                sb.AppendLine();
            }

            // FK properties
            foreach (var fk in table.ForeignKeys)
            {
                string fkType  = $"{bl}{fk.PkTable}";
                var fkField    = table.Fields.FirstOrDefault(f => f.FieldName == fk.FkColumn);
                bool fkNullable = fkField?.IsNullable ?? false;
                // إذا nullable: نتحقق من HasValue قبل إنشاء الكائن، ونستخدم .Value
                string fkVal   = fkNullable ? $"Me.m_{fk.FkColumn}.Value" : $"Me.m_{fk.FkColumn}";
                string fkGuard = fkNullable
                    ? $"If ((Me.my{fk.FkColumn} Is Nothing) AndAlso Me.m_{fk.FkColumn}.HasValue) Then"
                    : $"If ((Me.my{fk.FkColumn} Is Nothing)) Then";

                W(sb, 1, $"Public Property F{fk.FkColumn} As {fkType}");
                W(sb, 2, "Get");
                W(sb, 3, fkGuard);
                W(sb, 4, $"Me.my{fk.FkColumn} = New {fkType}({fkVal})");
                W(sb, 3, "End If");
                W(sb, 3, $"Return Me.my{fk.FkColumn}");
                W(sb, 2, "End Get");
                W(sb, 2, $"Set(ByVal value As {fkType})");
                W(sb, 3, $"Me.my{fk.FkColumn} = value");
                W(sb, 2, "End Set");
                W(sb, 1, "End Property");
                sb.AppendLine();
            }

            sb.AppendLine("#End Region");
            sb.AppendLine();

            // ── Connection Routines ───────────────────────────────────────────────
            sb.AppendLine("#Region \"Connection Routines\"");
            W(sb, 1, "Protected Sub SetConnectString()");
            W(sb, 2, $"m_ConnectString = {dl}Universal.GetConnectionString()");
            W(sb, 1, "End Sub");
            sb.AppendLine();
            W(sb, 1, "Protected Function GetConnectString() As String");
            W(sb, 2, "Return m_ConnectString");
            W(sb, 1, "End Function");
            sb.AppendLine();
            W(sb, 1, "Protected Shared Function GetConnectionString() As String");
            W(sb, 2, $"Return {dl}Universal.GetConnectionString()");
            W(sb, 1, "End Function");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // ── Copy ─────────────────────────────────────────────────────────────
            sb.AppendLine("#Region \"Copy SubRoutine\"");
            W(sb, 1, "Public Sub Copy(dtrRow As DataRow)");
            foreach (var f in table.Fields)
            {
                W(sb, 2, "Try");
                W(sb, 3, $"Me.m_{f.FieldName} = {VbReadFromRow(f, $"dtrRow(\"{f.FieldName}\")")}");
                W(sb, 2, "Catch ex As Exception");
                W(sb, 2, "End Try");
                sb.AppendLine();
            }
            W(sb, 1, "End Sub");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // ── Fill ─────────────────────────────────────────────────────────────
            sb.AppendLine("#Region \"Fill SubRoutine\"");
            W(sb, 1, "Protected Sub Fill(ByVal dtrRow As DataRow)");
            foreach (var f in table.Fields)
            {
                W(sb, 2, "Try");
                W(sb, 3, $"If (Not dtrRow(\"{f.FieldName}\") Is DBNull.Value) Then");
                W(sb, 4, $"Me.m_{f.FieldName} = {VbReadFromRow(f, $"dtrRow(\"{f.FieldName}\")")}");
                if (f.IsPrimaryKey)
                    W(sb, 4, "m_IsUpdate = True");
                W(sb, 3, "Else");
                W(sb, 4, $"Me.m_{f.FieldName} = {DefaultForFill(f)}");
                if (f.IsPrimaryKey)
                    W(sb, 4, "m_IsUpdate = False");
                W(sb, 3, "End If");
                W(sb, 2, "Catch ex As Exception");
                if (f.IsPrimaryKey)
                    W(sb, 3, "m_IsUpdate = False");
                W(sb, 2, "End Try");
            }
            W(sb, 1, "End Sub");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // ── Get ───────────────────────────────────────────────────────────────
            if (pkField != null)
            {
                string spSel = $"[dbo].{SpName(table.TableName, "Select")}";
                sb.AppendLine("#Region \"Get SubRoutine\"");
                W(sb, 1, $"Private Sub [Get](ByVal primID As {VbType(pkField)})");
                W(sb, 2, $"Dim storedProcedure As String = \"{spSel}\"");
                W(sb, 2, "Dim cmd As New SqlClient.SqlCommand");
                W(sb, 2, "Dim ds As DataSet");
                W(sb, 2, "Dim ConnectionString As String = GetConnectString()");
                W(sb, 2, "Dim conn As New SqlClient.SqlConnection");
                W(sb, 2, "Dim da As SqlClient.SqlDataAdapter");
                sb.AppendLine();
                W(sb, 2, "conn.ConnectionString = ConnectionString");
                W(sb, 2, "cmd.CommandType = CommandType.StoredProcedure");
                W(sb, 2, "cmd.Connection = conn");
                W(sb, 2, "cmd.CommandText = storedProcedure");
                W(sb, 2, $"cmd.Parameters.Add(\"@{pk}\", SqlDbType.{pkField.SqlDbTypeStr()})");
                W(sb, 2, $"cmd.Parameters(\"@{pk}\").Value = primID");
                sb.AppendLine();
                W(sb, 2, "Try");
                W(sb, 3, "conn.Open()");
                W(sb, 3, "da = New SqlClient.SqlDataAdapter()");
                W(sb, 3, "da.SelectCommand = cmd");
                W(sb, 3, "ds = New DataSet()");
                W(sb, 3, "da.Fill(ds)");
                W(sb, 3, "If ds.Tables.Count > 0 Then");
                W(sb, 4, "If ds.Tables(0).Rows.Count > 0 Then");
                W(sb, 5, "Fill(ds.Tables(0).Rows(0))");
                W(sb, 4, "End If");
                W(sb, 3, "End If");
                W(sb, 2, "Catch ex As Exception");
                W(sb, 3, "Throw ex");
                W(sb, 2, "End Try");
                W(sb, 2, "conn.Close()");
                W(sb, 1, "End Sub");
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }

            // ── Delete ────────────────────────────────────────────────────────────
            if (pkField != null && !table.IsView)
            {
                string spDel = $"[dbo].{SpName(table.TableName, "Delete")}";
                sb.AppendLine("#Region \"Delete SubRoutine\"");
                if (!suppressComments && falseErase && falseEraseField != "")
                {
                    W(sb, 1, "' This sub will not really delete the row from the database.");
                    W(sb, 1, $"' The stored procedure [{spDel}] sets [{falseEraseField}] = 1.");
                    W(sb, 1, "' Records where \"" + falseEraseField + "\" is true are excluded from all queries.");
                }
                W(sb, 1, "Public Sub Delete()");
                W(sb, 2, $"Dim storedProcedure As String = \"{spDel}\"");
                W(sb, 2, "Dim cmd As New SqlClient.SqlCommand");
                W(sb, 2, "Dim ConnectionString As String = GetConnectString()");
                W(sb, 2, "Dim conn As New SqlClient.SqlConnection");
                sb.AppendLine();
                W(sb, 2, "conn.ConnectionString = ConnectionString");
                W(sb, 2, "cmd.CommandType = CommandType.StoredProcedure");
                W(sb, 2, "cmd.Connection = conn");
                W(sb, 2, "cmd.CommandText = storedProcedure");
                W(sb, 2, $"cmd.Parameters.Add(\"@{pk}\", SqlDbType.{pkField.SqlDbTypeStr()})");
                W(sb, 2, $"cmd.Parameters(\"@{pk}\").Value = Me.m_{pk}");
                sb.AppendLine();
                W(sb, 2, "Try");
                W(sb, 3, "conn.Open()");
                W(sb, 3, "cmd.ExecuteNonQuery()");
                W(sb, 2, "Catch ex As Exception");
                W(sb, 3, "Throw ex");
                W(sb, 2, "End Try");
                W(sb, 2, "conn.Close()");
                W(sb, 1, "End Sub");
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }

            // ── Save ─────────────────────────────────────────────────────────────
            if (!table.IsView)
            {
                string spIns = $"[dbo].{SpName(table.TableName, "Insert")}";
                string spUpd = $"[dbo].{SpName(table.TableName, "Update")}";

                sb.AppendLine("#Region \"Save Subroutine\"");
                sb.AppendLine();
                W(sb, 1, "Public Sub Save()");
                W(sb, 2, "Dim storedProcedure As String");
                W(sb, 2, "Dim cmd As New SqlClient.SqlCommand");
                W(sb, 2, "Dim ConnectionString As String = GetConnectString()");
                W(sb, 2, "Dim conn As New SqlClient.SqlConnection");
                sb.AppendLine();

                // FK cascade
                foreach (var fk in table.ForeignKeys)
                {
                    W(sb, 2, $"If Not (Me.my{fk.FkColumn} Is Nothing) Then");
                    W(sb, 3, $"my{fk.FkColumn}.Save()");
                    W(sb, 2, "End If");
                    sb.AppendLine();
                }

                // FalseErase field is always excluded (managed only by Delete SP)
                bool hasFEField = falseEraseField != "" && table.Fields.Any(ff => ff.FieldName.Equals(falseEraseField, StringComparison.OrdinalIgnoreCase));

                // Build validation condition for NOT NULL fields
                var requiredFields = table.Fields
                    .Where(f => !f.IsNullable && !f.IsPrimaryKey && !f.IsComputed &&
                                !(hasFEField && f.FieldName.Equals(falseEraseField, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Build combined If: m_IsDirty AndAlso field1OK AndAlso ...
                // Only String/Byte() NOT NULL need a runtime check — value types (Boolean, Integer…)
                // always have a value so no check needed.
                var allConditions = new List<string> { "m_IsDirty" };
                foreach (var rf in requiredFields)
                {
                    bool rfIsRef = rf.NetType is SqlNetType.String or SqlNetType.ByteArray;
                    if (rfIsRef)
                        allConditions.Add($"Not IsNothing(Me.m_{rf.FieldName})");
                    // non-nullable value types always have a value — no condition needed
                }

                // Write single If with all conditions joined
                string ifLine = "If (" + string.Join(" AndAlso _" + Environment.NewLine + "\t\t\t", allConditions) + ") Then";
                W(sb, 2, ifLine);
                sb.AppendLine();

                W(sb, 3, "conn.ConnectionString = ConnectionString");
                W(sb, 3, "cmd.Connection = conn");
                W(sb, 3, "cmd.CommandType = CommandType.StoredProcedure");
                sb.AppendLine();

                // PK parameter first
                if (pkField != null)
                    W(sb, 3, $"cmd.Parameters.Add(\"@{pk}\", SqlDbType.{pkField.SqlDbTypeStr()})");

                // Other parameters
                foreach (var f in table.Fields)
                {
                    if (f.IsPrimaryKey) continue;
                    if (f.IsComputed) continue;
                    if (hasFEField && f.FieldName.Equals(falseEraseField, StringComparison.OrdinalIgnoreCase)) continue;

                    string vt = VbType(f);
                    string tLow = f.SqlTypeName.ToLowerInvariant();
                    string sqlDbType = f.SqlDbTypeStr();

                    if (tLow is "nvarchar" or "varchar" or "char" or "nchar")
                    {
                        int sz = f.Length == -1 ? -1 : f.Length;
                        W(sb, 3, $"cmd.Parameters.Add(\"@{f.FieldName}\", SqlDbType.{sqlDbType}, {sz})");
                    }
                    else if (tLow is "decimal" or "numeric")
                    {
                        W(sb, 3, $"cmd.Parameters.Add(\"@{f.FieldName}\", SqlDbType.{sqlDbType})");
                        W(sb, 3, $"cmd.Parameters(\"@{f.FieldName}\").Precision = {f.Precision}");
                        W(sb, 3, $"cmd.Parameters(\"@{f.FieldName}\").Scale = {f.Scale}");
                    }
                    else
                        W(sb, 3, $"cmd.Parameters.Add(\"@{f.FieldName}\", SqlDbType.{sqlDbType})");

                    // Set parameter value with proper null handling
                    bool isRefType = f.NetType is SqlNetType.String or SqlNetType.ByteArray;
                    bool isNullableVal = f.IsNullable && !isRefType;  // Boolean?, Integer? …

                    if (isRefType && f.IsNullable)
                    {
                        // String nullable → DBNull when Nothing
                        W(sb, 3, $"If Not IsNothing(Me.m_{f.FieldName}) Then");
                        W(sb, 4, $"cmd.Parameters(\"@{f.FieldName}\").Value = Me.m_{f.FieldName}");
                        W(sb, 3, "Else");
                        W(sb, 4, $"cmd.Parameters(\"@{f.FieldName}\").Value = DBNull.Value");
                        W(sb, 3, "End If");
                    }
                    else if (isRefType)
                    {
                        // String NOT NULL → send directly
                        W(sb, 3, $"cmd.Parameters(\"@{f.FieldName}\").Value = Me.m_{f.FieldName}");
                    }
                    else if (isNullableVal)
                    {
                        // Boolean?, Integer?, Date? → HasValue
                        W(sb, 3, $"If Me.m_{f.FieldName}.HasValue Then");
                        W(sb, 4, $"cmd.Parameters(\"@{f.FieldName}\").Value = Me.m_{f.FieldName}.Value");
                        W(sb, 3, "Else");
                        W(sb, 4, $"cmd.Parameters(\"@{f.FieldName}\").Value = DBNull.Value");
                        W(sb, 3, "End If");
                    }
                    else
                    {
                        // Boolean, Integer, Date NOT NULL → send directly
                        W(sb, 3, $"cmd.Parameters(\"@{f.FieldName}\").Value = Me.m_{f.FieldName}");
                    }
                    sb.AppendLine();
                }

                W(sb, 3, "If m_IsUpdate = True Then");
                W(sb, 4, $"storedProcedure = \"{spUpd}\"");
                W(sb, 4, "cmd.CommandText = storedProcedure");
                sb.AppendLine();
                if (pkField != null)
                {
                    W(sb, 4, $"cmd.Parameters(\"@{pk}\").Value = Me.m_{pk}");
                    W(sb, 4, $"cmd.Parameters(\"@{pk}\").Direction = ParameterDirection.Input");
                }
                W(sb, 4, "Try");
                W(sb, 5, "conn.Open()");
                W(sb, 5, "cmd.ExecuteNonQuery()");
                W(sb, 4, "Catch ex As Exception");
                W(sb, 5, "Throw ex");
                W(sb, 4, "End Try");
                W(sb, 4, "conn.Close()");
                W(sb, 3, "Else");
                W(sb, 4, $"storedProcedure = \"{spIns}\"");
                W(sb, 4, "cmd.CommandText = storedProcedure");
                sb.AppendLine();
                if (pkField != null)
                {
                    W(sb, 4, $"cmd.Parameters(\"@{pk}\").Value = Me.m_{pk}");
                    W(sb, 4, $"cmd.Parameters(\"@{pk}\").Direction = ParameterDirection.Output");
                }
                W(sb, 4, "Try");
                W(sb, 5, "conn.Open()");
                W(sb, 5, "cmd.ExecuteNonQuery()");
                W(sb, 4, "Catch ex As Exception");
                W(sb, 5, "Throw ex");
                W(sb, 4, "End Try");
                if (pkField != null)
                    W(sb, 4, $"Me.m_{pk} = CType(cmd.Parameters(\"@{pk}\").Value, {VbType(pkField, false)})");
                W(sb, 4, "conn.Close()");
                W(sb, 3, "End If");
                W(sb, 2, "End If");
                W(sb, 2, "m_IsDirty = False");
                W(sb, 2, "m_IsUpdate = True");
                W(sb, 1, "End Sub");
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }

            // ── GetAll ────────────────────────────────────────────────────────────
            {
                string spGetAll = $"[dbo].{SpName(table.TableName, "GetAll")}";
                sb.AppendLine("#Region \"GetAll SubRoutine\"");
                W(sb, 1, "Public Shared Function GetAll() As DataTable");
                W(sb, 2, $"Dim storedProcedure As String = \"{spGetAll}\"");
                W(sb, 2, "Dim cmd As New SqlClient.SqlCommand");
                W(sb, 2, "Dim ConnectionString As String = GetConnectionString()");
                W(sb, 2, "Dim conn As New SqlClient.SqlConnection");
                sb.AppendLine();
                W(sb, 2, "conn.ConnectionString = ConnectionString");
                W(sb, 2, "cmd.Connection = conn");
                W(sb, 2, "cmd.CommandType = CommandType.StoredProcedure");
                W(sb, 2, "cmd.CommandText = storedProcedure");
                sb.AppendLine();
                W(sb, 2, "Try");
                W(sb, 3, "Dim ds As DataSet");
                W(sb, 3, "Dim da As SqlClient.SqlDataAdapter");
                W(sb, 3, "Dim Table As DataTable = Nothing");
                sb.AppendLine();
                W(sb, 3, "conn.Open()");
                W(sb, 3, "da = New SqlClient.SqlDataAdapter()");
                W(sb, 3, "da.SelectCommand = cmd");
                W(sb, 3, "ds = New DataSet()");
                W(sb, 3, "da.Fill(ds)");
                sb.AppendLine();
                W(sb, 3, "If ds.Tables.Count > 0 Then");
                W(sb, 4, "If ds.Tables(0).Rows.Count > 0 Then");
                W(sb, 5, "Table = ds.Tables(0)");
                W(sb, 4, "End If");
                W(sb, 3, "End If");
                W(sb, 3, "conn.Close()");
                W(sb, 3, "Return Table");
                W(sb, 2, "Catch ex As Exception");
                W(sb, 3, "Throw ex");
                W(sb, 2, "End Try");
                W(sb, 1, "End Function");
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }

            // ── GetByFK methods ───────────────────────────────────────────────────
            foreach (var fk in table.ForeignKeys)
            {
                var fkColField = table.Fields.FirstOrDefault(f => f.FieldName.Equals(fk.FkColumn, StringComparison.OrdinalIgnoreCase));
                string fkVbType = fkColField != null ? VbType(fkColField) : "Integer";
                string fkSqlType = fkColField?.SqlDbTypeStr() ?? "Int";
                string spGetBy = $"[dbo].{SpName(table.TableName, $"GetBy{fk.FkColumn}")}";

                sb.AppendLine($"#Region \"GetBy{fk.FkColumn} SubRoutine\"");
                W(sb, 1, $"Public Shared Function GetBy{fk.FkColumn}(ByVal {fk.FkColumn} As {fkVbType}) As DataTable");
                W(sb, 2, $"Dim storedProcedure As String = \"{spGetBy}\"");
                W(sb, 2, "Dim cmd As New SqlClient.SqlCommand");
                W(sb, 2, "Dim ConnectionString As String = GetConnectionString()");
                W(sb, 2, "Dim Conn As New SqlClient.SqlConnection");
                sb.AppendLine();
                W(sb, 2, "conn.ConnectionString = ConnectionString");
                W(sb, 2, "cmd.CommandType = CommandType.StoredProcedure");
                W(sb, 2, "cmd.Connection = Conn");
                W(sb, 2, "cmd.CommandText = storedProcedure");
                W(sb, 2, $"cmd.Parameters.Add(\"@{fk.FkColumn}\", SqlDbType.{fkSqlType})");
                W(sb, 2, $"cmd.Parameters(\"@{fk.FkColumn}\").Value = {fk.FkColumn}");
                sb.AppendLine();
                W(sb, 2, "Try");
                W(sb, 3, "Dim ds As DataSet");
                W(sb, 3, "Dim da As SqlClient.SqlDataAdapter");
                W(sb, 3, "Dim Table As DataTable = Nothing");
                sb.AppendLine();
                W(sb, 3, "Conn.Open()");
                W(sb, 3, "da = New SqlClient.SqlDataAdapter");
                W(sb, 3, "da.SelectCommand = cmd");
                W(sb, 3, "ds = New DataSet()");
                W(sb, 3, "da.Fill(ds)");
                sb.AppendLine();
                W(sb, 3, "If ds.Tables.Count > 0 Then");
                W(sb, 4, "If ds.Tables(0).Rows.Count > 0 Then");
                W(sb, 5, "Table = ds.Tables(0)");
                W(sb, 4, "End If");
                W(sb, 3, "End If");
                W(sb, 3, "conn.Close()");
                W(sb, 3, "Return Table");
                W(sb, 2, "Catch ex As Exception");
                W(sb, 3, "Throw ex");
                W(sb, 2, "End Try");
                W(sb, 2, "Conn.Close()");
                W(sb, 1, "End Function");
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }

            // ── GetBySQLStatement ─────────────────────────────────────────────────
            sb.AppendLine("#Region \"GetBySQLStatement SubRoutine\"");
            W(sb, 1, "Public Shared Function GetBySQLStatement(ByVal SQLText As String) As DataTable");
            W(sb, 2, "Dim cmd As New SqlClient.SqlCommand");
            W(sb, 2, "Dim ConnectionString As String = GetConnectionString()");
            W(sb, 2, "Dim Conn As New SqlClient.SqlConnection");
            sb.AppendLine();
            W(sb, 2, "conn.ConnectionString = ConnectionString");
            W(sb, 2, "cmd.CommandType = CommandType.Text");
            W(sb, 2, "cmd.Connection = Conn");
            W(sb, 2, "cmd.CommandText = SQLText");
            sb.AppendLine();
            W(sb, 2, "Try");
            W(sb, 3, "Dim ds As DataSet");
            W(sb, 3, "Dim da As SqlClient.SqlDataAdapter");
            W(sb, 3, "Dim Table As DataTable = Nothing");
            sb.AppendLine();
            W(sb, 3, "Conn.Open()");
            W(sb, 3, "da = New SqlClient.SqlDataAdapter");
            W(sb, 3, "da.SelectCommand = cmd");
            W(sb, 3, "ds = New DataSet()");
            W(sb, 3, "da.Fill(ds)");
            sb.AppendLine();
            W(sb, 3, "If ds.Tables.Count > 0 Then");
            W(sb, 4, "If ds.Tables(0).Rows.Count > 0 Then");
            W(sb, 5, "Table = ds.Tables(0)");
            W(sb, 4, "End If");
            W(sb, 3, "End If");
            W(sb, 3, "conn.Close()");
            W(sb, 3, "Return Table");
            W(sb, 2, "Catch ex As Exception");
            W(sb, 3, "Throw ex");
            W(sb, 2, "End Try");
            W(sb, 1, "End Function");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            sb.AppendLine("End Class");
            sb.AppendLine("#End Region");
            if (!string.IsNullOrWhiteSpace(NsFull(ns, "DataLayer"))) { sb.AppendLine(); WriteNsClose(sb, ns, "DataLayer"); }

            return sb.ToString();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUSINESS LAYER
        // ═══════════════════════════════════════════════════════════════════════════

        public static string GenerateBusinessLayer(TableInfo table, string ns, bool suppressComments)
        {
            var sb = new StringBuilder();
            string cn   = SafeName(table.TableName);
            string cns  = cn + "s";
            string dl   = DL(ns);
            string bl   = BL(ns);
            var pkField = table.PrimaryKey != null
                ? table.Fields.FirstOrDefault(f => f.FieldName == table.PrimaryKey)
                : null;

            sb.AppendLine("Option Strict On");
            sb.AppendLine("Option Explicit On");
            sb.AppendLine();
            sb.AppendLine("Imports Microsoft.VisualBasic");
            sb.AppendLine("Imports System");
            sb.AppendLine("Imports System.Data");
            sb.AppendLine();
            WriteNsOpen(sb, ns, "BusinessLayer");
            if (!string.IsNullOrWhiteSpace(NsFull(ns, "BusinessLayer"))) sb.AppendLine();

            // ── Business single class ─────────────────────────────────────────────
            sb.AppendLine($"#Region \"{cn} Class\"");
            string dlcn = string.IsNullOrWhiteSpace(ns) ? $"DataLayer.{cn}" : $"{ns}.DataLayer.{cn}";
            sb.AppendLine($"<Serializable()> Public Class {cn}");
            W(sb, 1, $"Inherits {dlcn}");
            W(sb, 1, "Implements BusinessLayer.IBusiness");
            sb.AppendLine();

            sb.AppendLine("#Region \"Constructors\"");
            W(sb, 1, "Public Sub New()");
            W(sb, 2, "MyBase.New()");
            W(sb, 1, "End Sub");
            sb.AppendLine();
            if (pkField != null && !table.IsView)
            {
                W(sb, 1, $"Public Sub New(ByVal PrimaryKey As {VbType(pkField, respectNullable: false)})");
                W(sb, 2, "MyBase.New(PrimaryKey)");
                W(sb, 1, "End Sub");
                sb.AppendLine();
            }
            W(sb, 1, "Public Sub New(ByVal dtrRow As DataRow)");
            W(sb, 2, "MyBase.New(dtrRow)");
            W(sb, 1, "End Sub");
            sb.AppendLine();
            sb.AppendLine("#End Region");
            sb.AppendLine();

            sb.AppendLine("#Region \"Interface Implementation\"");
            W(sb, 1, "Public Sub Rollback() Implements IBusiness.Rollback");
            W(sb, 1, "End Sub");
            sb.AppendLine();
            W(sb, 1, "Public Sub Validate() Implements IBusiness.Validate");
            W(sb, 2, "' TODO: Write your own validation code");
            W(sb, 1, "End Sub");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            sb.AppendLine("End Class");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // ── Collection class ──────────────────────────────────────────────────
            string blcn = string.IsNullOrWhiteSpace(ns) ? $"BusinessLayer.{cn}" : $"{ns}.BusinessLayer.{cn}";
            sb.AppendLine($"#Region \"{cns} Class\"");
            sb.AppendLine($"<Serializable()> Partial Public Class {cns} : Inherits CollectionBase");
            W(sb, 1, $"Implements System.Collections.Generic.IEnumerable(Of {blcn})");
            sb.AppendLine();

            // LINQ
            sb.AppendLine("#Region \"LINQ Implementation\"");
            W(sb, 1, $"Public Shadows Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of {blcn}) Implements IEnumerable(Of {blcn}).GetEnumerator");
            W(sb, 2, $"Return InnerList.Cast(Of {blcn})().GetEnumerator()");
            W(sb, 1, "End Function");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // Sort fields enum
            var sortableFields = table.Fields
                .Where(f => f.NetType is not (SqlNetType.ByteArray or SqlNetType.Object or SqlNetType.Bool))
                .ToList();

            sb.AppendLine("#Region \"Enumerated Sort Fields\"");
            W(sb, 1, "Public Enum SortFields");
            foreach (var f in sortableFields)
                W(sb, 2, $"Sort_{f.FieldName}");
            W(sb, 1, "End Enum");
            sb.AppendLine();
            W(sb, 1, "Public Enum SortType");
            W(sb, 2, "Ascending = 0");
            W(sb, 2, "Descending = 1");
            W(sb, 1, "End Enum");
            sb.AppendLine();
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // Sort Comparers
            sb.AppendLine("#Region \"Sort Comparers\"");
            foreach (var f in sortableFields)
            {
                string fn  = SafeName(f.FieldName);
                // للـ String نستخدم String.Compare، للـ Nullable نستخدم GetValueOrDefault، للبقية CompareTo مباشرة
                string cmpAsc = f.NetType == SqlNetType.String
                    ? $"String.Compare(o1.{fn}, o2.{fn}, StringComparison.CurrentCulture)"
                    : f.IsNullable
                        ? $"o1.{fn}.GetValueOrDefault().CompareTo(o2.{fn}.GetValueOrDefault())"
                        : $"o1.{fn}.CompareTo(o2.{fn})";

                // Ascending
                W(sb, 1, $"Private Class Comp_{f.FieldName}");
                W(sb, 2, "Implements IComparer");
                sb.AppendLine();
                W(sb, 2, $"Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare");
                W(sb, 3, $"Dim o1 As {blcn} = CType(x, {blcn})");
                W(sb, 3, $"Dim o2 As {blcn} = CType(y, {blcn})");
                W(sb, 3, "Try");
                W(sb, 4, $"Return {cmpAsc}");
                W(sb, 3, "Catch ex As Exception");
                W(sb, 4, "Return 0");
                W(sb, 3, "End Try");
                W(sb, 2, "End Function");
                W(sb, 1, "End Class");
                sb.AppendLine();

                // Descending
                W(sb, 1, $"Private Class Comp_{f.FieldName}_D");
                W(sb, 2, "Implements IComparer");
                sb.AppendLine();
                W(sb, 2, $"Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare");
                W(sb, 3, $"Dim o1 As {blcn} = CType(x, {blcn})");
                W(sb, 3, $"Dim o2 As {blcn} = CType(y, {blcn})");
                W(sb, 3, "Dim j As Integer");
                W(sb, 3, "Try");
                W(sb, 4, $"j = {cmpAsc}");
                W(sb, 4, "If j > 0 Then Return -1");
                W(sb, 4, "If j < 0 Then Return 1");
                W(sb, 3, "Catch ex As Exception");
                W(sb, 4, "j = 0");
                W(sb, 3, "End Try");
                W(sb, 3, "Return 0");
                W(sb, 2, "End Function");
                W(sb, 1, "End Class");
                sb.AppendLine();
            }
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // Sort Functions
            sb.AppendLine("#Region \"Sort Functions\"");
            W(sb, 1, "Public Sub Sort(ByVal SortField As SortFields, ByVal SortMethod As SortType)");
            foreach (var f in sortableFields)
            {
                W(sb, 2, $"If SortField = SortFields.Sort_{f.FieldName} Then");
                W(sb, 3, "If SortMethod = SortType.Ascending Then");
                W(sb, 4, $"Me.InnerList.Sort(New Comp_{f.FieldName})");
                W(sb, 3, "Else");
                W(sb, 4, $"Me.InnerList.Sort(New Comp_{f.FieldName}_D)");
                W(sb, 3, "End If");
                W(sb, 2, "End If");
                sb.AppendLine();
            }
            W(sb, 1, "End Sub");
            sb.AppendLine();
            W(sb, 1, "Public Sub Sort(ByVal SortField As SortFields)");
            W(sb, 2, "Sort(SortField, SortType.Ascending)");
            W(sb, 1, "End Sub");
            sb.AppendLine();
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // Constructors
            sb.AppendLine("\t#Region \"Constructors\"");
            W(sb, 2, "Public Sub New()");
            W(sb, 2, "End Sub");
            sb.AppendLine();
            W(sb, 2, "Public Sub New(ByVal datarows As DataRowCollection)");
            W(sb, 3, "Me.New()");
            W(sb, 3, "Me.Load(datarows)");
            W(sb, 2, "End Sub");
            sb.AppendLine("\t#End Region");
            sb.AppendLine();

            // Member Functions
            sb.AppendLine("#Region \"Member Functions\"");
            W(sb, 1, "Protected Sub Load(ByVal dataRows As DataRowCollection)");
            W(sb, 2, $"For Each dr As DataRow In dataRows");
            W(sb, 3, $"Me.Add(New {cn}(dr))");
            W(sb, 2, "Next");
            W(sb, 1, "End Sub");
            sb.AppendLine();

            W(sb, 1, $"Default Public Property {cn}(ByVal index As Integer) As {cn}");
            W(sb, 2, "Get");
            W(sb, 3, $"Return CType(MyBase.InnerList.Item(index), {cn})");
            W(sb, 2, "End Get");
            W(sb, 2, $"Set(ByVal value As {cn})");
            W(sb, 3, "MyBase.InnerList.Item(index) = value");
            W(sb, 2, "End Set");
            W(sb, 1, "End Property");
            sb.AppendLine();
            W(sb, 1, $"Public Function Add(ByVal val As {cn}) As Integer");
            W(sb, 2, "Return MyBase.InnerList.Add(val)");
            W(sb, 1, "End Function");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // Save
            if (!table.IsView)
            {
                sb.AppendLine("#Region \"Save Function\"");
                W(sb, 1, "Public Sub Save()");
                W(sb, 2, $"For Each a As {cn} In Me.InnerList");
                W(sb, 3, "a.Save()");
                W(sb, 2, "Next");
                W(sb, 1, "End Sub");
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }

            // Rollback
            sb.AppendLine("#Region \"Rollback Sub\"");
            W(sb, 1, "Public Sub Rollback");
            W(sb, 2, $"For Each a As {cn} In Me.InnerList");
            W(sb, 3, "a.Rollback()");
            W(sb, 2, "Next");
            W(sb, 1, "End Sub");
            sb.AppendLine("#End Region");
            sb.AppendLine();

            // GetAll
            sb.AppendLine("#Region \"GetAll Subroutine\"");
            W(sb, 1, "Public Sub GetAll()");
            W(sb, 2, $"Dim dt As DataTable = {dl}{cn}.GetAll()");
            W(sb, 2, "If Not (dt Is Nothing) Then");
            W(sb, 3, "Me.Load(dt.Rows)");
            W(sb, 2, "End If");
            W(sb, 1, "End Sub");
            sb.AppendLine();
            sb.AppendLine("#End Region");
            sb.AppendLine();

            W(sb, 1, "Public Sub GetBySQLStatement(SQLText As String)");
            W(sb, 2, $"Dim dt As DataTable = {dl}{cn}.GetBySQLStatement(SQLText)");
            W(sb, 2, "If Not (dt Is Nothing) Then");
            W(sb, 3, "Me.Load(dt.Rows)");
            W(sb, 2, "End If");
            W(sb, 1, "End Sub");
            sb.AppendLine();

            // GetByFK subs
            foreach (var fk in table.ForeignKeys)
            {
                var fkColField = table.Fields.FirstOrDefault(f => f.FieldName.Equals(fk.FkColumn, StringComparison.OrdinalIgnoreCase));
                string fkVbType = fkColField != null ? VbType(fkColField) : "Integer";
                W(sb, 1, $"Public Sub GetBy{fk.FkColumn}(ByVal {fk.FkColumn} As {fkVbType})");
                W(sb, 2, $"Dim dt As DataTable = {dl}{cn}.GetBy{fk.FkColumn}({fk.FkColumn})");
                W(sb, 2, "If Not (dt Is Nothing) Then");
                W(sb, 3, "Me.Load(dt.Rows)");
                W(sb, 2, "End If");
                W(sb, 1, "End Sub");
                sb.AppendLine();
            }

            sb.AppendLine("\tEnd Class");
            sb.AppendLine("#End Region");

            if (!string.IsNullOrWhiteSpace(NsFull(ns, "BusinessLayer"))) { sb.AppendLine(); WriteNsClose(sb, ns, "BusinessLayer"); }

            return sb.ToString();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  UNIVERSAL FILE
        // ═══════════════════════════════════════════════════════════════════════════

        public static string GenerateUniversalFile(string ns, string connectionString, bool suppressComments)
        {
            var sb = new StringBuilder();
            WriteNsOpen(sb, ns);
            if (!string.IsNullOrWhiteSpace(ns)) sb.AppendLine();
            sb.AppendLine("Public Class Universal");
            sb.AppendLine("\tPublic Shared Function GetConnectionString() As String");
            if (!suppressComments)
            {
                sb.AppendLine("\t\t' TODO: Need to decide on some kind of connection string method");
                sb.AppendLine("\t\t' It may not be the best idea to have your connection string exposed right here unencrypted");
                sb.AppendLine("\t\t' If this is going to be an ASP.NET application, your connection string should");
                sb.AppendLine("\t\t' be in your web config file");
                sb.AppendLine($"\t\t' Return \"{EscapeVbString(connectionString)}\"");
                sb.AppendLine("\t\t' If this is an ASP.NET application, comment the line above and uncomment");
                sb.AppendLine("\t\t' the line below. The line below will pull the connection string from the web.config file");
            }
            sb.AppendLine("\t\tReturn Configuration.ConfigurationManager.ConnectionStrings(\"SqlConnection\").ConnectionString");
            sb.AppendLine("\tEnd Function");
            sb.AppendLine("End Class");
            if (!string.IsNullOrWhiteSpace(ns)) { sb.AppendLine(); WriteNsClose(sb, ns); }
            return sb.ToString();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  INTERFACES FILE
        // ═══════════════════════════════════════════════════════════════════════════

        public static string GenerateInterfacesFile(string ns, bool suppressComments)
        {
            var sb = new StringBuilder();
            WriteNsOpen(sb, ns);
            if (!string.IsNullOrWhiteSpace(ns)) sb.AppendLine();
            sb.AppendLine("Public Interface IBusiness");
            sb.AppendLine("\tSub Validate()");
            sb.AppendLine("\tSub Rollback()");
            sb.AppendLine("End Interface");
            if (!string.IsNullOrWhiteSpace(ns)) { sb.AppendLine(); WriteNsClose(sb, ns); }
            return sb.ToString();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  CUSTOM FILES
        // ═══════════════════════════════════════════════════════════════════════════

        public static string GenerateDataCustomFile(TableInfo table, string ns, bool suppressComments)
        {
            var sb = new StringBuilder();
            string cn = SafeName(table.TableName);
            sb.AppendLine("Option Strict On");
            sb.AppendLine("Option Explicit On");
            sb.AppendLine();
            sb.AppendLine("Imports Microsoft.VisualBasic");
            sb.AppendLine("Imports System");
            sb.AppendLine("Imports System.Data");
            sb.AppendLine();
            WriteNsOpen(sb, ns, "DataLayer");
            if (!string.IsNullOrWhiteSpace(NsFull(ns, "DataLayer"))) sb.AppendLine();
            sb.AppendLine($"#Region \"{cn} Custom Class\"");
            sb.AppendLine($"Partial Public Class {cn}");
            sb.AppendLine();
            sb.AppendLine("#Region \"Custom Data Queries\"");
            sb.AppendLine("\t' Add your custom data query methods here.");
            sb.AppendLine("\t' This file will NOT be regenerated.");
            sb.AppendLine("\t'");
            sb.AppendLine($"\t' Public Shared Function GetByCustom(ByVal param As Integer) As DataTable");
            sb.AppendLine($"\t'     Dim storedProcedure As String = \"[dbo].sp_{table.TableName}_CustomSelect\"");
            sb.AppendLine("\t'     ...");
            sb.AppendLine("\t' End Function");
            sb.AppendLine("#End Region");
            sb.AppendLine();
            sb.AppendLine("End Class");
            sb.AppendLine("#End Region");
            if (!string.IsNullOrWhiteSpace(NsFull(ns, "DataLayer"))) { sb.AppendLine(); WriteNsClose(sb, ns, "DataLayer"); }
            return sb.ToString();
        }

        public static string GenerateBusinessCustomFile(TableInfo table, string ns, bool suppressComments)
        {
            var sb = new StringBuilder();
            string cn = SafeName(table.TableName);
            sb.AppendLine("Option Strict On");
            sb.AppendLine("Option Explicit On");
            sb.AppendLine();
            sb.AppendLine("Imports Microsoft.VisualBasic");
            sb.AppendLine("Imports System");
            sb.AppendLine("Imports System.Data");
            sb.AppendLine();
            WriteNsOpen(sb, ns, "BusinessLayer");
            if (!string.IsNullOrWhiteSpace(NsFull(ns, "BusinessLayer"))) sb.AppendLine();
            sb.AppendLine($"#Region \"{cn} Custom Class\"");
            sb.AppendLine($"Partial Public Class {cn}");
            sb.AppendLine();
            sb.AppendLine("#Region \"Custom Business Logic\"");
            sb.AppendLine("\t' Add your custom business logic here.");
            sb.AppendLine("\t' This file will NOT be regenerated.");
            sb.AppendLine("\t'");
            sb.AppendLine("\t' Public Overrides Sub Validate()");
            sb.AppendLine("\t'     MyBase.Validate()");
            sb.AppendLine("\t'     ' Custom validation...");
            sb.AppendLine("\t' End Sub");
            sb.AppendLine("#End Region");
            sb.AppendLine();
            sb.AppendLine("End Class");
            sb.AppendLine("#End Region");
            if (!string.IsNullOrWhiteSpace(NsFull(ns, "BusinessLayer"))) { sb.AppendLine(); WriteNsClose(sb, ns, "BusinessLayer"); }
            return sb.ToString();
        }

        private static string EscapeVbString(string s) => s.Replace("\"", "\"\"");
    }
}
