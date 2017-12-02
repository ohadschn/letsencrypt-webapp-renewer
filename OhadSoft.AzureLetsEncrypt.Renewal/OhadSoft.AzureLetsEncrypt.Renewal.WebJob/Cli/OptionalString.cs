namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli
{
    public class OptionalString
    {
        private readonly string m_value;

        public OptionalString(string value)
        {
            m_value = value;
        }

        public static implicit operator string(OptionalString optionalString)
        {
            return optionalString?.m_value;
        }

        public override string ToString()
        {
            return m_value;
        }
    }
}