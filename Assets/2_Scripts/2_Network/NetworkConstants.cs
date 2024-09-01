
namespace GimJem.Network
{
    public static class NetworkConstants
    {
        #region Lobby Constants
        public const int LOBBY_READY_OP_CODE = 101;
        public const int LOBBY_GAME_START_OP_CODE = 102;
        public const int LOBBY_PLAYER_LOADED_OP_CODE = 103;
        public const int LOBBY_PLAYER_JOINED_OP_CODE = 104;
        #endregion

        #region Gameplay Constants
        public const int GAMEPLAY_PLAYER_STATE_OP_CODE = 201;
        public const int GAMEPLAY_WORLD_STATE_OP_CODE = 202;
        #endregion
    }

}
