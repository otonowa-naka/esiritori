namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using EsiritoriApi.Domain.Errors;
using Xunit;

public sealed class AnswerTests
{
    [Fact]
    public void 正常なひらがなでAnswerが作成される()
    {
        var answer = new Answer("りんご");

        Assert.Equal("りんご", answer.Value);
    }

    [Fact]
    public void 50文字のひらがなでAnswerが作成される()
    {
        var longAnswer = new string('あ', 50);
        var answer = new Answer(longAnswer);

        Assert.Equal(longAnswer, answer.Value);
    }

    [Fact]
    public void 50文字を超える場合例外が発生する()
    {
        var longAnswer = new string('あ', 51);
        var exception = Assert.Throws<DomainErrorException>(() => new Answer(longAnswer));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPoints, exception.ErrorCode);
    }

    [Fact]
    public void ひらがな以外の文字が含まれる場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new Answer("りんご123"));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPoints, exception.ErrorCode);
    }

    [Fact]
    public void カタカナが含まれる場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new Answer("りんゴ"));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPoints, exception.ErrorCode);
    }

    [Fact]
    public void 漢字が含まれる場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new Answer("林檎"));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPoints, exception.ErrorCode);
    }

    [Fact]
    public void 英数字が含まれる場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new Answer("りんごapple"));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPoints, exception.ErrorCode);
    }

    [Fact]
    public void 空文字の場合は正常に作成される()
    {
        var answer = new Answer("");

        Assert.Equal("", answer.Value);
    }

    [Fact]
    public void nullの場合は空文字として扱われる()
    {
        var answer = new Answer(null!);

        Assert.Equal("", answer.Value);
    }

    [Fact]
    public void 前後の空白が除去される()
    {
        var answer = new Answer("  りんご  ");

        Assert.Equal("りんご", answer.Value);
    }

    [Fact]
    public void Emptyが正しく動作する()
    {
        var answer = Answer.Empty();

        Assert.Equal("", answer.Value);
    }

    [Fact]
    public void IsCorrect_完全一致の場合trueを返す()
    {
        var answer = new Answer("りんご");
        var playerAnswer = new Answer("りんご");

        var result = answer.IsCorrect(playerAnswer);

        Assert.True(result);
    }

    [Fact]
    public void IsCorrect_空白を含む場合でも正解と判定される()
    {
        var answer = new Answer("りんご");
        var playerAnswer = new Answer("  りんご  ");

        var result = answer.IsCorrect(playerAnswer);

        Assert.True(result);
    }

    [Fact]
    public void IsCorrect_不一致の場合falseを返す()
    {
        var answer = new Answer("りんご");
        var playerAnswer = new Answer("みかん");

        var result = answer.IsCorrect(playerAnswer);

        Assert.False(result);
    }

    [Fact]
    public void IsCorrect_プレイヤー回答がnullの場合例外が発生する()
    {
        var answer = new Answer("りんご");

        var exception = Assert.Throws<DomainErrorException>(() => answer.IsCorrect(null!));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPlayerId, exception.ErrorCode);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var answer1 = new Answer("りんご");
        var answer2 = new Answer("りんご");
        var answer3 = new Answer("みかん");

        Assert.Equal(answer1, answer2);
        Assert.NotEqual(answer1, answer3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var answer1 = new Answer("りんご");
        var answer2 = new Answer("りんご");

        Assert.Equal(answer1.GetHashCode(), answer2.GetHashCode());
    }

    [Fact]
    public void ToStringが正しく動作する()
    {
        var answer = new Answer("りんご");

        Assert.Equal("りんご", answer.ToString());
    }
} 