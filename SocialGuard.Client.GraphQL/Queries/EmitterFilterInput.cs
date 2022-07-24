namespace SocialGuard.Client.GraphQL
{
    public partial class EmitterFilterInput
    {
        public List<EmitterFilterInput> And { get; set; }
        public List<EmitterFilterInput> Or { get; set; }
        public StringOperationFilterInput Login { get; set; }
        public EmitterTypeOperationFilterInput EmitterType { get; set; }
        public ComparableUInt64OperationFilterInput Snowflake { get; set; }
        public StringOperationFilterInput DisplayName { get; set; }
    }
}