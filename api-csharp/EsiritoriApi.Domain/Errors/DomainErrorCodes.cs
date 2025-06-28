namespace EsiritoriApi.Domain.Errors;

/// <summary>
/// ドメインエラーコードの定義
/// </summary>
public static class DomainErrorCodes
{
    // Game関連のエラーコード
    public static class Game
    {
        public const string NotFound = "GAME_NOT_FOUND";
        public const string AlreadyStarted = "GAME_ALREADY_STARTED";
        public const string AlreadyEnded = "GAME_ALREADY_ENDED";
        public const string InsufficientPlayers = "GAME_INSUFFICIENT_PLAYERS";
        public const string NotAllPlayersReady = "GAME_NOT_ALL_PLAYERS_READY";
        public const string PlayerLimitExceeded = "GAME_PLAYER_LIMIT_EXCEEDED";
        public const string PlayerAlreadyJoined = "GAME_PLAYER_ALREADY_JOINED";
        public const string PlayerNotFound = "GAME_PLAYER_NOT_FOUND";
        public const string CannotAddPlayerAfterStart = "GAME_CANNOT_ADD_PLAYER_AFTER_START";
    }

    // Player関連のエラーコード
    public static class Player
    {
        public const string InvalidName = "PLAYER_INVALID_NAME";
        public const string InvalidId = "PLAYER_INVALID_ID";
    }

    // GameSettings関連のエラーコード
    public static class GameSettings
    {
        public const string InvalidTimeLimit = "GAME_SETTINGS_INVALID_TIME_LIMIT";
        public const string InvalidRoundCount = "GAME_SETTINGS_INVALID_ROUND_COUNT";
        public const string InvalidPlayerCount = "GAME_SETTINGS_INVALID_PLAYER_COUNT";
    }

    // Turn関連のエラーコード
    public static class Turn
    {
        public const string InvalidTurnNumber = "TURN_INVALID_TURN_NUMBER";
        public const string InvalidDrawerId = "TURN_INVALID_DRAWER_ID";
        public const string AlreadyEnded = "TURN_ALREADY_ENDED";
    }

    // Round関連のエラーコード
    public static class Round
    {
        public const string InvalidRoundNumber = "ROUND_INVALID_ROUND_NUMBER";
        public const string InvalidCurrentTurn = "ROUND_INVALID_CURRENT_TURN";
    }

    // ScoreHistory関連のエラーコード
    public static class ScoreHistory
    {
        public const string InvalidPlayerId = "SCORE_HISTORY_INVALID_PLAYER_ID";
        public const string InvalidRoundNumber = "SCORE_HISTORY_INVALID_ROUND_NUMBER";
        public const string InvalidTurnNumber = "SCORE_HISTORY_INVALID_TURN_NUMBER";
        public const string InvalidPoints = "SCORE_HISTORY_INVALID_POINTS";
    }
} 