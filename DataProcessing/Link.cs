using StarBot;

class Link
{
    public string? link;

    public Link(string link)
    {
        string processedLink = link.ToLower().Trim();
        this.link = IsValidLink(processedLink) ? processedLink : null;
    }

    public bool IsValidLink()
    {
        bool result = Uri.TryCreate(link, UriKind.Absolute, out Uri? uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        return result;
    }

    private bool IsValidLink(string link)
    {
        bool result = Uri.TryCreate(link, UriKind.Absolute, out Uri? uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        return result;
    }


    public bool IsImageLink()
    {

        for (int i = 0; i < Config.IMAGE_EXTENSIONS.Length; i++)
        {
            if (link.EndsWith(Config.IMAGE_EXTENSIONS[i]))
            {
                return true;
            }
        }
        return false;
    }
}