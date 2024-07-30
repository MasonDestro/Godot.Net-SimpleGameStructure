using Godot;
using System;

public partial class EscPanel : Panel
{
	[Export]
	Button ResumeBtn;
	[Export]
	Button QuitBtn;

    public Action OnResume { get; set; }

    public override void _Ready()
    {
        ResumeBtn.Pressed += Resume;
        QuitBtn.Pressed += Quit;
    }

    private void Resume()
    {
        OnResume?.Invoke();
    }

    private void Quit()
    {
        GetTree().Quit();
    }
}
