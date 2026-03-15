using ClosedXML.Excel;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Models;

namespace NetApi.Infrastructure.Persistence.Services;

public class ExcelService : IExcelService
{
    public async Task<byte[]> ExportAsync(IEnumerable<ExcelCell> headers, IEnumerable<IEnumerable<ExcelCell>> data, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");
        var headerRow = worksheet.Row(1);
        var lastRow = WriteIntoRow(headerRow, headers);

        var nextRow = lastRow.RowBelow();
        foreach (var item in data) {
            WriteIntoRow(nextRow, item);
            nextRow = nextRow.RowBelow();
        }

        worksheet.Columns(1, headerRow.CellCount()).AdjustToContents();

        return GetWorkbookBytes(workbook);
    }

    private static IXLRow WriteIntoRow(IXLRow row, IEnumerable<ExcelCell> cells, int cellOffset = 0)
    {
        var maxRow = row;
        for (var i = 0; i < cells.Count(); i++) {
            var cellNumber = i + 1 + cellOffset;
            var cell = row.Cell(cellNumber);

            var cellData = cells.ElementAt(i);
            cell.Value = cellData.Value;
            cell.Style.Alignment.Horizontal = cellData.HorizontalAlign switch {
                ExcelCellHorizontalAlign.Center => XLAlignmentHorizontalValues.Center,
                ExcelCellHorizontalAlign.Left => XLAlignmentHorizontalValues.Left,
                ExcelCellHorizontalAlign.Right => XLAlignmentHorizontalValues.Right,
                _ => XLAlignmentHorizontalValues.General
            };
            cell.Style.Alignment.Vertical = cellData.VerticalAlign switch {
                ExcelCellVerticalAlign.Middle => XLAlignmentVerticalValues.Center,
                ExcelCellVerticalAlign.Top => XLAlignmentVerticalValues.Top,
                ExcelCellVerticalAlign.Bottom => XLAlignmentVerticalValues.Bottom,
                _ => XLAlignmentVerticalValues.Center
            };

            if (!cellData.SubValues.Any()) continue;
            var subRow = cell.WorksheetRow().RowBelow();
            var nextRow = WriteIntoRow(subRow, cellData.SubValues, i + cellOffset);
            cellOffset += cellData.SubValues.Count() - 1;
            var ws = row.Worksheet;
            ws.Range(ws.Cell(row.RowNumber(), cellNumber), ws.Cell(row.RowNumber(), cellNumber + cellData.SubValues.Count() - 1)).Merge();

            if (nextRow.RowNumber() <= maxRow.RowNumber()) continue;
            maxRow = nextRow;
        }
        return maxRow;
    }

    private static byte[] GetWorkbookBytes(XLWorkbook workbook)
    {
        byte[] fileBytes;
        using (var memoryStream = new MemoryStream()) {
            workbook.SaveAs(memoryStream);
            fileBytes = memoryStream.ToArray();
        }
        return fileBytes;
    }
}
