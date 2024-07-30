using Godot;
using System;
using System.Diagnostics;
using static GlobalTypes;

public partial class UIManager : CanvasLayer, IResponseToInputAction
{
	[Export]
	Control Crosshairs;
	[Export]
	EscPanel EscPanel;


	public static UIManager Instance { get; private set; }

    InputActionMessage _iaMsg;
    public InputActionMessage InputActionMessage { get => _iaMsg; set => _iaMsg = value; }

    public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
            InitInputActionMessage();

			Crosshairs.Visible = true;
			EscPanel.Visible = false;
        }
        else
		{
			QueueFree();
		}
	}

	public void OpenEscPanel(IResponseToInputAction user)
	{
		RegisterInputActions();
		EscPanel.Visible = true;
		Crosshairs.Visible = false;

		EscPanel.OnResume = () =>
		{
			EscPanel.Visible = false;
			Crosshairs.Visible = true;
			user.RegisterInputActions();
		};
	}

    public void RegisterInputActions()
    {
        //InputManager.Instance.RegisterInputMethods(IRMsg);
        InputManager.Instance.RegisterInputMethods(InputActionMessage);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public void InitInputActionMessage()
    {
		DefaultMessage();

    }

	public void DefaultMessage()
	{
        _iaMsg = new()
        {
            InstanceID = GetInstanceId(),
            Actions = new()
        };

        _iaMsg.Actions.Add(new InputActionParams
        {
            ActionName_NegativeAction_NegativeX = "Esc"
        }, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustPressed,
            KeyButtonMethod = (double delta, InputActionType iaType) =>
            {
                EscPanel.OnResume?.Invoke();
            }
        });
    }
}
