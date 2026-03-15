using NetApi.Application.Common.Abstractions;

namespace NetApi.Application.Approvals.Commands;

public class CreateApprovalCommand : AuthorizedCommand<bool>
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}
