using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace translatorKurs
{
    class LeksicheskiyAnalizator
    {
        private static readonly int maxDlinaIdenta = 9;
        private static int colDvoit = 0, colTochSZap = 0, colTochBegin = 0,
            colOtSkob = 0, colZakSkob = 0, ifExist = 0, elseExist = 0, endIfExist = 0, thenExist = 0;
        private static string lastOperator = "";
        private static bool varExist = false, beginExist = false, endExist = false, logicalExist = false, tochPosleEnd = false;

        public static void Main()
        {
            try
            {
                var choto = Analiz(new StreamReader("TestFile.txt"));
                if (choto.Item1 != null)
                    choto = Proverka_Per(choto.Item1);

                if (choto.Item1 == null)
                {
                    Console.WriteLine(choto.Item2);
                }
                else
                {
                    Magazine(choto.Item1);
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

        private static (List<string>, string) Analiz(StreamReader potokChteniya)
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
                                colDvoit++;
                                if (colDvoit > 1)
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
                                colTochSZap++;
                                if (colTochSZap > 1)
                                {
                                    return (null, "Слишком много символа - ';'");
                                }
                            }
                            break;
                        case "BEGIN":
                            var operators = new List<string> { ";", "=", "(", ")", "." };
                            if (operators.Exists(op => op.Equals(symbol.ToString())))
                            {
                                if (symbol == '(')
                                {
                                    colOtSkob++;
                                }
                                if (symbol == ')')
                                {
                                    colZakSkob++;
                                    #region
                                    //Вроде не надо, но оставлю на всякий
                                    //if (lex[lex.Count - 2].Equals("("))
                                    //{
                                    //    if (lex[lex.Count - 3].Equals("READ") || lex[lex.Count - 3].Equals("WRITE"))
                                    //    {
                                    //        lex[lex.Count - 3] += lex[lex.Count - 2] + lex[lex.Count - 1] + ")";
                                    //        lex[lex.Count - 2] = null;
                                    //        lex[lex.Count - 1] = null;
                                    //        lex.RemoveAll(x => x == null);
                                    //        symbol = ' ';
                                    //    }
                                    //}
                                    #endregion
                                }
                                if (symbol == ';')
                                {
                                    if (lex.Last().Equals(";"))
                                    {
                                        return (null, "Слишком много символа - ';'");
                                    }
                                }
                                if (symbol == '.')
                                {
                                    colTochBegin++;
                                    if (lex.Last().Equals("."))
                                    {
                                        return (null, "Слишком много символа - '.'");
                                    }
                                    if ((colTochBegin % 2 == 0) && !(lex[lex.Count - 2].Equals(".") && (lex.Last().Equals("NOT") || lex.Last().Equals("AND") || lex.Last().Equals("OR") || lex.Last().Equals("EQU"))))
                                    {
                                        return (null, "Ошибка синтаксиса");
                                    }
                                }
                            }
                            else if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t')
                            {
                                return (null, $"'{symbol}' - неверный символ.");
                            }

                            break;
                        case "END":
                            if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t' && symbol != '.' && symbol != '_')
                            {
                                return (null, $"'{symbol}' - неверный символ.");
                            }
                            if (symbol == '.')
                            {
                                tochPosleEnd = true;
                                if (lex.Last().Equals("."))
                                {
                                    return (null, "Слишком много символа - '.'");
                                }
                            }
                            if (symbol == '_')
                            {
                                if (lex.Last().Equals("END"))
                                {
                                    lexBuffer = lex.Last() + "_";
                                    break;
                                }
                                else
                                {
                                    return (null, "Ошибка синтаксиса.");
                                }
                            }
                            if (lex.Last().Equals("END_IF"))
                            {
                                lastOperator = "BEGIN";
                                lex[lex.Count - 3] = null;
                                lex[lex.Count - 2] = null;
                                lex.RemoveAll(x => x == null);
                                break;
                            }
                            if (!lex.Last().Equals("END"))
                            {
                                return (null, $"Ошибка синтаксиса.");
                            }
                            break;
                    }
                    if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t')
                    {
                        lex.Add(symbol.ToString());
                    }
                    #region
                    //временный кусок для копипасты
                    //
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
            if (colOtSkob != colZakSkob)
            {
                return (null, "Количество открывающих и закрыввающих скобок не равно.");
            }
            if (!tochPosleEnd)
            {
                return (null, "Нету точки после END");
            }
            if (ifExist != endIfExist)
            {
                return (null, "Один из блоков IF не закрыт.");
            }
            Obiedinenie(lex);
            return (lex, string.Empty);
        }

        /// <summary>
        /// Объединяет три найденных лексемы в одну по принципу: . EQU . => .EQU.
        /// (Работает с EQU, AND, NOT, OR)
        /// </summary>
        /// <param name="spis">Список лексем</param>
        private static void Obiedinenie(List<string> spis)
        {
            for (var i = spis.IndexOf("BEGIN"); i < spis.IndexOf("END"); i++)
            {
                if (spis[i].Equals(".") && spis[i + 2].Equals(".") && (spis[i + 1].Equals("NOT") || spis[i + 1].Equals("AND") || spis[i + 1].Equals("EQU") || spis[i + 1].Equals("OR")))
                {
                    spis[i] = spis[i] + spis[i + 1] + spis[i + 2];
                    spis[i + 1] = null;
                    spis[i + 2] = null;
                    i += 2;
                }
            }
            spis.RemoveAll(x => x is null);
        }

        /// <summary>
        /// Метод проверки лексемы на ключевые слова.
        /// </summary>
        /// <param name="stroka">Анализируемая лексима.</param>
        /// <returns>1 - ошибка, 0 - ошибок нет.</returns>
        private static bool Proverka(string stroka)
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

            if (stroka.Equals("IF"))
            {
                ifExist++;
            }

            if (stroka.Equals("THEN"))
            {
                thenExist++;
                if (thenExist != ifExist)
                {
                    return true;
                }
            }

            if (stroka.Equals("ELSE"))
            {
                elseExist++;
                if (elseExist > ifExist)
                {
                    return true;
                }
            }

            if (stroka.Equals("END_IF"))
            {
                endIfExist++;
                endExist = false;
                if (endIfExist > ifExist)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Проверка на неинициализированные переменные
        /// </summary>
        /// <param name="spisok">Списко лексем</param>
        /// <returns></returns>
        private static (List<string>, string) Proverka_Per(List<string> spisok)
        {
            List<string> spPer = new List<string>();
            List<string> allPer = new List<string>();
            int index = 0;

            for (int i = 0; i < spisok.Count; i++)
            {
                var iter = spisok[i];

                if (iter.Equals(":"))
                {
                    index = i + 2;
                    break;
                }
                if (!iter.Equals(iter.ToUpper()))
                    spPer.Add(iter);
            }

            for (int i = index; i < spisok.Count; i++)
            {
                var iter = spisok[i];

                if (iter.Equals("END"))
                    break;
                if (!iter.Equals(iter.ToUpper()))
                    allPer.Add(iter);
            }
            string mess = "Следующие переменные не определены: ";
            bool val = true;
            foreach (var i in allPer)
            {
                if (spPer.FindIndex(x => x == i) == -1)
                {
                    mess += (i + ",");
                    val = false;
                }
            }

            if (!val)
            {
                mess = mess.Remove(mess.Length - 1);
                return (null, mess);
            }

            return (spisok, string.Empty);
        }

        private static void Magazine(List<string> text)
        {
            List<string> magazine = new List<string>();
            magazine.Add("h0");
            magazine.Add(text[0]);
            if (!magazine[1].Equals("VAR"))
            {
                Console.Write("Hет объявления переменных!\n");
                return;
            }
            


        }
    }
}