using System;
using System.Collections.Generic;

namespace ProcessMemoryDataFinder.API
{
    public abstract class SigMemoryReader : MemoryReader
    {
        protected Dictionary<int, SigEx> Signatures = new Dictionary<int, SigEx>();
        protected IObjectReader ObjectReader;
        protected SigMemoryReader(ProcessTargetOptions processTargetOptions) : base(processTargetOptions)
        {
            ObjectReader = new ObjectReader(this);
            ProcessChanged += (_, __) => ResetAllSignatures();
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
            sig.SetObjectReader(ObjectReader);
            return sig;
        }

        #region Signature readers

        protected virtual bool GetBoolean(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetBoolean();
        }

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

        protected virtual byte GetByte(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetByte();
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

        protected virtual int[] GetIntArray(int signatureId)
        {
            var sig = InitSignature(signatureId);
            return sig.GetIntArray();
        }

        #endregion
    }
}