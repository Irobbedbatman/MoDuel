namespace MoDuelBase;

public interface IActionFunction {


    public bool IsAssigned { get; }

    public object Call(params object[] args);

}
