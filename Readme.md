<!-- default file list -->
*Files to look at*:

* [Program.cs](./CS/XpoImport/Program.cs) (VB: [Program.vb](./VB/XpoImport/Program.vb))
* **[XpoImportHelper.cs](./CS/XpoImport/XpoImportHelper.cs) (VB: [XpoImportHelper.vb](./VB/XpoImport/XpoImportHelper.vb))**
<!-- default file list end -->
# How to import a large data set using XPO efficiently within a transaction


When you are required to import a large data set into a database as XPO persistent objects, the straightforward approach might be inappropriate. Specifically, if you would create objects objects one by one and commit them individually, you cannot roll back changes if one object failed to commit. If you use an XPO transaction or unit of work, changes can be rolled back, but it will require a lot of memory to keep all objects until the final commit.<br><br>The solution demonstrated in this example commits objects in small batches by creating a unit of work for each batch and disposing of it after it is committed. To be able to roll back all batches at once, it utilizes the database-level transaction using the XPO data layer's command channel. Although XPO provides a public API for using database transactions (<a href="https://documentation.devexpress.com/CoreLibraries/CustomDocument9070.aspx">Using Explicit Transactions</a>), it cannot be used in this scenario because explicit transactions belong to sessions, but here we use separate sessions for each batch.<br><br>Below is a managed memory allocation chart produced by this example if you log GC.GetTotalMemory(true) values in the CreatePersistentObject method:<br><img src="https://raw.githubusercontent.com/DevExpress-Examples/how-to-import-a-large-data-set-using-xpo-efficiently-within-a-transaction-t333879/15.2.4+/media/acf4ec89-badb-11e5-80bf-00155d62480c.png"><br><br>

<br/>


