using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Styx
{
    public static class DataGridHelper
    {
        public class TableDataRow
        {
            public TableDataRow(List<string> cells)
            {
                Cells = cells;
            }

            public List<string> Cells { get; }
        }

        public class TableData
        {
            public TableData(List<string> columnHeaders, List<TableDataRow> rows)
            {
                for (int i = 0; i < rows.Count; i++)
                    if (rows[i].Cells.Count != columnHeaders.Count)
                        throw new ArgumentException(nameof(rows));

                ColumnHeaders = columnHeaders;
                Rows = rows;
            }

            public List<string> ColumnHeaders { get; }
            public List<TableDataRow> Rows { get; }
        }

        private static void TableDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as System.Windows.Controls.DataGrid;
            var tableData = e.NewValue as TableData;
            if (dataGrid != null && tableData != null)
            {
                dataGrid.Columns.Clear();
                for (int i = 0; i < tableData.ColumnHeaders.Count; i++)
                {
                    DataGridColumn column = new DataGridTextColumn
                    {
                       // Binding = new Binding($"Cells[{i}]"),
                        Header = tableData.ColumnHeaders[i]
                    };
                    dataGrid.Columns.Add(column);
                }

                dataGrid.ItemsSource = tableData.Rows;
            }
        }

        public static TableData GetTableData(DependencyObject obj)
        {
            return (TableData)obj.GetValue(TableDataProperty);
        }

        public static void SetTableData(DependencyObject obj, TableData value)
        {
            obj.SetValue(TableDataProperty, value);
        }

        // Using a DependencyProperty as the backing store for TableData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableDataProperty =
            DependencyProperty.RegisterAttached("TableData",
                typeof(TableData),
                typeof(DataGridHelper),
                new PropertyMetadata(null, TableDataChanged));
    }
}
