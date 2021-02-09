﻿using System;
using System.Collections.Generic;
using ProcessMemoryDataFinder.API;
using ProcessMemoryDataFinder.Structured.Tokenizer;

namespace ProcessMemoryDataFinder.Structured
{
    public class AddressFinder
    {
        private readonly MemoryReader _memoryReader;
        private readonly Dictionary<string, string> _constantAddresses;
        private readonly Dictionary<string, IntPtr> _constantAddressesCache;
        private readonly Dictionary<(IReadOnlyList<DslToken> tokens, IntPtr baseAddress), IntPtr> _groupReadAddressesCache;

        private readonly AddressTokenizer _addressTokenizer = new AddressTokenizer();
        public AddressFinder(MemoryReader memoryReader, Dictionary<string, string> constantAddresses)
        {
            _memoryReader = memoryReader;
            _memoryReader.ProcessChanged += MemoryReaderOnProcessChanged;
            _constantAddresses = constantAddresses;
            _constantAddressesCache = new Dictionary<string, IntPtr>();
            _groupReadAddressesCache = new Dictionary<(IReadOnlyList<DslToken> tokens, IntPtr baseAddress), IntPtr>(64);
        }

        private void MemoryReaderOnProcessChanged(object sender, EventArgs e)
        {
            ResetCache();
        }

        private void ResetCache()
        {
            _constantAddressesCache.Clear();
            ResetGroupReadCache();
        }

        public void ResetGroupReadCache() => _groupReadAddressesCache.Clear();

        public IntPtr FindAddress(IReadOnlyList<DslToken> tokens, IntPtr baseAddress)
        {
            if (_groupReadAddressesCache.ContainsKey((tokens, baseAddress)))
                return _groupReadAddressesCache[(tokens, baseAddress)];

            var lastToken = TokenType.SequenceTerminator;
            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case TokenType.StringValue:

                        if (!_constantAddresses.ContainsKey(token.Value))
                            throw new UnknownConstantAddressException();
                        if (_constantAddressesCache.ContainsKey(token.Value))
                            baseAddress = _constantAddressesCache[token.Value];
                        else
                        {
                            var findResult = FindAddress(_addressTokenizer.Tokenize(_constantAddresses[token.Value]), IntPtr.Zero);
                            if (findResult == IntPtr.Zero)
                                return IntPtr.Zero;
                            baseAddress = _constantAddressesCache[token.Value] = findResult;
                        }

                        break;
                    case TokenType.HexPatternValue:
                        var pattern = PatternHelpers.StringToBytePatternArray(token.Value);
                        baseAddress = _memoryReader.FindPattern(pattern.Bytes, pattern.Mask, 0, pattern.Mask.Contains("?"));
                        break;
                    case TokenType.CloseBracket:
                        var result = _memoryReader.ReadData(baseAddress, (uint)IntPtr.Size);
                        if (result == null || result.Length != IntPtr.Size || AllZeros(result))
                            return _groupReadAddressesCache[(tokens, baseAddress)] = IntPtr.Zero;

                        baseAddress = new IntPtr(BitConverter.ToInt32(result, 0));

                        break;
                    case TokenType.HexValue when lastToken == TokenType.Add || lastToken == TokenType.Subtract:
                        ProcessValue(Convert.ToInt32(token.Value, 16));
                        break;
                    case TokenType.NumberValue when lastToken == TokenType.Add || lastToken == TokenType.Subtract:
                        ProcessValue(Convert.ToInt32(token.Value));
                        break;
                    case TokenType.OpenBracket:
                    case TokenType.Add:
                    case TokenType.Subtract:
                    case TokenType.SequenceTerminator:
                        break;
                    default:
                        throw new InvalidAddressPatternException();
                }

                lastToken = token.TokenType;
            }

            return _groupReadAddressesCache[(tokens, baseAddress)] = baseAddress;

            bool AllZeros(byte[] bytes)
            {
                //Intentionally not using linq
                //bytes.All(b=>b==0);
                foreach (var b in bytes)
                {
                    if (b != 0)
                        return false;
                }

                return true;
            }
            void ProcessValue(int value)
            {
                baseAddress += lastToken == TokenType.Add
                    ? value
                    : -value;
            }
        }
        public class UnknownConstantAddressException : Exception { }
        public class InvalidAddressPatternException : Exception { }

    }
}