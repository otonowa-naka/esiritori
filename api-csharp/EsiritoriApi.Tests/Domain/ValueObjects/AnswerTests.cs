namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class AnswerTests
{
    [Fact]
    public void 有効な値でAnswerが正常に作成される()
    {
        var value = "ねこ";

        var answer = new Answer(value);

        Assert.Equal(value, answer.Value);
    }

    [Fact]
    public void Emptyファクトリメソッドで空のAnswerが正常に作成される()
    {
        var answer = Answer.Empty();

        Assert.Equal("", answer.Value);
    }

    [Fact]
    public void 正解の回答でIsCorrectがtrueを返す()
    {
        var answer = new Answer("ねこ");
        var playerAnswer = new Answer("ねこ");

        var result = answer.IsCorrect(playerAnswer);

        Assert.True(result);
    }

    [Fact]
    public void 不正解の回答でIsCorrectがfalseを返す()
    {
        var answer = new Answer("ねこ");
        var playerAnswer = new Answer("いぬ");

        var result = answer.IsCorrect(playerAnswer);

        Assert.False(result);
    }

    [Fact]
    public void 空白を含む回答でIsCorrectが正しく動作する()
    {
        var answer = new Answer("ねこ");
        var playerAnswer = new Answer(" ねこ ");

        var result = answer.IsCorrect(playerAnswer);

        Assert.True(result);
    }

    [Fact]
    public void 空の回答でIsCorrectがfalseを返す()
    {
        var answer = new Answer("ねこ");
        var playerAnswer = Answer.Empty();

        var result = answer.IsCorrect(playerAnswer);

        Assert.False(result);
    }

    [Fact]
    public void nullの回答でIsCorrectが例外を投げる()
    {
        var answer = new Answer("ねこ");

        Assert.Throws<ArgumentNullException>(() => answer.IsCorrect(null!));
    }

    [Fact]
    public void 同じ値のAnswer同士は等価である()
    {
        var answer1 = new Answer("ねこ");
        var answer2 = new Answer("ねこ");

        Assert.Equal(answer1, answer2);
        Assert.True(answer1 == answer2);
        Assert.False(answer1 != answer2);
        Assert.Equal(answer1.GetHashCode(), answer2.GetHashCode());
    }

    [Fact]
    public void 異なる値のAnswer同士は等価でない()
    {
        var answer1 = new Answer("ねこ");
        var answer2 = new Answer("いぬ");

        Assert.NotEqual(answer1, answer2);
        Assert.False(answer1 == answer2);
        Assert.True(answer1 != answer2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var answer = new Answer("ねこ");

        Assert.False(answer.Equals(null));
        Assert.False(answer == null);
        Assert.True(answer != null);
    }

    [Fact]
    public void 無効な回答の場合例外が発生する()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Answer("invalid"));
        Assert.Equal("お題はひらがなで入力してください (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void ToStringで値が返される()
    {
        var value = "ねこ";
        var answer = new Answer(value);

        var result = answer.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void 空白を含む値でAnswerを生成するとトリムされる()
    {
        var answer = new Answer("  ねこ  ");
        Assert.Equal("ねこ", answer.Value);
    }

    [Fact]
    public void 空白を含む正解の回答で正解と判定される()
    {
        var answer = new Answer("ねこ");
        var playerAnswer = new Answer(" ねこ ");
        Assert.True(answer.IsCorrect(playerAnswer));
    }
} 