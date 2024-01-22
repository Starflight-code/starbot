using Newtonsoft.Json.Linq;
using StarBot;

static class Validation {

    public static string preProcessValue(string value) {
        return value.ToLower().Trim();
    }
    public static bool IsLinkToImage(string link) {
        string[] extensions = {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif"
        };

        for (int i = 0; i < Config.IMAGE_EXTENSIONS.Length; i++) {
            if (link.EndsWith(Config.IMAGE_EXTENSIONS[i])) {
                return true;
            }
        }
        return false;
    }
    public static bool DuplicateAndImageCheck(JToken? post, string postIDHistory) {
        try {
            if (!IsLinkToImage(post["url_overridden_by_dest"].ToString())) {
                return false;
            }
        } catch (NullReferenceException) {
            return false;
        }

        string[] postIDs = postIDHistory.Split(",");
        string currentPostID = GeneratePostID(post);
        for (int i = 0; i < postIDs.Length; i++) {
            if (currentPostID == postIDs[i]) {
                return false;
            }
        }
        return true;
    }
    public static string GeneratePostID(JToken? post) {
        return post["subreddit_id"].ToString() + "-" + post["id"].ToString();
    }
}