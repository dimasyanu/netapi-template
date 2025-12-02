using FluentAssertions;
using NetApi.Application.Common.Extensions;

namespace NetApi.Application.Test.UnitTests.Extensions;

public class StringExtensionTest
{
    [Theory]
    [InlineData("AnotherTestString", "another_test_string")]
    [InlineData("TestRole_eace4a7c", "test_role_eace4a7c")]
    public void ToSnakeCase_ShouldConvertToSnakeCase(string input, string expected)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.Should().Be(expected);
    }
}
