namespace StarBot {
    internal static class Config {

        public const int POSTS_TO_CACHE = 5; // number of posts to look back on and make sure there aren't any duplicates
        public const int HOURS_TO_CACHE = 24; // number of hours to maintain caches before re-fetching
        public const bool MEMORY_CACHE = true; // enables the JSON cache for Reddit fetching
        public const ulong ADMIN_ROLE_ID = 696818216080769025;
        public const ulong STARBOT_INTEREST_ROLE_ID = 1143808465194713108;
        public const bool DEBUG_MODE = false; // reduces side effects by
        // preventing changes to Discord session sync
        // blocks lambda message send events
        public const bool DISCORD_NET_LOGGING = false;

        public const string KEY = ""; // override key for debug mode, has no effect when DEBUG_MODE = false
    }
}