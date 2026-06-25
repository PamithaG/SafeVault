using NUnit.Framework;
using SafeVaultApi.Util;

namespace SafeVault.Tests;

[TestFixture]
public class TestInputValidation
{
    [Test]
    public void Sanitize_RemovesHtmlTagsFromUsername_ButPayloadIsStillRejected()
    {
        var rawInput = "<script>alert('xss')</script>safeUser";
        var sanitized = InputValidator.Sanitize(rawInput);

        Assert.That(sanitized, Is.EqualTo("alert('xss')safeUser"));
        Assert.That(InputValidator.IsValidUsername(sanitized), Is.False);
    }

    [Test]
    public void IsValidUsername_RejectsSqlInjectionLikePayload()
    {
        var attackPayload = "admin' OR '1'='1";

        Assert.That(InputValidator.IsValidUsername(attackPayload), Is.False);
    }

    [Test]
    public void IsValidUsername_RejectsXssHtmlContent()
    {
        var attackPayload = "<img src=x onerror=alert(1)>";

        Assert.That(InputValidator.IsValidUsername(attackPayload), Is.False);
    }

    [Test]
    public void Sanitize_RemovesHtmlTagsFromEmail()
    {
        var rawEmail = "user@example.com<script>alert('xss')</script>";
        var sanitized = InputValidator.Sanitize(rawEmail);

        Assert.That(sanitized, Is.EqualTo("user@example.comalert('xss')"));
        Assert.That(InputValidator.IsValidEmail(sanitized), Is.True);
    }

    [Test]
    public void IsValidEmail_RejectsSqlInjectionLikeEmail()
    {
        var attackEmail = "user@example.com' OR '1'='1";

        Assert.That(InputValidator.IsValidEmail(attackEmail), Is.False);
    }
}
