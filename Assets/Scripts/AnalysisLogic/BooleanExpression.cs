using System.Collections.Generic;
using System.IO;

namespace CellexalVR.AnalysisLogic
{

    public static class BooleanExpression
    {
        public static Dictionary<string, string> aliases = new Dictionary<string, string>();
        private enum Token { INVALID, AND, OR, XOR, NOT, L_PAR, R_PAR, VALUE }

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
                root = ParseLine(line);

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
            int i = 0;
            List<Expr> exprs = new List<Expr>();
            while (i < line.Length)
            {
                exprs.Add(ParseExpr(line, ref i));
            }
            for (i = 0; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is NotExpr)
                {
                    var notExpr = exprs[i] as NotExpr;
                    notExpr.subExpr = exprs[i + 1];
                    exprs.RemoveAt(i + 1);
                }
            }

            for (i = 1; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is XorExpr)
                {
                    var xorExpr = exprs[i] as XorExpr;
                    xorExpr.subExpr1 = exprs[i - 1];
                    xorExpr.subExpr2 = exprs[i + 1];
                    exprs.RemoveAt(i - 1);
                    exprs.RemoveAt(i);
                    i--;
                }
            }

            for (i = 1; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is AndExpr)
                {
                    var andExpr = exprs[i] as AndExpr;
                    andExpr.subExpr1 = exprs[i - 1];
                    andExpr.subExpr2 = exprs[i + 1];
                    exprs.RemoveAt(i - 1);
                    exprs.RemoveAt(i);
                    i--;
                }
            }

            for (i = 1; i < exprs.Count - 1; ++i)
            {
                if (exprs[i] is OrExpr)
                {
                    var orExpr = exprs[i] as OrExpr;
                    orExpr.subExpr1 = exprs[i - 1];
                    orExpr.subExpr2 = exprs[i + 1];
                    exprs.RemoveAt(i - 1);
                    exprs.RemoveAt(i);
                    i--;
                }
            }

            return exprs[0];
        }

        private static Expr ParseExpr(string s, ref int i)
        {
            char c = s[i];
            while (c == ' ')
            {
                i++;
                c = s[i];
            }

            i++;
            Token currentToken = ParseChar(c);
            switch (currentToken)
            {
                case Token.AND:
                    return new AndExpr();
                case Token.OR:
                    return new OrExpr();
                case Token.NOT:
                    return new NotExpr();
                case Token.XOR:
                    return new XorExpr();
                case Token.L_PAR:
                    int rightParIndex = s.IndexOf(')', i);
                    string exprInPar = s.Substring(i, rightParIndex - i);
                    i = rightParIndex + 1;
                    return ParseLine(exprInPar);
                case Token.VALUE:
                    int tokenLength = 1;
                    do
                    {
                        tokenLength++;
                        i++;
                        if (i >= s.Length)
                            break;

                        c = s[i];
                    } while (ParseChar(c) == Token.VALUE);
                    return new AttributeExpr(s.Substring(i - tokenLength, tokenLength));
            }
            return null;
        }

        private static Token ParseChar(char c)
        {
            if (c == '&')
            {
                return Token.AND;
            }
            else if (c == '|')
            {
                return Token.OR;
            }
            else if (c == '!')
            {
                return Token.NOT;
            }
            else if (c == '^')
            {
                return Token.XOR;
            }
            else if (c == '(')
            {
                return Token.L_PAR;
            }
            else if (c == ')')
            {
                return Token.R_PAR;
            }
            else if (!char.IsWhiteSpace(c))
            {
                return Token.VALUE;
            }
            else
            {
                return Token.INVALID;
            }
        }

        public abstract class Expr
        {
            public abstract bool Eval(Cell cell);
            public abstract override string ToString();
        }

        public class GeneExpr : Expr
        {
            public string gene;
            public GeneExpr() { }
            public GeneExpr(string gene)
            {
                if (aliases.ContainsKey(gene))
                {
                    this.gene = aliases[gene];
                }
                else
                {
                    this.gene = gene;
                }
            }

            public override bool Eval(Cell cell)
            {
                return false;
            }

            public override string ToString()
            {
                return gene;
            }
        }

        public class FacsExpr : Expr
        {
            public string facs;
            public FacsExpr() { }
            public FacsExpr(string facs)
            {
                if (aliases.ContainsKey(facs))
                {
                    this.facs = aliases[facs];
                }
                else
                {
                    this.facs = facs;
                }
            }

            public override bool Eval(Cell cell)
            {
                return cell.Facs.ContainsKey(facs.ToLower());
            }

            public override string ToString()
            {
                return facs;
            }
        }

        public class AttributeExpr : Expr
        {
            public string attribute;
            public AttributeExpr() { }
            public AttributeExpr(string attribute)
            {
                if (aliases.ContainsKey(attribute))
                {
                    this.attribute = aliases[attribute];
                }
                else
                {
                    this.attribute = attribute;
                }
            }

            public override bool Eval(Cell cell)
            {
                return cell.Attributes.ContainsKey(attribute.ToLower());
            }

            public override string ToString()
            {
                return attribute;
            }
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

            public override string ToString()
            {
                return "(" + subExpr1.ToString() + " & " + subExpr2.ToString() + ")";
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

            public override string ToString()
            {
                return "(" + subExpr1.ToString() + " | " + subExpr2.ToString() + ")";
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

            public override string ToString()
            {
                return "(" + subExpr1.ToString() + " ^ " + subExpr2.ToString() + ")";
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

            public override string ToString()
            {
                return "!(" + subExpr.ToString() + ")";
            }
        }

    }
}