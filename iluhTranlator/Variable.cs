namespace translatorKurs
{
    class Variable
    {
        public enum VariableType : byte { KEYWORD, OPERATOR, IDENT, NUMBER }

        public readonly VariableType lexemeType;
        public readonly string name;

        public Variable(string stroka, VariableType lexemeType)
        {
            name = stroka;
            this.lexemeType = lexemeType;
        }
    }
}
