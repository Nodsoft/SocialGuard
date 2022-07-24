namespace SocialGuard.Client.GraphQL
{
    public partial class EmitterTypeOperationFilterInput
    {
        public EmitterType Eq { get; set; }
        public EmitterType Neq { get; set; }
        public List<EmitterType> In { get; set; }
        public List<EmitterType> Nin { get; set; }
    }
}