namespace SocialGuard.Client.GraphQL
{
    public partial class ComparableByteOperationFilterInput
    {
        public string Eq { get; set; }
        public string Neq { get; set; }
        public List<string> In { get; set; }
        public List<string> Nin { get; set; }
        public string Gt { get; set; }
        public string Ngt { get; set; }
        public string Gte { get; set; }
        public string Ngte { get; set; }
        public string Lt { get; set; }
        public string Nlt { get; set; }
        public string Lte { get; set; }
        public string Nlte { get; set; }
    }
}