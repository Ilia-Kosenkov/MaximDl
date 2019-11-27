namespace Playground
{
    internal struct ResultItem
    {
        public ObjectInfo ResultFirst { get; }
        public ObjectInfo ResultSecond { get; }

        public ResultItem(in ObjectInfo resultFirst, in ObjectInfo resultSecond)
        {
            ResultFirst = resultFirst;
            ResultSecond = resultSecond;
        }
    }
}
