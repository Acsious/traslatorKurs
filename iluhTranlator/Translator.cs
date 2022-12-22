using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace translatorKurs
{
    class Translator
    {
        enum States { S, VAR, EXP, IF, READ, WRITE, FIN }

        static public string Analyze(List<Variable> listOfLexemes)
        {
            var stackOfLexemes = new Stack<Variable>(listOfLexemes.AsEnumerable().Reverse());
            var sourceCSCode = new StringBuilder("using System;");

            var state = States.S;

            while (!state.Equals(States.FIN))
            {
                switch (state)
                {
                    case States.S:
                        {
                            var tmp = stackOfLexemes.Peek();

                            if (tmp.name.Equals("VAR"))
                            {
                                state = States.VAR;
                                break;
                            }

                            if (tmp.lexemeType.Equals(Variable.VariableType.IDENT))
                            {
                                state = States.EXP;
                                break;
                            }

                            if (tmp.name.Equals("IF"))
                            {
                                state = States.IF;
                                break;
                            }

                            if (tmp.name.Equals("ELSE"))
                            {
                                sourceCSCode.Append("}else{");
                            }

                            if (tmp.name.Equals("END_IF"))
                            {
                                sourceCSCode.Append('}');
                            }

                            if (tmp.name.Equals("READ"))
                            {
                                state = States.READ;
                                break;
                            }

                            if (tmp.name.Equals("WRITE"))
                            {
                                state = States.WRITE;
                                break;
                            }

                            stackOfLexemes.Pop();

                            if (!stackOfLexemes.Any())
                            {
                                state = States.FIN;
                            }

                            break;
                        }

                    case States.VAR:
                        {
                            var lexeme = stackOfLexemes.Pop();

                            if (lexeme.lexemeType.Equals(Variable.VariableType.IDENT))
                            {
                                sourceCSCode.Append($"int {lexeme.name};");
                                break;
                            }

                            if (lexeme.name.Equals(":"))
                            {
                                state = States.S;
                            }

                            break;
                        }

                    case States.EXP:
                        {
                            var lexeme = stackOfLexemes.Pop();
                            Zamena(sourceCSCode, lexeme);
                            if (lexeme.name.Equals(";"))
                            {
                                state = States.S;
                            }

                            break;
                        }

                    case States.IF:
                        {
                            stackOfLexemes.Pop();
                            var it = stackOfLexemes.Pop();
                            sourceCSCode.Append($"if({it.name} ");

                            it = stackOfLexemes.Pop();
                            Zamena(sourceCSCode, it);

                            it = stackOfLexemes.Pop();
                            sourceCSCode.Append($"{it.name}){{");


                            stackOfLexemes.Pop();

                            state = States.S;
                            break;
                        }

                    case States.READ:
                        {
                            var lexeme = stackOfLexemes.Pop();

                            if (lexeme.lexemeType.Equals(Variable.VariableType.IDENT))
                            {
                                sourceCSCode.Append($"{lexeme.name}=int.Parse(Console.ReadLine());");
                                break;
                            }

                            if (lexeme.name.Equals(";"))
                            {
                                state = States.S;
                            }

                            break;
                        }

                    case States.WRITE:
                        {
                            var lexeme = stackOfLexemes.Pop();

                            if (lexeme.lexemeType.Equals(Variable.VariableType.IDENT))
                            {
                                sourceCSCode.Append($"Console.WriteLine({lexeme.name});");
                                break;
                            }

                            if (lexeme.name.Equals(";"))
                            {
                                state = States.S;
                            }

                            break;
                        }
                }
            }

            return sourceCSCode.ToString();
        }

        static public Task<string> AnalyzeAsync(List<Variable> listOfLexemes) => Task.Run(() => Analyze(listOfLexemes));

        static private void Zamena(StringBuilder source, Variable lexeme)
        {
            switch (lexeme.name)
            {
                case ".OR.":
                    {
                        source.Append(" | ");
                        break;
                    }
                case ".AND.":
                    {
                        source.Append(" & ");
                        break;
                    }
                case ".EQU.":
                    {
                        source.Append(" == ");
                        break;
                    }
                default:
                    source.Append(lexeme.name);
                    break;
            }
        }

        static public List<Variable> TranslateToVar(List<string> lexemes)
        {
            List<Variable> variables = new List<Variable>();

            List<string> keyword = new List<string>() { "VAR", "LOGICAL", "BEGIN", "END", "WRITE", "READ", "IF", "ELSE", "THEN", "END_IF" };
            List<string> operators = new List<string>() { ".AND.", ".OR.", ".EQU.", ":", "=", ",", ";", "(", ")", "." };
            List<string> numbers = new List<string>() { "0", "1" };

            foreach (var i in lexemes)
            {
                if (keyword.FindIndex(x => x == i) != -1)
                {
                    Variable variable = new Variable(i, Variable.VariableType.KEYWORD);
                    variables.Add(variable);
                    continue;
                }
                if (operators.FindIndex(x => x == i) != -1)
                {
                    Variable variable = new Variable(i, Variable.VariableType.OPERATOR);
                    variables.Add(variable);
                    continue;
                }
                if (numbers.FindIndex(x => x == i) != -1)
                {
                    Variable variable = new Variable(i, Variable.VariableType.NUMBER);
                    variables.Add(variable);
                    continue;
                }
                Variable per = new Variable(i, Variable.VariableType.IDENT);
                variables.Add(per);
            }

            return variables;
        }
    }
}

