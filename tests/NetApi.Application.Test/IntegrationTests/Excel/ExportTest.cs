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
                Value = "Psikotest",
                SubValues = [
                    new() { Key = "date", Value = "Date" },
                    new() { Key = "status", Value = "Status"}
                ]
            }
        };

        var data = new List<IEnumerable<ExcelCell>> {
            ([
            new() { Key = "name", Value = "John Doe" },
                new() { Key = "age", Value = "30" },
                new() { Key = "date", Value = "2024-01-01" },
                new() { Key = "status", Value = "Passed" },
            ]),
            ([
                new() { Key = "name", Value = "Jane Smith" },
                new() { Key = "age", Value = "25" },
                new() { Key = "date", Value = "2024-02-01" },
                new() { Key = "status", Value = "Failed" },
            ])
        };

        var file = await excelService.ExportAsync(headers, data, cancellationToken);
        file.Length.Should().BeGreaterThan(0);

        // Write the file to disk for manual verification
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"ExportedFile_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        await File.WriteAllBytesAsync(filePath, file, cancellationToken);
    }
}

