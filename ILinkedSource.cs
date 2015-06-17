namespace ShowMeAnExampleOfAutomapperFromLinkedSource
{
    public interface ILinkedSource<T> {
        T Model { get; }
    }
}