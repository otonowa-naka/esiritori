namespace EsiritoriApi.Domain.ValueObjects;

/// <summary>
/// お題（ひらがな）を表す値オブジェクト
/// </summary>
public sealed class Answer : IEquatable<Answer>
{
    /// <summary>お題の文字列</summary>
    public string Value { get; }

    /// <summary>
    /// お題の新しいインスタンスを作成します
    /// </summary>
    /// <param name="value">お題の文字列（ひらがな、1-50文字）</param>
    /// <exception cref="ArgumentException">
    /// お題が50文字を超える、お題がひらがな以外の文字を含む場合
    /// </exception>
    public Answer(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length > 50)
        {
            throw new ArgumentException("お題は50文字以下である必要があります", nameof(value));
        }

        if (!string.IsNullOrEmpty(trimmed) && !System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[\u3041-\u3096]+$"))
        {
            throw new ArgumentException("お題はひらがなで入力してください", nameof(value));
        }

        Value = trimmed;
    }

    /// <summary>
    /// 空のお題を作成します
    /// </summary>
    /// <returns>空のお題</returns>
    public static Answer Empty()
    {
        return new Answer("");
    }

    /// <summary>
    /// 回答が正解かどうかを判定します
    /// </summary>
    /// <param name="playerAnswer">プレイヤーの回答</param>
    /// <returns>正解の場合はtrue、不正解の場合はfalse</returns>
    public bool IsCorrect(Answer playerAnswer)
    {
        if (playerAnswer == null)
        {
            throw new ArgumentNullException(nameof(playerAnswer));
        }

        return Value.Trim() == playerAnswer.Value.Trim();
    }

    /// <summary>
    /// 他のお題と等価かどうかを判定します
    /// </summary>
    /// <param name="other">比較対象のお題</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(Answer? other)
    {
        return other is not null && Value == other.Value;
    }

    /// <summary>
    /// 他のオブジェクトと等価かどうかを判定します
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Answer);
    }

    /// <summary>
    /// ハッシュコードを取得します
    /// </summary>
    /// <returns>ハッシュコード</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// 等価演算子
    /// </summary>
    /// <param name="left">左辺のお題</param>
    /// <param name="right">右辺のお題</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public static bool operator ==(Answer? left, Answer? right)
    {
        return EqualityComparer<Answer>.Default.Equals(left, right);
    }

    /// <summary>
    /// 不等価演算子
    /// </summary>
    /// <param name="left">左辺のお題</param>
    /// <param name="right">右辺のお題</param>
    /// <returns>不等価な場合はtrue、そうでない場合はfalse</returns>
    public static bool operator !=(Answer? left, Answer? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 文字列表現を取得します
    /// </summary>
    /// <returns>お題の文字列</returns>
    public override string ToString()
    {
        return Value;
    }
} 