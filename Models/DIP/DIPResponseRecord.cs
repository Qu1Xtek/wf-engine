namespace WorkflowConfigurator.Models.DIP
{
    public class DIPResponseRecord<T>
    {
        public string IntegrityRecordId { get; set; }
        public string PreviousTransactionId { get; set; }

        public string TransactionId { get; set; }

        public string[] Index1s { get; set; }
        public string[] Index2s { get; set; }
        public string[] Index3s { get; set; }

        public long CreatedAt { get; set; }
        public long DeletedAt { get; set; }
        public long UpdatedAt { get; set; }

        public string Ipfs { get; set; }

        public T Meta { get; set; }
    }
}
