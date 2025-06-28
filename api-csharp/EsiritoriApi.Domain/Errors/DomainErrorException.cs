namespace EsiritoriApi.Domain.Errors;

/// <summary>
/// ドメインエラーを表す例外クラス
/// </summary>
public class DomainErrorException : Exception
{
    /// <summary>
    /// エラーコード
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    public DomainErrorException(string errorCode, string errorMessage)
        : base(errorMessage)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }

    /// <summary>
    /// コンストラクタ（内部例外付き）
    /// </summary>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <param name="innerException">内部例外</param>
    public DomainErrorException(string errorCode, string errorMessage, Exception innerException)
        : base(errorMessage, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }

    /// <summary>
    /// エラーコードとメッセージを文字列として返す
    /// </summary>
    /// <returns>エラーコードとメッセージの文字列</returns>
    public override string ToString()
    {
        return $"DomainErrorException: {ErrorCode} - {ErrorMessage}";
    }
} 