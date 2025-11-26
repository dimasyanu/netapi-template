namespace NetApi.Application.Common.Contracts;

public interface IEmailTemplateManager
{
    IEmailTemplate GetTemplate(string templateName);
}
