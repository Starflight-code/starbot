namespace StarBot {
    internal static class Config {
        public const ulong ADMIN_ROLE_ID = 696818216080769025;
        public const ulong STARBOT_INTEREST_ROLE_ID = 1143808465194713108;
        public const bool DEBUG_MODE = false; // reduces side effects by
        // - preventing changes to Discord session sync
        // - blocks lambda message send events
        public const bool DISCORD_NET_LOGGING = false;

        public const string KEY = ""; // override key for debug mode, has no effect when DEBUG_MODE = false
    }
}