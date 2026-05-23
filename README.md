<div dir="rtl">

# LayerGen For DotNet 4

**مولّد طبقات الكود لـ VB.NET 4 / ASP.NET 4**

> 🇬🇧 [English version below](#english)

---

## ما هو هذا البرنامج؟

**LayerGen For DotNet 4** هو أداة سطح مكتب (WinForms) تتصل بقاعدة بيانات SQL Server وتولّد تلقائياً طبقات الكود الكاملة بأسلوب VB.NET 4 / ASP.NET 4 الكلاسيكي:

- **DataLayer** — كلاس يحتوي على جميع عمليات القراءة والحفظ والحذف
- **BusinessLayer** — كلاس الأعمال مع دعم المجموعات (CollectionBase)
- **Stored Procedures** — سكريبت SQL جاهز للتنفيذ على قاعدة البيانات

---

## المتطلبات

- Windows 10 / 11
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- SQL Server (أي إصدار)

---

## طريقة الاستخدام

1. شغّل `LayerGenForDotNet4.exe` (أو `run.bat` للتحقق من وجود .NET تلقائياً)
2. أدخل بيانات الاتصال بقاعدة البيانات
3. اضغط **اتصال**
4. اختر الجداول التي تريد توليد كودها
5. حدد خيارات التوليد (VB.NET / C# / Stored Procedures)
6. حدد مجلد الإخراج
7. اضغط **إنشاء الكود** 🚀

---

## خيارات التوليد

| الخيار | الوصف |
|---|---|
| VB.NET | يولّد `TableData.VB` و `TableBusiness.VB` |
| C# | يولّد `TableData.cs` و `TableBusiness.cs` |
| Stored Procedures | يولّد ملف `Procedures.SQL` |
| Universal + Interfaces | يولّد ملفات البنية التحتية المشتركة |
| ملفات Custom | يولّد ملفات قابلة للتعديل اليدوي (لا تُكتب فوقها) |
| تطبيق SP مباشرةً | ينفّذ السكريبت على قاعدة البيانات فور التوليد |
| False Erase | حذف منطقي — يضع `IsDeleted = 1` بدلاً من حذف السجل |

---

## الملفات المولّدة

```
OutputFolder/
├── Universal.VB          ← مشترك لجميع الجداول (يُنشأ مرة واحدة)
├── Interfaces.VB         ← الواجهات المشتركة
├── TableNameData.VB      ← DataLayer
├── TableNameBusiness.VB  ← BusinessLayer
├── TableNameDataCustom.VB     ← للتعديل اليدوي (لا يُكتب فوقه)
├── TableNameBusinessCustom.VB ← للتعديل اليدوي (لا يُكتب فوقه)
└── Procedures.SQL        ← جميع الـ Stored Procedures
```

---

## بناء المشروع من المصدر

```bash
git clone https://github.com/your-username/LayerGenForDotNet4.git
cd LayerGenForDotNet4
dotnet build -c Release
```

</div>

---

<a name="english"></a>

# LayerGen For DotNet 4

**Code Layer Generator for VB.NET 4 / ASP.NET 4**

---

## What is this?

**LayerGen For DotNet 4** is a WinForms desktop tool that connects to a SQL Server database and automatically generates complete code layers in classic VB.NET 4 / ASP.NET 4 style:

- **DataLayer** — class containing all read, save, and delete operations
- **BusinessLayer** — business class with CollectionBase support
- **Stored Procedures** — ready-to-run SQL script

---

## Requirements

- Windows 10 / 11
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- SQL Server (any version)

---

## How to Use

1. Run `LayerGenForDotNet4.exe` (or `run.bat` to auto-check for .NET)
2. Enter your SQL Server connection details
3. Click **Connect**
4. Select the tables you want to generate code for
5. Choose generation options (VB.NET / C# / Stored Procedures)
6. Select an output folder
7. Click **Generate Code** 🚀

---

## Generation Options

| Option | Description |
|---|---|
| VB.NET | Generates `TableData.VB` and `TableBusiness.VB` |
| C# | Generates `TableData.cs` and `TableBusiness.cs` |
| Stored Procedures | Generates `Procedures.SQL` file |
| Universal + Interfaces | Generates shared infrastructure files |
| Custom Files | Generates manually-editable files (never overwritten) |
| Apply SP directly | Executes the script against the database immediately |
| False Erase | Soft delete — sets `IsDeleted = 1` instead of deleting the record |

---

## Generated Files

```
OutputFolder/
├── Universal.VB          ← shared for all tables (created once)
├── Interfaces.VB         ← shared interfaces
├── TableNameData.VB      ← DataLayer
├── TableNameBusiness.VB  ← BusinessLayer
├── TableNameDataCustom.VB     ← manually editable (never overwritten)
├── TableNameBusinessCustom.VB ← manually editable (never overwritten)
└── Procedures.SQL        ← all Stored Procedures
```

---

## Build from Source

```bash
git clone https://github.com/your-username/LayerGenForDotNet4.git
cd LayerGenForDotNet4
dotnet build -c Release
```

---

## License

MIT License
