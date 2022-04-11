using JetBrains.Annotations;

namespace SpeckleConnectionManagerUI.Models
{
    public partial class Row
    {
        public string hash { get; set; }
        public Speckle.Core.Credentials.Account content { get; set; }
    }

    public partial class Tokens
    {
        public string token { get; set; }
        public string refreshToken { get; set; }
    }
}

