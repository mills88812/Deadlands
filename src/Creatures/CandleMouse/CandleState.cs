using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Deadlands.Creatures.CandleMouse
{
    internal class CandleState : HealthState
    {
        public CandleState(AbstractCreature creature) : base(creature)
        {
        }
        public override string ToString()
        {
            string text = base.HealthBaseSaveString();
            foreach (KeyValuePair<string, string> keyValuePair in this.unrecognizedSaveStrings)
            {
                text = string.Concat(new string[]
                {
                text,
                "<cB>",
                keyValuePair.Key,
                "<cC>",
                keyValuePair.Value
                });
            }
            return text;
        }

        public override void LoadFromString(string[] s)
        {
            base.LoadFromString(s);
            for (int i = 0; i < s.Length; i++)
            {
                string text = Regex.Split(s[i], "<cC>")[0];
            }
        }
    }
}
