[ReactiveTables](https://bitbucket.org/mendelmonteiro/reactivetables)
================================================

A .NET library for displaying and manipulating real-time data through a push based data model.

Do you need to receive large amounts of streaming data from a remote source and then display it in a GUI efficiently?  Do you need to be able perform standard operations over varied data models?  How joining and filtering the data sources to be displayed?

ReactiveTables is written to be as efficient as possible and to put as little pressure as possible on the Garbage Collector.  The data model is reactive in that all updates to the model will be pushed to observers, hence the reference to the (excellent) Rx framework.

### [>>> Get ReactiveTables via NuGet](http://nuget.org/List/Packages/ReactiveTables.Framework)

```
Install-Package ReactiveTables.Framework
```

The source for ReactiveTables is available from the [BitBucket](https://bitbucket.org/mendelmonteiro/reactivetables) page.

## Usage examples

See the Demo project available along with the [source code](https://bitbucket.org/mendelmonteiro/reactivetables/src) for more in depth examples.

Creating a table:

```csharp
ReactiveTable table = new ReactiveTable();
table.AddColumn(new ReactiveColumn<int>("Table.IdColumn"));
var nameCol = table.AddColumn(new ReactiveColumn<string>("Table.Name"));
var valCol = table.AddColumn(new ReactiveColumn<double>("Table.Value"));

```
    
Getting updates from a table:

```csharp
var subscription = table.Subscribe(update => {
    if (update.Action == TableUpdateAction.Update && update.Column == valCol) 
    Console.WriteLine("New value {0}", table.GetValue<double>(update.Column.ColumnId, update.RowIndex);
    }));
    
var rowIndex = table.AddRow();
table.SetValue("Table.Id", rowIndex, 1);
table.SetValue("Table.Name", rowIndex, "Mendel");
table.SetValue("Table.Value", rowIndex, 42.13);

subscription.Dispose();
```
Creating calculated columns, joining and filtering:

```csharp
table.AddColumn(new ReactiveCalculatedColumn2("Table.NameAndValue", nameCol, valCol, (n, v) => n + " has " + v);

var joinedTable = table.Join(otherTable, new Join<int>(
                table, "Table.Id",
                otherTable, "OtherTable.Id"));

// Display joined table and or add calculated columns that reference both tables

var filteredTable = table.Filter(new DelegatePredicate1<double>(
                                        (ReactiveColumn  <double>) valCol, 
                                        v => v < 50d));
```

# Why does this library exist?

In multi-threaded real-time GUIs there's always so much code necessary to keep you  model collections up to date and then there's the question of when to marshal your updates to the GUI thread - wouldn't it be great if you could just do that once in a generic manner that would work for all your types of models?  



# FAQ

If you have any questions or suggestions please contact me at <mailto:reactivetables@gmail.com>

# Credits

Thanks to:


# License

ReactiveTables is available under the [Gnu General Public Licence 3.0](http://www.gnu.org/licenses/).

# Release History / Changelog

See the [Releases page](https://bitbucket.org/mendelmonteiro/reactivetables/downloads).
