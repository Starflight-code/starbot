namespace StarBot {
    // VERSION: 1.0.1-DEV
    internal static class Config {

        public struct StatusMessage {
            public string message;
            public Discord.ActivityType activity;
            public StatusMessage(string message, Discord.ActivityType activity) {
                this.message = message;
                this.activity = activity;
            }
        }

        // **** Caching and Optimization ****
        public const bool MEMORY_CACHE = true; // enables the JSON cache for Reddit fetching
        public const int POSTS_TO_CACHE = 6; // number of posts to look back on and make sure there aren't any duplicates
        public const int HOURS_TO_CACHE = 36; // number of hours to maintain caches before re-fetching

        // **** Server Specific (Hardcoded) Values ****
        public const ulong ERROR_LOG_CHANNEL = 1187007545357905980; // this links to StarBot's error channel. (change this before deploying to another enviroment)

        // **** Logging/Debug Switches ****
        public const bool DEBUG_MODE = false; // reduces side effects by
        // preventing changes to database
        // blocks lambda message send events
        public const bool DISCORD_NET_LOGGING = false;

        public const string KEY = ""; // override key, if specified the argument given will be ignored

        // **** Production Settings ****

        public static string[] IMAGE_EXTENSIONS = { // extensions to mark as images (can be decoded by Discord)
            ".jpg",
            ".jpeg",
            ".png",
            ".gif"
        };

        public static string[] TASK_NAMES = { // names of tasks, used to generate task keys for the database
            "XKCD",
            "Cat",
            "Anime",
            "AniMemes",
            "QOTD",
            "DBD"
        };

        public static StatusMessage[] STATUS_MESSAGES = { // extensions to mark as images (can be decoded by Discord)
            new("the sun rise, and seeing all the horizons just beyond view.", Discord.ActivityType.Watching),
            new("for intriguing, high quality content for your server.", Discord.ActivityType.Watching),
            new("urgent reports and keeping your server clean.", Discord.ActivityType.Listening),
            new("days pass by. Wondering if the constant passage of time will pause for even a brief break.", Discord.ActivityType.Watching),
            new("feedback and growing into a better bot.", Discord.ActivityType.Listening),
            new("for the next day, and glacing up the the sunrise, marking the beginning.", Discord.ActivityType.Watching)
        };

        public static string DATABASE_DIRECTORY = Compatiblity.buildPath(Directory.GetCurrentDirectory() + "/guilds/");
    }
}