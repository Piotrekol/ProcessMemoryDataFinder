using System;
using System.Collections.Generic;

namespace ProcessMemoryDataFinder.API
{
    public abstract class MemoryReaderEx : MemoryReader
    {
        protected Dictionary<int, SigEx> Signatures = new Dictionary<int, SigEx>();

        protected MemoryReaderEx(string processName, string mainWindowTitleHint = null) : base(processName, mainWindowTitleHint)
        {
        }

        protected override void ProcessChanged()
        {
            ResetAllSignatures();
        }

        public void ResetAllSignatures()
        {
            foreach (var signature in Signatures)
            {
                signature.Value.Reset();
            }
        }

        protected virtual void ResetPointer(int signatureId)
        {
            var sig = Signatures[signatureId];
            sig.ResetPointer();
        }

        protected virtual void Reset(int signatureId, bool fowardToParent = true)
        {
            var sig = Signatures[signatureId];
            sig.Reset();
        }

        protected virtual SigEx InitSignature(int signatureId)
        {
            if (!Signatures.ContainsKey(signatureId))
            {
                throw new KeyNotFoundException();
            }

            var sig = Signatures[signatureId];
            sig.SetFindPatternF(FindPattern);
            sig.SetReadDataF(ReadData);
            return sig;
        }

        protected static byte[] UnpackStr(string str)
        {
            return StringToByteArray(str);
        }

        protected static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        #region Signature readers

        protected virtual int GetInt(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetInt();
        }

        protected virtual float GetFloat(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetFloat();
        }

        protected virtual IntPtr GetPointer(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetPointer();
        }

        protected virtual ushort GetUShort(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetUShort();
        }

        protected virtual double GetDouble(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetDouble();
        }

        protected virtual string GetString(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetString();
        }


        protected virtual List<int> GetIntList(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetIntList();
        }

        #endregion
    }
}