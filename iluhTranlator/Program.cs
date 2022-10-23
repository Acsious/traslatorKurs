using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace translatorKurs
{
    class LeksicheskiyAnalizator
    {
        private static readonly int maxDlinaIdenta = 9;
        private static int colichestvoDvoitochiy = 0, colichestvoTochekSZapyatoy = 0;
        private static string lastOperator = "";
        private static bool varExist = false, beginExist = false, endExist = false,
            readExist = false, ifExist = false, thenExist = false, logicalExist = false,
            elseExist = false, endIfExist = false, writeExist = false;

        public static void Main()
        {
            try
            {
                var choto = Analiz(new StreamReader("TestFile.txt"));
                if (choto.Item1 == null)
                {
                    Console.WriteLine(choto.Item2);
                }
                else
                {
                    Console.Write("Найденные лексемы: ");
                    foreach (var it in choto.Item1)
                    {
                        Console.Write(it + " ");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Не удается открыть файл:");
                Console.WriteLine(e.Message);
            }
        }

        public static (List<string>, string) Analiz(StreamReader potokChteniya)
        {
            var lex = new List<string>();
            var lexBuffer = string.Empty;

            while (!potokChteniya.EndOfStream)
            {
                var symbol = (char)potokChteniya.Read();
                if (char.IsLetter(symbol))
                {
                    lexBuffer += symbol;

                    if (lexBuffer.Length > maxDlinaIdenta)
                    {
                        return (null, $"Длина идентефикатора '{lexBuffer}'  превышает максимально разрешенную: {maxDlinaIdenta}.");
                    }
                    if (lexBuffer.Equals(lexBuffer.ToUpper()))
                    {
                        if (Proverka(lexBuffer))
                        {
                            return (null, "Синтаксическая ошибка");
                        }
                    }
                }
                else
                {
                    if (lexBuffer != string.Empty)
                    {
                        lex.Add(lexBuffer);
                        lexBuffer = string.Empty;
                    }

                    switch (lastOperator)
                    {
                        case "VAR":
                            if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t' && symbol != ',' && symbol != ':')
                            {
                                return (null, $"'{symbol}' - неверный символ.");
                            }
                            if (symbol == ',')
                            {
                                if (lex.Last().Equals(",") || lex.Last().Equals("VAR"))
                                {
                                    return (null, "Слишком много символа - ','");
                                }
                            }
                            if (symbol == ':')
                            {
                                colichestvoDvoitochiy++;
                                if (colichestvoDvoitochiy > 1)
                                {
                                    return (null, "Слишком много символа - ':'");
                                }
                            }
                            break;
                        case "LOGICAL":
                            if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t' && symbol != ';')
                            {
                                return (null, $"'{symbol}' - неверный символ.");
                            }
                            if (symbol == ';')
                            {
                                colichestvoTochekSZapyatoy++;
                                if (colichestvoTochekSZapyatoy > 1)
                                {
                                    return (null, "Слишком много символа - ';'");
                                }
                            }
                            break;
                        case "BEGIN":

                            break;
                    }
                    if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t')
                    {
                        lex.Add(symbol.ToString());
                    }
                    #region
                    //var operators = new List<string> { ":", ";", ".", ",", "(", ")", "=", ".NOT.", ".AND.", ".OR.", ".EQU." };
                    //if (operators.Exists(op => op.Equals(symbol.ToString())))
                    //{
                    //    lex.Add(symbol.ToString());
                    //}
                    //else if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t')
                    //{
                    //    return (null, $"'{symbol}' - неверный символ.");
                    //}
                    #endregion
                }
            }
            return (lex, string.Empty);
        }

        /// <summary>
        /// Метод проверки лексемы на ключевые слова.
        /// </summary>
        /// <param name="stroka">Анализируемая лексима.</param>
        /// <returns>1 - ошибка, 0 - ошибок нет.</returns>
        public static bool Proverka(string stroka)
        {
            if (stroka.Equals("VAR"))
            {
                if (!varExist)
                {
                    varExist = true;
                }
                else
                {
                    return true;
                }

                if (!lastOperator.Equals(""))
                {
                    return true;
                }
                lastOperator = "VAR";
            }

            if (stroka.Equals("LOGICAL"))
            {
                if (!logicalExist)
                {
                    logicalExist = true;
                }
                else
                {
                    return true;
                }
                if (!lastOperator.Equals("VAR"))
                {
                    return true;
                }
                lastOperator = "LOGICAL";
            }

            if (stroka.Equals("BEGIN"))
            {
                if (!beginExist)
                {
                    beginExist = true;
                }
                else
                {
                    return true;
                }

                if (!lastOperator.Equals("LOGICAL"))
                {
                    return true;
                }
                lastOperator = "BEGIN";
            }

            if (stroka.Equals("END"))
            {
                if (!endExist)
                {
                    endExist = true;
                }
                else
                {
                    return true;
                }

                if (!lastOperator.Equals("BEGIN"))
                {
                    return true;
                }
                lastOperator = "END";
            }

            if (stroka.Equals("READ"))
            {
                if (!readExist)
                {
                    readExist = true;
                }
                else
                {
                    return true;
                }
            }

            if (stroka.Equals("IF"))
            {
                if (!ifExist)
                {
                    ifExist = true;
                }
                else
                {
                    return true;
                }
            }

            if (stroka.Equals("THEN"))
            {
                if (!thenExist)
                {
                    thenExist = true;
                }
                else
                {
                    return true;
                }
            }

            if (stroka.Equals("ELSE"))
            {
                if (!elseExist)
                {
                    elseExist = true;
                }
                else
                {
                    return true;
                }
            }

            if (stroka.Equals("END_IF"))
            {
                if (!endIfExist)
                {
                    endIfExist = true;
                }
                else
                {
                    return true;
                }
            }

            if (stroka.Equals("WRITE"))
            {
                if (!writeExist)
                {
                    writeExist = true;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}