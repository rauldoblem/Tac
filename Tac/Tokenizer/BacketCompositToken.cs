﻿//using Newtonsoft.Json;
//using System.Collections.Generic;

//namespace Tac.Parser
//{
//    public class BacketCompositToken : CompositToken
//    {
//        public BacketCompositToken(char openBracket, char closeBracket, IEnumerable<IToken> tokens) : base(tokens)
//        {
//            OpenBracket = openBracket;
//            CloseBracket = closeBracket;
//        }

//        public char OpenBracket { get; }
//        public char CloseBracket { get; }

//        public override string ToString()
//        {
//            return JsonConvert.SerializeObject(this, Formatting.Indented);
//        }

//    }

//}