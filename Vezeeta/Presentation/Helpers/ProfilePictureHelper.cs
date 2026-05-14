namespace Presentation.Helpers
{
    public static class ProfilePictureHelper
    {
        public static string GetUrl(
            string? profilePicture,
            string? fullName,
            string background = "1a73e8",
            string color = "ffffff",
            int size = 160)
        {
            if (string.IsNullOrWhiteSpace(profilePicture))
            {
                var name = string.IsNullOrWhiteSpace(fullName)
                    ? "User"
                    : fullName.Trim();

                return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background={background}&color={color}&size={size}";
            }



            //var path = profilePicture.Trim().Replace("\\", "/");

            //if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            //    || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            //    || path.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            //{
            //    return path;
            //}

            //if (path.StartsWith("~/", StringComparison.Ordinal))
            //    return "/" + path[2..];

            //if (path.StartsWith("/", StringComparison.Ordinal))
            //    return path;

            //return "/" + path;
            return "";
        }
    }
}
