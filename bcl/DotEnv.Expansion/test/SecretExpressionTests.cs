namespace Hyprx.DotEnv.Expansion.Tests;

public class SecretExpressionTests
{
    [Fact]
    public void Verify_ParseArgs()
    {
        var args = SecretExpression.ParseArgs("secret akv://kv-hyprx-tmp/secret3 --create --size 32 --expires-at 2024-12-31T23:59:59Z");
        Assert.NotNull(args);

        Assert.Collection(
            args,
            arg => Assert.Equal("secret", arg),
            arg => Assert.Equal("akv://kv-hyprx-tmp/secret3", arg),
            arg => Assert.Equal("--create", arg),
            arg => Assert.Equal("--size", arg),
            arg => Assert.Equal("32", arg),
            arg => Assert.Equal("--expires-at", arg),
            arg => Assert.Equal("2024-12-31T23:59:59Z", arg));
    }

    [Fact]
    public void Verify_ParseArgs_Empty()
    {
        var args = SecretExpression.ParseArgs(string.Empty);
        Assert.NotNull(args);
        Assert.Empty(args);

        args = SecretExpression.ParseArgs("   ");
        Assert.NotNull(args);
        Assert.Empty(args);

        args = SecretExpression.ParseArgs(null!);
        Assert.NotNull(args);
        Assert.Empty(args);
    }

    [Fact]
    public void Verify_ParseArgs_SingleWord()
    {
        var args = SecretExpression.ParseArgs("secret");
        Assert.NotNull(args);
        Assert.Single(args);
        Assert.Equal("secret", args[0]);
    }

    [Fact]
    public void Verify_Parse_NoMatch()
    {
        var args = SecretExpression.ParseArgs("not-a-secret");
        var (res, err) = SecretExpression.Parse(args);
        Assert.Null(res);
        Assert.NotNull(err);
        Assert.StartsWith("Invalid URL: ", err);
    }

    [Fact]
    public void Verify_Parse_ValidSecretUrl()
    {
        var args = SecretExpression.ParseArgs("akv://kv-hyprx-tmp/secret3");
        var (res, err) = SecretExpression.Parse(args);
        Assert.NotNull(res);
        Assert.Null(err);
        Assert.Equal("akv://kv-hyprx-tmp/secret3", res.Uri.ToString());

        args = SecretExpression.ParseArgs("secret akv://kv-hyprx-tmp/secret3");
        (res, err) = SecretExpression.Parse(args);
        Assert.NotNull(res);
        Assert.Null(err);
        Assert.Equal("akv://kv-hyprx-tmp/secret3", res.Uri.ToString());

        args = SecretExpression.ParseArgs("my-command sops://kv-hyprx-tmp/secret3");
        (res, err) = SecretExpression.Parse(args, "my-command");
        Assert.NotNull(res);
        Assert.Null(err);
        Assert.Equal("sops://kv-hyprx-tmp/secret3", res.Uri.ToString());
    }

    [Fact]
    public void Verify_Parse_WithVariousOptions()
    {
        var args = SecretExpression.ParseArgs("secret akv://kv-hyprx-tmp/secret3 --create --size 32 --expires-at 2024-12-31T23:59:59Z");
        var (res, err) = SecretExpression.Parse(args);
        Assert.NotNull(res);
        Assert.Null(err);
        Assert.Equal("akv://kv-hyprx-tmp/secret3", res.Uri.ToString());
        Assert.True(res.Create);
        Assert.Equal(32, res.Size);
        Assert.Equal(new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc), res.ExpiresAt);

        args = SecretExpression.ParseArgs("secret sops://kv-hyprx-tmp/secret-100");
        (res, err) = SecretExpression.Parse(args);
        Assert.NotNull(res);
        Assert.Null(err);
        Assert.Equal("sops://kv-hyprx-tmp/secret-100", res.Uri.ToString());
        Assert.False(res.Create);
        Assert.Equal(16, res.Size);
        Assert.Null(res.ExpiresAt);
        Assert.True(res.Upper);
        Assert.True(res.Lower);
        Assert.True(res.Digits);
        Assert.Null(res.Chars);
        Assert.Equal("@#~`_-[]|:^", res.Special);

        args = SecretExpression.ParseArgs("secret sops://kv-hyprx-tmp/secret-100 --create --no-upper --no-lower --no-digits --chars abcdefghijklmnopqrstuvwxyz");

        (res, err) = SecretExpression.Parse(args);
        Assert.NotNull(res);
        Assert.Null(err);
        Assert.Equal("sops://kv-hyprx-tmp/secret-100", res.Uri.ToString());
        Assert.True(res.Create);
        Assert.Equal(16, res.Size);
        Assert.Null(res.ExpiresAt);
        Assert.False(res.Upper);
        Assert.False(res.Lower);
        Assert.False(res.Digits);
        Assert.Equal("abcdefghijklmnopqrstuvwxyz", res.Chars);
        Assert.Equal("@#~`_-[]|:^", res.Special);

        args = SecretExpression.ParseArgs("secret sops://kv-hyprx-tmp/secret-100 --create --no-upper --special '[]|:'");
        (res, err) = SecretExpression.Parse(args);
        Assert.NotNull(res);
        Assert.Null(err);
        Assert.Equal("sops://kv-hyprx-tmp/secret-100", res.Uri.ToString());
        Assert.True(res.Create);
        Assert.Equal(16, res.Size);
        Assert.Null(res.ExpiresAt);
        Assert.False(res.Upper);
        Assert.True(res.Lower);
        Assert.True(res.Digits);
        Assert.Null(res.Chars);
        Assert.Equal("[]|:", res.Special);
    }
}