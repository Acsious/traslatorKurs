using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace translatorKurs
{
    internal class LeksicheskiyAnalizator
    {
        private static readonly int maxDlinaIdenta = 9;
        private static int colDvoit = 0, colTochSZap = 0, colTochBegin = 0,
            colOtSkob = 0, colZakSkob = 0, ifExist = 0, elseExist = 0, endIfExist = 0, thenExist = 0;
        private static string lastOperator = "";
        private static bool varExist = false, beginExist = false, endExist = false, logicalExist = false, tochPosleEnd = false;

        public static async Task Main()
        {
            try
            {
                (List<string>, string) choto = Analiz(new StreamReader("TestFile.txt"));
                if (choto.Item1 != null)
                {
                    choto = Proverka_Per(choto.Item1);
                }

                if (choto.Item1 == null)
                {
                    Console.WriteLine(choto.Item2);
                }
                else
                {
                    Magazine(choto.Item1);
                    List<Variable> variables = Translator.TranslateToVar(choto.Item1);
                    string finCode = Translator.Analyze(variables);
                    (bool compilerResult, string msg) = await Compiler.CompileAsync(finCode);
                    if (compilerResult)
                    {
                        Console.WriteLine(msg);
                        return;
                    }

                    int exitCode = await Compiler.RunAsync();
                    Console.WriteLine($"Программа закончилась с кодом - {exitCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Не удается открыть файл:");
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// Разбивает поток чтения на лексемы
        /// </summary>
        /// <param name="potokChteniya">поток чтения файла</param>
        /// <returns>список найденных лексем</returns>
        private static (List<string>, string) Analiz(StreamReader potokChteniya)
        {
            var lex = new List<string>();
            string lexBuffer = string.Empty;

            while (!potokChteniya.EndOfStream)
            {
                char symbol = (char)potokChteniya.Read();
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
                                if (lex.Last().Equals("END_IF"))
                                {
                                    lex[^3] = null;
                                    lex[^2] = null;
                                    _ = lex.RemoveAll(x => x == null);
                                    break;
                                }
                                if (symbol == '.')
                                {
                                    colTochBegin++;
                                    if (lex.Last().Equals("."))
                                    {
                                        return (null, "Слишком много символа - '.'");
                                    }
                                    if ((colTochBegin % 2 == 0) && !(lex[^2].Equals(".") && (lex.Last().Equals("NOT") || lex.Last().Equals("AND") || lex.Last().Equals("OR") || lex.Last().Equals("EQU"))))
                                    {
                                        return (null, "Ошибка синтаксиса");
                                    }
                                }
                            }
                            else if (symbol != ' ' && symbol != '\n' && symbol != '\r' && symbol != '\t' && symbol != '1' && symbol != '0')
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
                                    lastOperator = "BEGIN";
                                    break;
                                }
                                else
                                {
                                    return (null, "Ошибка синтаксиса.");
                                }
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
            if (ifExist != thenExist)
            {
                return (null, "Для IF не хватает THEN");
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
            for (int i = spis.IndexOf("BEGIN"); i < spis.IndexOf("END"); i++)
            {
                if (spis[i].Equals(".") && spis[i + 2].Equals(".") && (spis[i + 1].Equals("NOT") || spis[i + 1].Equals("AND") || spis[i + 1].Equals("EQU") || spis[i + 1].Equals("OR")))
                {
                    spis[i] = spis[i] + spis[i + 1] + spis[i + 2];
                    spis[i + 1] = null;
                    spis[i + 2] = null;
                    i += 2;
                }
            }
            _ = spis.RemoveAll(x => x is null);
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
            var spPer = new List<string>();
            var allPer = new List<string>();
            int index = 0;

            for (int i = 0; i < spisok.Count; i++)
            {
                string iter = spisok[i];

                if (iter.Equals(":"))
                {
                    index = i + 2;
                    break;
                }
                if (!iter.Equals(iter.ToUpper()))
                {
                    spPer.Add(iter);
                }
            }

            for (int i = index; i < spisok.Count; i++)
            {
                string iter = spisok[i];

                if (iter.Equals("END"))
                {
                    break;
                }

                if (!iter.Equals(iter.ToUpper()))
                {
                    allPer.Add(iter);
                }
            }
            string mess = "Следующие переменные не определены: ";
            bool val = true;
            foreach (string i in allPer)
            {
                if (spPer.FindIndex(x => x == i) == -1)
                {
                    mess += i + ",";
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
        /// <summary>
        /// Проверяет инициализированы переменные прежде чем их использовать в операциях
        /// </summary>
        /// <param name="idents">идентификаторы</param>
        /// <param name="text">исходный текст</param>
        /// <returns>1-да; 0-нет</returns>
        private static bool Proverka_Inic(List<string> idents, List<string> text)
        {
            var variable = new List<string>();
            var other = new List<string>() { ".AND.", ".OR.", ".EQU.", "THEN", "IF", "ELSE", "END_IF", ")", ";" };
            foreach (string i in idents)
            {
                for (int j = 0; j < text.Count; j++)
                {
                    if (text[j].Equals("READ") && text[j + 1].Equals("(") && text[j + 2].Equals(i) && text[j + 3].Equals(")"))
                    {
                        variable.Add(text[j] + text[j + 1] + text[j + 2] + text[j + 3]);
                        j += 3;
                        continue;
                    }
                    if (i.Equals(text[j]))
                    {
                        variable.Add(text[j] + text[j + 1]);
                    }
                }
            }
            foreach (string i in idents)
            {
                _ = variable.RemoveAll(x => x == i + ",");
            }
            foreach (string i in idents)
            {
                int index_f = variable.FindIndex(x => x == i + "=");
                int index_s = 0;
                int index_R = variable.FindIndex(x => x == $"READ({i})");
                foreach (string j in other)
                {
                    index_s = variable.FindIndex(x => x == i + j);
                    if (index_s != -1)
                    {
                        break;
                    }
                }
                if (index_s != -1)
                {
                    if (index_f > index_s || (index_f == -1 && index_R == -1) || index_R > index_s)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Определяет является ли слово идентификатором
        /// </summary>
        /// <param name="value">Слово</param>
        /// <returns>1 - да; 0 - нет</returns>
        private static bool IsIdent(string value)
        {
            if (!value.Equals(value.ToUpper()))
            {
                return true;
            }
            else
            {
                if (Proverka(value))
                {
                    return false;
                }
                else
                {
                    char[] vs = value.ToCharArray();
                    return vs.Length != 1;
                }
            }
        }
        /// <summary>
        /// Определяет является ли слово идентификатором
        /// </summary>
        /// <param name="val">Слово</param>
        /// <param name="spisok_identov">Список идентификаторов</param>
        /// <returns>1 - да; 0 - нет</returns>
        private static bool IsIdent(string val, List<string> spisok_identov)
        {
            if (spisok_identov.FindIndex(x => x == val) != -1)
            {
                return true;
            }
            else
            {
                return IsConstant(val);
            }
        }
        /// <summary>
        /// Определяет является ли слово константой
        /// </summary>
        /// <param name="value">слово</param>
        /// <returns>1 - да; 0 - нет</returns>
        private static bool IsConstant(string value)
        {
            if (value.Equals("0"))
            {
                return true;
            }
            else
            {
                return value.Equals("1");
            }
        }
        /// <summary>
        /// Определяет является ли строка выражением
        /// </summary>
        /// <param name="idents">идентификаторы</param>
        /// <param name="text">исходный текст</param>
        /// <param name="value">место, где начинается проверка</param>
        /// <returns></returns>
        private static bool VJ(List<string> idents, List<string> text, ref int value)
        {
            var index = value;
            var binOpetors = new List<string>() { ".AND.", ".OR.", ".EQU." };

            if (IsIdent(text[index], idents) && text[index + 1].Equals(";"))
            {
                index++;
                value = index;
                return true;
            }
            if (text[index].Equals("NOT") && text[index + 1].Equals("("))
            {
                index += 2;
                value = index;
                return VJ(idents, text, ref value);
            }
            if (IsIdent(text[index], idents) && binOpetors.Find(it => it == text[index + 1]) != null && IsIdent(text[index + 2], idents))
            {
                value = index + 3;
                return true;
            }

            if (text[index].Equals("("))
            {
                index++;
                if (VJ(idents, text, ref index))
                {
                    if (binOpetors.Find(x => x == text[index + 1]) != null && IsIdent(text[index + 2], idents))
                    {
                        index += 2;
                        value = index;
                        return true;
                    }
                    else if (binOpetors.Find(x => x == text[index + 1]) != null && text[index + 2].Equals("("))
                    {
                        index += 3;
                        value = index;
                        return VJ(idents, text, ref value);
                    }
                    else if (text[index].Equals(")") && text[index + 1].Equals(";"))
                    {
                        value = index;
                        return true;
                    }
                    else if (text[index].Equals(")") && text[index + 1].Equals(")"))
                    {
                        value = index + 2;
                        if (binOpetors.Find(x => x == text[index + 2]) != null)
                        {
                            value++;
                            return VJ(idents, text, ref value);
                        }
                        if (text[index + 2].Equals(";"))
                        {
                            return true;
                        }
                    }
                }
            }
            if (IsIdent(text[index], idents) && binOpetors.Find(it => it == text[index + 1]) != null && text[index + 2].Equals("("))
            {
                value = index + 3;
                return VJ(idents, text, ref value);
            }
            value = index;
            return false;
        }
        /// <summary>
        /// Определяет является ли строка присвоением
        /// </summary>
        /// <param name="idents">идентификаторы</param>
        /// <param name="text">исходный текст</param>
        /// <param name="step">место, где идет проверка</param>
        /// <returns></returns>
        private static bool Assignment(List<string> idents, List<string> text, ref int step)
        {
            var IOoperators = new List<string>() { "READ", "WRITE" };
            int index = step;

            if (IOoperators.FindIndex(item => item == text[index]) != -1)
            {
                if (text[index + 1].Equals("(") && IsIdent(text[index + 2], idents) && text[index + 3].Equals(")") && text[index + 4].Equals(";"))
                {
                    step = index + 4;
                    return true;
                }
            }

            if (idents.FindIndex(it => it == text[index]) != -1 && text[index + 1].Equals("="))
            {
                index += 2;
                if (VJ(idents, text, ref index))
                {
                    if (text[index + 1].Equals(";"))
                    {
                        step = index + 1;
                        return true;
                    }
                    else if (text[index].Equals(";"))
                    {
                        step = index;
                        return true;
                    }
                    else if (text[index + 1].Equals(")") && text[index + 2].Equals(";"))
                    {
                        step = index + 2;
                        return true;
                    }
                    else if (text[index + 1].Equals(")") && text[index + 2].Equals(")") && text[index + 3].Equals(";"))
                    {
                        step = index + 3;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (text[index].Equals("IF"))
            {
                step = index + 1;
                if (VJ(idents, text, ref step))
                {
                    if (text[step].Equals("THEN"))
                    {
                        while (!text[step + 1].Equals("ELSE") && !text[step + 1].Equals("END_IF"))
                        {
                            step++;
                            if (!Assignment(idents, text, ref step))
                            {
                                return false;
                            }
                        }
                        if (text[step + 1].Equals("ELSE"))
                        {
                            step++;
                            while (!text[step + 1].Equals("END_IF"))
                            {
                                step++;
                                if (!Assignment(idents, text, ref step))
                                {
                                    return false;
                                }
                            }
                        }
                        if (text[step + 1].Equals("END_IF"))
                        {
                            step += 2;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Выполняет работа магазинного автомата
        /// </summary>
        /// <param name="text">строка</param>
        private static void Magazine(List<string> text)
        {
            var magazine = new List<string>();
            var idents = new List<string>();
            magazine.Add("h0");
            int step = 0;
            magazine.Add(text[step]);
            if (!magazine[1].Equals("VAR"))
            {
                Console.Write("Hет объявления переменных!\n");
                return;
            }

            if (text.FindIndex(x => x == ":") == -1)
            {
                Console.WriteLine("Синтаксичекая ошибка! Нет ':'");
                return;
            }
            step += 1;
            while (text[step] != ":")
            {
                magazine.Add(text[step]);
                idents.Add(text[step]);
                step++;
            }
            _ = idents.RemoveAll(item => item == ",");
            if (!Proverka_Inic(idents, text))
            {
                Console.WriteLine("Не все переменные инициализированы!");
                return;
            }

            if (text[step + 1].Equals("LOGICAL") && text[step + 2].Equals(";") && IsIdent(text[step - 1]))
            {
                magazine.Clear();
                magazine.Add("h0");
                magazine.Add("Per");
            }
            else
            {
                Console.WriteLine("Синтаксическая ошибка в присвоении типа переменных!");
            }
            step += 3;

            if (text[step].Equals("BEGIN"))
            {
                magazine.Add(text[step]);
            }
            else
            {
                Console.WriteLine("Синтаксическая ошибка! Отсутсвует ключевое слово BEGIN!");
            }
            step++;
            while (!text[step].Equals("END"))
            {
                if (Assignment(idents, text, ref step))
                {
                    magazine.Add("A");
                }
                else
                {
                    Console.WriteLine("Ошибка в присвоении!");
                    break;
                }
                if (text[step].Equals(";"))
                {
                    step++;
                }
            }
            magazine.Add("END");
            magazine[3] = "Sa";
            _ = magazine.RemoveAll(x => x == "A");
            if (magazine[1].Equals("Per") && magazine[2].Equals("BEGIN") && magazine[3].Equals("Sa") && magazine[4].Equals("END"))
            {
                magazine.Clear();
                magazine.Add("Programm");
            }
            if (magazine[0].Equals("Programm"))
            {
                Console.WriteLine("Это программа");
            }
        }
    }
}