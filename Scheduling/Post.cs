using System.Reflection;

struct Post {
    string? title;
    string? body;
    string? multimediaURL;
    string? linkToPost;
    List<string> additionalInformation;
    public Post(string? title = null, string? body = null, string? multimediaURL = null, string? linkToPost = null, string[]? additionalInformation = null) {
        this.additionalInformation = new();
        for (int i = 0; i < additionalInformation.Length; i++) {
            this.additionalInformation.Add(additionalInformation[i]); // populates additionalInformation list with parameter data
        }
        this.title = title;
        this.body = body;
        this.multimediaURL = multimediaURL;
        this.linkToPost = linkToPost;
    }

};