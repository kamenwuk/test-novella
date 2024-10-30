namespace Assembly.FindPair
{
    public readonly struct DataByCell
    {
        public readonly ViewToCell View;
        public readonly int Identifier;
        
        public DataByCell(ViewToCell view, int identifier)
        {
            View = view;
            Identifier = identifier;
        }
    }
}
