using MaxImDL;

namespace Playground
{
    internal struct ResultItem
    {
        public ObjectInfo FirstResult { get; }
        public ObjectInfo SecondResult { get; }

        public ResultItem(in ObjectInfo firstResult, in ObjectInfo secondResult)
        {
            FirstResult = firstResult;
            SecondResult = secondResult;
        }
    }
}
