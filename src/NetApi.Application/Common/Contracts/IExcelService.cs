using NetApi.Application.Common.Models;

namespace NetApi.Application.Common.Contracts;

public interface IExcelService
{
    Task<byte[]> ExportAsync(IEnumerable<ExcelCell> headers, IEnumerable<IEnumerable<ExcelCell>> data, CancellationToken cancellationToken = default);
}
