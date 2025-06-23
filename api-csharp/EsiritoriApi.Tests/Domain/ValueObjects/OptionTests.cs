using EsiritoriApi.Domain.ValueObjects;
using Xunit;
using System;

namespace EsiritoriApi.Tests.Domain.ValueObjects;

public sealed class OptionTests
{
    [Fact]
    public void NoneはHasValueがfalseでValueはdefault()
    {
        var none = Option<string>.None();
        Assert.False(none.HasValue);
        Assert.Equal(default(string), none.Value);
    }

    [Fact]
    public void SomeはHasValueがtrueでValueが設定される()
    {
        var some = Option<int>.Some(42);
        Assert.True(some.HasValue);
        Assert.Equal(42, some.Value);
    }

    [Fact]
    public void Someにnullを渡すと例外()
    {
        Assert.Throws<ArgumentNullException>(() => Option<string>.Some(null!));
    }

    [Fact]
    public void Match_Someの場合onSomeが呼ばれる()
    {
        var some = Option<int>.Some(10);
        var result = some.Match(x => x * 2, () => -1);
        Assert.Equal(20, result);
    }

    [Fact]
    public void Match_Noneの場合onNoneが呼ばれる()
    {
        var none = Option<int>.None();
        var result = none.Match(x => x * 2, () => -1);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Match_Action_Someの場合onSomeが呼ばれる()
    {
        var some = Option<string>.Some("abc");
        string? called = null;
        some.Match(x => called = x, () => called = "none");
        Assert.Equal("abc", called);
    }

    [Fact]
    public void Match_Action_Noneの場合onNoneが呼ばれる()
    {
        var none = Option<string>.None();
        string? called = null;
        none.Match(x => called = x, () => called = "none");
        Assert.Equal("none", called);
    }

    [Fact]
    public void Map_Someの場合mapperが適用される()
    {
        var some = Option<int>.Some(5);
        var mapped = some.Map(x => x.ToString());
        Assert.True(mapped.HasValue);
        Assert.Equal("5", mapped.Value);
    }

    [Fact]
    public void Map_Noneの場合Noneが返る()
    {
        var none = Option<int>.None();
        var mapped = none.Map(x => x.ToString());
        Assert.False(mapped.HasValue);
    }

    [Fact]
    public void GetValueOrDefault_Someは値を返す()
    {
        var some = Option<int>.Some(7);
        Assert.Equal(7, some.GetValueOrDefault(99));
    }

    [Fact]
    public void GetValueOrDefault_Noneはデフォルト値を返す()
    {
        var none = Option<int>.None();
        Assert.Equal(99, none.GetValueOrDefault(99));
    }

    [Fact]
    public void Equals_object_同値はtrue()
    {
        var a = Option<string>.Some("x");
        object b = Option<string>.Some("x");
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_object_異なる値はfalse()
    {
        var a = Option<string>.Some("x");
        object b = Option<string>.Some("y");
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_object_None同士はtrue()
    {
        var a = Option<int>.None();
        object b = Option<int>.None();
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_object_型違いはfalse()
    {
        var a = Option<int>.Some(1);
        object b = "not option";
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void ToString_SomeはSome値形式()
    {
        var some = Option<string>.Some("abc");
        Assert.Equal("Some(abc)", some.ToString());
    }

    [Fact]
    public void ToString_NoneはNone()
    {
        var none = Option<int>.None();
        Assert.Equal("None", none.ToString());
    }
} 