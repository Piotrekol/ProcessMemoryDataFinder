using System.Collections.Generic;
using System.Linq;

namespace ProcessMemoryDataFinder.Structured.Tokenizer
{
    public class AddressTokenizer
    {
        private readonly List<TokenDefinition> _tokens;
        private readonly Dictionary<string, List<DslToken>> _dslCache = new Dictionary<string, List<DslToken>>();

        public AddressTokenizer()
        {
            _tokens = new List<TokenDefinition>
            {
                new TokenDefinition(TokenType.CloseBracket,"\\]",1),
                new TokenDefinition(TokenType.OpenBracket,"\\[",1),
                new TokenDefinition(TokenType.HexPatternValue,"([ABCDEF0-9?]){2,}",1),
                new TokenDefinition(TokenType.NumberValue,"\\d+",2),
                new TokenDefinition(TokenType.HexValue,"0x[ABCDEF0-9]+",1),
                new TokenDefinition(TokenType.StringValue,"\\w+",2),
                new TokenDefinition(TokenType.Add,"\\+",1),
                new TokenDefinition(TokenType.Subtract,"\\-",1)
            };
        }

        public List<DslToken> Tokenize(string pattern)
        {
            if (_dslCache.ContainsKey(pattern))
                return _dslCache[pattern];

            var result = new List<DslToken>();
            var tokenMatches = FindTokenMatches(pattern);
            var groupedByIndex = tokenMatches.GroupBy(x => x.StartIndex)
                .OrderBy(x => x.Key)
                .ToList();
            TokenMatch lastMatch = null;
            for (int i = 0; i < groupedByIndex.Count; i++)
            {
                var bestMatch = groupedByIndex[i].OrderBy(x => x.Precedence).First();
                if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                    continue;

                result.Add(new DslToken(bestMatch.TokenType, bestMatch.Value));
                lastMatch = bestMatch;
            }

            result.Add(new DslToken(TokenType.SequenceTerminator));
            return _dslCache[pattern] = result;
        }

        private List<TokenMatch> FindTokenMatches(string errorMessage)
        {
            var tokenMatches = new List<TokenMatch>();
            foreach (var tokenDefinition in _tokens)
                tokenMatches.AddRange(tokenDefinition.FindMatches(errorMessage).ToList());

            return tokenMatches;
        }
    }
}