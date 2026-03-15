using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Approvals;
using NetApi.Domain.Common.Models;

namespace NetApi.Application.Approvals.Queries;

public class GetApprovalsQuery : AuthorizedQuery<Paginated<Approval>>
{
    public int PageSize { get; set; }
}
