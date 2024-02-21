using System.Reflection;

struct Post {
    public string? title;
    public string? body;
    public string? multimediaURL;
    public string? linkToPost;
    public List<string> additionalInformation;
    public bool? iterative;
    public Post(string? title = null, string? body = null, string? multimediaURL = null, string? linkToPost = null, string[]? additionalInformation = null, bool? iterative = false) {
        this.additionalInformation = new();
        for (int i = 0; i < additionalInformation.Length; i++) {
            this.additionalInformation.Add(additionalInformation[i]); // populates additionalInformation list with parameter data
        }
        this.title = title;
        this.body = body;
        this.multimediaURL = multimediaURL;
        this.linkToPost = linkToPost;
        this.iterative = iterative;
    }

    public bool isValid() {
        return title != null && body != null && linkToPost != null && iterative != null;
    }

};