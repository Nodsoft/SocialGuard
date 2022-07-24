namespace SocialGuard.Client.GraphQL
{
    public partial class TrustlistEntryFilterInput
    {
        public List<TrustlistEntryFilterInput> And { get; set; }
        public List<TrustlistEntryFilterInput> Or { get; set; }
        public ComparableGuidOperationFilterInput Id { get; set; }
        public ComparableUInt64OperationFilterInput UserId { get; set; }
        public ComparableDateTimeOperationFilterInput EntryAt { get; set; }
        public ComparableDateTimeOperationFilterInput LastEscalated { get; set; }
        public ComparableByteOperationFilterInput EscalationLevel { get; set; }
        public StringOperationFilterInput EscalationNote { get; set; }
        public StringOperationFilterInput EmitterId { get; set; }
        public EmitterFilterInput Emitter { get; set; }
    }
}