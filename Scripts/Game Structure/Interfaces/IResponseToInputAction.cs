using System;

public interface IResponseToInputAction
{
    public GlobalTypes.InputActionMessage InputActionMessage { get; set; }

    public void RegisterInputActions();

    public void InitInputActionMessage();

    public void DefaultMessage();
}
