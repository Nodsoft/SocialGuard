namespace SocialGuard.Client.GraphQL
{
    public partial class StringOperationFilterInput
    {
        public List<StringOperationFilterInput> And { get; set; }
        public List<StringOperationFilterInput> Or { get; set; }
        public string Eq { get; set; }
        public string Neq { get; set; }
        public string Contains { get; set; }
        public string Ncontains { get; set; }
        public List<string> In { get; set; }
        public List<string> Nin { get; set; }
        public string StartsWith { get; set; }
        public string NstartsWith { get; set; }
        public string EndsWith { get; set; }
        public string NendsWith { get; set; }
    }
}