namespace SocialGuard.Client.GraphQL
{
    public partial class TrustlistEntrySortInput
    {
        public SortEnumType Id { get; set; }
        public SortEnumType UserId { get; set; }
        public SortEnumType EntryAt { get; set; }
        public SortEnumType LastEscalated { get; set; }
        public SortEnumType EscalationLevel { get; set; }
        public SortEnumType EscalationNote { get; set; }
        public SortEnumType EmitterId { get; set; }
        public EmitterSortInput Emitter { get; set; }
    }
}