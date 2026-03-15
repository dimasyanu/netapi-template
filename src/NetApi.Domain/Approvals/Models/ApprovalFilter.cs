using NetApi.Domain.Common.Abstractions;

namespace NetApi.Domain.Approvals.Models;

public class ApprovalFilter : Filter
{
    public byte Status { get; set; }
}