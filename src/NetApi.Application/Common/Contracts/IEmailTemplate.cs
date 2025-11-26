namespace NetApi.Application.Common.Contracts;

public interface IEmailTemplate
{
    string Subject { get; }
    void SetProperty(string key, string value);
    string Render();
}
