using NetApi.Application.Common.Contracts;

namespace NetApi.Application.Test.Mocks;

public class DummyEmailTemplate(string subject, string body) : IEmailTemplate
{
    private readonly string _subject = subject;
    private readonly string _body = body;
    private readonly Dictionary<string, string> _properties = new();

    public string Subject => _subject;

    public string Render()
    {
        var renderedBody = _body;
        foreach (var prop in _properties) {
            renderedBody = renderedBody.Replace($"{{{{{prop.Key}}}}}", prop.Value);
        }
        return renderedBody;
    }

    public void SetProperty(string key, string value)
    {
        if (_properties.ContainsKey(key)) {
            _properties[key] = value;
            return;
        }
        _properties.Add(key, value);
    }
}
