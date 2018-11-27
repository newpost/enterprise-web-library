using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.WebSessionState;
using Humanizer;

namespace EnterpriseWebLibrary.WebSite.TestPages {
	partial class Checkboxes: EwfPage {
		protected override void loadData() {
			ph.AddControlsReturnThis(
				FormState.ExecuteWithDataModificationsAndDefaultAction(
					PostBack.CreateFull().ToCollection(),
					() => {
						var table = FormItemBlock.CreateFormItemTable( formItems: getControls().Select( ( getter, i ) => getter( ( i + 1 ).ToString() ) ) );
						table.IncludeButtonWithThisText = "Submit";
						return table;
					} ) );
		}

		private IReadOnlyCollection<Func<string, FormItem>> getControls() =>
			new[]
				{
					getCheckbox( "Standard", null ), id => {
						var pb = PostBack.CreateIntermediate( null, id: id );
						return FormState.ExecuteWithDataModificationsAndDefaultAction(
							FormState.Current.DataModifications.Append( pb ),
							() => getCheckbox( "Separate value-changed action", CheckboxSetup.Create( valueChangedAction: new PostBackFormAction( pb ) ) )( id ) );
					},
					new Func<Func<string, FormItem>>(
						() => {
							var pmv = new PageModificationValue<bool>();
							return getCheckbox( "Page modification", CheckboxSetup.Create( pageModificationValue: pmv ), pageModificationValue: pmv );
						} )(),
					getCheckbox( "Read-only", CheckboxSetup.CreateReadOnly() ), getFlowCheckbox( "Flow", null ),
					getFlowCheckbox( "Flow with highlighting", FlowCheckboxSetup.Create( highlightedWhenChecked: true ) ),
					getFlowCheckbox(
						"Flow with nested content",
						FlowCheckboxSetup.Create(
							nestedContentGetter: () => new Paragraph(
								"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin id vestibulum neque. Suspendisse vel sem ac nunc condimentum tempus eget quis nunc. Morbi mattis elementum cursus. Integer eros mi, porttitor vitae orci eget, facilisis pretium diam. Aenean et nisi leo. Aenean nibh ligula, suscipit sit amet nulla ac, faucibus suscipit ipsum. Nunc quis faucibus ex."
									.ToComponents() ).ToCollection() ) ),
					getFlowCheckbox(
						"Flow with nested content always displayed",
						FlowCheckboxSetup.Create(
							nestedContentGetter: () => new Paragraph(
								"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vel diam quis felis accumsan tempus. Nunc hendrerit mi in hendrerit finibus. Nullam aliquet pharetra mauris ac vehicula. Quisque vehicula imperdiet pulvinar. Morbi ullamcorper est non arcu suscipit, quis interdum ex egestas. Sed vel risus vitae nisl scelerisque pretium. Aliquam vel pretium orci, eu scelerisque felis. Morbi ac auctor lacus, sit amet congue nunc."
									.ToComponents() ).ToCollection(),
							nestedContentAlwaysDisplayed: true ) ),
					getRadioGroup( "Radio group", null ),
					getRadioGroup( "Radio group with no selection", null, noSelection: true ),
					getRadioGroup( "Radio group with single button", null, singleButton: true ), id => {
						var pb = PostBack.CreateIntermediate( null, id: id );
						return FormState.ExecuteWithDataModificationsAndDefaultAction(
							FormState.Current.DataModifications.Append( pb ),
							() => getRadioGroup( "Radio group with separate selection-changed action", null, selectionChangedAction: new PostBackFormAction( pb ) )( id ) );
					},
					new Func<Func<string, FormItem>>(
						() => {
							var pmv = new PageModificationValue<bool>();
							return getRadioGroup( "Radio group with page modification", RadioButtonSetup.Create( pageModificationValue: pmv ), pageModificationValue: pmv );
						} )()
				};

