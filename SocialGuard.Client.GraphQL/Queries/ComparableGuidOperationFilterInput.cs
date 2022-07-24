namespace SocialGuard.Client.GraphQL
{
    using System;

    public partial class ComparableGuidOperationFilterInput
    {
        public Guid? Eq { get; set; }
        public Guid? Neq { get; set; }
        public List<Guid> In { get; set; }
        public List<Guid> Nin { get; set; }
        public Guid? Gt { get; set; }
        public Guid? Ngt { get; set; }
        public Guid? Gte { get; set; }
        public Guid? Ngte { get; set; }
        public Guid? Lt { get; set; }
        public Guid? Nlt { get; set; }
        public Guid? Lte { get; set; }
        public Guid? Nlte { get; set; }
    }
}