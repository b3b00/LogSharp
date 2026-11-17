namespace logSharp.model
{
    public readonly record struct FunctorArity(string Functor, int Arity)
    {
        public override string ToString() => $"{Functor}/{Arity}";
    }
}
