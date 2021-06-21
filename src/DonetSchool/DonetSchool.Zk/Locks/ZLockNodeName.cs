using System;

namespace DonetSchool.Zk.Locks
{
    internal sealed class ZLockNodeName : IComparable<ZLockNodeName>
    {
        private readonly int _sequence;
        private readonly string _perfix;

        public ZLockNodeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new LockKeyErrorException(name);
            }
            _sequence = -1;
            Name = name;
            _perfix = name;
            int idx = name.LastIndexOf('-');
            if (idx >= 0)
            {
                _perfix = name.Substring(0, idx);
                if (idx + 1 >= name.Length)
                {
                    throw new LockKeyErrorException(name);
                }
                else if (!int.TryParse(name.Substring(idx + 1), out _sequence))
                {
                    throw new LockKeyErrorException(name);
                }
            }
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ZLockNodeName nodeName && string.Equals(Name, nodeName.Name);
        }

        public int CompareTo(ZLockNodeName that)
        {
            int answer = this._sequence - that._sequence;
            if (answer == 0)
            {
                return string.Compare(this._perfix, that._perfix, StringComparison.Ordinal);
            }
            return answer;
        }
    }
}