		private Func<string, FormItem> getCheckbox( string label, CheckboxSetup setup, PageModificationValue<bool> pageModificationValue = null ) =>
			id => new Checkbox(
				false,
				"Lorem ipsum dolor sit amet".ToComponents(),
				setup: setup,
				validationMethod: ( postBackValue, validator ) => AddStatusMessage(
					StatusMessageType.Info,
					"{0}: {1}".FormatWith( id, postBackValue.Value.ToString() ) ) ).ToFormItem(
				label: "{0}. {1}".FormatWith( id, label )
					.ToComponents()
					.Concat(
						pageModificationValue != null
							? new LineBreak().ToCollection<PhrasingComponent>()
								.Append(
									new SideComments(
										"Value: ".ToComponents()
											.Concat(
												pageModificationValue.ToGenericPhrasingContainer(
													v => v.ToString(),
													valueExpression => "{0} ? 'True' : 'False'".FormatWith( valueExpression ) ) )
											.Materialize() ) )
							: Enumerable.Empty<PhrasingComponent>() )
					.Materialize() );

		private Func<string, FormItem> getFlowCheckbox( string label, FlowCheckboxSetup setup ) =>
			id => new FlowCheckbox(
				false,
				"Lorem ipsum dolor sit amet".ToComponents(),
				setup: setup,
				validationMethod: ( postBackValue, validator ) => AddStatusMessage(
					StatusMessageType.Info,
					"{0}: {1}".FormatWith( id, postBackValue.Value.ToString() ) ) ).ToFormItem( label: "{0}. {1}".FormatWith( id, label ).ToComponents() );

		private Func<string, FormItem> getRadioGroup(
			string label, RadioButtonSetup setup, bool noSelection = false, bool singleButton = false, FormAction selectionChangedAction = null,
			PageModificationValue<bool> pageModificationValue = null ) =>
			id => {
				var group = new RadioButtonGroup( noSelection, disableSingleButtonDetection: singleButton, selectionChangedAction: selectionChangedAction );
				return FormItem.Create(
					new PlaceHolder().AddControlsReturnThis(
						"{0}. {1}".FormatWith( id, label )
							.ToComponents()
							.Concat(
								pageModificationValue != null
									? new LineBreak().ToCollection<PhrasingComponent>()
										.Append(
											new SideComments(
												"First button value: ".ToComponents()
													.Concat(
														pageModificationValue.ToGenericPhrasingContainer(
															v => v.ToString(),
															valueExpression => "{0} ? 'True' : 'False'".FormatWith( valueExpression ) ) )
													.Materialize() ) )
									: Enumerable.Empty<PhrasingComponent>() )
							.GetControls() ),
					ControlStack.CreateWithControls(
						true,
						group
							.CreateRadioButton(
								!noSelection,
								"First".ToComponents(),
								setup: setup,
								validationMethod: ( postBackValue, validator ) => AddStatusMessage(
									StatusMessageType.Info,
									"{0}-1: {1}".FormatWith( id, postBackValue.Value.ToString() ) ) )
							.ToFormItem()
							.ToControl()
							.ToCollection()
							.Concat(
								singleButton
									? Enumerable.Empty<Control>()
									: group.CreateFlowRadioButton(
											false,
											"Second".ToComponents(),
											setup: FlowRadioButtonSetup.Create(
												nestedContentGetter: () => "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit.".ToComponents() ),
											validationMethod: ( postBackValue, validator ) => AddStatusMessage(
												StatusMessageType.Info,
												"{0}-2: {1}".FormatWith( id, postBackValue.Value.ToString() ) ) )
										.ToFormItem()
										.ToControl()
										.ToCollection()
										.Append(
											group.CreateRadioButton(
													false,
													"Third".ToComponents(),
													setup: RadioButtonSetup.Create(),
													validationMethod: ( postBackValue, validator ) => AddStatusMessage(
														StatusMessageType.Info,
														"{0}-3: {1}".FormatWith( id, postBackValue.Value.ToString() ) ) )
												.ToFormItem()
												.ToControl() ) )
							.ToArray() ) );
			};
	}
}