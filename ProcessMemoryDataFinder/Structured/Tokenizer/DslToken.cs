namespace ProcessMemoryDataFinder.Structured.Tokenizer
{
    public class DslToken
    {
        public TokenType TokenType { get; set; }
        public string Value { get; set; }

        public DslToken(TokenType tokenType)
        {
            TokenType = tokenType;
            Value = string.Empty;
        }

        public DslToken(TokenType tokenType, string value)
        {
            TokenType = tokenType;
            Value = value;
        }

        public DslToken Clone()
        {
            return new DslToken(TokenType, Value);
        }
    }
}