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
        WriteIntoRow(headerRow, headers);

        return GetWorkbookBytes(workbook);
    }

    private static IXLRow WriteIntoRow(IXLRow row, IEnumerable<ExcelCell> cells, uint cellOffset = 0)
    {
        var currRow = row;
        for (var i = 0; i < cells.Count(); i++) {
            var cellData = cells.ElementAt(i);
            var cell = row.Cell(i + 1);
            cell.Value = cellData.Value;
            if (cellData.SubValues.Any()) {
                var subRow = cell.WorksheetRow().RowBelow();
                currRow = WriteIntoRow(subRow, cellData.SubValues);
            }
        }
        return currRow;
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

