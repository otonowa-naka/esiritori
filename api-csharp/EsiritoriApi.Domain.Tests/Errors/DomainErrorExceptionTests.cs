using EsiritoriApi.Domain.Errors;
using Xunit;

namespace EsiritoriApi.Domain.Tests.Errors;

[Trait("Category", "ドメインモデル")]
public class DomainErrorExceptionTests
{
    [Fact]
    public void 有効なエラーコードとメッセージでインスタンス作成_正常に作成される()
    {
        var errorCode = "INVALID_VALUE";
        var errorMessage = "値が無効です";

        var exception = new DomainErrorException(errorCode, errorMessage);

        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(errorMessage, exception.ErrorMessage);
        Assert.Equal(errorMessage, exception.Message);
    }

    [Fact]
    public void エラーコードがnull_ArgumentNullExceptionが発生する()
    {
        var errorMessage = "値が無効です";

        var exception = Assert.Throws<ArgumentNullException>(() => 
            new DomainErrorException(null!, errorMessage));

        Assert.Equal("errorCode", exception.ParamName);
    }

    [Fact]
    public void エラーメッセージがnull_ArgumentNullExceptionが発生する()
    {
        var errorCode = "INVALID_VALUE";

        var exception = Assert.Throws<ArgumentNullException>(() => 
            new DomainErrorException(errorCode, null!));

        Assert.Equal("errorMessage", exception.ParamName);
    }

    [Fact]
    public void 内部例外を含むコンストラクター_正常に作成される()
    {
        var errorCode = "INNER_ERROR";
        var errorMessage = "内部エラーが発生しました";
        var innerException = new InvalidOperationException("内部例外");

        var exception = new DomainErrorException(errorCode, errorMessage, innerException);

        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(errorMessage, exception.ErrorMessage);
        Assert.Equal(errorMessage, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void 内部例外を含むコンストラクターでエラーコードがnull_ArgumentNullExceptionが発生する()
    {
        var errorMessage = "値が無効です";
        var innerException = new InvalidOperationException("内部例外");

        var exception = Assert.Throws<ArgumentNullException>(() => 
            new DomainErrorException(null!, errorMessage, innerException));

        Assert.Equal("errorCode", exception.ParamName);
    }

    [Fact]
    public void 内部例外を含むコンストラクターでエラーメッセージがnull_ArgumentNullExceptionが発生する()
    {
        var errorCode = "INVALID_VALUE";
        var innerException = new InvalidOperationException("内部例外");

        var exception = Assert.Throws<ArgumentNullException>(() => 
            new DomainErrorException(errorCode, null!, innerException));

        Assert.Equal("errorMessage", exception.ParamName);
    }

    [Fact]
    public void ToString_適切な形式で文字列を返す()
    {
        var errorCode = "GAME_NOT_FOUND";
        var errorMessage = "ゲームが見つかりません";

        var exception = new DomainErrorException(errorCode, errorMessage);

        var result = exception.ToString();

        Assert.Equal($"DomainErrorException: {errorCode} - {errorMessage}", result);
    }

    [Fact]
    public void ToString_空文字でも適切に処理される()
    {
        var errorCode = "";
        var errorMessage = "";

        var exception = new DomainErrorException(errorCode, errorMessage);

        var result = exception.ToString();

        Assert.Equal("DomainErrorException:  - ", result);
    }

    [Fact]
    public void ToString_特殊文字を含む場合も適切に処理される()
    {
        var errorCode = "ERROR_CODE@#$%";
        var errorMessage = "エラーメッセージ & 特殊文字 <>";

        var exception = new DomainErrorException(errorCode, errorMessage);

        var result = exception.ToString();

        Assert.Equal($"DomainErrorException: {errorCode} - {errorMessage}", result);
    }

    [Fact]
    public void ErrorCodeプロパティ_読み取り専用である()
    {
        var errorCode = "READONLY_TEST";
        var errorMessage = "読み取り専用テスト";

        var exception = new DomainErrorException(errorCode, errorMessage);

        Assert.Equal(errorCode, exception.ErrorCode);
        
        var property = typeof(DomainErrorException).GetProperty(nameof(DomainErrorException.ErrorCode));
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.False(property.CanWrite);
    }

    [Fact]
    public void ErrorMessageプロパティ_読み取り専用である()
    {
        var errorCode = "READONLY_TEST";
        var errorMessage = "読み取り専用テスト";

        var exception = new DomainErrorException(errorCode, errorMessage);

        Assert.Equal(errorMessage, exception.ErrorMessage);
        
        var property = typeof(DomainErrorException).GetProperty(nameof(DomainErrorException.ErrorMessage));
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.False(property.CanWrite);
    }

    [Fact]
    public void 定義済みエラーコードとの統合_正常に動作する()
    {
        var errorCode = DomainErrorCodes.Game.NotFound;
        var errorMessage = "ゲームが見つかりません";

        var exception = new DomainErrorException(errorCode, errorMessage);

        Assert.Equal(DomainErrorCodes.Game.NotFound, exception.ErrorCode);
        Assert.Equal(errorMessage, exception.ErrorMessage);
    }
}