using System;
using EFT.InputSystem;

public class Class1716 : GInterface232
{
	[NonSerialized]
	public IHandsThrowController IHandsThrowController;

	public Class1716(IHandsThrowController controller)
	{
		IHandsThrowController = controller;
	}

	public InputNode.ETranslateResult TranslateCommand(ECommand command)
	{
		if (command == ECommand.ExamineWeapon)
		{
			IHandsThrowController.ExamineWeapon();
		}
		return InputNode.ETranslateResult.Ignore;
	}

	public void TranslateAxes(ref float[] axes)
	{
	}
}
