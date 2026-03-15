using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Models;
using NetApi.Infrastructure.Persistence.Services;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Excel;

public class ExportTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddTransient<IExcelService, ExcelService>();
    }

    [Fact]
    public async Task ExportExcel_ShouldReturnFile()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        using var scope = Service.CreateScope();
        var excelService = scope.ServiceProvider.GetRequiredService<IExcelService>();

        var headers = new List<ExcelCell> {
            new() { Key = "name", Value = "Name" },
            new() { Key = "age", Value = "Age" },
            new() {
                Value = "Selection",
                SubValues = [
                    new() { Key = "selectionDate", Value = "Date" },
                    new() { Key = "selectionStatus", Value = "Status"}
                ],
                HorizontalAlign = ExcelCellHorizontalAlign.Center
            },
            new() {
                Value = "Psikotest",
                SubValues = [
                    new() { Key = "psikotestDate", Value = "Date" },
                    new() { Key = "psikotestStatus", Value = "Status"}
                ],
                HorizontalAlign = ExcelCellHorizontalAlign.Center
            },
            new() {
                Value = "Interview",
                SubValues = [
                    new() { Key = "interviewDate", Value = "Date" },
                    new() { Key = "interviewStatus", Value = "Status"}
                ],
                HorizontalAlign = ExcelCellHorizontalAlign.Center
            },
        };

        var data = new List<IEnumerable<ExcelCell>> {
            ([
            new() { Key = "name", Value = "John Doe" },
                new() { Key = "age", Value = "30" },
                new() { Key = "selectionDate", Value = "2024-01-01" },
                new() { Key = "selectionStatus", Value = "Passed" },
                new() { Key = "psikotestDate", Value = "2024-01-02" },
                new() { Key = "psikotestStatus", Value = "Passed" },
                new() { Key = "interviewDate", Value = "2024-01-03" },
                new() { Key = "interviewStatus", Value = "Passed" },
            ]),
            ([
                new() { Key = "name", Value = "Jane Smith" },
                new() { Key = "age", Value = "25" },
                new() { Key = "selectionDate", Value = "2024-02-01" },
                new() { Key = "selectionStatus", Value = "Failed" },
                new() { Key = "psikotestDate", Value = "2024-02-02" },
                new() { Key = "psikotestStatus", Value = "Failed" },
                new() { Key = "interviewDate", Value = "2024-02-03" },
                new() { Key = "interviewStatus", Value = "Failed" },
            ])
        };

        var file = await excelService.ExportAsync(headers, data, cancellationToken);
        file.Length.Should().BeGreaterThan(0);

        // Write the file to disk for manual verification
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"ExportedFile_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        await File.WriteAllBytesAsync(filePath, file, cancellationToken);
    }
}

