namespace ProcessMemoryDataFinder.Structured.Tokenizer
{
    public enum TokenType
    {
        /// <summary>
        /// Start of nested read
        /// </summary>
        OpenBracket,
        /// <summary>
        /// End of nested read, results in pointer read
        /// </summary>
        CloseBracket,
        /// <summary>
        /// AoB value, results in non-cached memory search
        /// </summary>
        HexPatternValue,
        /// <summary>
        /// Constant hex value used in memory calculations
        /// </summary>
        HexValue,
        /// <summary>
        /// Constant numerical value used in memory calculations
        /// </summary>
        NumberValue,
        /// <summary>
        /// Constant address with predefined AoB pattern, results in memory search. Final address will be cached
        /// </summary>
        StringValue,
        /// <summary>
        /// Add value to effective address of parent
        /// </summary>
        Add,
        /// <summary>
        /// Subtract value from effective address of parent
        /// </summary>
        Subtract,
        /// <summary>
        /// End of the sequence
        /// </summary>
        SequenceTerminator
    }
}