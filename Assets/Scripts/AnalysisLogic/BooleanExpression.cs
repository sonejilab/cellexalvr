using CellexalVR.Filters;
using CellexalVR.General;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static CellexalVR.Extensions.Definitions;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// Static class that can parse a text file with some boolean expression containing genes, facs and/or attributes.
    /// </summary>
    public static class BooleanExpression
    {
        private static Dictionary<string, Tuple<Token, Token>> aliases = new Dictionary<string, Tuple<Token, Token>>();

        private static readonly string reservedCharacters = ":&|^.%()=!<> \t\n";

        private static bool Contains(this string s, char c)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] == c)
                {
                    return true;
                }
            }
            return false;
        }

        public class Token
        {
            public enum Type
            {
                WHITESPACE,
                AND, OR, XOR, NOT,
                L_PAR, R_PAR,
                VALUE_NAME, VALUE_NUM, VALUE_PERCENT,
                TYPE_GENE, TYPE_FACS, TYPE_ATTR, TYPE_ALIAS,
                OP_EQ, OP_NEQ, OP_GT, OP_GTEQ, OP_LT, OP_LTEQ,
                ATTR_YES, ATTR_NO,
                ALIAS
            }

            public Type type;
            public string text;
            public int stringIndex;

            public Token(Type type, string text, int stringIndex)
            {
                this.type = type;
                this.text = text;
                this.stringIndex = stringIndex;
            }

            public bool IsOperator()
            {
                return type == Type.OP_EQ || type == Type.OP_NEQ || type == Type.OP_GT || type == Type.OP_GTEQ || type == Type.OP_LT || type == Type.OP_LTEQ;
            }

            public bool IsType()
            {
                return type == Type.TYPE_ALIAS || type == Type.TYPE_ATTR || type == Type.TYPE_FACS || type == Type.TYPE_GENE;
            }

            public override bool Equals(object obj)
            {
                if (obj is Token)
                {
                    Token t = (Token)obj;
                    return t.type == type && t.text == text && t.stringIndex == stringIndex;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return type.ToString() + " " + text;
            }

            public static bool operator ==(Token t, Type type)
            {
                return t.type == type;
            }

            public static bool operator !=(Token t, Type type)
            {
                return t.type != type;
            }
        }

        public static Expr ParseFile(string filePath)
        {
            var streamReader = new StreamReader(filePath);
            Expr root = null;
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();

                if (line.Length == 0)
                    continue;
                if (line[0] == '#')
                    continue;
                root = ParseLine(line.ToLower());

            }

            streamReader.Close();
            return root;
        }

        private static Expr ParseLine(string line)
        {
            if (line.Length == 0)
            {
                return null;
            }
            List<Token> tokens = ParseLineForTokens(line);
            int index = 0;
            List<Expr> exprs = ParseTokens(tokens, ref index);
            if (exprs == null)
            {
                return null;
            }
            return CombineExprs(exprs);
        }

        private static Expr CombineExprs(List<Expr> exprs)
        {

            // combine not expression
            for (int i = 0; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is NotExpr && !exprs[i].combined)
                {
                    var notExpr = exprs[i] as NotExpr;
                    notExpr.subExpr = exprs[i + 1];
                    notExpr.combined = true;
                    exprs.RemoveAt(i + 1);
                }
            }

            // combine xor expression
            for (int i = 1; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is XorExpr && !exprs[i].combined)
                {
                    var xorExpr = exprs[i] as XorExpr;
                    xorExpr.subExpr1 = exprs[i - 1];
                    xorExpr.subExpr2 = exprs[i + 1];
                    xorExpr.combined = true;
                    exprs.RemoveAt(i - 1);
                    exprs.RemoveAt(i);
                    i--;
                }
            }

            // combine and expression
            for (int i = 1; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is AndExpr && !exprs[i].combined)
                {
                    var andExpr = exprs[i] as AndExpr;
                    andExpr.subExpr1 = exprs[i - 1];
                    andExpr.subExpr2 = exprs[i + 1];
                    andExpr.combined = true;
                    exprs.RemoveAt(i - 1);
                    exprs.RemoveAt(i);
                    i--;
                }
            }

            // combine or expression
            for (int i = 1; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is OrExpr && !exprs[i].combined)
                {
                    var orExpr = exprs[i] as OrExpr;
                    orExpr.subExpr1 = exprs[i - 1];
                    orExpr.subExpr2 = exprs[i + 1];
                    orExpr.combined = true;
                    exprs.RemoveAt(i - 1);
                    exprs.RemoveAt(i);
                    i--;
                }
            }
            if (exprs.Count > 1)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Expr e in exprs)
                {
                    stringBuilder.Append(e.ToString()).Append(" ");
                }
                CellexalLog.Log("FILTER ERROR: Could not combine expression " + stringBuilder.ToString());
                return null;
            }
            return exprs[0];
        }

        private static List<Expr> ParseTokens(List<Token> tokens, ref int index, int parLevel = 0)
        {

            List<Expr> exprs = new List<Expr>();
            if (tokens.Count == 0)
            {
                CellexalLog.Log("FILTER ERROR: Can not parse line with 0 tokens.");
                return null;
            }

            if (tokens[0].type == Token.Type.ALIAS)
            {
                ParseAlias(tokens);
                return null;
            }

            for (; index < tokens.Count; ++index)
            {
                Token token = tokens[index];
                switch (token.type)
                {
                    case Token.Type.AND:
                        exprs.Add(new AndExpr());
                        break;
                    case Token.Type.OR:
                        exprs.Add(new OrExpr());
                        break;
                    case Token.Type.NOT:
                        exprs.Add(new NotExpr());
                        break;
                    case Token.Type.XOR:
                        exprs.Add(new XorExpr());
                        break;
                    case Token.Type.L_PAR:
                        // recursively call this method
                        index++;
                        //CellexalLog.Log("Added " + token.ToString());
                        List<Expr> exprInPar = ParseTokens(tokens, ref index, parLevel + 1);
                        if (exprInPar == null)
                        {
                            return null;
                        }
                        exprs.Add(CombineExprs(exprInPar));
                        break;
                    case Token.Type.R_PAR:
                        if (parLevel == 0)
                        {
                            CellexalLog.Log("FILTER ERROR: Found ')' without accompanying '(' at character " + token.stringIndex);
                            return null;
                        }
                        // recursion base case
                        //CellexalLog.Log("Added " + token.ToString());
                        return exprs;
                    case Token.Type.TYPE_GENE:
                    case Token.Type.TYPE_FACS:
                    case Token.Type.TYPE_ATTR:
                        Expr expr = ParseComparerExpr(tokens, ref index);
                        if (expr == null)
                        {
                            return null;
                        }
                        exprs.Add(expr);
                        break;
                    case Token.Type.TYPE_ALIAS:
                        if (!ReplaceAlias(tokens, ref index, token.text))
                        {
                            return null;
                        }
                        break;
                }
                //if (token != Token.Type.L_PAR)
                //CellexalLog.Log("Added " + token.ToString());
            }
            if (parLevel > 0)
            {
                CellexalLog.Log("FITER WARNING: " + parLevel + " paranthesis not closed at end of filter, assuming all should close here.");
            }
            return exprs;
        }

        private static bool ReplaceAlias(List<Token> tokens, ref int index, string alias)
        {
            if (!aliases.ContainsKey(alias))
            {
                CellexalLog.Log("FILTER ERROR: Alias " + alias + " not found. Did you define it using alias:[alias_name] = [type]:[name] at the start of the filter file?");
                return false;
            }
            var realTokens = aliases[alias];
            tokens.Insert(index + 1, realTokens.Item1);
            tokens.Insert(index + 2, realTokens.Item2);
            return true;
        }

        private static void ParseAlias(List<Token> tokens)
        {
            if (tokens.Count != 5)
            {
                CellexalLog.Log("FILTER ERROR: Wrong format for alias. Correct format is \"alias:[alias_name] = [type]:[real_name] \". E.g: \"alias:g = gene:gata1\"");
                return;
            }

            Token aliasName = tokens[1];
            if (aliasName != Token.Type.VALUE_NAME)
            {
                CellexalLog.Log("FILTER ERROR:Expected name for alias but found " + aliasName.text + "at character " + aliasName.stringIndex + " when parsing alias.");
                return;
            }

            Token op = tokens[2];
            if (op != Token.Type.OP_EQ)
            {
                CellexalLog.Log("FILTER ERROR: Expected '=' but found \'" + op.text + "\' at character " + op.stringIndex + " when parsing alias.");
                return;
            }

            Token aliasType = tokens[3];
            if (!aliasType.IsType())
            {
                CellexalLog.Log("FILTER ERROR: Expected a type but found \'" + aliasType.text + "\' at character " + aliasType.stringIndex + " when parsing alias.");
                return;
            }

            Token aliasRealName = tokens[4];
            if (aliasRealName != Token.Type.VALUE_NAME)
            {
                CellexalLog.Log("FILTER ERROR: Expected a name but found \'" + aliasRealName.text + "\' at character " + aliasRealName.stringIndex + " when parsing alias.");
                return;
            }

            if (aliases.ContainsKey(aliasName.text))
            {
                var alias = aliases[aliasRealName.text];
                CellexalLog.Log("FILTER ERROR: Alias " + aliasName.text + " already defined as " + alias.Item1.text + " " + alias.Item2.text + " when parsing alias.");
                return;
            }

            aliases[aliasName.text] = new Tuple<Token, Token>(aliasType, aliasRealName);

        }

        private static List<Token> ParseLineForTokens(string s)
        {
            List<Token> tokens = new List<Token>();
            for (int j = 0; j < s.Length; /* j is incremented in the loop */)
            {
                Token token = ParseToken(s, j);
                j += token.text.Length;
                if (token.type != Token.Type.WHITESPACE)
                {
                    tokens.Add(token);
                }
            }
            return tokens;
        }

        private static ComparerExpr ParseComparerExpr(List<Token> tokens, ref int i)
        {
            Token typeToken = tokens[i];
            i++;
            if (typeToken == Token.Type.TYPE_ATTR)
            {
                if (i + 1 > tokens.Count)
                {
                    CellexalLog.Log("FILTER ERROR: Expected attribute expression but reached end of file.");
                    return null;
                }

                Token nameToken = tokens[i];
                i++;
                if (nameToken.type != Token.Type.VALUE_NAME)
                {
                    CellexalLog.Log("FILTER ERROR: Expected name but found " + nameToken.text + " at character " + nameToken.stringIndex);
                    return null;
                }

                Token operatorToken = tokens[i];
                if (!(operatorToken.type == Token.Type.ATTR_YES || operatorToken.type == Token.Type.ATTR_NO))
                {
                    CellexalLog.Log("FILTER ERROR: Expected yes or no but found " + operatorToken.text + " at character " + operatorToken.stringIndex);
                    return null;
                }

                return new AttributeExpr(nameToken.text, operatorToken.type == Token.Type.ATTR_YES);
            }
            else if (typeToken == Token.Type.TYPE_GENE)
            {
                if (!ParseNameOperatorValueTokens(tokens, ref i, out Token nameToken, out Token operatorToken, out Token valueToken))
                {
                    return null;
                }
                float value;
                if (!float.TryParse(valueToken.text, out value))
                {
                    CellexalLog.Log("FILTER ERROR: Value " + valueToken.text + " could not be parsed as float at character " + typeToken.stringIndex);
                    return null;
                }

                return new GeneExpr(nameToken.text, operatorToken, value, valueToken == Token.Type.VALUE_PERCENT);
            }

            else if (typeToken == Token.Type.TYPE_FACS)
            {
                if (!ParseNameOperatorValueTokens(tokens, ref i, out Token nameToken, out Token operatorToken, out Token valueToken))
                {
                    return null;
                }
                float value;
                if (!float.TryParse(valueToken.text, out value))
                {
                    CellexalLog.Log("FILTER ERROR: Value " + valueToken.text + " could not be parsed as float at character " + typeToken.stringIndex);
                    return null;
                }

                return new FacsExpr(nameToken.text, operatorToken, value, valueToken == Token.Type.VALUE_PERCENT);
            }
            else
            {
                CellexalLog.Log("FILTER ERROR: Expected type but found " + typeToken.text + " at character " + typeToken.stringIndex);
                return null;
            }
        }

        private static bool ParseNameOperatorValueTokens(List<Token> tokens, ref int i, out Token nameToken, out Token operatorToken, out Token valueToken)
        {
            nameToken = null;
            operatorToken = null;
            valueToken = null;

            if (i + 3 > tokens.Count)
            {
                CellexalLog.Log("FILTER ERROR: Expected expression but reached end of file.");
                return false;
            }

            nameToken = tokens[i];
            operatorToken = tokens[++i];
            valueToken = tokens[++i];

            if (nameToken.type != Token.Type.VALUE_NAME)
            {
                CellexalLog.Log("FILTER ERROR: Expected name but found " + nameToken.text + " at character " + nameToken.stringIndex);
                return false;
            }

            if (!operatorToken.IsOperator())
            {
                CellexalLog.Log("FILTER ERROR: Expected operator but found " + operatorToken.text + " at character " + operatorToken.stringIndex);
                return false;
            }

            if (!(valueToken.type == Token.Type.VALUE_NUM || valueToken.type == Token.Type.VALUE_PERCENT))
            {
                CellexalLog.Log("FILTER ERROR: Expected value but found " + operatorToken.text + " at character " + operatorToken.stringIndex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses a token.
        /// </summary>
        /// <param name="s">The string containing the token.</param>
        /// <param name="i">The token's start index in the string.</param>
        /// <returns>The parsed token.</returns>
        public static Token ParseToken(string s, int i)
        {
            int startIndex = i;
            char c = s[i];
            i++;
            if (c == '&')
            {
                if (i < s.Length && s[i] == '&')
                {
                    return new Token(Token.Type.AND, "&&", startIndex);
                }
                else
                {
                    return new Token(Token.Type.AND, "&", startIndex);
                }
            }
            else if (c == '|')
            {
                if (i < s.Length && s[i] == '|')
                {
                    return new Token(Token.Type.OR, "||", startIndex);
                }
                else
                {
                    return new Token(Token.Type.OR, "|", startIndex);
                }
            }
            else if (c == '!')
            {
                if (i < s.Length && s[i] == '=')
                {
                    return new Token(Token.Type.OP_NEQ, "!=", startIndex);
                }
                else
                {
                    return new Token(Token.Type.NOT, "!", startIndex);
                }
            }
            else if (c == '^')
            {
                return new Token(Token.Type.XOR, "^", startIndex);
            }
            else if (c == '(')
            {
                return new Token(Token.Type.L_PAR, "(", startIndex);
            }
            else if (c == ')')
            {
                return new Token(Token.Type.R_PAR, ")", startIndex);
            }
            else if (c == '=')
            {
                if (i < s.Length && s[i] == '=')
                {
                    return new Token(Token.Type.OP_EQ, "==", startIndex);
                }
                else
                {
                    return new Token(Token.Type.OP_EQ, "=", startIndex);
                }
            }
            else if (c == '>')
            {
                if (i < s.Length && s[i] == '=')
                {
                    return new Token(Token.Type.OP_GTEQ, ">=", startIndex);
                }
                else
                {
                    return new Token(Token.Type.OP_GT, ">", startIndex);
                }
            }
            else if (c == '<')
            {
                if (i < s.Length && s[i] == '=')
                {
                    return new Token(Token.Type.OP_LTEQ, "<=", startIndex);
                }
                else
                {
                    return new Token(Token.Type.OP_LT, "<", startIndex);
                }
            }
            else if (char.IsDigit(c) || c == '.')
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                i--;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.'))
                {
                    stringBuilder.Append(s[i]);
                    i++;
                }
                string value = stringBuilder.ToString();
                if (i < s.Length && s[i] == '%')
                {
                    return new Token(Token.Type.VALUE_PERCENT, value, startIndex);
                }
                else
                {
                    return new Token(Token.Type.VALUE_NUM, value, startIndex);
                }
            }
            else if (c == ':')
            {
                // skip the ':'
                // put the rest into a string
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

                while (i < s.Length && !reservedCharacters.Contains(s[i]))
                {
                    stringBuilder.Append(s[i]);
                    i++;
                }
                string text = stringBuilder.ToString();
                return new Token(Token.Type.VALUE_NAME, text, startIndex);
            }
            else if (i + 1 < s.Length && c == 'y' && s[i] == 'e' && s[i + 1] == 's')
            {
                return new Token(Token.Type.ATTR_YES, "yes", startIndex);
            }
            else if (i < s.Length && c == 'n' && s[i] == 'o')
            {
                return new Token(Token.Type.ATTR_NO, "no", startIndex);
            }
            else if (!char.IsWhiteSpace(c))
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

                // get the type
                i--;
                while (i < s.Length && !reservedCharacters.Contains(s[i]))
                {
                    stringBuilder.Append(s[i]);
                    i++;
                }
                string type = stringBuilder.ToString();
                Token.Type typeToken;
                if (type == "attr")
                {
                    typeToken = Token.Type.TYPE_ATTR;
                }
                else if (type == "facs")
                {
                    typeToken = Token.Type.TYPE_FACS;
                }
                else if (type == "gene")
                {
                    typeToken = Token.Type.TYPE_GENE;
                }
                else if (type == "alias")
                {
                    typeToken = Token.Type.ALIAS;
                }
                else
                {
                    typeToken = Token.Type.TYPE_ALIAS;
                }
                return new Token(typeToken, type, startIndex);
            }
            else
            {
                // we are only left with the option of c being a whitespace here
                while (i < s.Length && char.IsWhiteSpace(s[i]))
                {
                    i++;
                }
                return new Token(Token.Type.WHITESPACE, " ", startIndex);
            }
        }

        public abstract class Expr
        {
            public bool combined = false;
            private bool OpEqual(float value1, float value2) { return value1 == value2; }
            private bool OpNotEqual(float value1, float value2) { return value1 != value2; }
            private bool OpGreaterThan(float value1, float value2) { return value1 > value2; }
            private bool OpGreaterThanEqual(float value1, float value2) { return value1 >= value2; }
            private bool OpLessThan(float value1, float value2) { return value1 < value2; }
            private bool OpLessThanEqual(float value1, float value2) { return value1 <= value2; }

            public string OpToString(Func<float, float, bool> op)
            {

                if (op == OpEqual)
                    return "==";
                else if (op == OpNotEqual)
                    return "!=";
                else if (op == OpGreaterThan)
                    return ">";
                else if (op == OpGreaterThanEqual)
                    return ">=";
                else if (op == OpLessThan)
                    return "<";
                else if (op == OpLessThanEqual)
                    return "<=";
                else
                    return "[unknown operator]";
            }

            protected Func<float, float, bool> TokenToFunc(Token op)
            {
                switch (op.type)
                {
                    case Token.Type.OP_EQ:
                        return OpEqual;
                    case Token.Type.OP_NEQ:
                        return OpNotEqual;
                    case Token.Type.OP_GT:
                        return OpGreaterThan;
                    case Token.Type.OP_GTEQ:
                        return OpGreaterThanEqual;
                    case Token.Type.OP_LT:
                        return OpLessThan;
                    case Token.Type.OP_LTEQ:
                        return OpLessThanEqual;
                    default:
                        throw new ArgumentException("No suitable function for token \"" + op.ToString() + "\"");
                }
            }

            public abstract bool Eval(Cell cell);

            public abstract override string ToString();

            public abstract void GetGenes(ref List<string> result, bool onlyPercent = false);

            public abstract void GetFacs(ref List<string> result, bool onlyPercent = false);

            public abstract void SetFilterManager(FilterManager filterManager);

            public abstract void SwapPercentExpressions(Tuple<string, float, float>[] ranges);
        }

        public abstract class ComparerExpr : Expr { }

        public class GeneExpr : ComparerExpr
        {
            public string gene;
            public Func<float, float, bool> compare;
            public float value;
            public bool percent;

            public FilterManager filterManager;

            public GeneExpr() { }

            public GeneExpr(string gene, Token op, float value, bool percent)
            {
                this.gene = gene.ToLower();
                this.compare = TokenToFunc(op);
                this.value = value;
                this.percent = percent;
            }

            public override bool Eval(Cell cell)
            {
                Tuple<string, string> tuple = new Tuple<string, string>(gene, cell.Label);
                if (filterManager.GeneExprs.ContainsKey(tuple))
                {
                    return compare(filterManager.GeneExprs[new Tuple<string, string>(gene, cell.Label)], value);
                }
                else
                {
                    return compare(0f, value);
                }
            }

            public override void GetGenes(ref List<string> result, bool onlyPercent = false)
            {
                // equivalent to if !(onlypercent && !percent)
                if ((!onlyPercent || percent) && !result.Contains(gene))
                {
                    result.Add(gene);
                }
            }

            public override void GetFacs(ref List<string> result, bool onlyPercent = false) { }

            public override string ToString()
            {
                return "gene:" + gene + " " + OpToString(compare) + " " + value;
            }

            public override void SwapPercentExpressions(Tuple<string, float, float>[] ranges)
            {
                if (percent)
                {
                    foreach (Tuple<string, float, float> tuple in ranges)
                    {
                        if (tuple.Item1 == this.gene)
                        {
                            value = (value / 100f) * tuple.Item3;
                            return;
                        }
                    }
                }
            }

            public override void SetFilterManager(FilterManager filterManager)
            {
                this.filterManager = filterManager;
            }
        }

        public class FacsExpr : ComparerExpr
        {
            public string facs;
            public Func<float, float, bool> compare;
            public float value;
            public bool percent;
            public FacsExpr() { }

            public FacsExpr(string facs, Token op, float value, bool percent)
            {
                this.facs = facs.ToLower();
                this.compare = TokenToFunc(op);
                this.value = value;
                this.percent = percent;
            }

            public override bool Eval(Cell cell)
            {
                return compare(cell.Facs[facs], value);
            }

            public override void GetGenes(ref List<string> result, bool onlyPercent = false) { }

            public override void GetFacs(ref List<string> result, bool onlyPercent = false)
            {
                // equivalent to if !(onlypercent && !percent)
                if ((!onlyPercent || percent) && !result.Contains(facs))
                {
                    result.Add(facs);
                }
            }

            public override string ToString()
            {
                return "facs:" + facs + " " + OpToString(compare) + " " + value;
            }

            public override void SwapPercentExpressions(Tuple<string, float, float>[] ranges)
            {
                if (percent)
                {
                    foreach (Tuple<string, float, float> tuple in ranges)
                    {
                        if (tuple.Item1 == this.facs)
                        {
                            value = (value / 100f) * tuple.Item3;
                            return;
                        }
                    }
                }
            }

            public override void SetFilterManager(FilterManager filterManager) { }
        }

        public class AttributeExpr : ComparerExpr
        {
            public string attribute;
            public bool include;
            public AttributeExpr() { }

            public AttributeExpr(string attribute, bool include)
            {
                this.attribute = attribute.ToLower();
                this.include = include;
            }

            public override bool Eval(Cell cell)
            {
                return cell.Attributes.ContainsKey(attribute);
            }

            public override void GetGenes(ref List<string> result, bool onlyPercent = false) { }

            public override void GetFacs(ref List<string> result, bool onlyPercent = false) { }

            public override string ToString()
            {
                return "attr:" + attribute + " " + include;
            }

            public override void SwapPercentExpressions(Tuple<string, float, float>[] ranges) { }

            public override void SetFilterManager(FilterManager filterManager) { }
        }

        public class AndExpr : Expr
        {
            public Expr subExpr1;
            public Expr subExpr2;
            public AndExpr() { }
            public AndExpr(Expr subExpr1, Expr subExpr2)
            {
                this.subExpr1 = subExpr1;
                this.subExpr2 = subExpr2;
            }

            public override bool Eval(Cell cell)
            {
                return subExpr1.Eval(cell) && subExpr2.Eval(cell);
            }

            public override void GetGenes(ref List<string> result, bool onlyPercent = false)
            {
                subExpr1.GetGenes(ref result, onlyPercent);
                subExpr2.GetGenes(ref result, onlyPercent);
            }

            public override void GetFacs(ref List<string> result, bool onlyPercent = false)
            {
                subExpr1.GetFacs(ref result, onlyPercent);
                subExpr2.GetFacs(ref result, onlyPercent);
            }

            public override string ToString()
            {
                return "(" + subExpr1.ToString() + " && " + subExpr2.ToString() + ")";
            }

            public override void SwapPercentExpressions(Tuple<string, float, float>[] ranges)
            {
                subExpr1.SwapPercentExpressions(ranges);
                subExpr2.SwapPercentExpressions(ranges);
            }
            public override void SetFilterManager(FilterManager filterManager)
            {
                subExpr1.SetFilterManager(filterManager);
                subExpr2.SetFilterManager(filterManager);
            }
        }

        public class OrExpr : Expr
        {
            public Expr subExpr1;
            public Expr subExpr2;
            public OrExpr() { }
            public OrExpr(Expr subExpr1, Expr subExpr2)
            {
                this.subExpr1 = subExpr1;
                this.subExpr2 = subExpr2;
            }

            public override bool Eval(Cell cell)
            {
                return subExpr1.Eval(cell) || subExpr2.Eval(cell);
            }

            public override void GetGenes(ref List<string> result, bool onlyPercent = false)
            {
                subExpr1.GetGenes(ref result, onlyPercent);
                subExpr2.GetGenes(ref result, onlyPercent);
            }

            public override void GetFacs(ref List<string> result, bool onlyPercent = false)
            {
                subExpr1.GetFacs(ref result, onlyPercent);
                subExpr2.GetFacs(ref result, onlyPercent);
            }

            public override string ToString()
            {
                return "(" + subExpr1.ToString() + " || " + subExpr2.ToString() + ")";
            }

            public override void SwapPercentExpressions(Tuple<string, float, float>[] ranges)
            {
                subExpr1.SwapPercentExpressions(ranges);
                subExpr2.SwapPercentExpressions(ranges);
            }

            public override void SetFilterManager(FilterManager filterManager)
            {
                subExpr1.SetFilterManager(filterManager);
                subExpr2.SetFilterManager(filterManager);
            }
        }

        public class XorExpr : Expr
        {
            public Expr subExpr1;
            public Expr subExpr2;
            public XorExpr() { }
            public XorExpr(Expr subExpr1, Expr subExpr2)
            {
                this.subExpr1 = subExpr1;
                this.subExpr2 = subExpr2;
            }

            public override bool Eval(Cell cell)
            {
                return subExpr1.Eval(cell) ^ subExpr2.Eval(cell);
            }

            public override void GetGenes(ref List<string> result, bool onlyPercent = false)
            {
                subExpr1.GetGenes(ref result, onlyPercent);
                subExpr2.GetGenes(ref result, onlyPercent);
            }

            public override void GetFacs(ref List<string> result, bool onlyPercent = false)
            {
                subExpr1.GetFacs(ref result, onlyPercent);
                subExpr2.GetFacs(ref result, onlyPercent);
            }

            public override string ToString()
            {
                return "(" + subExpr1.ToString() + " ^ " + subExpr2.ToString() + ")";
            }

            public override void SwapPercentExpressions(Tuple<string, float, float>[] ranges)
            {
                subExpr1.SwapPercentExpressions(ranges);
                subExpr2.SwapPercentExpressions(ranges);
            }

            public override void SetFilterManager(FilterManager filterManager)
            {
                subExpr1.SetFilterManager(filterManager);
                subExpr2.SetFilterManager(filterManager);
            }
        }

        public class NotExpr : Expr
        {
            public Expr subExpr;
            public NotExpr() { }
            public NotExpr(Expr subExpr)
            {
                this.subExpr = subExpr;

            }

            public override bool Eval(Cell cell)
            {
                return !(subExpr.Eval(cell));
            }

            public override void GetGenes(ref List<string> result, bool onlyPercent = false)
            {
                subExpr.GetGenes(ref result, onlyPercent);
            }

            public override void GetFacs(ref List<string> result, bool onlyPercent = false)
            {
                subExpr.GetFacs(ref result, onlyPercent);
            }

            public override string ToString()
            {
                return "!(" + subExpr.ToString() + ")";
            }

            public override void SwapPercentExpressions(Tuple<string, float, float>[] ranges)
            {
                subExpr.SwapPercentExpressions(ranges);
            }

            public override void SetFilterManager(FilterManager filterManager)
            {
                subExpr.SetFilterManager(filterManager);
            }
        }

    }
}
